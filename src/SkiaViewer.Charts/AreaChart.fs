namespace SkiaViewer.Charts

open System
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

module AreaChart =

    let defaultConfig (width: float32) (height: float32) : ChartConfig =
        Defaults.chartConfig width height

    let areaChart (config: ChartConfig) (series: DataSeries list) : Element =
        // Prepare each series: filter valid points and sort by X
        let preparedSeries =
            series
            |> List.map (fun s ->
                let validPoints =
                    s.Points
                    |> List.filter (fun p ->
                        not (Double.IsNaN p.Y) && not (Double.IsInfinity p.Y)
                        && not (Double.IsNaN p.X) && not (Double.IsInfinity p.X))
                    |> List.sortBy (fun p -> p.X)
                { s with Points = validPoints })

        // Build cumulative (stacked) Y values per series
        // cumulativeSeries.[i] holds the top-line Y values for series i
        // baselines.[i] holds the baseline Y values for series i
        let cumulativeSeries, baselines =
            let mutable prevTopPoints: DataPoint list = []
            preparedSeries
            |> List.map (fun s ->
                let baseline = prevTopPoints
                let topPoints =
                    s.Points
                    |> List.map (fun p ->
                        // Find matching baseline point
                        let baseY =
                            baseline
                            |> List.tryFind (fun bp -> bp.X = p.X)
                            |> Option.map (fun bp -> bp.Y)
                            |> Option.defaultValue 0.0
                        { X = p.X; Y = p.Y + baseY })
                // Merge top points into the running accumulation for next series
                prevTopPoints <-
                    let existingMap =
                        prevTopPoints
                        |> List.map (fun p -> p.X, p.Y)
                        |> Map.ofList
                    let topMap =
                        topPoints
                        |> List.map (fun p -> p.X, p.Y)
                        |> Map.ofList
                    let merged =
                        Map.fold (fun acc k v ->
                            Map.add k v acc) existingMap topMap
                    merged |> Map.toList |> List.map (fun (x, y) -> { X = x; Y = y })
                (topPoints, baseline))
            |> List.unzip

        // Collect all values for axis range computation
        let allAccumulatedPoints =
            cumulativeSeries |> List.collect id

        let allXValues =
            allAccumulatedPoints
            |> List.map (fun p -> p.X)

        let allYValues =
            allAccumulatedPoints
            |> List.map (fun p -> p.Y)

        // Compute auto ranges
        let yAutoMin, yAutoMax =
            if allYValues.IsEmpty then 0.0, 1.0
            else
                let mn = List.min allYValues |> min 0.0
                let mx = List.max allYValues
                Axis.computeAutoRange [mn; mx]

        let xAutoMin, xAutoMax = Axis.computeAutoRange allXValues

        // Apply manual overrides
        let yMin = config.YAxis.Min |> Option.defaultValue yAutoMin
        let yMax = config.YAxis.Max |> Option.defaultValue yAutoMax
        let xMin = config.XAxis.Min |> Option.defaultValue xAutoMin
        let xMax = config.XAxis.Max |> Option.defaultValue xAutoMax

        // Compute axis ticks
        let yAxisMin, yAxisMax, yTicks = Axis.computeAxisTicks yMin yMax config.YAxis.TickCount
        let xAxisMin, xAxisMax, xTicks = Axis.computeAxisTicks xMin xMax config.XAxis.TickCount

        // Compute chart area
        let hasLegend = config.Legend.Visible && series.Length > 1
        let area =
            ChartHelpers.computeChartArea
                config.Width config.Height config.Padding
                config.Title.IsSome config.TitleFontSize hasLegend

        // Build elements
        let elements = ResizeArray<Element>()

        // Optional background
        match config.BackgroundColor with
        | Some bgColor ->
            elements.Add(Scene.rect 0.0f 0.0f config.Width config.Height (Scene.fill bgColor))
        | None -> ()

        // Optional title
        match config.Title with
        | Some title ->
            elements.Add(ChartHelpers.renderTitle title config.TitleFontSize config.Width)
        | None -> ()

        // Axes
        let xAxisElements = ChartHelpers.renderXAxis area xTicks xAxisMin xAxisMax config.XAxis.Label
        let yAxisElements = ChartHelpers.renderYAxis area yTicks yAxisMin yAxisMax config.YAxis.Label
        for el in xAxisElements do elements.Add(el)
        for el in yAxisElements do elements.Add(el)

        // Grid lines
        if config.XAxis.ShowGridLines || config.YAxis.ShowGridLines then
            let gridColor =
                config.XAxis.GridLineColor
                |> Option.defaultValue (SKColor(0xD0uy, 0xD0uy, 0xD0uy))
            let gridXTicks = if config.XAxis.ShowGridLines then xTicks else []
            let gridYTicks = if config.YAxis.ShowGridLines then yTicks else []
            let gridElements =
                ChartHelpers.renderGridLines area gridXTicks gridYTicks xAxisMin xAxisMax yAxisMin yAxisMax gridColor
            for el in gridElements do elements.Add(el)

        // Render each series area (bottom to top order, which is list order)
        cumulativeSeries |> List.iteri (fun seriesIndex topPoints ->
            let baselinePoints = baselines.[seriesIndex]
            let color = ChartHelpers.paletteColor config.Palette seriesIndex

            match topPoints with
            | [] -> ()
            | _ ->
                // Map top points to pixel coords
                let mappedTop =
                    topPoints
                    |> List.map (fun p ->
                        ChartHelpers.mapX p.X xAxisMin xAxisMax area,
                        ChartHelpers.mapY p.Y yAxisMin yAxisMax area)

                // Map baseline points to pixel coords (reverse order for closing the path)
                let mappedBaseline =
                    if baselinePoints.IsEmpty then
                        // Baseline is Y=0
                        let baseY = ChartHelpers.mapY 0.0 yAxisMin yAxisMax area
                        let firstX = fst mappedTop.[0]
                        let lastX = fst mappedTop.[mappedTop.Length - 1]
                        [ (lastX, baseY); (firstX, baseY) ]
                    else
                        // Use baseline points matching top X values, reversed
                        topPoints
                        |> List.map (fun tp ->
                            let baseY =
                                baselinePoints
                                |> List.tryFind (fun bp -> bp.X = tp.X)
                                |> Option.map (fun bp -> bp.Y)
                                |> Option.defaultValue 0.0
                            ChartHelpers.mapX tp.X xAxisMin xAxisMax area,
                            ChartHelpers.mapY baseY yAxisMin yAxisMax area)
                        |> List.rev

                // Build closed path: top line forward, then baseline backward
                let pathCommands =
                    let first = mappedTop.[0]
                    let topMoves =
                        mappedTop
                        |> List.tail
                        |> List.map (fun (x, y) -> PathCommand.LineTo(x, y))
                    let baselineMoves =
                        mappedBaseline
                        |> List.map (fun (x, y) -> PathCommand.LineTo(x, y))
                    [ PathCommand.MoveTo(fst first, snd first) ]
                    @ topMoves
                    @ baselineMoves
                    @ [ PathCommand.Close ]

                // Semi-transparent fill
                let fillPaint = Scene.fill color |> Scene.withOpacity 0.5f
                elements.Add(Scene.path pathCommands fillPaint)

                // Top line with full opacity
                let linePaint = Scene.stroke color 2.0f
                mappedTop
                |> List.pairwise
                |> List.iter (fun ((x1, y1), (x2, y2)) ->
                    elements.Add(Scene.line x1 y1 x2 y2 linePaint))
        )

        // Legend
        if hasLegend then
            let names = series |> List.map (fun s -> s.Name)
            let legend =
                ChartHelpers.renderLegend names config.Palette config.Legend.Position
                    config.Width config.Height area
            elements.Add(legend)

        Scene.group None None (Seq.toList elements)

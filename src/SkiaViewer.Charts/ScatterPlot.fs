namespace SkiaViewer.Charts

open System
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

module ScatterPlot =

    let defaultConfig (width: float32) (height: float32) : ChartConfig =
        Defaults.chartConfig width height

    let scatterPlot (config: ChartConfig) (series: DataSeries list) : Element =
        let allPoints =
            series |> List.collect (fun s -> s.Points)

        let allYValues =
            allPoints
            |> List.map (fun p -> p.Y)
            |> List.filter (fun v -> not (Double.IsNaN v) && not (Double.IsInfinity v))

        let allXValues =
            allPoints
            |> List.map (fun p -> p.X)
            |> List.filter (fun v -> not (Double.IsNaN v) && not (Double.IsInfinity v))

        // Compute auto ranges
        let yAutoMin, yAutoMax = Axis.computeAutoRange allYValues
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

        // Render each series as scatter points
        series |> List.iteri (fun seriesIndex s ->
            let color = ChartHelpers.paletteColor config.Palette seriesIndex
            let dotPaint = Scene.fill color

            s.Points |> List.iter (fun p ->
                if not (Double.IsNaN p.Y) && not (Double.IsInfinity p.Y)
                   && not (Double.IsNaN p.X) && not (Double.IsInfinity p.X) then
                    let px = ChartHelpers.mapX p.X xAxisMin xAxisMax area
                    let py = ChartHelpers.mapY p.Y yAxisMin yAxisMax area
                    elements.Add(Scene.circle px py 3.0f dotPaint)
            )
        )

        // Legend
        if hasLegend then
            let names = series |> List.map (fun s -> s.Name)
            let legend =
                ChartHelpers.renderLegend names config.Palette config.Legend.Position
                    config.Width config.Height area
            elements.Add(legend)

        Scene.group None None (Seq.toList elements)

namespace SkiaViewer.Charts

open System
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

module BarChart =

    let defaultConfig (width: float32) (height: float32) : ChartConfig =
        Defaults.chartConfig width height

    let barChart (config: ChartConfig) (layout: BarLayout) (data: CategoryValue list) : Element =
        // Collect all series names across categories
        let seriesNames =
            data
            |> List.collect (fun cv -> cv.Values |> List.map fst)
            |> List.distinct

        // Compute Y range
        let maxValue =
            if data.IsEmpty then 1.0
            else
                match layout with
                | BarLayout.Grouped ->
                    data
                    |> List.collect (fun cv -> cv.Values |> List.map snd)
                    |> List.filter (fun v -> not (Double.IsNaN v) && not (Double.IsInfinity v))
                    |> function
                        | [] -> 1.0
                        | vals -> List.max vals
                | BarLayout.Stacked ->
                    data
                    |> List.map (fun cv ->
                        cv.Values
                        |> List.map snd
                        |> List.filter (fun v -> not (Double.IsNaN v) && not (Double.IsInfinity v))
                        |> List.sum)
                    |> function
                        | [] -> 1.0
                        | sums -> List.max sums

        let yMin = 0.0
        let yMax = config.YAxis.Max |> Option.defaultValue (max maxValue 0.001)

        // Compute axis ticks for Y
        let yAxisMin, yAxisMax, yTicks = Axis.computeAxisTicks yMin yMax config.YAxis.TickCount

        // Compute chart area
        let hasLegend = config.Legend.Visible && seriesNames.Length > 1
        let area =
            ChartHelpers.computeChartArea
                config.Width config.Height config.Padding
                config.Title.IsSome config.TitleFontSize hasLegend

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

        // Y axis
        let yAxisElements = ChartHelpers.renderYAxis area yTicks yAxisMin yAxisMax config.YAxis.Label
        for el in yAxisElements do elements.Add(el)

        // X axis line
        let axisLinePaint = Scene.stroke SKColors.Black 1.0f
        elements.Add(Scene.line area.Left area.Bottom area.Right area.Bottom axisLinePaint)

        // Grid lines
        if config.YAxis.ShowGridLines then
            let gridColor =
                config.XAxis.GridLineColor
                |> Option.defaultValue (SKColor(0xD0uy, 0xD0uy, 0xD0uy))
            let gridElements =
                ChartHelpers.renderGridLines area [] yTicks 0.0 1.0 yAxisMin yAxisMax gridColor
            for el in gridElements do elements.Add(el)

        // Category positioning
        let categoryCount = max 1 data.Length
        let chartWidth = area.Right - area.Left
        let categoryWidth = chartWidth / float32 categoryCount
        let categoryPadding = categoryWidth * 0.1f
        let usableWidth = categoryWidth - 2.0f * categoryPadding

        let labelPaint = Scene.fill SKColors.Black

        // Render bars
        data |> List.iteri (fun catIndex cv ->
            let catLeft = area.Left + float32 catIndex * categoryWidth + categoryPadding

            match layout with
            | BarLayout.Grouped ->
                let seriesCount = max 1 seriesNames.Length
                let barWidth = usableWidth / float32 seriesCount

                seriesNames |> List.iteri (fun si sName ->
                    let value =
                        cv.Values
                        |> List.tryFind (fun (n, _) -> n = sName)
                        |> Option.map snd
                        |> Option.defaultValue 0.0

                    if not (Double.IsNaN value) && not (Double.IsInfinity value) && value > 0.0 then
                        let color = ChartHelpers.paletteColor config.Palette si
                        let barX = catLeft + float32 si * barWidth
                        let barTop = ChartHelpers.mapY value yAxisMin yAxisMax area
                        let barBottom = ChartHelpers.mapY 0.0 yAxisMin yAxisMax area
                        let barHeight = barBottom - barTop
                        elements.Add(Scene.rect barX barTop barWidth barHeight (Scene.fill color))
                )

            | BarLayout.Stacked ->
                let mutable cumulativeY = 0.0

                seriesNames |> List.iteri (fun si sName ->
                    let value =
                        cv.Values
                        |> List.tryFind (fun (n, _) -> n = sName)
                        |> Option.map snd
                        |> Option.defaultValue 0.0

                    if not (Double.IsNaN value) && not (Double.IsInfinity value) && value > 0.0 then
                        let color = ChartHelpers.paletteColor config.Palette si
                        let stackBottom = cumulativeY
                        let stackTop = cumulativeY + value
                        let barTop = ChartHelpers.mapY stackTop yAxisMin yAxisMax area
                        let barBottom = ChartHelpers.mapY stackBottom yAxisMin yAxisMax area
                        let barHeight = barBottom - barTop
                        elements.Add(Scene.rect catLeft barTop usableWidth barHeight (Scene.fill color))
                        cumulativeY <- stackTop
                )

            // Category label
            let labelX = area.Left + float32 catIndex * categoryWidth + categoryWidth / 2.0f
            let labelY = area.Bottom + 18.0f
            elements.Add(Scene.text cv.Category labelX labelY 10.0f labelPaint)
        )

        // Legend
        if hasLegend then
            let legend =
                ChartHelpers.renderLegend seriesNames config.Palette config.Legend.Position
                    config.Width config.Height area
            elements.Add(legend)

        Scene.group None None (Seq.toList elements)

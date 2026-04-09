namespace SkiaViewer.Charts

open System
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

module Histogram =

    let defaultConfig (width: float32) (height: float32) : HistogramConfig =
        Defaults.histogramConfig width height

    let histogram (config: HistogramConfig) (values: float list) : Element =
        // Filter out NaN/Infinity
        let validValues =
            values
            |> List.filter (fun v -> not (Double.IsNaN v) && not (Double.IsInfinity v))

        // Compute chart area
        let hasLegend = false
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

        match validValues with
        | [] ->
            // Empty: render axes only with default range
            let _, _, yTicks = Axis.computeAxisTicks 0.0 1.0 config.YAxis.TickCount
            let _, _, xTicks = Axis.computeAxisTicks 0.0 1.0 config.XAxis.TickCount
            let xAxisElements = ChartHelpers.renderXAxis area xTicks 0.0 1.0 config.XAxis.Label
            let yAxisElements = ChartHelpers.renderYAxis area yTicks 0.0 1.0 config.YAxis.Label
            for el in xAxisElements do elements.Add(el)
            for el in yAxisElements do elements.Add(el)
        | _ ->
            let dataMin = List.min validValues
            let dataMax = List.max validValues

            let binCount = max 1 config.BinCount

            // Compute bin edges: evenly divide [dataMin, dataMax]
            let range = dataMax - dataMin
            let binWidth =
                if range = 0.0 then 1.0
                else range / float binCount

            let binEdges =
                [| for i in 0 .. binCount ->
                    dataMin + float i * binWidth |]

            // Count values per bin
            let frequencies = Array.zeroCreate binCount

            for v in validValues do
                // Determine bin index
                let idx =
                    if range = 0.0 then 0
                    else
                        let i = int ((v - dataMin) / binWidth)
                        // Last bin includes upper boundary
                        min i (binCount - 1)
                frequencies.[idx] <- frequencies.[idx] + 1

            let maxFreq =
                if frequencies.Length = 0 then 1
                else Array.max frequencies

            // Axis ranges
            let xMin = config.XAxis.Min |> Option.defaultValue dataMin
            let xMax = config.XAxis.Max |> Option.defaultValue binEdges.[binCount]
            let yMin = 0.0
            let yMax = config.YAxis.Max |> Option.defaultValue (float maxFreq)

            // Compute axis ticks
            let xAxisMin, xAxisMax, xTicks = Axis.computeAxisTicks xMin xMax config.XAxis.TickCount
            let yAxisMin, yAxisMax, yTicks = Axis.computeAxisTicks yMin yMax config.YAxis.TickCount

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

            // Bar color
            let barColor =
                config.BarColor
                |> Option.defaultValue (ChartHelpers.paletteColor config.Palette 0)

            let barPaint = Scene.fill barColor

            // Render bins as adjacent rectangles
            for i in 0 .. binCount - 1 do
                let freq = frequencies.[i]
                if freq > 0 then
                    let binStart = binEdges.[i]
                    let binEnd = binEdges.[i + 1]
                    let x1 = ChartHelpers.mapX binStart xAxisMin xAxisMax area
                    let x2 = ChartHelpers.mapX binEnd xAxisMin xAxisMax area
                    let yTop = ChartHelpers.mapY (float freq) yAxisMin yAxisMax area
                    let yBottom = ChartHelpers.mapY 0.0 yAxisMin yAxisMax area
                    let barWidth = x2 - x1
                    let barHeight = yBottom - yTop
                    elements.Add(Scene.rect x1 yTop barWidth barHeight barPaint)

        Scene.group None None (Seq.toList elements)

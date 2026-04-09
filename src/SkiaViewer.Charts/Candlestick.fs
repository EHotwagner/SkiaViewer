namespace SkiaViewer.Charts

open System
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

module Candlestick =

    let defaultConfig (width: float32) (height: float32) : CandlestickConfig =
        Defaults.candlestickConfig width height

    let candlestickChart (config: CandlestickConfig) (data: OhlcData list) : Element =
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

        match data with
        | [] ->
            // Empty data: render axes only
            let yAxisMin, yAxisMax, yTicks = Axis.computeAxisTicks 0.0 1.0 config.YAxis.TickCount
            let area =
                ChartHelpers.computeChartArea
                    config.Width config.Height config.Padding
                    config.Title.IsSome config.TitleFontSize false

            let yAxisElements = ChartHelpers.renderYAxis area yTicks yAxisMin yAxisMax config.YAxis.Label
            for el in yAxisElements do elements.Add(el)

            let axisLinePaint = Scene.stroke SKColors.Black 1.0f
            elements.Add(Scene.line area.Left area.Bottom area.Right area.Bottom axisLinePaint)

            Scene.group None None (Seq.toList elements)

        | _ ->
            // Compute Y range from all Low/High values
            let allLows = data |> List.map (fun d -> d.Low)
            let allHighs = data |> List.map (fun d -> d.High)
            let yMin =
                config.YAxis.Min
                |> Option.defaultValue (List.min allLows)
            let yMax =
                config.YAxis.Max
                |> Option.defaultValue (List.max allHighs)

            // Compute axis ticks for Y
            let yAxisMin, yAxisMax, yTicks = Axis.computeAxisTicks yMin yMax config.YAxis.TickCount

            // X axis: index-based positioning; build tick labels from OhlcData.Label
            let xTicks =
                data
                |> List.mapi (fun i d ->
                    let xVal = float i + 0.5
                    (xVal, d.Label))

            let xMin = 0.0
            let xMax = float data.Length

            // Compute chart area
            let hasLegend = false
            let area =
                ChartHelpers.computeChartArea
                    config.Width config.Height config.Padding
                    config.Title.IsSome config.TitleFontSize hasLegend

            // Axes
            let yAxisElements = ChartHelpers.renderYAxis area yTicks yAxisMin yAxisMax config.YAxis.Label
            for el in yAxisElements do elements.Add(el)

            let xAxisElements = ChartHelpers.renderXAxis area xTicks xMin xMax config.XAxis.Label
            for el in xAxisElements do elements.Add(el)

            // Grid lines
            if config.YAxis.ShowGridLines then
                let gridColor =
                    config.YAxis.GridLineColor
                    |> Option.defaultValue (SKColor(0xD0uy, 0xD0uy, 0xD0uy))
                let gridElements =
                    ChartHelpers.renderGridLines area [] yTicks xMin xMax yAxisMin yAxisMax gridColor
                for el in gridElements do elements.Add(el)

            // Candle width: 60% of available slot width
            let chartWidth = area.Right - area.Left
            let slotWidth = chartWidth / float32 data.Length
            let candleWidth = slotWidth * 0.6f

            let wickPaint = Scene.stroke SKColors.Black 1.0f

            // Render each candle
            data |> List.iteri (fun i d ->
                let centerX = ChartHelpers.mapX (float i + 0.5) xMin xMax area

                // Wick: vertical line from Low to High
                let wickTop = ChartHelpers.mapY d.High yAxisMin yAxisMax area
                let wickBottom = ChartHelpers.mapY d.Low yAxisMin yAxisMax area
                elements.Add(Scene.line centerX wickTop centerX wickBottom wickPaint)

                // Body: rect from min(Open,Close) to max(Open,Close)
                let isUp = d.Close > d.Open
                let bodyColor = if isUp then config.UpColor else config.DownColor
                let bodyTop = ChartHelpers.mapY (max d.Open d.Close) yAxisMin yAxisMax area
                let bodyBottom = ChartHelpers.mapY (min d.Open d.Close) yAxisMin yAxisMax area
                let bodyHeight = max (bodyBottom - bodyTop) 1.0f
                let bodyX = centerX - candleWidth / 2.0f
                elements.Add(Scene.rect bodyX bodyTop candleWidth bodyHeight (Scene.fill bodyColor))
            )

            Scene.group None None (Seq.toList elements)

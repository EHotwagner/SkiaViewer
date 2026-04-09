namespace SkiaViewer.Charts

open System
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

module PieChart =

    let defaultConfig (width: float32) (height: float32) : PieConfig =
        Defaults.pieConfig width height

    let pieChart (config: PieConfig) (slices: SliceData list) : Element =
        let elements = ResizeArray<Element>()

        // Optional background
        match config.BackgroundColor with
        | Some bgColor ->
            elements.Add(Scene.rect 0.0f 0.0f config.Width config.Height (Scene.fill bgColor))
        | None -> ()

        // Optional title
        let titleOffset =
            match config.Title with
            | Some title ->
                elements.Add(ChartHelpers.renderTitle title config.TitleFontSize config.Width)
                config.TitleFontSize + 10.0f
            | None -> 0.0f

        let cx = config.Width / 2.0f
        let cy = config.Padding + titleOffset + (config.Height - config.Padding * 2.0f - titleOffset) / 2.0f
        let radius = (min config.Width config.Height) / 2.0f - config.Padding - titleOffset / 2.0f

        match slices with
        | [] ->
            // No data
            elements.Add(Scene.text "No data" cx cy 14.0f (Scene.fill SKColors.Gray))
            Scene.group None None (Seq.toList elements)
        | _ ->
            let total = slices |> List.sumBy (fun s -> s.Value)

            if total <= 0.0 then
                // All-zero values: empty circle + "No data"
                let boundingRect = SKRect(cx - radius, cy - radius, cx + radius, cy + radius)
                elements.Add(Scene.arc boundingRect 0.0f 360.0f true (Scene.stroke SKColors.Gray 1.0f))
                elements.Add(Scene.text "No data" cx cy 14.0f (Scene.fill SKColors.Gray))
                Scene.group None None (Seq.toList elements)
            else
                let boundingRect = SKRect(cx - radius, cy - radius, cx + radius, cy + radius)

                let mutable startAngle = -90.0f

                slices |> List.iteri (fun i slice ->
                    let sweepAngle =
                        if slices.Length = 1 then 360.0f
                        else float32 (slice.Value / total) * 360.0f

                    let color = ChartHelpers.paletteColor config.Palette i
                    let arcPaint = Scene.fill color
                    elements.Add(Scene.arc boundingRect startAngle sweepAngle true arcPaint)

                    // Labels
                    if config.ShowLabels then
                        let midAngle = startAngle + sweepAngle / 2.0f
                        let rad = float midAngle * Math.PI / 180.0
                        let lx = cx + float32 (cos rad) * radius * 0.7f
                        let ly = cy + float32 (sin rad) * radius * 0.7f
                        elements.Add(Scene.text slice.Label lx ly 10.0f (Scene.fill SKColors.Black))

                    startAngle <- startAngle + sweepAngle
                )

                // Donut hole
                if config.DonutRatio > 0.0f then
                    let innerRadius = radius * config.DonutRatio
                    elements.Add(
                        Scene.ellipse cx cy innerRadius innerRadius (Scene.fill SKColors.White))

                // Legend
                if config.Legend.Visible then
                    let hasTitle = config.Title.IsSome
                    let hasLegend = true
                    let area =
                        ChartHelpers.computeChartArea
                            config.Width config.Height config.Padding
                            hasTitle config.TitleFontSize hasLegend
                    let names = slices |> List.map (fun s -> s.Label)
                    let legend =
                        ChartHelpers.renderLegend names config.Palette config.Legend.Position
                            config.Width config.Height area
                    elements.Add(legend)

                Scene.group None None (Seq.toList elements)

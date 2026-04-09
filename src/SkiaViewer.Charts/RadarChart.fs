namespace SkiaViewer.Charts

open System
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

module RadarChart =

    let defaultConfig (width: float32) (height: float32) (categories: string list) : RadarConfig =
        Defaults.radarConfig width height categories

    let radarChart (config: RadarConfig) (series: RadarSeries list) : Element =
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

        let categories = config.Categories
        let n = categories.Length

        match series, n with
        | [], _ | _, 0 ->
            // No data or no categories
            let cx = config.Width / 2.0f
            let cy = config.Height / 2.0f
            elements.Add(Scene.text "No data" cx cy 14.0f (Scene.fill SKColors.Gray))
            Scene.group None None (Seq.toList elements)

        | _ ->
            // Compute center with title offset
            let cx = config.Width / 2.0f
            let cy = config.Padding + titleOffset + (config.Height - config.Padding * 2.0f - titleOffset) / 2.0f

            // Compute radius with extra space for labels
            let labelPadding = 30.0f
            let radius =
                (min config.Width config.Height) / 2.0f - config.Padding - labelPadding

            // Determine maxValue
            let maxValue =
                match config.MaxValue with
                | Some mv -> mv
                | None ->
                    series
                    |> List.collect (fun s -> s.Values)
                    |> List.filter (fun v -> not (Double.IsNaN v) && not (Double.IsInfinity v))
                    |> function
                        | [] -> 1.0
                        | vals -> List.max vals
                    |> max 0.001

            // Compute angle for each category: starting at -PI/2 (top)
            let angleFor i =
                -Math.PI / 2.0 + 2.0 * Math.PI * float i / float n

            // Helper: vertex position at given fraction of radius
            let vertexAt i fraction =
                let angle = angleFor i
                let x = float cx + Math.Cos(angle) * float radius * fraction
                let y = float cy + Math.Sin(angle) * float radius * fraction
                (float32 x, float32 y)

            // Grid: concentric polygons
            if config.ShowGrid then
                let gridPaint = Scene.stroke (SKColor(0xD0uy, 0xD0uy, 0xD0uy)) 1.0f
                for level in 1 .. config.GridLevels do
                    let fraction = float level / float config.GridLevels
                    let commands =
                        [ for i in 0 .. n - 1 do
                            let (vx, vy) = vertexAt i fraction
                            if i = 0 then
                                yield PathCommand.MoveTo(vx, vy)
                            else
                                yield PathCommand.LineTo(vx, vy)
                          yield PathCommand.Close ]
                    elements.Add(Scene.path commands gridPaint)

            // Axis lines: from center to each vertex at full radius
            let axisPaint = Scene.stroke (SKColor(0xA0uy, 0xA0uy, 0xA0uy)) 1.0f
            for i in 0 .. n - 1 do
                let (vx, vy) = vertexAt i 1.0
                elements.Add(Scene.line cx cy vx vy axisPaint)

            // Category labels at each axis endpoint (slightly beyond radius)
            let labelPaint = Scene.fill SKColors.Black
            for i in 0 .. n - 1 do
                let (lx, ly) = vertexAt i 1.15
                elements.Add(Scene.text categories.[i] lx ly 10.0f labelPaint)

            // Render each series
            series |> List.iteri (fun seriesIndex s ->
                let color = ChartHelpers.paletteColor config.Palette seriesIndex

                // Compute vertex positions for this series
                let vertices =
                    [ for i in 0 .. n - 1 do
                        let value =
                            if i < s.Values.Length then s.Values.[i]
                            else 0.0
                        let fraction = value / maxValue
                        let angle = angleFor i
                        let vx = float cx + Math.Cos(angle) * float radius * fraction
                        let vy = float cy + Math.Sin(angle) * float radius * fraction
                        yield (float32 vx, float32 vy) ]

                // Build closed path polygon
                let pathCommands =
                    [ for i in 0 .. vertices.Length - 1 do
                        let (vx, vy) = vertices.[i]
                        if i = 0 then
                            yield PathCommand.MoveTo(vx, vy)
                        else
                            yield PathCommand.LineTo(vx, vy)
                      yield PathCommand.Close ]

                // Fill with palette color at 0.3 opacity
                let fillPaint = Scene.fill color |> Scene.withOpacity 0.3f
                elements.Add(Scene.path pathCommands fillPaint)

                // Outline with full opacity stroke
                let strokePaint = Scene.stroke color 2.0f
                elements.Add(Scene.path pathCommands strokePaint)
            )

            // Legend
            if config.Legend.Visible && series.Length > 1 then
                let area =
                    ChartHelpers.computeChartArea
                        config.Width config.Height config.Padding
                        config.Title.IsSome config.TitleFontSize true
                let names = series |> List.map (fun s -> s.Name)
                let legend =
                    ChartHelpers.renderLegend names config.Palette config.Legend.Position
                        config.Width config.Height area
                elements.Add(legend)

            Scene.group None None (Seq.toList elements)

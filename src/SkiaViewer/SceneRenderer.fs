namespace SkiaViewer

open System
open SkiaSharp

module internal SceneRenderer =

    let rec private toMatrix (transform: Transform) : SKMatrix =
        match transform with
        | Transform.Translate(x, y) ->
            SKMatrix.CreateTranslation(x, y)
        | Transform.Rotate(degrees, cx, cy) ->
            SKMatrix.CreateRotationDegrees(degrees, cx, cy)
        | Transform.Scale(sx, sy, cx, cy) ->
            SKMatrix.CreateScale(sx, sy, cx, cy)
        | Transform.Matrix m ->
            m
        | Transform.Compose transforms ->
            transforms
            |> List.fold (fun acc t -> SKMatrix.Concat(acc, toMatrix t)) SKMatrix.Identity

    let private makeSKPaint (paint: Paint) (forStroke: bool) : SKPaint =
        let skPaint = new SKPaint()
        skPaint.IsAntialias <- paint.IsAntialias

        let opacity = Math.Clamp(paint.Opacity, 0.0f, 1.0f)

        if forStroke then
            skPaint.IsStroke <- true
            skPaint.StrokeWidth <- paint.StrokeWidth
            match paint.Stroke with
            | Some c ->
                let alpha = byte (float32 c.Alpha * opacity)
                skPaint.Color <- c.WithAlpha(alpha)
            | None ->
                skPaint.Color <- SKColor(0uy, 0uy, 0uy, 0uy)
        else
            skPaint.IsStroke <- false
            match paint.Fill with
            | Some c ->
                let alpha = byte (float32 c.Alpha * opacity)
                skPaint.Color <- c.WithAlpha(alpha)
            | None ->
                skPaint.Color <- SKColor(0uy, 0uy, 0uy, 0uy)

        skPaint

    let private drawWithPaint (paint: Paint) (canvas: SKCanvas) (drawFn: SKPaint -> unit) =
        // Draw fill
        match paint.Fill with
        | Some _ ->
            use skPaint = makeSKPaint paint false
            drawFn skPaint
        | None -> ()

        // Draw stroke
        match paint.Stroke with
        | Some _ ->
            use skPaint = makeSKPaint paint true
            drawFn skPaint
        | None -> ()

    let rec private renderElement (canvas: SKCanvas) (element: Element) =
        match element with
        | Element.Rect(x, y, w, h, paint) ->
            drawWithPaint paint canvas (fun skPaint ->
                canvas.DrawRect(x, y, w, h, skPaint))

        | Element.Ellipse(cx, cy, rx, ry, paint) ->
            drawWithPaint paint canvas (fun skPaint ->
                canvas.DrawOval(cx, cy, rx, ry, skPaint))

        | Element.Line(x1, y1, x2, y2, paint) ->
            drawWithPaint paint canvas (fun skPaint ->
                // Lines are always stroked; use stroke color, or fill as fallback
                match paint.Stroke with
                | Some _ -> ()
                | None ->
                    match paint.Fill with
                    | Some c ->
                        let opacity = Math.Clamp(paint.Opacity, 0.0f, 1.0f)
                        let alpha = byte (float32 c.Alpha * opacity)
                        skPaint.Color <- c.WithAlpha(alpha)
                    | None -> ()
                skPaint.IsStroke <- true
                if skPaint.StrokeWidth <= 0.0f then
                    skPaint.StrokeWidth <- 1.0f
                canvas.DrawLine(x1, y1, x2, y2, skPaint))

        | Element.Text(text, x, y, fontSize, paint) ->
            drawWithPaint paint canvas (fun skPaint ->
                skPaint.TextSize <- fontSize
                canvas.DrawText(text, x, y, skPaint))

        | Element.Image(bitmap, x, y, w, h, paint) ->
            if isNull bitmap then
                eprintfn "[SceneRenderer] Warning: Image element has null bitmap, skipping"
            else
                try
                    let destRect = SKRect(x, y, x + w, y + h)
                    use skPaint = makeSKPaint paint false
                    canvas.DrawBitmap(bitmap, destRect, skPaint)
                with
                | :? ObjectDisposedException ->
                    eprintfn "[SceneRenderer] Warning: Image element has disposed bitmap, skipping"

        | Element.Path(commands, paint) ->
            use skPath = new SKPath()
            for cmd in commands do
                match cmd with
                | PathCommand.MoveTo(x, y) -> skPath.MoveTo(x, y)
                | PathCommand.LineTo(x, y) -> skPath.LineTo(x, y)
                | PathCommand.QuadTo(cx, cy, x, y) -> skPath.QuadTo(cx, cy, x, y)
                | PathCommand.CubicTo(c1x, c1y, c2x, c2y, x, y) -> skPath.CubicTo(c1x, c1y, c2x, c2y, x, y)
                | PathCommand.ArcTo(rect, startAngle, sweepAngle) -> skPath.ArcTo(rect, startAngle, sweepAngle, false)
                | PathCommand.Close -> skPath.Close()

            drawWithPaint paint canvas (fun skPaint ->
                canvas.DrawPath(skPath, skPaint))

        | Element.Group(transform, groupPaint, children) ->
            let useLayer =
                match groupPaint with
                | Some p when p.Opacity < 1.0f -> true
                | _ -> false

            if useLayer then
                let opacity = Math.Clamp(groupPaint.Value.Opacity, 0.0f, 1.0f)
                use layerPaint = new SKPaint()
                layerPaint.Color <- SKColor(0uy, 0uy, 0uy, byte (255.0f * opacity))
                canvas.SaveLayer(layerPaint) |> ignore
            else
                canvas.Save() |> ignore

            match transform with
            | Some t ->
                let mutable matrix = toMatrix t
                canvas.Concat(&matrix)
            | None -> ()

            for child in children do
                renderElement canvas child

            canvas.Restore()

    let render (scene: Scene) (canvas: SKCanvas) =
        canvas.Clear(scene.BackgroundColor)
        for element in scene.Elements do
            renderElement canvas element

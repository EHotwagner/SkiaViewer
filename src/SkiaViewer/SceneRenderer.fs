namespace SkiaViewer

open System
open SkiaSharp

module internal SceneRenderer =

    let private toSKPathDirection (dir: PathDirection) =
        match dir with
        | PathDirection.Clockwise -> SKPathDirection.Clockwise
        | PathDirection.CounterClockwise -> SKPathDirection.CounterClockwise

    let private toSKBlendMode (mode: BlendMode) : SKBlendMode =
        match mode with
        | BlendMode.Clear -> SKBlendMode.Clear
        | BlendMode.Src -> SKBlendMode.Src
        | BlendMode.Dst -> SKBlendMode.Dst
        | BlendMode.SrcOver -> SKBlendMode.SrcOver
        | BlendMode.DstOver -> SKBlendMode.DstOver
        | BlendMode.SrcIn -> SKBlendMode.SrcIn
        | BlendMode.DstIn -> SKBlendMode.DstIn
        | BlendMode.SrcOut -> SKBlendMode.SrcOut
        | BlendMode.DstOut -> SKBlendMode.DstOut
        | BlendMode.SrcATop -> SKBlendMode.SrcATop
        | BlendMode.DstATop -> SKBlendMode.DstATop
        | BlendMode.Xor -> SKBlendMode.Xor
        | BlendMode.Plus -> SKBlendMode.Plus
        | BlendMode.Modulate -> SKBlendMode.Modulate
        | BlendMode.Screen -> SKBlendMode.Screen
        | BlendMode.Overlay -> SKBlendMode.Overlay
        | BlendMode.Darken -> SKBlendMode.Darken
        | BlendMode.Lighten -> SKBlendMode.Lighten
        | BlendMode.ColorDodge -> SKBlendMode.ColorDodge
        | BlendMode.ColorBurn -> SKBlendMode.ColorBurn
        | BlendMode.HardLight -> SKBlendMode.HardLight
        | BlendMode.SoftLight -> SKBlendMode.SoftLight
        | BlendMode.Difference -> SKBlendMode.Difference
        | BlendMode.Exclusion -> SKBlendMode.Exclusion
        | BlendMode.Multiply -> SKBlendMode.Multiply
        | BlendMode.Hue -> SKBlendMode.Hue
        | BlendMode.Saturation -> SKBlendMode.Saturation
        | BlendMode.Color -> SKBlendMode.Color
        | BlendMode.Luminosity -> SKBlendMode.Luminosity

    let private toSKShaderTileMode (mode: TileMode) : SKShaderTileMode =
        match mode with
        | TileMode.Clamp -> SKShaderTileMode.Clamp
        | TileMode.Repeat -> SKShaderTileMode.Repeat
        | TileMode.Mirror -> SKShaderTileMode.Mirror
        | TileMode.Decal -> SKShaderTileMode.Decal

    let rec private toSKShader (shader: Shader) : SKShader =
        match shader with
        | Shader.LinearGradient(start, endPt, colors, positions, tileMode) ->
            SKShader.CreateLinearGradient(start, endPt, colors, positions, toSKShaderTileMode tileMode)
        | Shader.RadialGradient(center, radius, colors, positions, tileMode) ->
            SKShader.CreateRadialGradient(center, radius, colors, positions, toSKShaderTileMode tileMode)
        | Shader.SweepGradient(center, colors, positions, startAngle, endAngle) ->
            SKShader.CreateSweepGradient(center, colors, positions, toSKShaderTileMode TileMode.Clamp, startAngle, endAngle)
        | Shader.TwoPointConicalGradient(start, startRadius, endPt, endRadius, colors, positions, tileMode) ->
            SKShader.CreateTwoPointConicalGradient(start, startRadius, endPt, endRadius, colors, positions, toSKShaderTileMode tileMode)
        | Shader.PerlinNoiseFractalNoise(baseFreqX, baseFreqY, numOctaves, seed) ->
            SKShader.CreatePerlinNoiseFractalNoise(baseFreqX, baseFreqY, numOctaves, seed)
        | Shader.PerlinNoiseTurbulence(baseFreqX, baseFreqY, numOctaves, seed) ->
            SKShader.CreatePerlinNoiseTurbulence(baseFreqX, baseFreqY, numOctaves, seed)
        | Shader.SolidColor(color) ->
            SKShader.CreateColor(color)
        | Shader.Image(bitmap, tileModeX, tileModeY) ->
            if isNull bitmap then
                SKShader.CreateColor(SKColors.Transparent)
            else
                SKShader.CreateBitmap(bitmap, toSKShaderTileMode tileModeX, toSKShaderTileMode tileModeY)
        | Shader.Compose(s1, s2, blendMode) ->
            let sk1 = toSKShader s1
            let sk2 = toSKShader s2
            SKShader.CreateCompose(sk1, sk2, toSKBlendMode blendMode)
        | Shader.RuntimeEffect(source, uniforms) ->
            let mutable errors = ""
            let effect = SKRuntimeEffect.Create(source, &errors)
            if isNull effect then
                raise (InvalidOperationException($"SkSL compilation error: {errors}"))
            // Build uniform data: pack float32 values in order of uniform declarations
            let uniformBytes = Array.zeroCreate<byte> (int effect.UniformSize)
            let mutable byteOffset = 0
            for uniformName in effect.Uniforms do
                let value =
                    uniforms
                    |> List.tryFind (fun (n, _) -> n = uniformName)
                    |> Option.map snd
                    |> Option.defaultValue 0f
                let valueBytes = System.BitConverter.GetBytes(value)
                if byteOffset + 4 <= uniformBytes.Length then
                    System.Array.Copy(valueBytes, 0, uniformBytes, byteOffset, 4)
                byteOffset <- byteOffset + 4
            let skUniforms = new SKRuntimeEffectUniforms(effect)
            let mutable idx = 0
            for uniformName in effect.Uniforms do
                let value =
                    uniforms
                    |> List.tryFind (fun (n, _) -> n = uniformName)
                    |> Option.map snd
                    |> Option.defaultValue 0f
                skUniforms.[uniformName] <- value
                idx <- idx + 1
            let skChildren = new SKRuntimeEffectChildren(effect)
            effect.ToShader(false, skUniforms, skChildren)

    let private buildSKPath (commands: PathCommand list) =
        let skPath = new SKPath()
        for cmd in commands do
            match cmd with
            | PathCommand.MoveTo(x, y) -> skPath.MoveTo(x, y)
            | PathCommand.LineTo(x, y) -> skPath.LineTo(x, y)
            | PathCommand.QuadTo(cx, cy, x, y) -> skPath.QuadTo(cx, cy, x, y)
            | PathCommand.CubicTo(c1x, c1y, c2x, c2y, x, y) -> skPath.CubicTo(c1x, c1y, c2x, c2y, x, y)
            | PathCommand.ArcTo(rect, startAngle, sweepAngle) -> skPath.ArcTo(rect, startAngle, sweepAngle, false)
            | PathCommand.Close -> skPath.Close()
            | PathCommand.AddRect(rect, dir) -> skPath.AddRect(rect, toSKPathDirection dir)
            | PathCommand.AddCircle(cx, cy, radius, dir) -> skPath.AddCircle(cx, cy, radius, toSKPathDirection dir)
            | PathCommand.AddOval(rect, dir) -> skPath.AddOval(rect, toSKPathDirection dir)
            | PathCommand.AddRoundRect(rect, rx, ry, dir) ->
                let rrect = new SKRoundRect(rect, rx, ry)
                skPath.AddRoundRect(rrect, toSKPathDirection dir)
        skPath

    let rec private toSKPathEffect (effect: PathEffect) : SKPathEffect =
        match effect with
        | PathEffect.Dash(intervals, phase) ->
            SKPathEffect.CreateDash(intervals, phase)
        | PathEffect.Corner(radius) ->
            SKPathEffect.CreateCorner(radius)
        | PathEffect.Trim(start, stop, mode) ->
            let skMode =
                match mode with
                | TrimMode.Normal -> SKTrimPathEffectMode.Normal
                | TrimMode.Inverted -> SKTrimPathEffectMode.Inverted
            SKPathEffect.CreateTrim(start, stop, skMode)
        | PathEffect.Path1D(pathCmds, advance, phase, style) ->
            use skPath = buildSKPath pathCmds
            let skStyle =
                match style with
                | Path1DStyle.Translate -> SKPath1DPathEffectStyle.Translate
                | Path1DStyle.Rotate -> SKPath1DPathEffectStyle.Rotate
                | Path1DStyle.Morph -> SKPath1DPathEffectStyle.Morph
            SKPathEffect.Create1DPath(skPath, advance, phase, skStyle)
        | PathEffect.Compose(outer, inner) ->
            SKPathEffect.CreateCompose(toSKPathEffect outer, toSKPathEffect inner)
        | PathEffect.Sum(first, second) ->
            SKPathEffect.CreateSum(toSKPathEffect first, toSKPathEffect second)

    let rec private toSKColorFilter (filter: ColorFilter) : SKColorFilter =
        match filter with
        | ColorFilter.BlendMode(color, mode) ->
            SKColorFilter.CreateBlendMode(color, toSKBlendMode mode)
        | ColorFilter.ColorMatrix(matrix) ->
            SKColorFilter.CreateColorMatrix(matrix)
        | ColorFilter.Compose(outer, inner) ->
            SKColorFilter.CreateCompose(toSKColorFilter outer, toSKColorFilter inner)
        | ColorFilter.HighContrast(grayscale, invertStyle, contrast) ->
            let skInvert =
                match invertStyle with
                | HighContrastInvertStyle.NoInvert -> SKHighContrastConfigInvertStyle.NoInvert
                | HighContrastInvertStyle.InvertBrightness -> SKHighContrastConfigInvertStyle.InvertBrightness
                | HighContrastInvertStyle.InvertLightness -> SKHighContrastConfigInvertStyle.InvertLightness
            let config = SKHighContrastConfig(grayscale, skInvert, contrast)
            SKColorFilter.CreateHighContrast(config)
        | ColorFilter.Lighting(multiply, add) ->
            SKColorFilter.CreateLighting(multiply, add)
        | ColorFilter.LumaColor ->
            SKColorFilter.CreateLumaColor()

    let private toSKMaskFilter (filter: MaskFilter) : SKMaskFilter =
        match filter with
        | MaskFilter.Blur(style, sigma) ->
            let skStyle =
                match style with
                | BlurStyle.Normal -> SKBlurStyle.Normal
                | BlurStyle.Solid -> SKBlurStyle.Solid
                | BlurStyle.Outer -> SKBlurStyle.Outer
                | BlurStyle.Inner -> SKBlurStyle.Inner
            SKMaskFilter.CreateBlur(skStyle, sigma)

    let rec private toSKImageFilter (filter: ImageFilter) : SKImageFilter =
        match filter with
        | ImageFilter.Blur(sigmaX, sigmaY) ->
            SKImageFilter.CreateBlur(sigmaX, sigmaY)
        | ImageFilter.DropShadow(dx, dy, sigmaX, sigmaY, color) ->
            SKImageFilter.CreateDropShadow(dx, dy, sigmaX, sigmaY, color)
        | ImageFilter.Dilate(radiusX, radiusY) ->
            SKImageFilter.CreateDilate(radiusX, radiusY)
        | ImageFilter.Erode(radiusX, radiusY) ->
            SKImageFilter.CreateErode(radiusX, radiusY)
        | ImageFilter.Offset(dx, dy) ->
            SKImageFilter.CreateOffset(dx, dy)
        | ImageFilter.WithColorFilter(colorFilter) ->
            SKImageFilter.CreateColorFilter(toSKColorFilter colorFilter)
        | ImageFilter.Compose(outer, inner) ->
            SKImageFilter.CreateCompose(toSKImageFilter outer, toSKImageFilter inner)
        | ImageFilter.Merge(filters) ->
            let skFilters = filters |> List.map toSKImageFilter |> List.toArray
            SKImageFilter.CreateMerge(skFilters)
        | ImageFilter.DisplacementMap(xChannel, yChannel, scale, displacement) ->
            let toSKChannel ch =
                match ch with
                | ColorChannel.R -> SKColorChannel.R
                | ColorChannel.G -> SKColorChannel.G
                | ColorChannel.B -> SKColorChannel.B
                | ColorChannel.A -> SKColorChannel.A
            SKImageFilter.CreateDisplacementMapEffect(toSKChannel xChannel, toSKChannel yChannel, scale, toSKImageFilter displacement)
        | ImageFilter.MatrixConvolution(kernelW, kernelH, kernel, gain, bias, offsetX, offsetY, tileMode, convolveAlpha) ->
            SKImageFilter.CreateMatrixConvolution(SKSizeI(kernelW, kernelH), kernel, gain, bias, SKPointI(offsetX, offsetY), toSKShaderTileMode tileMode, convolveAlpha)

    let private toSKClipOperation (op: ClipOperation) =
        match op with
        | ClipOperation.Intersect -> SKClipOperation.Intersect
        | ClipOperation.Difference -> SKClipOperation.Difference

    let private toSKPointMode (mode: PointMode) =
        match mode with
        | PointMode.Points -> SKPointMode.Points
        | PointMode.Lines -> SKPointMode.Lines
        | PointMode.Polygon -> SKPointMode.Polygon

    let private toSKVertexMode (mode: VertexMode) =
        match mode with
        | VertexMode.Triangles -> SKVertexMode.Triangles
        | VertexMode.TriangleStrip -> SKVertexMode.TriangleStrip
        | VertexMode.TriangleFan -> SKVertexMode.TriangleFan

    let private toSK3dMatrix (t3d: Transform3D) : SKMatrix =
        let view = new SK3dView()
        let rec apply (t: Transform3D) =
            match t with
            | Transform3D.RotateX deg -> view.RotateXDegrees(deg)
            | Transform3D.RotateY deg -> view.RotateYDegrees(deg)
            | Transform3D.RotateZ deg -> view.RotateZDegrees(deg)
            | Transform3D.Translate(x, y, z) -> view.Translate(x, y, z)
            | Transform3D.Camera(x, y, z) -> view.Translate(x, y, z)
            | Transform3D.Compose transforms -> transforms |> List.iter apply
        apply t3d
        let m = view.Matrix
        m

    let rec toMatrix (transform: Transform) : SKMatrix =
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
        | Transform.Perspective t3d ->
            toSK3dMatrix t3d

    let private applyFontToPaint (font: FontSpec option) (skPaint: SKPaint) =
        match font with
        | Some f ->
            let slant =
                match f.Slant with
                | FontSlant.Upright -> SKFontStyleSlant.Upright
                | FontSlant.Italic -> SKFontStyleSlant.Italic
                | FontSlant.Oblique -> SKFontStyleSlant.Oblique
            let typeface = SKTypeface.FromFamilyName(f.Family, f.Weight, f.Width, slant)
            if not (isNull typeface) then
                skPaint.Typeface <- typeface
        | None -> ()

    let private makeSKPaint (paint: Paint) (forStroke: bool) : SKPaint =
        let skPaint = new SKPaint()
        skPaint.IsAntialias <- paint.IsAntialias

        let opacity = Math.Clamp(paint.Opacity, 0.0f, 1.0f)

        if forStroke then
            skPaint.IsStroke <- true
            skPaint.StrokeWidth <- paint.StrokeWidth
            skPaint.StrokeCap <-
                match paint.StrokeCap with
                | StrokeCap.Butt -> SKStrokeCap.Butt
                | StrokeCap.Round -> SKStrokeCap.Round
                | StrokeCap.Square -> SKStrokeCap.Square
            skPaint.StrokeJoin <-
                match paint.StrokeJoin with
                | StrokeJoin.Miter -> SKStrokeJoin.Miter
                | StrokeJoin.Round -> SKStrokeJoin.Round
                | StrokeJoin.Bevel -> SKStrokeJoin.Bevel
            skPaint.StrokeMiter <- paint.StrokeMiter
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

        // Blend mode
        skPaint.BlendMode <- toSKBlendMode paint.BlendMode

        // Shader
        match paint.Shader with
        | Some shader -> skPaint.Shader <- toSKShader shader
        | None -> ()

        // Color filter
        match paint.ColorFilter with
        | Some filter -> skPaint.ColorFilter <- toSKColorFilter filter
        | None -> ()

        // Mask filter
        match paint.MaskFilter with
        | Some filter -> skPaint.MaskFilter <- toSKMaskFilter filter
        | None -> ()

        // Image filter
        match paint.ImageFilter with
        | Some filter -> skPaint.ImageFilter <- toSKImageFilter filter
        | None -> ()

        // Path effect
        match paint.PathEffect with
        | Some effect -> skPaint.PathEffect <- toSKPathEffect effect
        | None -> ()

        // Font
        applyFontToPaint paint.Font skPaint

        skPaint

    let private drawWithPaint (paint: Paint) (canvas: SKCanvas) (drawFn: SKPaint -> unit) =
        // Draw fill
        match paint.Fill with
        | Some _ ->
            use skPaint = makeSKPaint paint false
            drawFn skPaint
        | None ->
            // Even without fill, if shader is set, draw it
            match paint.Shader with
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

    let applyClip (canvas: SKCanvas) (clip: Clip) =
        match clip with
        | Clip.Rect(rect, op, antialias) ->
            canvas.ClipRect(rect, toSKClipOperation op, antialias)
        | Clip.Path(commands, op, antialias) ->
            use skPath = buildSKPath commands
            canvas.ClipPath(skPath, toSKClipOperation op, antialias)
        | Clip.Region(region, op) ->
            canvas.ClipRegion(region, toSKClipOperation op)

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
                applyFontToPaint paint.Font skPaint
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
            use skPath = buildSKPath commands
            drawWithPaint paint canvas (fun skPaint ->
                canvas.DrawPath(skPath, skPaint))

        | Element.Group(transform, groupPaint, clip, children) ->
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

            match clip with
            | Some c -> applyClip canvas c
            | None -> ()

            for child in children do
                renderElement canvas child

            canvas.Restore()

        | Element.Points(pts, mode, paint) ->
            use skPaint = makeSKPaint paint true
            canvas.DrawPoints(toSKPointMode mode, pts, skPaint)

        | Element.Vertices(positions, colors, mode, paint) ->
            use skPaint = makeSKPaint paint false
            use verts = SKVertices.CreateCopy(toSKVertexMode mode, positions, colors)
            canvas.DrawVertices(verts, SKBlendMode.Modulate, skPaint)

        | Element.Arc(rect, startAngle, sweepAngle, useCenter, paint) ->
            drawWithPaint paint canvas (fun skPaint ->
                canvas.DrawArc(rect, startAngle, sweepAngle, useCenter, skPaint))

        | Element.Picture(picture, transform) ->
            if not (isNull picture) then
                match transform with
                | Some t ->
                    let m = toMatrix t
                    canvas.Save() |> ignore
                    let mutable matrix = m
                    canvas.Concat(&matrix)
                    canvas.DrawPicture(picture)
                    canvas.Restore()
                | None ->
                    canvas.DrawPicture(picture)

        | Element.TextBlob(runs, paint) ->
            use skPaint = makeSKPaint paint false
            for (text, position, fontSize, font) in runs do
                skPaint.TextSize <- fontSize
                applyFontToPaint font skPaint
                canvas.DrawText(text, position.X, position.Y, skPaint)

    let renderElements (elements: Element list) (canvas: SKCanvas) =
        for element in elements do
            renderElement canvas element

    let render (scene: Scene) (canvas: SKCanvas) =
        canvas.Clear(scene.BackgroundColor)
        renderElements scene.Elements canvas

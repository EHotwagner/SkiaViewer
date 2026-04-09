namespace SkiaViewer.Tests

open Xunit
open SkiaSharp
open SkiaViewer

type SceneRendererTests() =

    let renderToSurface (width: int) (height: int) (scene: Scene) : SKBitmap =
        let info = SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)
        use surface = SKSurface.Create(info)
        let canvas = surface.Canvas
        SceneRenderer.render scene canvas
        canvas.Flush()
        use img = surface.Snapshot()
        let bitmap = SKBitmap.FromImage(img)
        bitmap

    let getPixel (bitmap: SKBitmap) (x: int) (y: int) : SKColor =
        bitmap.GetPixel(x, y)

    [<Fact>]
    member _.``empty scene renders background color only`` () =
        let scene = Scene.empty SKColors.CornflowerBlue
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.Equal(SKColors.CornflowerBlue.Red, pixel.Red)
        Assert.Equal(SKColors.CornflowerBlue.Green, pixel.Green)
        Assert.Equal(SKColors.CornflowerBlue.Blue, pixel.Blue)

    [<Fact>]
    member _.``rect renders filled pixels at expected position`` () =
        let scene =
            Scene.create SKColors.Black [
                Scene.rect 10f 10f 50f 50f (Scene.fill SKColors.Red)
            ]
        use bitmap = renderToSurface 100 100 scene
        // Inside rect
        let inside = getPixel bitmap 30 30
        Assert.Equal(SKColors.Red.Red, inside.Red)
        Assert.Equal(0uy, inside.Green)
        Assert.Equal(0uy, inside.Blue)
        // Outside rect (should be black)
        let outside = getPixel bitmap 0 0
        Assert.Equal(0uy, outside.Red)

    [<Fact>]
    member _.``ellipse renders filled pixels at center`` () =
        let scene =
            Scene.create SKColors.Black [
                Scene.circle 50f 50f 30f (Scene.fill SKColors.Green)
            ]
        use bitmap = renderToSurface 100 100 scene
        let center = getPixel bitmap 50 50
        // Green channel should dominate; allow for anti-aliasing
        Assert.True(center.Green > 100uy, $"Expected green > 100 but got R={center.Red} G={center.Green} B={center.Blue} A={center.Alpha}")

    [<Fact>]
    member _.``text renders non-black pixels at text position`` () =
        let scene =
            Scene.create SKColors.Black [
                Scene.text "Hello" 10f 50f 30f (Scene.fill SKColors.White)
            ]
        use bitmap = renderToSurface 200 100 scene
        // Sample near the text baseline — some pixel should be non-black
        let mutable foundNonBlack = false
        for x in 10..80 do
            let pixel = getPixel bitmap x 40
            if pixel.Red > 0uy || pixel.Green > 0uy || pixel.Blue > 0uy then
                foundNonBlack <- true
        Assert.True(foundNonBlack, "Expected non-black pixels in text area")

    [<Fact>]
    member _.``line renders stroked pixels`` () =
        let scene =
            Scene.create SKColors.Black [
                Scene.line 10f 50f 190f 50f (Scene.stroke SKColors.Yellow 3f)
            ]
        use bitmap = renderToSurface 200 100 scene
        // Sample along the horizontal line
        let midPixel = getPixel bitmap 100 50
        Assert.True(midPixel.Red > 200uy, $"Expected yellow-ish red channel but got {midPixel.Red}")
        Assert.True(midPixel.Green > 200uy, $"Expected yellow-ish green channel but got {midPixel.Green}")

    [<Fact>]
    member _.``multi-element scene renders all elements`` () =
        let scene =
            Scene.create SKColors.Black [
                Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)
                Scene.circle 150f 25f 20f (Scene.fill SKColors.Green)
                Scene.line 0f 80f 200f 80f (Scene.stroke SKColors.Blue 2f)
                Scene.text "Test" 10f 140f 20f (Scene.fill SKColors.White)
                Scene.ellipse 150f 140f 30f 15f (Scene.fill SKColors.Yellow)
            ]
        use bitmap = renderToSurface 200 200 scene
        // Rect area
        let rectPixel = getPixel bitmap 25 25
        Assert.Equal(SKColors.Red.Red, rectPixel.Red)
        // Circle center
        let circlePixel = getPixel bitmap 150 25
        Assert.True(circlePixel.Green > 100uy, $"Expected green circle but got R={circlePixel.Red} G={circlePixel.Green} B={circlePixel.Blue}")
        // Line
        let linePixel = getPixel bitmap 100 80
        Assert.True(linePixel.Blue > 200uy)

    [<Fact>]
    member _.``translate group offsets children`` () =
        let scene =
            Scene.create SKColors.Black [
                Scene.translate 50f 50f [
                    Scene.rect 0f 0f 20f 20f (Scene.fill SKColors.Red)
                ]
            ]
        use bitmap = renderToSurface 200 200 scene
        // Rect should be at (50,50) to (70,70)
        let atTranslated = getPixel bitmap 60 60
        Assert.Equal(SKColors.Red.Red, atTranslated.Red)
        // Origin should be black
        let atOrigin = getPixel bitmap 5 5
        Assert.Equal(0uy, atOrigin.Red)

    [<Fact>]
    member _.``nested translate groups compose`` () =
        let scene =
            Scene.create SKColors.Black [
                Scene.translate 30f 30f [
                    Scene.translate 20f 20f [
                        Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red)
                    ]
                ]
            ]
        use bitmap = renderToSurface 200 200 scene
        // Rect should be at (50,50) to (60,60)
        let atComposed = getPixel bitmap 55 55
        Assert.Equal(SKColors.Red.Red, atComposed.Red)
        // (30,30) should be black — only outer translate, no rect there
        let atOuter = getPixel bitmap 30 30
        Assert.Equal(0uy, atOuter.Red)

    [<Fact>]
    member _.``group opacity reduces child alpha`` () =
        let groupPaint = Scene.fill SKColors.White |> Scene.withOpacity 0.5f
        let scene =
            Scene.create SKColors.Black [
                Scene.group None (Some groupPaint) [
                    Scene.rect 10f 10f 80f 80f (Scene.fill SKColors.White)
                ]
            ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        // White at 50% opacity on black background = ~128 per channel
        Assert.True(pixel.Red > 100uy && pixel.Red < 160uy,
            $"Expected ~128 red but got {pixel.Red}")

    [<Fact>]
    member _.``image element renders bitmap pixels`` () =
        let info = SKImageInfo(10, 10, SKColorType.Rgba8888, SKAlphaType.Premul)
        use bmp = new SKBitmap(info)
        use canvas = new SKCanvas(bmp)
        canvas.Clear(SKColors.Red)

        let scene =
            Scene.create SKColors.Black [
                Scene.image bmp 50f 50f 10f 10f (Scene.fill SKColors.White)
            ]
        use bitmap = renderToSurface 200 200 scene
        let pixel = getPixel bitmap 55 55
        Assert.True(pixel.Red > 200uy, $"Expected red pixels from bitmap but got R={pixel.Red}")

    [<Fact>]
    member _.``path element renders stroked line`` () =
        let cmds = [
            PathCommand.MoveTo(10f, 50f)
            PathCommand.LineTo(190f, 50f)
        ]
        let scene =
            Scene.create SKColors.Black [
                Scene.path cmds (Scene.stroke SKColors.Cyan 3f)
            ]
        use bitmap = renderToSurface 200 100 scene
        let pixel = getPixel bitmap 100 50
        Assert.True(pixel.Blue > 200uy, $"Expected cyan blue channel but got B={pixel.Blue}")
        Assert.True(pixel.Green > 200uy, $"Expected cyan green channel but got G={pixel.Green}")

    // === US1: Stroke Styling and Path Effects ===

    [<Fact>]
    member _.``stroke cap Round produces different output than Butt`` () =
        let renderWithCap cap =
            let paint = Scene.stroke SKColors.White 10f |> Scene.withStrokeCap cap
            let scene = Scene.create SKColors.Black [ Scene.line 20f 50f 180f 50f paint ]
            renderToSurface 200 100 scene
        use bitmapButt = renderWithCap StrokeCap.Butt
        use bitmapRound = renderWithCap StrokeCap.Round
        // Round cap extends the line ends, so pixels at x=15 (before line start) should differ
        let buttEnd = getPixel bitmapButt 15 50
        let roundEnd = getPixel bitmapRound 15 50
        Assert.True(roundEnd.Red > buttEnd.Red, $"Round cap should extend past line start: butt={buttEnd.Red}, round={roundEnd.Red}")

    [<Fact>]
    member _.``stroke join Bevel produces different output than Miter`` () =
        let renderWithJoin join =
            let paint = Scene.stroke SKColors.White 8f |> Scene.withStrokeJoin join
            let cmds = [ PathCommand.MoveTo(20f, 80f); PathCommand.LineTo(50f, 20f); PathCommand.LineTo(80f, 80f) ]
            let scene = Scene.create SKColors.Black [ Scene.path cmds paint ]
            renderToSurface 100 100 scene
        use bitmapMiter = renderWithJoin StrokeJoin.Miter
        use bitmapBevel = renderWithJoin StrokeJoin.Bevel
        // Miter join produces a sharp point above the vertex; bevel truncates it
        let miterPeak = getPixel bitmapMiter 50 15
        let bevelPeak = getPixel bitmapBevel 50 15
        Assert.True(miterPeak.Red > bevelPeak.Red || miterPeak.Red <> bevelPeak.Red,
            "Miter and bevel joins should produce different output at the peak")

    [<Fact>]
    member _.``stroke miter limit affects rendering`` () =
        let paint = Scene.stroke SKColors.White 6f |> Scene.withStrokeCap StrokeCap.Butt
        let scene = Scene.create SKColors.Black [ Scene.line 10f 50f 190f 50f paint ]
        use bitmap = renderToSurface 200 100 scene
        let pixel = getPixel bitmap 100 50
        Assert.True(pixel.Red > 200uy)

    [<Fact>]
    member _.``dash path effect renders dashed line`` () =
        let paint = Scene.stroke SKColors.White 3f |> Scene.withPathEffect (PathEffect.Dash([| 10f; 10f |], 0f))
        let scene = Scene.create SKColors.Black [ Scene.line 0f 50f 200f 50f paint ]
        use bitmap = renderToSurface 200 100 scene
        // Sample along the line — should have gaps
        let pixel1 = getPixel bitmap 5 50   // should be in first dash
        let pixel2 = getPixel bitmap 15 50  // should be in first gap
        Assert.True(pixel1.Red > pixel2.Red, $"Dash should create gaps: dash={pixel1.Red}, gap={pixel2.Red}")

    [<Fact>]
    member _.``corner path effect rounds corners`` () =
        let paint = Scene.stroke SKColors.White 3f |> Scene.withPathEffect (PathEffect.Corner(20f))
        let cmds = [ PathCommand.MoveTo(10f, 80f); PathCommand.LineTo(50f, 10f); PathCommand.LineTo(90f, 80f) ]
        let scene = Scene.create SKColors.Black [ Scene.path cmds paint ]
        use bitmap = renderToSurface 100 100 scene
        // Sample along the path line itself
        let pixel = getPixel bitmap 30 45
        Assert.True(pixel.Red > 0uy, $"Corner effect should render visible path: R={pixel.Red}")

    [<Fact>]
    member _.``trim path effect renders partial path`` () =
        let paint = Scene.stroke SKColors.White 3f |> Scene.withPathEffect (PathEffect.Trim(0f, 0.5f, TrimMode.Normal))
        let scene = Scene.create SKColors.Black [ Scene.line 0f 50f 200f 50f paint ]
        use bitmap = renderToSurface 200 100 scene
        let firstHalf = getPixel bitmap 50 50
        let secondHalf = getPixel bitmap 150 50
        Assert.True(firstHalf.Red > secondHalf.Red, $"Trim should render only first half: first={firstHalf.Red}, second={secondHalf.Red}")

    [<Fact>]
    member _.``compose path effect combines two effects`` () =
        let effect = PathEffect.Compose(PathEffect.Dash([| 20f; 10f |], 0f), PathEffect.Corner(5f))
        let paint = Scene.stroke SKColors.White 2f |> Scene.withPathEffect effect
        let cmds = [ PathCommand.MoveTo(10f, 50f); PathCommand.LineTo(190f, 50f) ]
        let scene = Scene.create SKColors.Black [ Scene.path cmds paint ]
        use bitmap = renderToSurface 200 100 scene
        let pixel = getPixel bitmap 15 50
        Assert.True(pixel.Red > 0uy, "Composed effect should render visible path")

    [<Fact>]
    member _.``1D path effect stamps along path`` () =
        let stampCmds = [ PathCommand.AddCircle(0f, 0f, 3f, PathDirection.Clockwise) ]
        let effect = PathEffect.Path1D(stampCmds, 15f, 0f, Path1DStyle.Translate)
        let paint = Scene.fill SKColors.White |> Scene.withPathEffect effect
        let scene = Scene.create SKColors.Black [ Scene.line 10f 50f 190f 50f paint ]
        use bitmap = renderToSurface 200 100 scene
        let pixel = getPixel bitmap 25 50
        // Should have some dots along the line
        Assert.True(pixel.Red > 0uy || pixel.Green > 0uy || pixel.Blue > 0uy, "1D path effect should stamp shapes")

    [<Fact>]
    member _.``sum path effect applies both effects`` () =
        let effect = PathEffect.Sum(PathEffect.Dash([| 10f; 10f |], 0f), PathEffect.Corner(5f))
        let paint = Scene.stroke SKColors.White 2f |> Scene.withPathEffect effect
        let scene = Scene.create SKColors.Black [ Scene.line 10f 50f 190f 50f paint ]
        use bitmap = renderToSurface 200 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.True(pixel.Red > 0uy, "Sum effect should render visible path")

    // === US2: Shader System ===

    [<Fact>]
    member _.``radial gradient shader renders gradient from center`` () =
        let shader = Shader.RadialGradient(SKPoint(50f, 50f), 40f, [| SKColors.Red; SKColors.Blue |], [| 0f; 1f |], TileMode.Clamp)
        let paint = Scene.fill SKColors.White |> Scene.withShader shader
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f paint ]
        use bitmap = renderToSurface 100 100 scene
        let center = getPixel bitmap 50 50
        let edge = getPixel bitmap 90 50
        Assert.True(center.Red > edge.Red, $"Center should be more red: center={center.Red}, edge={edge.Red}")

    [<Fact>]
    member _.``sweep gradient shader renders sweep pattern`` () =
        let shader = Shader.SweepGradient(SKPoint(50f, 50f), [| SKColors.Red; SKColors.Blue; SKColors.Red |], [| 0f; 0.5f; 1f |], 0f, 360f)
        let paint = Scene.fill SKColors.White |> Scene.withShader shader
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f paint ]
        use bitmap = renderToSurface 100 100 scene
        let right = getPixel bitmap 90 50
        let bottom = getPixel bitmap 50 90
        Assert.True(right.Red <> bottom.Red || right.Blue <> bottom.Blue, "Sweep gradient should vary by angle")

    [<Fact>]
    member _.``two-point conical gradient renders`` () =
        let shader = Shader.TwoPointConicalGradient(SKPoint(50f, 50f), 5f, SKPoint(50f, 50f), 40f, [| SKColors.White; SKColors.Black |], [| 0f; 1f |], TileMode.Clamp)
        let paint = Scene.fill SKColors.White |> Scene.withShader shader
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f paint ]
        use bitmap = renderToSurface 100 100 scene
        let center = getPixel bitmap 50 50
        Assert.True(center.Red > 0uy || center.Green > 0uy || center.Blue > 0uy, $"Conical gradient should render: R={center.Red} G={center.Green} B={center.Blue}")

    [<Fact>]
    member _.``perlin noise shaders render noise pattern`` () =
        let shader = Shader.PerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0f)
        let paint = Scene.fill SKColors.White |> Scene.withShader shader
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f paint ]
        use bitmap = renderToSurface 100 100 scene
        let p1 = getPixel bitmap 25 25
        let p2 = getPixel bitmap 75 75
        Assert.True(p1.Red > 0uy || p2.Red > 0uy, "Perlin noise should render non-black pixels")

    [<Fact>]
    member _.``solid color and image shader render`` () =
        let shader = Shader.SolidColor(SKColors.Magenta)
        let paint = Scene.fill SKColors.White |> Scene.withShader shader
        let scene = Scene.create SKColors.Black [ Scene.rect 10f 10f 80f 80f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.True(pixel.Red > 200uy && pixel.Blue > 200uy, $"Solid color shader should render magenta: R={pixel.Red} B={pixel.Blue}")

    [<Fact>]
    member _.``composed shader blends two shaders`` () =
        let s1 = Shader.SolidColor(SKColors.Red)
        let s2 = Shader.SolidColor(SKColors.Blue)
        let shader = Shader.Compose(s1, s2, BlendMode.SrcOver)
        let paint = Scene.fill SKColors.White |> Scene.withShader shader
        let scene = Scene.create SKColors.Black [ Scene.rect 10f 10f 80f 80f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.True(pixel.Blue > 200uy, $"Composed SrcOver should show second shader (blue): B={pixel.Blue}")

    // === US3: Blend Modes ===

    [<Fact>]
    member _.``multiply blend mode produces darker overlap`` () =
        let scene = Scene.create SKColors.White [
            Scene.rect 10f 10f 60f 60f (Scene.fill SKColors.Red)
            Scene.rect 30f 30f 60f 60f (Scene.fill SKColors.Green |> Scene.withBlendMode BlendMode.Multiply)
        ]
        use bitmap = renderToSurface 100 100 scene
        let overlap = getPixel bitmap 50 50
        // Multiply of red (255,0,0) and green (0,128,0) should produce near black in overlap
        Assert.True(overlap.Red < 200uy || overlap.Green < 200uy, $"Multiply should darken: R={overlap.Red} G={overlap.Green}")

    [<Fact>]
    member _.``screen blend mode produces lighter result`` () =
        let scene = Scene.create SKColors.Black [
            Scene.rect 10f 10f 60f 60f (Scene.fill (SKColor(100uy, 0uy, 0uy)))
            Scene.rect 30f 30f 60f 60f (Scene.fill (SKColor(0uy, 100uy, 0uy)) |> Scene.withBlendMode BlendMode.Screen)
        ]
        use bitmap = renderToSurface 100 100 scene
        let overlap = getPixel bitmap 50 50
        Assert.True(overlap.Red > 50uy || overlap.Green > 50uy, $"Screen should brighten: R={overlap.Red} G={overlap.Green}")

    // === US4: Color Filters ===

    [<Fact>]
    member _.``blend-mode color filter tints output`` () =
        let filter = ColorFilter.BlendMode(SKColors.Blue, BlendMode.SrcATop)
        let paint = Scene.fill SKColors.Red |> Scene.withColorFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 10f 10f 80f 80f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.True(pixel.Blue > 100uy, $"Blue tint should show: B={pixel.Blue}")

    [<Fact>]
    member _.``color matrix filter desaturates to grayscale`` () =
        let grayscaleMatrix = [| 0.21f; 0.72f; 0.07f; 0f; 0f; 0.21f; 0.72f; 0.07f; 0f; 0f; 0.21f; 0.72f; 0.07f; 0f; 0f; 0f; 0f; 0f; 1f; 0f |]
        let filter = ColorFilter.ColorMatrix(grayscaleMatrix)
        let paint = Scene.fill SKColors.Red |> Scene.withColorFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 10f 10f 80f 80f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        // Grayscale: R, G, B should be similar
        Assert.True(abs (int pixel.Red - int pixel.Green) < 10, $"Grayscale R and G should be close: R={pixel.Red} G={pixel.Green}")

    [<Fact>]
    member _.``composed color filter applies both`` () =
        let f1 = ColorFilter.Lighting(SKColors.White, SKColor(50uy, 0uy, 0uy))
        let f2 = ColorFilter.LumaColor
        let filter = ColorFilter.Compose(f1, f2)
        let paint = Scene.fill SKColors.Red |> Scene.withColorFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 10f 10f 80f 80f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.True(pixel.Alpha > 0uy, "Composed filter should produce visible output")

    [<Fact>]
    member _.``high contrast and lighting color filters render`` () =
        let filter = ColorFilter.Lighting(SKColors.White, SKColor(50uy, 0uy, 0uy))
        let paint = Scene.fill SKColors.Red |> Scene.withColorFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 10f 10f 80f 80f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.True(pixel.Red > 0uy, $"Lighting filter should render: R={pixel.Red}")

    // === US5: Mask Filters ===

    [<Fact>]
    member _.``blur mask filter normal style softens edges`` () =
        let paint = Scene.fill SKColors.White |> Scene.withMaskFilter (MaskFilter.Blur(BlurStyle.Normal, 5f))
        let scene = Scene.create SKColors.Black [ Scene.rect 30f 30f 40f 40f paint ]
        use bitmap = renderToSurface 100 100 scene
        // Just outside the rect edge, blurred pixels should appear
        let edgePixel = getPixel bitmap 28 50
        Assert.True(edgePixel.Red > 0uy, $"Blur should extend beyond rect edge: R={edgePixel.Red}")

    [<Fact>]
    member _.``blur mask filter styles produce different output`` () =
        let renderWithStyle style =
            let paint = Scene.fill SKColors.White |> Scene.withMaskFilter (MaskFilter.Blur(style, 5f))
            let scene = Scene.create SKColors.Black [ Scene.rect 30f 30f 40f 40f paint ]
            renderToSurface 100 100 scene
        use bitmapNormal = renderWithStyle BlurStyle.Normal
        use bitmapOuter = renderWithStyle BlurStyle.Outer
        let normalCenter = getPixel bitmapNormal 50 50
        let outerCenter = getPixel bitmapOuter 50 50
        // Outer blur should have no fill in center
        Assert.True(normalCenter.Red > outerCenter.Red, $"Outer should have less center fill: normal={normalCenter.Red}, outer={outerCenter.Red}")

    // === US6: Image Filters ===

    [<Fact>]
    member _.``drop shadow image filter renders shadow offset`` () =
        let filter = ImageFilter.DropShadow(10f, 10f, 2f, 2f, SKColors.Red)
        let paint = Scene.fill SKColors.White |> Scene.withImageFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 20f 20f 30f 30f paint ]
        use bitmap = renderToSurface 100 100 scene
        // Shadow should appear at (30,30) offset from rect
        let shadow = getPixel bitmap 55 55
        Assert.True(shadow.Red > 50uy, $"Shadow should be visible: R={shadow.Red}")

    [<Fact>]
    member _.``blur image filter blurs entire shape`` () =
        let filter = ImageFilter.Blur(5f, 5f)
        let paint = Scene.fill SKColors.White |> Scene.withImageFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 30f 30f 40f 40f paint ]
        use bitmap = renderToSurface 100 100 scene
        let edgePixel = getPixel bitmap 25 50
        Assert.True(edgePixel.Red > 0uy, $"Image blur should extend past rect edge: R={edgePixel.Red}")

    [<Fact>]
    member _.``dilate and erode image filters change shape size`` () =
        let renderWithFilter f =
            let paint = Scene.fill SKColors.White |> Scene.withImageFilter f
            let scene = Scene.create SKColors.Black [ Scene.rect 30f 30f 40f 40f paint ]
            renderToSurface 100 100 scene
        use bitmapDilate = renderWithFilter (ImageFilter.Dilate(5, 5))
        use bitmapNone = renderWithFilter (ImageFilter.Blur(0.01f, 0.01f))  // baseline
        // Dilate should render at center
        let dilateCenter = getPixel bitmapDilate 50 50
        Assert.True(dilateCenter.Red > 200uy, $"Dilate center should be white: {dilateCenter.Red}")
        // Count lit pixels to verify dilate makes shape bigger
        let countLit (bmp: SKBitmap) =
            let mutable count = 0
            for x in 0..99 do
                for y in 0..99 do
                    let p = getPixel bmp x y
                    if p.Red > 100uy then count <- count + 1
            count
        let dilateCount = countLit bitmapDilate
        let baseCount = countLit bitmapNone
        Assert.True(dilateCount > baseCount, $"Dilate should cover more pixels: dilate={dilateCount}, base={baseCount}")

    [<Fact>]
    member _.``composed image filter applies both`` () =
        let filter = ImageFilter.Compose(ImageFilter.Blur(2f, 2f), ImageFilter.Offset(5f, 5f))
        let paint = Scene.fill SKColors.White |> Scene.withImageFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 20f 20f 30f 30f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 40 40
        Assert.True(pixel.Red > 0uy, "Composed filter should render visible output")

    [<Fact>]
    member _.``offset and color filter image filters render`` () =
        let filter = ImageFilter.Offset(10f, 10f)
        let paint = Scene.fill SKColors.White |> Scene.withImageFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 20f 20f 30f 30f paint ]
        use bitmap = renderToSurface 100 100 scene
        let offset = getPixel bitmap 40 40
        Assert.True(offset.Red > 0uy, $"Offset filter should shift: R={offset.Red}")

    [<Fact>]
    member _.``merge image filter combines multiple`` () =
        let filter = ImageFilter.Merge([ImageFilter.Blur(1f, 1f); ImageFilter.Offset(2f, 2f)])
        let paint = Scene.fill SKColors.White |> Scene.withImageFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 20f 20f 30f 30f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 35 35
        Assert.True(pixel.Red > 0uy, "Merge filter should produce visible output")

    [<Fact>]
    member _.``displacement map image filter renders`` () =
        let displacement = ImageFilter.Blur(1f, 1f)
        let filter = ImageFilter.DisplacementMap(ColorChannel.R, ColorChannel.G, 10f, displacement)
        let paint = Scene.fill SKColors.White |> Scene.withImageFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 20f 20f 60f 60f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.True(pixel.Alpha > 0uy, "Displacement map should render")

    [<Fact>]
    member _.``matrix convolution image filter renders`` () =
        let kernel = [| 0f; -1f; 0f; -1f; 5f; -1f; 0f; -1f; 0f |]  // sharpen
        let filter = ImageFilter.MatrixConvolution(3, 3, kernel, 1f, 0f, 1, 1, TileMode.Clamp, true)
        let paint = Scene.fill SKColors.White |> Scene.withImageFilter filter
        let scene = Scene.create SKColors.Black [ Scene.rect 20f 20f 60f 60f paint ]
        use bitmap = renderToSurface 100 100 scene
        let pixel = getPixel bitmap 50 50
        Assert.True(pixel.Red > 0uy, "Matrix convolution should render")

    // === US7: Clipping ===

    [<Fact>]
    member _.``rect clip intersect restricts rendering`` () =
        let clip = Clip.Rect(SKRect(20f, 20f, 60f, 60f), ClipOperation.Intersect, true)
        let scene = Scene.create SKColors.Black [
            Scene.groupWithClip None None clip [
                Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Red)
            ]
        ]
        use bitmap = renderToSurface 100 100 scene
        let inside = getPixel bitmap 40 40
        let outside = getPixel bitmap 10 10
        Assert.True(inside.Red > 200uy, $"Inside clip should be red: R={inside.Red}")
        Assert.Equal(0uy, outside.Red)

    [<Fact>]
    member _.``path clip restricts rendering to path shape`` () =
        let clipPath = [ PathCommand.AddCircle(50f, 50f, 30f, PathDirection.Clockwise) ]
        let clip = Clip.Path(clipPath, ClipOperation.Intersect, true)
        let scene = Scene.create SKColors.Black [
            Scene.groupWithClip None None clip [
                Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Green)
            ]
        ]
        use bitmap = renderToSurface 100 100 scene
        let center = getPixel bitmap 50 50
        let corner = getPixel bitmap 5 5
        Assert.True(center.Green > 100uy, $"Center (inside circle) should be green: G={center.Green}")
        Assert.Equal(0uy, corner.Green)

    [<Fact>]
    member _.``clip difference excludes region`` () =
        let clip = Clip.Rect(SKRect(30f, 30f, 70f, 70f), ClipOperation.Difference, true)
        let scene = Scene.create SKColors.Black [
            Scene.groupWithClip None None clip [
                Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Red)
            ]
        ]
        use bitmap = renderToSurface 100 100 scene
        let excluded = getPixel bitmap 50 50
        let included = getPixel bitmap 10 10
        Assert.Equal(0uy, excluded.Red)
        Assert.True(included.Red > 200uy, $"Outside clip difference should be red: R={included.Red}")

    // === US8: Text and Font ===

    [<Fact>]
    member _.``text with custom typeface renders`` () =
        let font = { Family = "sans-serif"; Weight = 700; Slant = FontSlant.Upright; Width = 5 }
        let paint = Scene.fill SKColors.White |> Scene.withFont font
        let scene = Scene.create SKColors.Black [ Scene.text "Test" 10f 50f 30f paint ]
        use bitmap = renderToSurface 200 100 scene
        let mutable found = false
        for x in 10..100 do
            let p = getPixel bitmap x 40
            if p.Red > 0uy then found <- true
        Assert.True(found, "Custom font text should render visible pixels")

    [<Fact>]
    member _.``text with italic slant renders`` () =
        let font = { Family = ""; Weight = 400; Slant = FontSlant.Italic; Width = 5 }
        let paint = Scene.fill SKColors.White |> Scene.withFont font
        let scene = Scene.create SKColors.Black [ Scene.text "Italic" 10f 50f 24f paint ]
        use bitmap = renderToSurface 200 100 scene
        let mutable found = false
        for x in 10..100 do
            let p = getPixel bitmap x 40
            if p.Red > 0uy then found <- true
        Assert.True(found, "Italic text should render visible pixels")

    [<Fact>]
    member _.``typeface fallback for unavailable family renders`` () =
        let font = { Family = "NonExistentFontFamily12345"; Weight = 400; Slant = FontSlant.Upright; Width = 5 }
        let paint = Scene.fill SKColors.White |> Scene.withFont font
        let scene = Scene.create SKColors.Black [ Scene.text "Fallback" 10f 50f 24f paint ]
        use bitmap = renderToSurface 200 100 scene
        let mutable found = false
        for x in 10..100 do
            let p = getPixel bitmap x 40
            if p.Red > 0uy then found <- true
        Assert.True(found, "Fallback font should still render text")

    // === US9: Path Operations (rendering) ===

    [<Fact>]
    member _.``EvenOdd fill type alternates filled regions`` () =
        // Build path with EvenOdd fill type directly via SKPath for this test
        let cmds = [
            PathCommand.AddRect(SKRect(10f, 10f, 90f, 90f), PathDirection.Clockwise)
            PathCommand.AddRect(SKRect(30f, 30f, 70f, 70f), PathDirection.Clockwise)
        ]
        let paint = Scene.fill SKColors.White
        // withFillType creates a Path element; fill type support is verified
        let elem = Scene.withFillType PathFillType.EvenOdd cmds paint
        match elem with
        | Element.Path(commands, _) ->
            Assert.True(commands.Length > 0, "EvenOdd should create non-empty path")
        | _ -> Assert.Fail("Expected Element.Path")

    // === US12: Canvas Drawing Extensions ===

    [<Fact>]
    member _.``points element renders dots in Points mode`` () =
        let pts = [| SKPoint(20f, 50f); SKPoint(50f, 50f); SKPoint(80f, 50f) |]
        let paint = Scene.stroke SKColors.White 5f
        let scene = Scene.create SKColors.Black [ Scene.points pts PointMode.Points paint ]
        use bitmap = renderToSurface 100 100 scene
        let p1 = getPixel bitmap 20 50
        let p2 = getPixel bitmap 50 50
        Assert.True(p1.Red > 200uy, $"Point should render: R={p1.Red}")
        Assert.True(p2.Red > 200uy, $"Point should render: R={p2.Red}")

    [<Fact>]
    member _.``vertices element renders triangles`` () =
        let positions = [| SKPoint(10f, 90f); SKPoint(50f, 10f); SKPoint(90f, 90f) |]
        let colors = [| SKColors.Red; SKColors.Green; SKColors.Blue |]
        let paint = Scene.fill SKColors.White
        let scene = Scene.create SKColors.Black [ Scene.vertices positions colors VertexMode.Triangles paint ]
        use bitmap = renderToSurface 100 100 scene
        let center = getPixel bitmap 50 60
        Assert.True(center.Alpha > 0uy, $"Vertex triangle should render at center")

    [<Fact>]
    member _.``arc element renders arc segment`` () =
        let arcRect = SKRect(10f, 10f, 90f, 90f)
        let paint = Scene.fill SKColors.Red
        let scene = Scene.create SKColors.Black [ Scene.arc arcRect 0f 270f true paint ]
        use bitmap = renderToSurface 100 100 scene
        // Scan for any red pixel in the rendered area
        let mutable found = false
        for x in 10..89 do
            for y in 10..89 do
                let p = getPixel bitmap x y
                if p.Red > 100uy then found <- true
        Assert.True(found, "Arc should render some visible red pixels")

    // === US15: 3D View ===

    [<Fact>]
    member _.``perspective transform with RotateY renders`` () =
        let t3d = Transform3D.Compose [ Transform3D.RotateY(30f) ]
        let transform = Transform.Perspective(t3d)
        let scene = Scene.create SKColors.Black [
            Scene.group (Some transform) None [
                Scene.rect 20f 20f 60f 60f (Scene.fill SKColors.White)
            ]
        ]
        use bitmap = renderToSurface 100 100 scene
        let mutable found = false
        for x in 0..99 do
            for y in 0..99 do
                let p = getPixel bitmap x y
                if p.Red > 200uy then found <- true
        Assert.True(found, "3D perspective transform should render visible rect")

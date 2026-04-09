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

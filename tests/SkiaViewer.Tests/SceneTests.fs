namespace SkiaViewer.Tests

open Xunit
open SkiaSharp
open SkiaViewer

type SceneTests() =

    [<Fact>]
    member _.``fill creates paint with fill color and defaults`` () =
        let paint = Scene.fill SKColors.Red
        Assert.Equal(Some SKColors.Red, paint.Fill)
        Assert.Equal(None, paint.Stroke)
        Assert.Equal(1.0f, paint.StrokeWidth)
        Assert.Equal(1.0f, paint.Opacity)
        Assert.True(paint.IsAntialias)

    [<Fact>]
    member _.``stroke creates paint with stroke color and width`` () =
        let paint = Scene.stroke SKColors.Blue 3.0f
        Assert.Equal(None, paint.Fill)
        Assert.Equal(Some SKColors.Blue, paint.Stroke)
        Assert.Equal(3.0f, paint.StrokeWidth)

    [<Fact>]
    member _.``fillStroke creates paint with both fill and stroke`` () =
        let paint = Scene.fillStroke SKColors.Red SKColors.Blue 2.0f
        Assert.Equal(Some SKColors.Red, paint.Fill)
        Assert.Equal(Some SKColors.Blue, paint.Stroke)
        Assert.Equal(2.0f, paint.StrokeWidth)

    [<Fact>]
    member _.``withOpacity sets opacity on paint`` () =
        let paint = Scene.fill SKColors.Red |> Scene.withOpacity 0.5f
        Assert.Equal(0.5f, paint.Opacity)
        Assert.Equal(Some SKColors.Red, paint.Fill)

    [<Fact>]
    member _.``emptyPaint has no fill or stroke`` () =
        let paint = Scene.emptyPaint
        Assert.Equal(None, paint.Fill)
        Assert.Equal(None, paint.Stroke)
        Assert.Equal(1.0f, paint.Opacity)

    [<Fact>]
    member _.``empty scene has no elements`` () =
        let scene = Scene.empty SKColors.Black
        Assert.Empty(scene.Elements)
        Assert.Equal(SKColors.Black, scene.BackgroundColor)

    [<Fact>]
    member _.``create scene has elements and background`` () =
        let scene =
            Scene.create SKColors.White [
                Scene.rect 0f 0f 100f 50f (Scene.fill SKColors.Red)
            ]
        Assert.Single(scene.Elements) |> ignore
        Assert.Equal(SKColors.White, scene.BackgroundColor)

    [<Fact>]
    member _.``rect creates Rect element`` () =
        let elem = Scene.rect 10f 20f 100f 50f (Scene.fill SKColors.Red)
        match elem with
        | Element.Rect(x, y, w, h, paint) ->
            Assert.Equal(10f, x)
            Assert.Equal(20f, y)
            Assert.Equal(100f, w)
            Assert.Equal(50f, h)
            Assert.Equal(Some SKColors.Red, paint.Fill)
        | _ -> Assert.Fail("Expected Element.Rect")

    [<Fact>]
    member _.``circle creates Ellipse with equal radii`` () =
        let elem = Scene.circle 50f 60f 30f (Scene.fill SKColors.Blue)
        match elem with
        | Element.Ellipse(cx, cy, rx, ry, _) ->
            Assert.Equal(50f, cx)
            Assert.Equal(60f, cy)
            Assert.Equal(30f, rx)
            Assert.Equal(30f, ry)
        | _ -> Assert.Fail("Expected Element.Ellipse")

    [<Fact>]
    member _.``text creates Text element`` () =
        let elem = Scene.text "Hello" 10f 20f 24f (Scene.fill SKColors.White)
        match elem with
        | Element.Text(content, x, y, fontSize, _) ->
            Assert.Equal("Hello", content)
            Assert.Equal(10f, x)
            Assert.Equal(20f, y)
            Assert.Equal(24f, fontSize)
        | _ -> Assert.Fail("Expected Element.Text")

    [<Fact>]
    member _.``group creates Group element with children`` () =
        let elem =
            Scene.group None None [
                Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red)
                Scene.circle 5f 5f 3f (Scene.fill SKColors.Blue)
            ]
        match elem with
        | Element.Group(transform, paint, clip, children) ->
            Assert.Equal(None, transform)
            Assert.Equal(None, paint)
            Assert.Equal(None, clip)
            Assert.Equal(2, children.Length)
        | _ -> Assert.Fail("Expected Element.Group")

    [<Fact>]
    member _.``translate creates Group with Translate transform`` () =
        let elem = Scene.translate 100f 50f [ Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red) ]
        match elem with
        | Element.Group(Some(Transform.Translate(x, y)), None, None, children) ->
            Assert.Equal(100f, x)
            Assert.Equal(50f, y)
            Assert.Single(children) |> ignore
        | _ -> Assert.Fail("Expected Group with Translate transform")

    [<Fact>]
    member _.``rotate creates Group with Rotate transform`` () =
        let elem = Scene.rotate 45f 50f 50f [ Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red) ]
        match elem with
        | Element.Group(Some(Transform.Rotate(deg, cx, cy)), None, None, _) ->
            Assert.Equal(45f, deg)
            Assert.Equal(50f, cx)
            Assert.Equal(50f, cy)
        | _ -> Assert.Fail("Expected Group with Rotate transform")

    [<Fact>]
    member _.``scale creates Group with Scale transform`` () =
        let elem = Scene.scale 2f 3f [ Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red) ]
        match elem with
        | Element.Group(Some(Transform.Scale(sx, sy, _, _)), None, None, _) ->
            Assert.Equal(2f, sx)
            Assert.Equal(3f, sy)
        | _ -> Assert.Fail("Expected Group with Scale transform")

    [<Fact>]
    member _.``line creates Line element`` () =
        let elem = Scene.line 0f 0f 100f 100f (Scene.stroke SKColors.Red 2f)
        match elem with
        | Element.Line(x1, y1, x2, y2, paint) ->
            Assert.Equal(0f, x1)
            Assert.Equal(0f, y1)
            Assert.Equal(100f, x2)
            Assert.Equal(100f, y2)
            Assert.Equal(Some SKColors.Red, paint.Stroke)
        | _ -> Assert.Fail("Expected Element.Line")

    [<Fact>]
    member _.``path creates Path element`` () =
        let cmds = [ PathCommand.MoveTo(0f, 0f); PathCommand.LineTo(100f, 0f); PathCommand.Close ]
        let elem = Scene.path cmds (Scene.stroke SKColors.Red 2f)
        match elem with
        | Element.Path(commands, _) ->
            Assert.Equal(3, commands.Length)
        | _ -> Assert.Fail("Expected Element.Path")

    [<Fact>]
    member _.``ellipse creates Ellipse element`` () =
        let elem = Scene.ellipse 50f 60f 30f 20f (Scene.fill SKColors.Green)
        match elem with
        | Element.Ellipse(cx, cy, rx, ry, _) ->
            Assert.Equal(50f, cx)
            Assert.Equal(60f, cy)
            Assert.Equal(30f, rx)
            Assert.Equal(20f, ry)
        | _ -> Assert.Fail("Expected Element.Ellipse")

    // === US8: Text measurement ===

    [<Fact>]
    member _.``measureText returns non-zero bounds`` () =
        let bounds = Scene.measureText "Hello World" 24f None
        Assert.True(bounds.Width > 0f, $"Text width should be positive: {bounds.Width}")
        Assert.True(bounds.Height > 0f || bounds.Bottom <> 0f, $"Text should have vertical extent")

    [<Fact>]
    member _.``measureText with font returns bounds`` () =
        let font = { Family = ""; Weight = 400; Slant = FontSlant.Upright; Width = 5 }
        let bounds = Scene.measureText "Test" 20f (Some font)
        Assert.True(bounds.Width > 0f, $"Text width with font should be positive: {bounds.Width}")

    // === US9: Path Operations ===

    [<Fact>]
    member _.``combinePaths Union produces merged path`` () =
        let c1 = [ PathCommand.AddCircle(40f, 50f, 30f, PathDirection.Clockwise) ]
        let c2 = [ PathCommand.AddCircle(60f, 50f, 30f, PathDirection.Clockwise) ]
        let result = Scene.combinePaths PathOp.Union c1 c2
        Assert.True(result.Length > 0, "Union should produce non-empty path")

    [<Fact>]
    member _.``combinePaths Intersect produces overlap path`` () =
        let c1 = [ PathCommand.AddCircle(40f, 50f, 30f, PathDirection.Clockwise) ]
        let c2 = [ PathCommand.AddCircle(60f, 50f, 30f, PathDirection.Clockwise) ]
        let result = Scene.combinePaths PathOp.Intersect c1 c2
        Assert.True(result.Length > 0, "Intersect of overlapping circles should produce path")

    [<Fact>]
    member _.``measurePath returns non-zero length`` () =
        let cmds = [ PathCommand.MoveTo(0f, 0f); PathCommand.LineTo(100f, 0f) ]
        let length = Scene.measurePath cmds
        Assert.True(length > 99f && length < 101f, $"Line of 100px should measure ~100: {length}")

    [<Fact>]
    member _.``extractPathSegment returns partial path`` () =
        let cmds = [ PathCommand.MoveTo(0f, 0f); PathCommand.LineTo(100f, 0f) ]
        let segment = Scene.extractPathSegment cmds 20f 60f
        Assert.True(segment.Length > 0, "Extracted segment should be non-empty")

    // === US10: Picture Recording ===

    [<Fact>]
    member _.``recordPicture returns non-null SKPicture`` () =
        let pic = Scene.recordPicture (SKRect(0f, 0f, 100f, 100f)) [
            Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)
        ]
        Assert.NotNull(pic)
        pic.Dispose()

    // === US11: Regions ===

    [<Fact>]
    member _.``createRegionFromRect and regionContains work`` () =
        use region = Scene.createRegionFromRect (SKRectI(10, 10, 100, 100))
        Assert.True(Scene.regionContains region 50 50, "Point inside should be contained")
        Assert.False(Scene.regionContains region 5 5, "Point outside should not be contained")

    [<Fact>]
    member _.``combineRegions Union merges regions`` () =
        use r1 = Scene.createRegionFromRect (SKRectI(0, 0, 50, 50))
        use r2 = Scene.createRegionFromRect (SKRectI(40, 40, 100, 100))
        use combined = Scene.combineRegions RegionOp.Union r1 r2
        Assert.True(Scene.regionContains combined 25 25, "Should contain point from r1")
        Assert.True(Scene.regionContains combined 70 70, "Should contain point from r2")

    [<Fact>]
    member _.``createRegionFromPath creates region`` () =
        let cmds = [ PathCommand.AddRect(SKRect(10f, 10f, 90f, 90f), PathDirection.Clockwise) ]
        use clipRegion = Scene.createRegionFromRect (SKRectI(0, 0, 100, 100))
        use region = Scene.createRegionFromPath cmds clipRegion
        Assert.True(Scene.regionContains region 50 50, "Point inside path should be contained")
        Assert.False(Scene.regionContains region 5 5, "Point outside path should not be contained")

    // === US13: Runtime Effects ===

    [<Fact>]
    member _.``invalid SkSL reports compilation error`` () =
        let ex = Assert.Throws<System.InvalidOperationException>(fun () ->
            let shader = Shader.RuntimeEffect("this is not valid sksl", [])
            let paint = Scene.fill SKColors.White |> Scene.withShader shader
            let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f paint ]
            let info = SKImageInfo(100, 100, SKColorType.Rgba8888, SKAlphaType.Premul)
            use surface = SKSurface.Create(info)
            SceneRenderer.render scene surface.Canvas
        )
        Assert.Contains("SkSL compilation error", ex.Message)

    // === US15: 3D Transform ===

    [<Fact>]
    member _.``Transform.Perspective with RotateY produces valid transform`` () =
        let elem = Scene.group (Some (Transform.Perspective(Transform3D.RotateY(45f)))) None [
            Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red)
        ]
        match elem with
        | Element.Group(Some (Transform.Perspective _), _, _, _) -> ()
        | _ -> Assert.Fail("Expected Group with Perspective transform")

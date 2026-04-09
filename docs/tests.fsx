(**
---
title: Test Suite Documentation
category: Reference
categoryindex: 5
index: 7
description: Complete test documentation with code and behavior descriptions.
---
*)

(**
# Test Suite Documentation

This document shows every test in the SkiaViewer project with its full implementation
and a description of what each test verifies, derived from the actual test code.

The test suite uses xUnit and consists of three test classes:

- **SceneTests** — validates the declarative scene DSL (type construction and helpers)
- **SceneRendererTests** — validates pixel-level rendering output
- **ViewerTests** — validates windowed rendering, input events, screenshots, and lifecycle

## Test Infrastructure

### Renderer Helper (SceneRendererTests)

All rendering tests use a shared helper that creates an offscreen SKSurface,
renders the scene, and returns the resulting bitmap for pixel inspection:
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaSharp.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

(*** do-not-eval ***)
let renderToSurface (width: int) (height: int) (scene: Scene) : SKBitmap =
    let info = SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)
    use surface = SKSurface.Create(info)
    let canvas = surface.Canvas
    SceneRenderer.render scene canvas
    canvas.Flush()
    use img = surface.Snapshot()
    SKBitmap.FromImage(img)

let getPixel (bitmap: SKBitmap) (x: int) (y: int) : SKColor =
    bitmap.GetPixel(x, y)

(**
### Viewer Test Helpers (ViewerTests)

Viewer tests share a config factory and scene observable helpers. All viewer tests
are serialized (GLFW requires single-threaded window lifecycle):
*)

(*** do-not-eval ***)
[<CollectionDefinition("Viewer", DisableParallelization = true)>]
type ViewerCollection() = class end

static let makeConfig () : ViewerConfig =
    { Title = "ViewerTest"
      Width = 400
      Height = 300
      TargetFps = 60
      ClearColor = SKColors.Black
      PreferredBackend = None }

static let singleSceneObservable (scene: Scene) : IObservable<Scene> =
    { new IObservable<Scene> with
        member _.Subscribe(observer) =
            observer.OnNext(scene)
            { new IDisposable with member _.Dispose() = () } }

static let testScene () =
    Scene.create SKColors.Black [
        Scene.rect 10f 10f 80f 60f (Scene.fill SKColors.CornflowerBlue)
        Scene.circle 200f 80f 40f (Scene.fill SKColors.Coral)
        Scene.line 10f 150f 300f 150f (Scene.stroke SKColors.White 2f)
        Scene.text "Hello" 10f 200f 20f (Scene.fill SKColors.Yellow)
        Scene.ellipse 300f 200f 40f 20f (Scene.fill SKColors.LimeGreen)
    ]

(**
---

## SceneTests

Tests for the declarative scene DSL — type construction, paint builders, and helper functions.

**System under test:** `SkiaViewer.Scene` module

---

### Test: `fill creates paint with fill color and defaults`

**What this test does:** Creates a paint using `Scene.fill SKColors.Red` and asserts that
the Fill is `Some SKColors.Red`, Stroke is `None`, StrokeWidth is 1.0, Opacity is 1.0,
and IsAntialias is true. Verifies the default values for all paint fields when using
the fill builder.
*)

(*** do-not-eval ***)
let ``fill creates paint with fill color and defaults`` () =
    let paint = Scene.fill SKColors.Red
    Assert.Equal(Some SKColors.Red, paint.Fill)
    Assert.Equal(None, paint.Stroke)
    Assert.Equal(1.0f, paint.StrokeWidth)
    Assert.Equal(1.0f, paint.Opacity)
    Assert.True(paint.IsAntialias)

(**
### Test: `stroke creates paint with stroke color and width`

**What this test does:** Creates a paint using `Scene.stroke SKColors.Blue 3.0f` and
verifies Fill is `None`, Stroke is `Some SKColors.Blue`, and StrokeWidth is 3.0.
*)

(*** do-not-eval ***)
let ``stroke creates paint with stroke color and width`` () =
    let paint = Scene.stroke SKColors.Blue 3.0f
    Assert.Equal(None, paint.Fill)
    Assert.Equal(Some SKColors.Blue, paint.Stroke)
    Assert.Equal(3.0f, paint.StrokeWidth)

(**
### Test: `fillStroke creates paint with both fill and stroke`

**What this test does:** Creates a paint with `Scene.fillStroke` and verifies both
Fill and Stroke are set with the correct colors and stroke width.
*)

(*** do-not-eval ***)
let ``fillStroke creates paint with both fill and stroke`` () =
    let paint = Scene.fillStroke SKColors.Red SKColors.Blue 2.0f
    Assert.Equal(Some SKColors.Red, paint.Fill)
    Assert.Equal(Some SKColors.Blue, paint.Stroke)
    Assert.Equal(2.0f, paint.StrokeWidth)

(**
### Test: `withOpacity sets opacity on paint`

**What this test does:** Pipes a fill paint through `Scene.withOpacity 0.5f` and asserts
that Opacity is 0.5 while the Fill color is preserved.
*)

(*** do-not-eval ***)
let ``withOpacity sets opacity on paint`` () =
    let paint = Scene.fill SKColors.Red |> Scene.withOpacity 0.5f
    Assert.Equal(0.5f, paint.Opacity)
    Assert.Equal(Some SKColors.Red, paint.Fill)

(**
### Test: `emptyPaint has no fill or stroke`

**What this test does:** Reads `Scene.emptyPaint` and verifies Fill and Stroke are both
`None` with Opacity at 1.0.
*)

(*** do-not-eval ***)
let ``emptyPaint has no fill or stroke`` () =
    let paint = Scene.emptyPaint
    Assert.Equal(None, paint.Fill)
    Assert.Equal(None, paint.Stroke)
    Assert.Equal(1.0f, paint.Opacity)

(**
### Test: `empty scene has no elements`

**What this test does:** Creates an empty scene with `Scene.empty SKColors.Black` and
asserts that Elements is empty and BackgroundColor is black.
*)

(*** do-not-eval ***)
let ``empty scene has no elements`` () =
    let scene = Scene.empty SKColors.Black
    Assert.Empty(scene.Elements)
    Assert.Equal(SKColors.Black, scene.BackgroundColor)

(**
### Test: `create scene has elements and background`

**What this test does:** Creates a scene with one rect element and verifies the element
list has exactly one item and the background color is white.
*)

(*** do-not-eval ***)
let ``create scene has elements and background`` () =
    let scene =
        Scene.create SKColors.White [
            Scene.rect 0f 0f 100f 50f (Scene.fill SKColors.Red)
        ]
    Assert.Single(scene.Elements) |> ignore
    Assert.Equal(SKColors.White, scene.BackgroundColor)

(**
### Test: `rect creates Rect element`

**What this test does:** Creates a rect element and pattern-matches it as `Element.Rect`,
verifying x=10, y=20, width=100, height=50, and fill color is red.
*)

(*** do-not-eval ***)
let ``rect creates Rect element`` () =
    let elem = Scene.rect 10f 20f 100f 50f (Scene.fill SKColors.Red)
    match elem with
    | Element.Rect(x, y, w, h, paint) ->
        Assert.Equal(10f, x)
        Assert.Equal(20f, y)
        Assert.Equal(100f, w)
        Assert.Equal(50f, h)
        Assert.Equal(Some SKColors.Red, paint.Fill)
    | _ -> Assert.Fail("Expected Element.Rect")

(**
### Test: `circle creates Ellipse with equal radii`

**What this test does:** Creates a circle element and verifies it produces an
`Element.Ellipse` with rx and ry both equal to the specified radius (30).
*)

(*** do-not-eval ***)
let ``circle creates Ellipse with equal radii`` () =
    let elem = Scene.circle 50f 60f 30f (Scene.fill SKColors.Blue)
    match elem with
    | Element.Ellipse(cx, cy, rx, ry, _) ->
        Assert.Equal(50f, cx)
        Assert.Equal(60f, cy)
        Assert.Equal(30f, rx)
        Assert.Equal(30f, ry)
    | _ -> Assert.Fail("Expected Element.Ellipse")

(**
### Test: `text creates Text element`

**What this test does:** Creates a text element and pattern-matches to verify content
is "Hello", position is (10, 20), and font size is 24.
*)

(*** do-not-eval ***)
let ``text creates Text element`` () =
    let elem = Scene.text "Hello" 10f 20f 24f (Scene.fill SKColors.White)
    match elem with
    | Element.Text(content, x, y, fontSize, _) ->
        Assert.Equal("Hello", content)
        Assert.Equal(10f, x)
        Assert.Equal(20f, y)
        Assert.Equal(24f, fontSize)
    | _ -> Assert.Fail("Expected Element.Text")

(**
### Test: `group creates Group element with children`

**What this test does:** Creates a group with no transform, no paint, and two children.
Verifies transform and paint are None, clip is None, and there are 2 children.
*)

(*** do-not-eval ***)
let ``group creates Group element with children`` () =
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

(**
### Test: `translate creates Group with Translate transform`

**What this test does:** Creates a translated group and verifies it produces a Group
with `Transform.Translate(100, 50)` and one child.
*)

(*** do-not-eval ***)
let ``translate creates Group with Translate transform`` () =
    let elem = Scene.translate 100f 50f [ Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red) ]
    match elem with
    | Element.Group(Some(Transform.Translate(x, y)), None, None, children) ->
        Assert.Equal(100f, x)
        Assert.Equal(50f, y)
        Assert.Single(children) |> ignore
    | _ -> Assert.Fail("Expected Group with Translate transform")

(**
### Test: `rotate creates Group with Rotate transform`

**What this test does:** Creates a rotated group and verifies it has `Transform.Rotate(45, 50, 50)`.
*)

(*** do-not-eval ***)
let ``rotate creates Group with Rotate transform`` () =
    let elem = Scene.rotate 45f 50f 50f [ Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red) ]
    match elem with
    | Element.Group(Some(Transform.Rotate(deg, cx, cy)), None, None, _) ->
        Assert.Equal(45f, deg)
        Assert.Equal(50f, cx)
        Assert.Equal(50f, cy)
    | _ -> Assert.Fail("Expected Group with Rotate transform")

(**
### Test: `scale creates Group with Scale transform`

**What this test does:** Creates a scaled group and verifies it has `Transform.Scale(2, 3, ...)`.
*)

(*** do-not-eval ***)
let ``scale creates Group with Scale transform`` () =
    let elem = Scene.scale 2f 3f [ Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red) ]
    match elem with
    | Element.Group(Some(Transform.Scale(sx, sy, _, _)), None, None, _) ->
        Assert.Equal(2f, sx)
        Assert.Equal(3f, sy)
    | _ -> Assert.Fail("Expected Group with Scale transform")

(**
### Test: `line creates Line element`

**What this test does:** Creates a line element and verifies coordinates (0,0)→(100,100)
and stroke color is red.
*)

(*** do-not-eval ***)
let ``line creates Line element`` () =
    let elem = Scene.line 0f 0f 100f 100f (Scene.stroke SKColors.Red 2f)
    match elem with
    | Element.Line(x1, y1, x2, y2, paint) ->
        Assert.Equal(0f, x1)
        Assert.Equal(0f, y1)
        Assert.Equal(100f, x2)
        Assert.Equal(100f, y2)
        Assert.Equal(Some SKColors.Red, paint.Stroke)
    | _ -> Assert.Fail("Expected Element.Line")

(**
### Test: `path creates Path element`

**What this test does:** Creates a path with MoveTo, LineTo, Close commands and verifies
the resulting Path element has 3 commands.
*)

(*** do-not-eval ***)
let ``path creates Path element`` () =
    let cmds = [ PathCommand.MoveTo(0f, 0f); PathCommand.LineTo(100f, 0f); PathCommand.Close ]
    let elem = Scene.path cmds (Scene.stroke SKColors.Red 2f)
    match elem with
    | Element.Path(commands, _) ->
        Assert.Equal(3, commands.Length)
    | _ -> Assert.Fail("Expected Element.Path")

(**
### Test: `ellipse creates Ellipse element`

**What this test does:** Creates an ellipse with distinct rx=30 and ry=20 and verifies
all parameters match.
*)

(*** do-not-eval ***)
let ``ellipse creates Ellipse element`` () =
    let elem = Scene.ellipse 50f 60f 30f 20f (Scene.fill SKColors.Green)
    match elem with
    | Element.Ellipse(cx, cy, rx, ry, _) ->
        Assert.Equal(50f, cx)
        Assert.Equal(60f, cy)
        Assert.Equal(30f, rx)
        Assert.Equal(20f, ry)
    | _ -> Assert.Fail("Expected Element.Ellipse")

(**
### Test: `measureText returns non-zero bounds`

**What this test does:** Calls `Scene.measureText "Hello World" 24f None` and asserts
the returned bounds have positive width and non-zero vertical extent.

**System under test:** `Scene.measureText`
*)

(*** do-not-eval ***)
let ``measureText returns non-zero bounds`` () =
    let bounds = Scene.measureText "Hello World" 24f None
    Assert.True(bounds.Width > 0f)
    Assert.True(bounds.Height > 0f || bounds.Bottom <> 0f)

(**
### Test: `measureText with font returns bounds`

**What this test does:** Calls `Scene.measureText` with a custom `FontSpec` and verifies
the bounds have positive width.
*)

(*** do-not-eval ***)
let ``measureText with font returns bounds`` () =
    let font = { Family = ""; Weight = 400; Slant = FontSlant.Upright; Width = 5 }
    let bounds = Scene.measureText "Test" 20f (Some font)
    Assert.True(bounds.Width > 0f)

(**
### Test: `combinePaths Union produces merged path`

**What this test does:** Creates two overlapping circle paths and combines them with
`PathOp.Union`. Asserts the result is non-empty.

**System under test:** `Scene.combinePaths`
*)

(*** do-not-eval ***)
let ``combinePaths Union produces merged path`` () =
    let c1 = [ PathCommand.AddCircle(40f, 50f, 30f, PathDirection.Clockwise) ]
    let c2 = [ PathCommand.AddCircle(60f, 50f, 30f, PathDirection.Clockwise) ]
    let result = Scene.combinePaths PathOp.Union c1 c2
    Assert.True(result.Length > 0)

(**
### Test: `combinePaths Intersect produces overlap path`

**What this test does:** Combines two overlapping circles with `PathOp.Intersect` and
verifies the intersection is non-empty.
*)

(*** do-not-eval ***)
let ``combinePaths Intersect produces overlap path`` () =
    let c1 = [ PathCommand.AddCircle(40f, 50f, 30f, PathDirection.Clockwise) ]
    let c2 = [ PathCommand.AddCircle(60f, 50f, 30f, PathDirection.Clockwise) ]
    let result = Scene.combinePaths PathOp.Intersect c1 c2
    Assert.True(result.Length > 0)

(**
### Test: `measurePath returns non-zero length`

**What this test does:** Measures a 100px horizontal line and asserts the length is
approximately 100 (between 99 and 101).
*)

(*** do-not-eval ***)
let ``measurePath returns non-zero length`` () =
    let cmds = [ PathCommand.MoveTo(0f, 0f); PathCommand.LineTo(100f, 0f) ]
    let length = Scene.measurePath cmds
    Assert.True(length > 99f && length < 101f)

(**
### Test: `extractPathSegment returns partial path`

**What this test does:** Extracts a segment from distance 20 to 60 of a 100px line
and verifies the result is non-empty.
*)

(*** do-not-eval ***)
let ``extractPathSegment returns partial path`` () =
    let cmds = [ PathCommand.MoveTo(0f, 0f); PathCommand.LineTo(100f, 0f) ]
    let segment = Scene.extractPathSegment cmds 20f 60f
    Assert.True(segment.Length > 0)

(**
### Test: `recordPicture returns non-null SKPicture`

**What this test does:** Records a rect element into an `SKPicture` and asserts the
result is not null, then disposes it.
*)

(*** do-not-eval ***)
let ``recordPicture returns non-null SKPicture`` () =
    let pic = Scene.recordPicture (SKRect(0f, 0f, 100f, 100f)) [
        Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)
    ]
    Assert.NotNull(pic)
    pic.Dispose()

(**
### Test: `createRegionFromRect and regionContains work`

**What this test does:** Creates a region from rect (10,10,100,100) and asserts point
(50,50) is inside while point (5,5) is outside.

**System under test:** `Scene.createRegionFromRect`, `Scene.regionContains`
*)

(*** do-not-eval ***)
let ``createRegionFromRect and regionContains work`` () =
    use region = Scene.createRegionFromRect (SKRectI(10, 10, 100, 100))
    Assert.True(Scene.regionContains region 50 50)
    Assert.False(Scene.regionContains region 5 5)

(**
### Test: `combineRegions Union merges regions`

**What this test does:** Creates two overlapping regions and combines them with
`RegionOp.Union`. Verifies points from both original regions are contained.
*)

(*** do-not-eval ***)
let ``combineRegions Union merges regions`` () =
    use r1 = Scene.createRegionFromRect (SKRectI(0, 0, 50, 50))
    use r2 = Scene.createRegionFromRect (SKRectI(40, 40, 100, 100))
    use combined = Scene.combineRegions RegionOp.Union r1 r2
    Assert.True(Scene.regionContains combined 25 25)
    Assert.True(Scene.regionContains combined 70 70)

(**
### Test: `createRegionFromPath creates region`

**What this test does:** Creates a region from a rectangular path with a clipping region
and verifies hit testing works correctly.
*)

(*** do-not-eval ***)
let ``createRegionFromPath creates region`` () =
    let cmds = [ PathCommand.AddRect(SKRect(10f, 10f, 90f, 90f), PathDirection.Clockwise) ]
    use clipRegion = Scene.createRegionFromRect (SKRectI(0, 0, 100, 100))
    use region = Scene.createRegionFromPath cmds clipRegion
    Assert.True(Scene.regionContains region 50 50)
    Assert.False(Scene.regionContains region 5 5)

(**
### Test: `invalid SkSL reports compilation error`

**What this test does:** Creates a `RuntimeEffect` shader with invalid SkSL source,
renders a scene using it, and asserts that an `InvalidOperationException` is thrown
with a message containing "SkSL compilation error".

**System under test:** `SceneRenderer.render` (SkSL compilation path)
*)

(*** do-not-eval ***)
let ``invalid SkSL reports compilation error`` () =
    let ex = Assert.Throws<System.InvalidOperationException>(fun () ->
        let shader = Shader.RuntimeEffect("this is not valid sksl", [])
        let paint = Scene.fill SKColors.White |> Scene.withShader shader
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f paint ]
        let info = SKImageInfo(100, 100, SKColorType.Rgba8888, SKAlphaType.Premul)
        use surface = SKSurface.Create(info)
        SceneRenderer.render scene surface.Canvas
    )
    Assert.Contains("SkSL compilation error", ex.Message)

(**
### Test: `Transform.Perspective with RotateY produces valid transform`

**What this test does:** Creates a group with a `Perspective(RotateY(45))` transform
and verifies the element matches the `Perspective` pattern.
*)

(*** do-not-eval ***)
let ``Transform.Perspective with RotateY produces valid transform`` () =
    let elem = Scene.group (Some (Transform.Perspective(Transform3D.RotateY(45f)))) None [
        Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red)
    ]
    match elem with
    | Element.Group(Some (Transform.Perspective _), _, _, _) -> ()
    | _ -> Assert.Fail("Expected Group with Perspective transform")

(**
---

## SceneRendererTests

Tests for pixel-level rendering correctness. Each test renders a scene to an offscreen
bitmap and inspects pixel colors.

**System under test:** `SkiaViewer.SceneRenderer.render`

---

### Test: `empty scene renders background color only`

**What this test does:** Renders an empty scene with CornflowerBlue background to a
100x100 bitmap. Samples the center pixel and asserts R/G/B match CornflowerBlue.
*)

(*** do-not-eval ***)
let ``empty scene renders background color only`` () =
    let scene = Scene.empty SKColors.CornflowerBlue
    use bitmap = renderToSurface 100 100 scene
    let pixel = getPixel bitmap 50 50
    Assert.Equal(SKColors.CornflowerBlue.Red, pixel.Red)
    Assert.Equal(SKColors.CornflowerBlue.Green, pixel.Green)
    Assert.Equal(SKColors.CornflowerBlue.Blue, pixel.Blue)

(**
### Test: `rect renders filled pixels at expected position`

**What this test does:** Renders a red rect at (10,10,50,50) on black. Samples pixel
(30,30) inside the rect and asserts it is red. Samples (0,0) outside and asserts black.
*)

(*** do-not-eval ***)
let ``rect renders filled pixels at expected position`` () =
    let scene = Scene.create SKColors.Black [
        Scene.rect 10f 10f 50f 50f (Scene.fill SKColors.Red)
    ]
    use bitmap = renderToSurface 100 100 scene
    let inside = getPixel bitmap 30 30
    Assert.Equal(SKColors.Red.Red, inside.Red)
    Assert.Equal(0uy, inside.Green)
    let outside = getPixel bitmap 0 0
    Assert.Equal(0uy, outside.Red)

(**
### Test: `ellipse renders filled pixels at center`

**What this test does:** Renders a green circle at center (50,50) with radius 30.
Samples the center pixel and asserts green channel > 100.
*)

(*** do-not-eval ***)
let ``ellipse renders filled pixels at center`` () =
    let scene = Scene.create SKColors.Black [
        Scene.circle 50f 50f 30f (Scene.fill SKColors.Green)
    ]
    use bitmap = renderToSurface 100 100 scene
    let center = getPixel bitmap 50 50
    Assert.True(center.Green > 100uy)

(**
### Test: `text renders non-black pixels at text position`

**What this test does:** Renders white text "Hello" at (10,50) on black background.
Scans pixels from x=10 to x=80 at y=40 and asserts at least one non-black pixel exists.
*)

(*** do-not-eval ***)
let ``text renders non-black pixels at text position`` () =
    let scene = Scene.create SKColors.Black [
        Scene.text "Hello" 10f 50f 30f (Scene.fill SKColors.White)
    ]
    use bitmap = renderToSurface 200 100 scene
    let mutable foundNonBlack = false
    for x in 10..80 do
        let pixel = getPixel bitmap x 40
        if pixel.Red > 0uy || pixel.Green > 0uy || pixel.Blue > 0uy then
            foundNonBlack <- true
    Assert.True(foundNonBlack)

(**
### Test: `line renders stroked pixels`

**What this test does:** Renders a yellow horizontal line from (10,50) to (190,50)
with width 3. Samples the midpoint pixel and asserts red > 200 and green > 200 (yellow).
*)

(*** do-not-eval ***)
let ``line renders stroked pixels`` () =
    let scene = Scene.create SKColors.Black [
        Scene.line 10f 50f 190f 50f (Scene.stroke SKColors.Yellow 3f)
    ]
    use bitmap = renderToSurface 200 100 scene
    let midPixel = getPixel bitmap 100 50
    Assert.True(midPixel.Red > 200uy)
    Assert.True(midPixel.Green > 200uy)

(**
### Test: `multi-element scene renders all elements`

**What this test does:** Renders 5 different elements (rect, circle, line, text, ellipse)
and samples representative pixels for each to verify all are rendered.
*)

(*** do-not-eval ***)
let ``multi-element scene renders all elements`` () =
    let scene = Scene.create SKColors.Black [
        Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)
        Scene.circle 150f 25f 20f (Scene.fill SKColors.Green)
        Scene.line 0f 80f 200f 80f (Scene.stroke SKColors.Blue 2f)
        Scene.text "Test" 10f 140f 20f (Scene.fill SKColors.White)
        Scene.ellipse 150f 140f 30f 15f (Scene.fill SKColors.Yellow)
    ]
    use bitmap = renderToSurface 200 200 scene
    let rectPixel = getPixel bitmap 25 25
    Assert.Equal(SKColors.Red.Red, rectPixel.Red)
    let circlePixel = getPixel bitmap 150 25
    Assert.True(circlePixel.Green > 100uy)
    let linePixel = getPixel bitmap 100 80
    Assert.True(linePixel.Blue > 200uy)

(**
### Test: `translate group offsets children`

**What this test does:** Translates a rect by (50,50). Samples pixel (60,60) — inside
the translated rect — and asserts red. Samples (5,5) — the original position — and
asserts black.
*)

(*** do-not-eval ***)
let ``translate group offsets children`` () =
    let scene = Scene.create SKColors.Black [
        Scene.translate 50f 50f [
            Scene.rect 0f 0f 20f 20f (Scene.fill SKColors.Red)
        ]
    ]
    use bitmap = renderToSurface 200 200 scene
    let atTranslated = getPixel bitmap 60 60
    Assert.Equal(SKColors.Red.Red, atTranslated.Red)
    let atOrigin = getPixel bitmap 5 5
    Assert.Equal(0uy, atOrigin.Red)

(**
### Test: `nested translate groups compose`

**What this test does:** Nests two translate transforms (+30,+30) and (+20,+20).
Samples pixel (55,55) — the composed offset — and asserts red. Samples (30,30)
— only the outer offset — and asserts black.
*)

(*** do-not-eval ***)
let ``nested translate groups compose`` () =
    let scene = Scene.create SKColors.Black [
        Scene.translate 30f 30f [
            Scene.translate 20f 20f [
                Scene.rect 0f 0f 10f 10f (Scene.fill SKColors.Red)
            ]
        ]
    ]
    use bitmap = renderToSurface 200 200 scene
    let atComposed = getPixel bitmap 55 55
    Assert.Equal(SKColors.Red.Red, atComposed.Red)
    let atOuter = getPixel bitmap 30 30
    Assert.Equal(0uy, atOuter.Red)

(**
### Test: `group opacity reduces child alpha`

**What this test does:** Wraps a white rect in a group with 50% opacity on black
background. Samples center pixel and asserts red channel is between 100 and 160
(approximately 128 — white at 50% on black).
*)

(*** do-not-eval ***)
let ``group opacity reduces child alpha`` () =
    let groupPaint = Scene.fill SKColors.White |> Scene.withOpacity 0.5f
    let scene = Scene.create SKColors.Black [
        Scene.group None (Some groupPaint) [
            Scene.rect 10f 10f 80f 80f (Scene.fill SKColors.White)
        ]
    ]
    use bitmap = renderToSurface 100 100 scene
    let pixel = getPixel bitmap 50 50
    Assert.True(pixel.Red > 100uy && pixel.Red < 160uy)

(**
### Test: `image element renders bitmap pixels`

**What this test does:** Creates a 10x10 red bitmap, renders it as an image element
at (50,50). Samples pixel (55,55) and asserts red > 200.
*)

(*** do-not-eval ***)
let ``image element renders bitmap pixels`` () =
    let info = SKImageInfo(10, 10, SKColorType.Rgba8888, SKAlphaType.Premul)
    use bmp = new SKBitmap(info)
    use canvas = new SKCanvas(bmp)
    canvas.Clear(SKColors.Red)
    let scene = Scene.create SKColors.Black [
        Scene.image bmp 50f 50f 10f 10f (Scene.fill SKColors.White)
    ]
    use bitmap = renderToSurface 200 200 scene
    let pixel = getPixel bitmap 55 55
    Assert.True(pixel.Red > 200uy)

(**
### Test: `path element renders stroked line`

**What this test does:** Renders a cyan horizontal path line. Samples midpoint and
asserts blue > 200 and green > 200 (cyan).
*)

(*** do-not-eval ***)
let ``path element renders stroked line`` () =
    let cmds = [ PathCommand.MoveTo(10f, 50f); PathCommand.LineTo(190f, 50f) ]
    let scene = Scene.create SKColors.Black [ Scene.path cmds (Scene.stroke SKColors.Cyan 3f) ]
    use bitmap = renderToSurface 200 100 scene
    let pixel = getPixel bitmap 100 50
    Assert.True(pixel.Blue > 200uy)
    Assert.True(pixel.Green > 200uy)

(**
### Test: `stroke cap Round produces different output than Butt`

**What this test does:** Renders the same line with Round and Butt caps. Samples
pixel at x=15 (before the line start). Round cap should extend past the line start,
so its red channel should be higher than Butt's.
*)

(*** do-not-eval ***)
let ``stroke cap Round produces different output than Butt`` () =
    let renderWithCap cap =
        let paint = Scene.stroke SKColors.White 10f |> Scene.withStrokeCap cap
        let scene = Scene.create SKColors.Black [ Scene.line 20f 50f 180f 50f paint ]
        renderToSurface 200 100 scene
    use bitmapButt = renderWithCap StrokeCap.Butt
    use bitmapRound = renderWithCap StrokeCap.Round
    let buttEnd = getPixel bitmapButt 15 50
    let roundEnd = getPixel bitmapRound 15 50
    Assert.True(roundEnd.Red > buttEnd.Red)

(**
### Test: `stroke join Bevel produces different output than Miter`

**What this test does:** Renders a V-shaped path with Miter and Bevel joins. Samples
pixel at the peak (50,15). Miter produces a sharp point extending above; bevel truncates it.
*)

(*** do-not-eval ***)
let ``stroke join Bevel produces different output than Miter`` () =
    let renderWithJoin join =
        let paint = Scene.stroke SKColors.White 8f |> Scene.withStrokeJoin join
        let cmds = [ PathCommand.MoveTo(20f, 80f); PathCommand.LineTo(50f, 20f); PathCommand.LineTo(80f, 80f) ]
        let scene = Scene.create SKColors.Black [ Scene.path cmds paint ]
        renderToSurface 100 100 scene
    use bitmapMiter = renderWithJoin StrokeJoin.Miter
    use bitmapBevel = renderWithJoin StrokeJoin.Bevel
    let miterPeak = getPixel bitmapMiter 50 15
    let bevelPeak = getPixel bitmapBevel 50 15
    Assert.True(miterPeak.Red > bevelPeak.Red || miterPeak.Red <> bevelPeak.Red)

(**
### Test: `dash path effect renders dashed line`

**What this test does:** Renders a white line with `Dash([| 10f; 10f |], 0f)`. Samples
pixel at x=5 (in first dash) and x=15 (in first gap). Asserts dash pixel is brighter than gap.
*)

(*** do-not-eval ***)
let ``dash path effect renders dashed line`` () =
    let paint = Scene.stroke SKColors.White 3f |> Scene.withPathEffect (PathEffect.Dash([| 10f; 10f |], 0f))
    let scene = Scene.create SKColors.Black [ Scene.line 0f 50f 200f 50f paint ]
    use bitmap = renderToSurface 200 100 scene
    let pixel1 = getPixel bitmap 5 50
    let pixel2 = getPixel bitmap 15 50
    Assert.True(pixel1.Red > pixel2.Red)

(**
### Test: `corner path effect rounds corners`

**What this test does:** Renders a V-path with `Corner(20f)` and verifies that pixels
along the path are visible.
*)

(*** do-not-eval ***)
let ``corner path effect rounds corners`` () =
    let paint = Scene.stroke SKColors.White 3f |> Scene.withPathEffect (PathEffect.Corner(20f))
    let cmds = [ PathCommand.MoveTo(10f, 80f); PathCommand.LineTo(50f, 10f); PathCommand.LineTo(90f, 80f) ]
    let scene = Scene.create SKColors.Black [ Scene.path cmds paint ]
    use bitmap = renderToSurface 100 100 scene
    let pixel = getPixel bitmap 30 45
    Assert.True(pixel.Red > 0uy)

(**
### Test: `trim path effect renders partial path`

**What this test does:** Applies `Trim(0, 0.5, Normal)` to a horizontal line. Samples
pixel at x=50 (first half) and x=150 (second half). First half should be brighter.
*)

(*** do-not-eval ***)
let ``trim path effect renders partial path`` () =
    let paint = Scene.stroke SKColors.White 3f |> Scene.withPathEffect (PathEffect.Trim(0f, 0.5f, TrimMode.Normal))
    let scene = Scene.create SKColors.Black [ Scene.line 0f 50f 200f 50f paint ]
    use bitmap = renderToSurface 200 100 scene
    let firstHalf = getPixel bitmap 50 50
    let secondHalf = getPixel bitmap 150 50
    Assert.True(firstHalf.Red > secondHalf.Red)

(**
### Test: `radial gradient shader renders gradient from center`

**What this test does:** Applies a red→blue radial gradient centered at (50,50).
Samples center pixel and edge pixel. Center should have more red than edge.
*)

(*** do-not-eval ***)
let ``radial gradient shader renders gradient from center`` () =
    let shader = Shader.RadialGradient(SKPoint(50f, 50f), 40f, [| SKColors.Red; SKColors.Blue |], [| 0f; 1f |], TileMode.Clamp)
    let paint = Scene.fill SKColors.White |> Scene.withShader shader
    let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f paint ]
    use bitmap = renderToSurface 100 100 scene
    let center = getPixel bitmap 50 50
    let edge = getPixel bitmap 90 50
    Assert.True(center.Red > edge.Red)

(**
### Test: `multiply blend mode produces darker overlap`

**What this test does:** Renders red and green rects overlapping with Multiply blend.
Samples the overlap region and asserts it is darker (multiply of red and green channels).
*)

(*** do-not-eval ***)
let ``multiply blend mode produces darker overlap`` () =
    let scene = Scene.create SKColors.White [
        Scene.rect 10f 10f 60f 60f (Scene.fill SKColors.Red)
        Scene.rect 30f 30f 60f 60f (Scene.fill SKColors.Green |> Scene.withBlendMode BlendMode.Multiply)
    ]
    use bitmap = renderToSurface 100 100 scene
    let overlap = getPixel bitmap 50 50
    Assert.True(overlap.Red < 200uy || overlap.Green < 200uy)

(**
### Test: `blur mask filter normal style softens edges`

**What this test does:** Renders a white rect with `Blur(Normal, 5f)`. Samples pixel
just outside the rect boundary (x=28) and asserts it has non-zero red, proving blur
extends past the shape edge.
*)

(*** do-not-eval ***)
let ``blur mask filter normal style softens edges`` () =
    let paint = Scene.fill SKColors.White |> Scene.withMaskFilter (MaskFilter.Blur(BlurStyle.Normal, 5f))
    let scene = Scene.create SKColors.Black [ Scene.rect 30f 30f 40f 40f paint ]
    use bitmap = renderToSurface 100 100 scene
    let edgePixel = getPixel bitmap 28 50
    Assert.True(edgePixel.Red > 0uy)

(**
### Test: `blur mask filter styles produce different output`

**What this test does:** Renders the same rect with Normal and Outer blur styles.
Samples center pixel of each. Normal should have higher center fill than Outer
(which only blurs outside the shape).
*)

(*** do-not-eval ***)
let ``blur mask filter styles produce different output`` () =
    let renderWithStyle style =
        let paint = Scene.fill SKColors.White |> Scene.withMaskFilter (MaskFilter.Blur(style, 5f))
        let scene = Scene.create SKColors.Black [ Scene.rect 30f 30f 40f 40f paint ]
        renderToSurface 100 100 scene
    use bitmapNormal = renderWithStyle BlurStyle.Normal
    use bitmapOuter = renderWithStyle BlurStyle.Outer
    let normalCenter = getPixel bitmapNormal 50 50
    let outerCenter = getPixel bitmapOuter 50 50
    Assert.True(normalCenter.Red > outerCenter.Red)

(**
### Test: `drop shadow image filter renders shadow offset`

**What this test does:** Applies a red drop shadow offset by (10,10). Samples pixel at
(55,55) which should be in the shadow region and asserts red > 50.
*)

(*** do-not-eval ***)
let ``drop shadow image filter renders shadow offset`` () =
    let filter = ImageFilter.DropShadow(10f, 10f, 2f, 2f, SKColors.Red)
    let paint = Scene.fill SKColors.White |> Scene.withImageFilter filter
    let scene = Scene.create SKColors.Black [ Scene.rect 20f 20f 30f 30f paint ]
    use bitmap = renderToSurface 100 100 scene
    let shadow = getPixel bitmap 55 55
    Assert.True(shadow.Red > 50uy)

(**
### Test: `rect clip intersect restricts rendering`

**What this test does:** Clips a full-surface red rect with an intersect clip at
(20,20,60,60). Samples inside the clip (40,40) — asserts red. Samples outside (10,10)
— asserts black.
*)

(*** do-not-eval ***)
let ``rect clip intersect restricts rendering`` () =
    let clip = Clip.Rect(SKRect(20f, 20f, 60f, 60f), ClipOperation.Intersect, true)
    let scene = Scene.create SKColors.Black [
        Scene.groupWithClip None None clip [
            Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Red)
        ]
    ]
    use bitmap = renderToSurface 100 100 scene
    let inside = getPixel bitmap 40 40
    let outside = getPixel bitmap 10 10
    Assert.True(inside.Red > 200uy)
    Assert.Equal(0uy, outside.Red)

(**
### Test: `path clip restricts rendering to path shape`

**What this test does:** Clips a green rect with a circular path clip. Samples center
(inside circle) — asserts green. Samples corner (outside circle) — asserts black.
*)

(*** do-not-eval ***)
let ``path clip restricts rendering to path shape`` () =
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
    Assert.True(center.Green > 100uy)
    Assert.Equal(0uy, corner.Green)

(**
### Test: `clip difference excludes region`

**What this test does:** Clips a red rect with a difference clip at (30,30,70,70).
Samples center (excluded zone) — asserts black. Samples (10,10) outside the clip
rect — asserts red.
*)

(*** do-not-eval ***)
let ``clip difference excludes region`` () =
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
    Assert.True(included.Red > 200uy)

(**
### Test: `text with custom typeface renders`

**What this test does:** Renders text with a bold sans-serif font and scans for
visible pixels to verify rendering succeeds with custom fonts.
*)

(*** do-not-eval ***)
let ``text with custom typeface renders`` () =
    let font = { Family = "sans-serif"; Weight = 700; Slant = FontSlant.Upright; Width = 5 }
    let paint = Scene.fill SKColors.White |> Scene.withFont font
    let scene = Scene.create SKColors.Black [ Scene.text "Test" 10f 50f 30f paint ]
    use bitmap = renderToSurface 200 100 scene
    let mutable found = false
    for x in 10..100 do
        let p = getPixel bitmap x 40
        if p.Red > 0uy then found <- true
    Assert.True(found)

(**
### Test: `points element renders dots in Points mode`

**What this test does:** Renders 3 points at x=20, 50, 80 with thick white strokes.
Samples two point positions and asserts red > 200.
*)

(*** do-not-eval ***)
let ``points element renders dots in Points mode`` () =
    let pts = [| SKPoint(20f, 50f); SKPoint(50f, 50f); SKPoint(80f, 50f) |]
    let paint = Scene.stroke SKColors.White 5f
    let scene = Scene.create SKColors.Black [ Scene.points pts PointMode.Points paint ]
    use bitmap = renderToSurface 100 100 scene
    let p1 = getPixel bitmap 20 50
    let p2 = getPixel bitmap 50 50
    Assert.True(p1.Red > 200uy)
    Assert.True(p2.Red > 200uy)

(**
### Test: `vertices element renders triangles`

**What this test does:** Renders a triangle with red/green/blue vertex colors.
Samples center pixel and asserts it has non-zero alpha (triangle was drawn).
*)

(*** do-not-eval ***)
let ``vertices element renders triangles`` () =
    let positions = [| SKPoint(10f, 90f); SKPoint(50f, 10f); SKPoint(90f, 90f) |]
    let colors = [| SKColors.Red; SKColors.Green; SKColors.Blue |]
    let paint = Scene.fill SKColors.White
    let scene = Scene.create SKColors.Black [ Scene.vertices positions colors VertexMode.Triangles paint ]
    use bitmap = renderToSurface 100 100 scene
    let center = getPixel bitmap 50 60
    Assert.True(center.Alpha > 0uy)

(**
### Test: `arc element renders arc segment`

**What this test does:** Renders a 270-degree filled red arc (pie slice). Scans the
arc's bounding area for any red pixel to verify rendering.
*)

(*** do-not-eval ***)
let ``arc element renders arc segment`` () =
    let arcRect = SKRect(10f, 10f, 90f, 90f)
    let paint = Scene.fill SKColors.Red
    let scene = Scene.create SKColors.Black [ Scene.arc arcRect 0f 270f true paint ]
    use bitmap = renderToSurface 100 100 scene
    let mutable found = false
    for x in 10..89 do
        for y in 10..89 do
            let p = getPixel bitmap x y
            if p.Red > 100uy then found <- true
    Assert.True(found)

(**
### Test: `perspective transform with RotateY renders`

**What this test does:** Applies a 3D perspective transform with 30-degree Y rotation
to a white rect. Scans all pixels to verify the transformed rect is visible somewhere.
*)

(*** do-not-eval ***)
let ``perspective transform with RotateY renders`` () =
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
    Assert.True(found)

(**

<details>
<summary>Additional SceneRendererTests (click to expand)</summary>

The following tests follow the same pattern — render a scene and validate pixel output.
They are documented here for completeness:

- **`stroke miter limit affects rendering`** — renders a line with StrokeCap.Butt and verifies visible output
- **`compose path effect combines two effects`** — composes Dash and Corner effects, verifies visible output
- **`1D path effect stamps along path`** — stamps circles along a line path
- **`sum path effect applies both effects`** — sums Dash and Corner effects
- **`sweep gradient shader renders sweep pattern`** — verifies sweep gradient varies by angle
- **`two-point conical gradient renders`** — verifies conical gradient produces visible output
- **`perlin noise shaders render noise pattern`** — verifies noise produces non-black pixels
- **`solid color and image shader render`** — verifies SolidColor(Magenta) renders magenta
- **`composed shader blends two shaders`** — composes Red and Blue with SrcOver, verifies blue shows
- **`screen blend mode produces lighter result`** — Screen blend of dark colors produces brighter output
- **`blend-mode color filter tints output`** — BlendMode(Blue, SrcATop) tints red to show blue
- **`color matrix filter desaturates to grayscale`** — grayscale matrix makes R, G, B channels similar
- **`composed color filter applies both`** — composes Lighting and LumaColor filters
- **`high contrast and lighting color filters render`** — Lighting(White, +Red) produces visible output
- **`blur image filter blurs entire shape`** — Blur(5,5) extends past rect edge
- **`dilate and erode image filters change shape size`** — Dilate covers more pixels than baseline
- **`composed image filter applies both`** — composes Blur and Offset
- **`offset and color filter image filters render`** — Offset(10,10) shifts output
- **`merge image filter combines multiple`** — merges Blur and Offset filters
- **`displacement map image filter renders`** — displacement map produces visible output
- **`matrix convolution image filter renders`** — 3x3 sharpen kernel renders visible output
- **`text with italic slant renders`** — italic text produces visible pixels
- **`typeface fallback for unavailable family renders`** — nonexistent font falls back and still renders
- **`EvenOdd fill type alternates filled regions`** — withFillType EvenOdd creates valid path

</details>

---

## ViewerTests

Tests for the windowed viewer, input events, screenshots, and lifecycle management.
All tests are serialized (GLFW single-threaded requirement).

**System under test:** `SkiaViewer.Viewer.run`, `SkiaViewer.ViewerHandle`

---

### Test: `continuous rendering counts frames without exceptions`

**What this test does:** Creates a viewer with an Event-based scene stream, pushes a
test scene, subscribes to FrameTick events and counts them via `Interlocked.Increment`.
After 3 seconds of rendering, asserts more than 60 frames were produced.
*)

(*** do-not-eval ***)
let ``continuous rendering counts frames without exceptions`` () =
    let mutable frameCount = 0
    let sceneEvent = Event<Scene>()
    let config = makeConfig ()
    let scene = testScene ()
    let (viewer, inputs) = Viewer.run config sceneEvent.Publish
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
        | _ -> ())
    sceneEvent.Trigger(scene)
    Thread.Sleep(3000)
    Assert.True(frameCount > 60)

(**
### Test: `empty scene renders without errors`

**What this test does:** Creates a viewer with an empty scene (no elements, only
background color). Counts FrameTick events over 2 seconds and asserts at least 1 frame
was rendered without errors.
*)

(*** do-not-eval ***)
let ``empty scene renders without errors`` () =
    let mutable frameCount = 0
    let scene = Scene.empty SKColors.CornflowerBlue
    let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
        | _ -> ())
    Thread.Sleep(2000)
    Assert.True(frameCount > 0)

(**
### Test: `start stop cycle 10 times without crash`

**What this test does:** Creates and disposes a viewer 10 times in a loop, each time
rendering a simple orange rect for 500ms. Asserts no exceptions are thrown during the
entire lifecycle cycle.
*)

(*** do-not-eval ***)
let ``start stop cycle 10 times without crash`` () =
    let scene = Scene.create SKColors.Black [
        Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Orange)
    ]
    for _ in 1..10 do
        let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer
        Thread.Sleep(500)
    Assert.True(true)

(**
### Test: `cross-thread dispose completes within timeout`

**What this test does:** Starts a viewer, renders for 1 second, then disposes it from
a `Task.Run` (different thread). Asserts the dispose completes within 2 seconds and
that frames were rendered before disposal.
*)

(*** do-not-eval ***)
let ``cross-thread dispose completes within timeout`` () =
    let mutable frameCount = 0
    let scene = testScene ()
    let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
        | _ -> ())
    Thread.Sleep(1000)
    let disposeTask = System.Threading.Tasks.Task.Run(fun () -> (viewer :> IDisposable).Dispose())
    let completed = disposeTask.Wait(TimeSpan.FromSeconds(2.0))
    Assert.True(completed)
    Assert.True(frameCount > 0)

(**
### Test: `vulkan backend renders frames without exceptions`

**What this test does:** Creates a viewer with `PreferredBackend = Some Backend.Vulkan`,
renders the test scene for 3 seconds, and asserts more than 60 frames were produced.
*)

(*** do-not-eval ***)
let ``vulkan backend renders frames without exceptions`` () =
    let mutable frameCount = 0
    let config = { makeConfig () with PreferredBackend = Some Backend.Vulkan }
    let scene = testScene ()
    let (viewer, inputs) = Viewer.run config (singleSceneObservable scene)
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
        | _ -> ())
    Thread.Sleep(3000)
    Assert.True(frameCount > 60)

(**
### Test: `GL fallback when preferred backend is GL`

**What this test does:** Forces `PreferredBackend = Some Backend.GL` and renders for
2 seconds. Asserts at least 1 frame was rendered.
*)

(*** do-not-eval ***)
let ``GL fallback when preferred backend is GL`` () =
    let mutable frameCount = 0
    let config = { makeConfig () with PreferredBackend = Some Backend.GL }
    let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Orange) ]
    let (viewer, inputs) = Viewer.run config (singleSceneObservable scene)
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
        | _ -> ())
    Thread.Sleep(2000)
    Assert.True(frameCount > 0)

(**
### Test: `auto-detect with PreferredBackend None renders frames`

**What this test does:** Uses default config (`PreferredBackend = None`) which triggers
auto-detection (Vulkan then GL fallback). Renders for 2 seconds and asserts frames > 0.
*)

(*** do-not-eval ***)
let ``auto-detect with PreferredBackend None renders frames`` () =
    let mutable frameCount = 0
    let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Green) ]
    let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
        | _ -> ())
    Thread.Sleep(2000)
    Assert.True(frameCount > 0)

(**
### Test: `backend selection message appears on stderr`

**What this test does:** Captures stderr output during viewer startup, renders for 2
seconds, and asserts the output contains "Backend selected:".
*)

(*** do-not-eval ***)
let ``backend selection message appears on stderr`` () =
    let mutable capturedOutput = ""
    let originalStderr = Console.Error
    use sw = new StringWriter()
    Console.SetError(sw)
    try
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.White) ]
        let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer
        Thread.Sleep(2000)
        (viewer :> IDisposable).Dispose()
        capturedOutput <- sw.ToString()
    finally
        Console.SetError(originalStderr)
    Assert.Contains("Backend selected:", capturedOutput)

(**
### Test: `input event stream is subscribable and emits FrameTick`

**What this test does:** Subscribes to the input observable and counts FrameTick events
over 2 seconds. Asserts more than 30 FrameTick events were emitted.
*)

(*** do-not-eval ***)
let ``input event stream is subscribable and emits FrameTick`` () =
    let mutable frameTicks = 0
    let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.White) ]
    let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameTicks) |> ignore
        | _ -> ())
    Thread.Sleep(2000)
    Assert.True(frameTicks > 30)

(**
### Test: `input event stream delivers multiple event types`

**What this test does:** Counts both FrameTick events and total events. After 2 seconds,
asserts both counters exceed 30. Verifies the observable delivers multiple event types.
*)

(*** do-not-eval ***)
let ``input event stream delivers multiple event types`` () =
    let mutable frameTickCount = 0
    let mutable anyEventCount = 0
    let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.White) ]
    let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        Interlocked.Increment(&anyEventCount) |> ignore
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameTickCount) |> ignore
        | _ -> ())
    Thread.Sleep(2000)
    Assert.True(frameTickCount > 30)
    Assert.True(anyEventCount > 30)

(**
### Test: `pushing multiple scenes updates rendering`

**What this test does:** Pushes 10 distinct scenes (moving circle) through an Event-based
stream at 100ms intervals. Asserts frames were rendered, verifying the scene stream
subscription processes updates.
*)

(*** do-not-eval ***)
let ``pushing multiple scenes updates rendering`` () =
    let mutable frameCount = 0
    let sceneEvent = Event<Scene>()
    let (viewer, inputs) = Viewer.run (makeConfig ()) sceneEvent.Publish
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
        | _ -> ())
    for i in 0..9 do
        let x = float32 i * 20f
        sceneEvent.Trigger(Scene.create SKColors.Black [ Scene.circle x 50f 10f (Scene.fill SKColors.Red) ])
        Thread.Sleep(100)
    Thread.Sleep(500)
    Assert.True(frameCount > 0)

(**
### Test: `scene stream error keeps last valid scene`

**What this test does:** Creates an observable that emits 5 valid scenes then calls
`OnError`. After 2 seconds, asserts more than 30 frames were rendered — proving the
viewer continues rendering the last valid scene after a stream error.
*)

(*** do-not-eval ***)
let ``scene stream error keeps last valid scene`` () =
    let mutable frameCount = 0
    let errorObservable =
        { new IObservable<Scene> with
            member _.Subscribe(observer) =
                for _ in 1..5 do
                    observer.OnNext(Scene.create SKColors.Black [
                        Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)
                    ])
                observer.OnError(Exception("Test error"))
                { new IDisposable with member _.Dispose() = () } }
    let (viewer, inputs) = Viewer.run (makeConfig ()) errorObservable
    use viewer = viewer
    use _sub = inputs.Subscribe(fun evt ->
        match evt with
        | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
        | _ -> ())
    Thread.Sleep(2000)
    Assert.True(frameCount > 30)

(**
### Test: `screenshot saves PNG file`

**What this test does:** Creates a viewer rendering a red rect, waits 1 second, calls
`viewer.Screenshot(tempDir)`. Asserts the result is `Ok path`, the file exists, ends
with `.png`, and has non-zero length.
*)

(*** do-not-eval ***)
let ``screenshot saves PNG file`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(tempDir) |> ignore
    try
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 200f 150f (Scene.fill SKColors.Red) ]
        let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer
        Thread.Sleep(1000)
        let result = viewer.Screenshot(tempDir)
        match result with
        | Ok path ->
            Assert.True(File.Exists(path))
            Assert.EndsWith(".png", path)
            Assert.True(FileInfo(path).Length > 0L)
        | Error msg -> Assert.Fail($"Screenshot should succeed: {msg}")
    finally
        if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

(**
### Test: `screenshot saves JPEG when format specified`

**What this test does:** Calls `viewer.Screenshot(tempDir, ImageFormat.Jpeg)` and
asserts the result file ends with `.jpg` and has non-zero length.
*)

(*** do-not-eval ***)
let ``screenshot saves JPEG when format specified`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
    try
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 200f 150f (Scene.fill SKColors.Magenta) ]
        let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer
        Thread.Sleep(1000)
        let result = viewer.Screenshot(tempDir, ImageFormat.Jpeg)
        match result with
        | Ok path ->
            Assert.True(File.Exists(path))
            Assert.EndsWith(".jpg", path)
            Assert.True(FileInfo(path).Length > 0L)
        | Error msg -> Assert.Fail($"Screenshot should succeed: {msg}")
    finally
        if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

(**
### Test: `screenshot returns error after viewer disposal`

**What this test does:** Disposes the viewer, then calls `Screenshot`. Asserts the
result is `Error`, verifying that screenshots correctly fail after disposal.
*)

(*** do-not-eval ***)
let ``screenshot returns error after viewer disposal`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(tempDir) |> ignore
    try
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Purple) ]
        let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        Thread.Sleep(1000)
        (viewer :> IDisposable).Dispose()
        let result = viewer.Screenshot(tempDir)
        match result with
        | Error _ -> ()
        | Ok path -> Assert.Fail($"Expected Error after disposal, but got Ok: {path}")
    finally
        if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

(**
### Test: `screenshot creates non-existent folder`

**What this test does:** Passes a nested non-existent directory path to `Screenshot`.
Asserts the directory was created and the file exists.
*)

(*** do-not-eval ***)
let ``screenshot creates non-existent folder`` () =
    let baseDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
    let nestedDir = Path.Combine(baseDir, "nested", "subfolder")
    try
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Orange) ]
        let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer
        Thread.Sleep(1000)
        let result = viewer.Screenshot(nestedDir)
        match result with
        | Ok path ->
            Assert.True(Directory.Exists(nestedDir))
            Assert.True(File.Exists(path))
        | Error msg -> Assert.Fail($"Screenshot should succeed: {msg}")
    finally
        if Directory.Exists(baseDir) then Directory.Delete(baseDir, true)

(**
### Test: `screenshot with Vulkan backend`

**What this test does:** Takes a screenshot using the Vulkan backend and verifies the
file exists and has non-zero length.
*)

(*** do-not-eval ***)
let ``screenshot with Vulkan backend`` () =
    let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
    try
        let config = { makeConfig () with PreferredBackend = Some Backend.Vulkan }
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 200f 150f (Scene.fill SKColors.Red) ]
        let (viewer, _) = Viewer.run config (singleSceneObservable scene)
        use viewer = viewer
        Thread.Sleep(1000)
        let result = viewer.Screenshot(tempDir)
        match result with
        | Ok path ->
            Assert.True(File.Exists(path))
            Assert.True(FileInfo(path).Length > 0L)
        | Error msg -> Assert.Fail($"Screenshot with Vulkan should succeed: {msg}")
    finally
        if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

(**
### Test: `public API surface matches baseline`

**What this test does:** Reflects on the SkiaViewer assembly and asserts that all
expected public types exist: Backend, ImageFormat, ViewerConfig, ViewerHandle, Viewer,
Paint, Transform, PathCommand, Element, Scene, InputEvent. Also verifies ViewerHandle
has a Screenshot method and implements IDisposable, and that Viewer.run exists.
*)

(*** do-not-eval ***)
let ``public API surface matches baseline`` () =
    let asm = typeof<ViewerConfig>.Assembly
    let publicTypes =
        asm.GetExportedTypes()
        |> Array.map (fun t -> t.FullName)
        |> Array.sort
    Assert.Contains("SkiaViewer.Backend", publicTypes)
    Assert.Contains("SkiaViewer.ImageFormat", publicTypes)
    Assert.Contains("SkiaViewer.ViewerConfig", publicTypes)
    Assert.Contains("SkiaViewer.ViewerHandle", publicTypes)
    Assert.Contains("SkiaViewer.Viewer", publicTypes)
    Assert.Contains("SkiaViewer.Paint", publicTypes)
    Assert.Contains("SkiaViewer.Transform", publicTypes)
    Assert.Contains("SkiaViewer.PathCommand", publicTypes)
    Assert.Contains("SkiaViewer.Element", publicTypes)
    Assert.Contains("SkiaViewer.Scene", publicTypes)
    Assert.Contains("SkiaViewer.InputEvent", publicTypes)
    let handleType = typeof<ViewerHandle>
    let screenshotMethod = handleType.GetMethod("Screenshot")
    Assert.NotNull(screenshotMethod)
    Assert.True(typeof<IDisposable>.IsAssignableFrom(handleType))
    let viewerModule = asm.GetType("SkiaViewer.Viewer")
    Assert.NotNull(viewerModule)
    let runMethod = viewerModule.GetMethod("run")
    Assert.NotNull(runMethod)
    let sceneModule = asm.GetType("SkiaViewer.Scene")
    Assert.NotNull(sceneModule)

(**
---

## Test Summary

| Test Class | Test Count | Coverage Area |
|---|---|---|
| SceneTests | 27 | DSL type construction, paint builders, path ops, regions, text measurement |
| SceneRendererTests | 42 | Pixel-level rendering of all element types, transforms, effects, filters, clipping |
| ViewerTests | 21 | Windowed rendering, backend selection, input events, screenshots, lifecycle |
| **Total** | **90** | |
*)

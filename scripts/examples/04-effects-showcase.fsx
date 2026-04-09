/// SkiaSharp API Coverage — Visual Feature Showcase
/// Cycles through 10 demos (10s each) showing all new features.
///
/// Run: dotnet fsi scripts/examples/04-effects-showcase.fsx

#load "../prelude.fsx"
open Prelude

open System
open System.Threading
open SkiaSharp
open SkiaViewer

// ── Helpers ──

let sceneEvent = Event<Scene>()
let config = { defaultConfig with Title = "SkiaViewer Feature Showcase"; Width = 900; Height = 650 }
let (viewerHandle, inputs) = Viewer.run config sceneEvent.Publish
use _viewerHandle = viewerHandle

let mutable running = true
use _sub = inputs.Subscribe(fun event ->
    match event with
    | InputEvent.KeyDown key when key = Silk.NET.Input.Key.Escape -> running <- false
    | _ -> ())

let showScene title (buildScene: float32 -> Scene) (seconds: int) =
    printfn $"\n═══ %s{title} ═══"
    let sw = System.Diagnostics.Stopwatch.StartNew()
    while running && sw.Elapsed.TotalSeconds < float seconds do
        let t = float32 sw.Elapsed.TotalSeconds
        sceneEvent.Trigger(buildScene t)
        Thread.Sleep(33) // ~30 fps

// ── Demo 1: Stroke Styling ──

let demo1 (t: float32) =
    let bg = SKColor(30uy, 30uy, 40uy)
    let joinPath join yOff =
        let cmds = [ PathCommand.MoveTo(470f, 120f + yOff); PathCommand.LineTo(530f, 80f + yOff); PathCommand.LineTo(590f, 120f + yOff) ]
        Scene.path cmds (Scene.stroke SKColors.DodgerBlue 8f |> Scene.withStrokeJoin join)
    let phase = t * 40f
    let zigzag = [
        PathCommand.MoveTo(40f, 400f); PathCommand.LineTo(140f, 350f)
        PathCommand.LineTo(240f, 400f); PathCommand.LineTo(340f, 350f)
        PathCommand.LineTo(440f, 400f); PathCommand.LineTo(540f, 350f)
    ]
    let trimStart = (sin (float t * 2.0) * 0.5 + 0.5) |> float32 |> fun v -> v * 0.5f
    let trimEnd = trimStart + 0.4f
    let circlePath = [ PathCommand.AddCircle(200f, 550f, 60f, PathDirection.Clockwise) ]
    Scene.create bg [
        Scene.text "1. Stroke Styling" 20f 40f 28f (Scene.fill SKColors.White)

        Scene.text "StrokeCap:" 20f 80f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        Scene.line 40f 110f 250f 110f (Scene.stroke SKColors.Coral 12f |> Scene.withStrokeCap StrokeCap.Butt)
        Scene.text "Butt" 260f 115f 14f (Scene.fill SKColors.White)
        Scene.line 40f 140f 250f 140f (Scene.stroke SKColors.Coral 12f |> Scene.withStrokeCap StrokeCap.Round)
        Scene.text "Round" 260f 145f 14f (Scene.fill SKColors.White)
        Scene.line 40f 170f 250f 170f (Scene.stroke SKColors.Coral 12f |> Scene.withStrokeCap StrokeCap.Square)
        Scene.text "Square" 260f 175f 14f (Scene.fill SKColors.White)

        Scene.text "StrokeJoin:" 450f 80f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        joinPath StrokeJoin.Miter 0f
        Scene.text "Miter" 600f 115f 14f (Scene.fill SKColors.White)
        joinPath StrokeJoin.Round 55f
        Scene.text "Round" 600f 170f 14f (Scene.fill SKColors.White)
        joinPath StrokeJoin.Bevel 110f
        Scene.text "Bevel" 600f 225f 14f (Scene.fill SKColors.White)

        Scene.text "Dash Effect (animated):" 20f 260f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        Scene.line 40f 290f 860f 290f
            (Scene.stroke SKColors.LimeGreen 4f |> Scene.withPathEffect (PathEffect.Dash([| 20f; 10f; 5f; 10f |], phase)))

        Scene.text "Corner Effect:" 20f 340f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        Scene.path zigzag (Scene.stroke SKColors.Orange 3f)
        Scene.path zigzag (Scene.stroke SKColors.Yellow 3f |> Scene.withPathEffect (PathEffect.Corner(25f)))
        Scene.text "Original (orange) vs Cornered (yellow)" 20f 430f 14f (Scene.fill (SKColor(180uy, 180uy, 180uy)))

        Scene.text "Trim Effect (animated):" 20f 470f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        Scene.path circlePath (Scene.stroke SKColors.Magenta 6f |> Scene.withPathEffect (PathEffect.Trim(trimStart, trimEnd, TrimMode.Normal)))
        Scene.text $"start=%.2f{trimStart} end=%.2f{trimEnd}" 280f 555f 14f (Scene.fill SKColors.White)
    ]

showScene "Stroke Styling — Caps, Joins, Dash, Corner, Trim" demo1 10

// ── Demo 2: Shaders ──

let demo2 (t: float32) =
    let bg = SKColor(20uy, 20uy, 30uy)
    Scene.create bg [
        Scene.text "2. Shader System" 20f 40f 28f (Scene.fill SKColors.White)

        // Linear gradient
        let linear = Shader.LinearGradient(SKPoint(40f, 70f), SKPoint(240f, 150f),
            [| SKColors.Red; SKColors.Yellow; SKColors.Green |], [| 0f; 0.5f; 1f |], TileMode.Clamp)
        Scene.rect 40f 70f 200f 80f (Scene.fill SKColors.White |> Scene.withShader linear)
        Scene.text "Linear" 250f 115f 14f (Scene.fill SKColors.White)

        // Radial gradient
        let radial = Shader.RadialGradient(SKPoint(450f, 110f), 60f,
            [| SKColors.Cyan; SKColors.Blue; SKColors.Transparent |], [| 0f; 0.7f; 1f |], TileMode.Clamp)
        Scene.rect 380f 60f 140f 100f (Scene.fill SKColors.White |> Scene.withShader radial)
        Scene.text "Radial" 530f 115f 14f (Scene.fill SKColors.White)

        // Sweep gradient (animated)
        let angle = t * 60f
        let sweep = Shader.SweepGradient(SKPoint(730f, 110f),
            [| SKColors.Red; SKColors.Orange; SKColors.Yellow; SKColors.Green; SKColors.Blue; SKColors.Purple; SKColors.Red |],
            [| 0f; 0.16f; 0.33f; 0.5f; 0.66f; 0.83f; 1f |], angle, angle + 360f)
        Scene.circle 730f 110f 55f (Scene.fill SKColors.White |> Scene.withShader sweep)
        Scene.text "Sweep" 795f 115f 14f (Scene.fill SKColors.White)

        // Perlin noise fractal
        let noise1 = Shader.PerlinNoiseFractalNoise(0.03f, 0.03f, 4, t * 0.5f)
        Scene.rect 40f 200f 180f 120f (Scene.fill SKColors.White |> Scene.withShader noise1)
        Scene.text "Fractal Noise" 40f 340f 14f (Scene.fill SKColors.White)

        // Perlin noise turbulence
        let noise2 = Shader.PerlinNoiseTurbulence(0.04f, 0.04f, 3, 42f)
        Scene.rect 260f 200f 180f 120f (Scene.fill SKColors.White |> Scene.withShader noise2)
        Scene.text "Turbulence" 260f 340f 14f (Scene.fill SKColors.White)

        // Solid color shader
        Scene.rect 480f 200f 80f 120f (Scene.fill SKColors.White |> Scene.withShader (Shader.SolidColor(SKColors.HotPink)))
        Scene.text "Solid" 480f 340f 14f (Scene.fill SKColors.White)

        // Composed shader
        let s1 = Shader.LinearGradient(SKPoint(600f, 200f), SKPoint(840f, 320f),
            [| SKColors.Red; SKColors.Transparent |], [| 0f; 1f |], TileMode.Clamp)
        let s2 = Shader.LinearGradient(SKPoint(840f, 200f), SKPoint(600f, 320f),
            [| SKColors.Blue; SKColors.Transparent |], [| 0f; 1f |], TileMode.Clamp)
        let composed = Shader.Compose(s1, s2, BlendMode.Screen)
        Scene.rect 600f 200f 240f 120f (Scene.fill SKColors.White |> Scene.withShader composed)
        Scene.text "Composed (Screen)" 600f 340f 14f (Scene.fill SKColors.White)

        // Tiling demo
        Scene.text "Tile Modes:" 40f 380f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        let tileGrad mode label x =
            let shader = Shader.LinearGradient(SKPoint(x, 400f), SKPoint(x + 60f, 400f),
                [| SKColors.Red; SKColors.Blue |], [| 0f; 1f |], mode)
            Scene.rect x 400f 180f 80f (Scene.fill SKColors.White |> Scene.withShader shader)
            |> ignore
            Scene.text label x 500f 12f (Scene.fill SKColors.White)
        [
            Scene.rect 40f 400f 180f 80f (Scene.fill SKColors.White |> Scene.withShader
                (Shader.LinearGradient(SKPoint(40f, 400f), SKPoint(100f, 400f), [| SKColors.Red; SKColors.Blue |], [| 0f; 1f |], TileMode.Repeat)))
            Scene.text "Repeat" 40f 500f 12f (Scene.fill SKColors.White)
            Scene.rect 240f 400f 180f 80f (Scene.fill SKColors.White |> Scene.withShader
                (Shader.LinearGradient(SKPoint(240f, 400f), SKPoint(300f, 400f), [| SKColors.Red; SKColors.Blue |], [| 0f; 1f |], TileMode.Mirror)))
            Scene.text "Mirror" 240f 500f 12f (Scene.fill SKColors.White)
            Scene.rect 440f 400f 180f 80f (Scene.fill SKColors.White |> Scene.withShader
                (Shader.LinearGradient(SKPoint(440f, 400f), SKPoint(500f, 400f), [| SKColors.Red; SKColors.Blue |], [| 0f; 1f |], TileMode.Clamp)))
            Scene.text "Clamp" 440f 500f 12f (Scene.fill SKColors.White)
        ] |> List.iter ignore
    ]

showScene "Shader System — Gradients, Noise, Compose, Tile Modes" demo2 10

// ── Demo 3: Blend Modes ──

let demo3 (t: float32) =
    let bg = SKColor(25uy, 25uy, 35uy)
    let modes = [
        ("SrcOver", BlendMode.SrcOver); ("Multiply", BlendMode.Multiply)
        ("Screen", BlendMode.Screen); ("Overlay", BlendMode.Overlay)
        ("Darken", BlendMode.Darken); ("Lighten", BlendMode.Lighten)
        ("ColorDodge", BlendMode.ColorDodge); ("ColorBurn", BlendMode.ColorBurn)
        ("HardLight", BlendMode.HardLight); ("SoftLight", BlendMode.SoftLight)
        ("Difference", BlendMode.Difference); ("Exclusion", BlendMode.Exclusion)
        ("Hue", BlendMode.Hue); ("Saturation", BlendMode.Saturation)
        ("Color", BlendMode.Color); ("Luminosity", BlendMode.Luminosity)
    ]
    let elems = [
        yield Scene.text "3. Blend Modes (Red base + Blue overlay)" 20f 35f 24f (Scene.fill SKColors.White)
        for i, (name, mode) in modes |> List.indexed do
            let col = i % 4
            let row = i / 4
            let x = 30f + float32 col * 215f
            let y = 60f + float32 row * 145f
            // Red base circle
            yield Scene.circle (x + 50f) (y + 55f) 40f (Scene.fill SKColors.Red)
            // Blue overlay circle with blend mode
            yield Scene.circle (x + 80f) (y + 55f) 40f (Scene.fill (SKColor(0uy, 100uy, 255uy)) |> Scene.withBlendMode mode)
            yield Scene.text name x (y + 110f) 13f (Scene.fill SKColors.White)
    ]
    Scene.create bg elems

showScene "Blend Modes — 16 blend modes on overlapping circles" (fun (_: float32) -> demo3 0f) 10

// ── Demo 4: Color Filters ──

let demo4 (t: float32) =
    let bg = SKColor(25uy, 25uy, 35uy)
    let baseGrad = Shader.LinearGradient(SKPoint(0f, 0f), SKPoint(160f, 100f),
        [| SKColors.Red; SKColors.Yellow; SKColors.Green; SKColors.Cyan; SKColors.Blue |],
        [| 0f; 0.25f; 0.5f; 0.75f; 1f |], TileMode.Clamp)
    let basePaint = Scene.fill SKColors.White |> Scene.withShader baseGrad

    Scene.create bg [
        Scene.text "4. Color Filters" 20f 40f 28f (Scene.fill SKColors.White)

        // Original
        Scene.rect 40f 70f 160f 100f basePaint
        Scene.text "Original" 40f 190f 14f (Scene.fill SKColors.White)

        // Grayscale via color matrix
        let grayMatrix = [| 0.21f; 0.72f; 0.07f; 0f; 0f; 0.21f; 0.72f; 0.07f; 0f; 0f; 0.21f; 0.72f; 0.07f; 0f; 0f; 0f; 0f; 0f; 1f; 0f |]
        Scene.rect 240f 70f 160f 100f (basePaint |> Scene.withColorFilter (ColorFilter.ColorMatrix(grayMatrix)))
        Scene.text "Grayscale" 240f 190f 14f (Scene.fill SKColors.White)

        // Sepia
        let sepiaMatrix = [| 0.393f; 0.769f; 0.189f; 0f; 0f; 0.349f; 0.686f; 0.168f; 0f; 0f; 0.272f; 0.534f; 0.131f; 0f; 0f; 0f; 0f; 0f; 1f; 0f |]
        Scene.rect 440f 70f 160f 100f (basePaint |> Scene.withColorFilter (ColorFilter.ColorMatrix(sepiaMatrix)))
        Scene.text "Sepia" 440f 190f 14f (Scene.fill SKColors.White)

        // Blue tint
        Scene.rect 640f 70f 160f 100f (basePaint |> Scene.withColorFilter (ColorFilter.BlendMode(SKColor(0uy, 0uy, 200uy, 100uy), BlendMode.SrcATop)))
        Scene.text "Blue Tint" 640f 190f 14f (Scene.fill SKColors.White)

        // LumaColor
        Scene.rect 40f 230f 160f 100f (basePaint |> Scene.withColorFilter ColorFilter.LumaColor)
        Scene.text "LumaColor" 40f 350f 14f (Scene.fill SKColors.White)

        // Lighting
        Scene.rect 240f 230f 160f 100f (basePaint |> Scene.withColorFilter (ColorFilter.Lighting(SKColors.White, SKColor(80uy, 0uy, 0uy))))
        Scene.text "Lighting (+red)" 240f 350f 14f (Scene.fill SKColors.White)

        // Composed
        Scene.rect 440f 230f 160f 100f (basePaint
            |> Scene.withColorFilter (ColorFilter.Compose(ColorFilter.ColorMatrix(grayMatrix), ColorFilter.Lighting(SKColors.White, SKColor(0uy, 50uy, 80uy)))))
        Scene.text "Composed" 440f 350f 14f (Scene.fill SKColors.White)
    ]

showScene "Color Filters — Matrix, Tint, Luma, Lighting, Compose" (fun (_: float32) -> demo4 0f) 10

// ── Demo 5: Mask & Image Filters ──

let demo5 (t: float32) =
    let bg = SKColor(25uy, 25uy, 35uy)
    Scene.create bg [
        Scene.text "5. Mask & Image Filters" 20f 40f 28f (Scene.fill SKColors.White)

        // Blur styles
        Scene.text "Mask Blur Styles:" 20f 75f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        let blurDemo style label x =
            [
                Scene.rect x 90f 100f 70f (Scene.fill SKColors.Coral |> Scene.withMaskFilter (MaskFilter.Blur(style, 6f)))
                Scene.text label x 180f 13f (Scene.fill SKColors.White)
            ]
        yield! blurDemo BlurStyle.Normal "Normal" 40f
        yield! blurDemo BlurStyle.Solid "Solid" 180f
        yield! blurDemo BlurStyle.Outer "Outer" 320f
        yield! blurDemo BlurStyle.Inner "Inner" 460f

        // Drop shadow (animated offset)
        Scene.text "Image Filters:" 20f 220f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        let shadowDx = sin (float t * 2.0) * 8.0 |> float32
        let shadowDy = cos (float t * 2.0) * 8.0 |> float32
        Scene.rect 40f 240f 120f 80f
            (Scene.fill SKColors.DodgerBlue |> Scene.withImageFilter (ImageFilter.DropShadow(shadowDx, shadowDy, 4f, 4f, SKColor(0uy, 0uy, 0uy, 180uy))))
        Scene.text "Drop Shadow" 40f 340f 13f (Scene.fill SKColors.White)

        // Blur
        Scene.rect 220f 240f 120f 80f
            (Scene.fill SKColors.LimeGreen |> Scene.withImageFilter (ImageFilter.Blur(4f, 4f)))
        Scene.text "Image Blur" 220f 340f 13f (Scene.fill SKColors.White)

        // Dilate
        Scene.rect 420f 255f 80f 50f
            (Scene.fill SKColors.Orange |> Scene.withImageFilter (ImageFilter.Dilate(5, 5)))
        Scene.text "Dilate" 420f 340f 13f (Scene.fill SKColors.White)

        // Erode
        Scene.rect 580f 240f 120f 80f
            (Scene.fill SKColors.Magenta |> Scene.withImageFilter (ImageFilter.Erode(3, 3)))
        Scene.text "Erode" 580f 340f 13f (Scene.fill SKColors.White)

        // Composed: shadow + blur
        Scene.rect 40f 380f 120f 80f
            (Scene.fill SKColors.Gold |> Scene.withImageFilter (ImageFilter.Compose(ImageFilter.Blur(3f, 3f), ImageFilter.DropShadow(5f, 5f, 2f, 2f, SKColors.Black))))
        Scene.text "Shadow + Blur" 40f 480f 13f (Scene.fill SKColors.White)
    ]

showScene "Mask & Image Filters — Blur, Shadow, Dilate, Erode" demo5 10

// ── Demo 6: Clipping ──

let demo6 (t: float32) =
    let bg = SKColor(25uy, 25uy, 35uy)
    let gradient = Shader.LinearGradient(SKPoint(0f, 0f), SKPoint(400f, 400f),
        [| SKColors.Red; SKColors.Yellow; SKColors.Green; SKColors.Cyan; SKColors.Blue; SKColors.Magenta |],
        [| 0f; 0.2f; 0.4f; 0.6f; 0.8f; 1f |], TileMode.Repeat)
    let colorRect = Scene.rect 0f 0f 900f 650f (Scene.fill SKColors.White |> Scene.withShader gradient)

    Scene.create bg [
        Scene.text "6. Clipping" 20f 40f 28f (Scene.fill SKColors.White)

        // Rect clip
        let rectClip = Clip.Rect(SKRect(60f, 80f, 260f, 230f), ClipOperation.Intersect, true)
        Scene.groupWithClip None None rectClip [ colorRect ]
        Scene.path [ PathCommand.AddRect(SKRect(60f, 80f, 260f, 230f), PathDirection.Clockwise) ]
            (Scene.stroke SKColors.White 2f)
        Scene.text "Rect Clip" 60f 260f 14f (Scene.fill SKColors.White)

        // Circle clip (animated radius)
        let r = 50f + float32 (sin (float t * 2.0) * 20.0)
        let circleClip = Clip.Path([ PathCommand.AddCircle(430f, 155f, r, PathDirection.Clockwise) ], ClipOperation.Intersect, true)
        Scene.groupWithClip None None circleClip [ colorRect ]
        Scene.text $"Circle Clip (r=%.0f{r})" 360f 260f 14f (Scene.fill SKColors.White)

        // Difference clip
        let diffClip = Clip.Rect(SKRect(600f, 80f, 800f, 230f), ClipOperation.Intersect, true)
        let hole = Clip.Path([ PathCommand.AddCircle(700f, 155f, 40f, PathDirection.Clockwise) ], ClipOperation.Difference, true)
        Scene.groupWithClip None None diffClip [
            Scene.groupWithClip None None hole [ colorRect ]
        ]
        Scene.text "Difference Clip (hole)" 600f 260f 14f (Scene.fill SKColors.White)

        // Star clip
        let starPath = [
            PathCommand.MoveTo(200f, 320f); PathCommand.LineTo(230f, 410f)
            PathCommand.LineTo(320f, 410f); PathCommand.LineTo(245f, 460f)
            PathCommand.LineTo(270f, 550f); PathCommand.LineTo(200f, 490f)
            PathCommand.LineTo(130f, 550f); PathCommand.LineTo(155f, 460f)
            PathCommand.LineTo(80f, 410f); PathCommand.LineTo(170f, 410f)
            PathCommand.Close
        ]
        let starClip = Clip.Path(starPath, ClipOperation.Intersect, true)
        Scene.groupWithClip None None starClip [ colorRect ]
        Scene.text "Star Clip" 140f 580f 14f (Scene.fill SKColors.White)
    ]

showScene "Clipping — Rect, Circle, Difference, Star" demo6 10

// ── Demo 7: Text & Fonts ──

let demo7 (t: float32) =
    let bg = SKColor(25uy, 25uy, 35uy)
    Scene.create bg [
        Scene.text "7. Text & Fonts" 20f 40f 28f (Scene.fill SKColors.White)

        // Default font
        Scene.text "Default font (no FontSpec)" 40f 90f 22f (Scene.fill SKColors.Coral)

        // Bold
        let boldFont = { Family = "sans-serif"; Weight = 700; Slant = FontSlant.Upright; Width = 5 }
        Scene.text "Bold sans-serif (weight=700)" 40f 130f 22f (Scene.fill SKColors.DodgerBlue |> Scene.withFont boldFont)

        // Italic
        let italicFont = { Family = "serif"; Weight = 400; Slant = FontSlant.Italic; Width = 5 }
        Scene.text "Italic serif" 40f 170f 22f (Scene.fill SKColors.LimeGreen |> Scene.withFont italicFont)

        // Bold italic
        let boldItalic = { Family = "sans-serif"; Weight = 700; Slant = FontSlant.Italic; Width = 5 }
        Scene.text "Bold Italic sans-serif" 40f 210f 22f (Scene.fill SKColors.Gold |> Scene.withFont boldItalic)

        // Monospace
        let mono = { Family = "monospace"; Weight = 400; Slant = FontSlant.Upright; Width = 5 }
        Scene.text "Monospace: let x = 42" 40f 250f 20f (Scene.fill SKColors.Magenta |> Scene.withFont mono)

        // Large text with shader
        let grad = Shader.LinearGradient(SKPoint(40f, 280f), SKPoint(600f, 340f),
            [| SKColors.Red; SKColors.Yellow; SKColors.Green; SKColors.Cyan; SKColors.Blue |],
            [| 0f; 0.25f; 0.5f; 0.75f; 1f |], TileMode.Clamp)
        Scene.text "Rainbow Gradient Text!" 40f 340f 40f (Scene.fill SKColors.White |> Scene.withShader grad)

        // Text with drop shadow
        Scene.text "Drop Shadow Text" 40f 420f 36f
            (Scene.fill SKColors.White |> Scene.withImageFilter (ImageFilter.DropShadow(4f, 4f, 2f, 2f, SKColors.Black)))

        // Text measurement demo
        let measureFont = { Family = "sans-serif"; Weight = 400; Slant = FontSlant.Upright; Width = 5 }
        let bounds = Scene.measureText "Measured Text" 28f (Some measureFont)
        let textX = 40f
        let textY = 510f
        Scene.rect (textX + bounds.Left) (textY + bounds.Top) bounds.Width bounds.Height
            (Scene.stroke SKColors.Yellow 1f)
        Scene.text "Measured Text" textX textY 28f (Scene.fill SKColors.White |> Scene.withFont measureFont)
        Scene.text $"bounds: w=%.1f{bounds.Width} h=%.1f{bounds.Height}" 40f 540f 14f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
    ]

showScene "Text & Fonts — Weights, Slants, Shaders, Measurement" (fun (_: float32) -> demo7 0f) 10

// ── Demo 8: Path Operations ──

let demo8 (t: float32) =
    let bg = SKColor(25uy, 25uy, 35uy)
    let c1 = [ PathCommand.AddCircle(150f, 150f, 80f, PathDirection.Clockwise) ]
    let c2 = [ PathCommand.AddCircle(220f, 150f, 80f, PathDirection.Clockwise) ]
    let unionPath = Scene.combinePaths PathOp.Union c1 c2
    let intersectPath = Scene.combinePaths PathOp.Intersect c1 c2
    let diffPath = Scene.combinePaths PathOp.Difference c1 c2
    let xorPath = Scene.combinePaths PathOp.Xor c1 c2
    let spiralPath = [
        PathCommand.MoveTo(500f, 350f)
        PathCommand.CubicTo(600f, 300f, 700f, 400f, 600f, 450f)
        PathCommand.CubicTo(500f, 500f, 400f, 400f, 500f, 350f)
    ]
    let length = Scene.measurePath spiralPath
    let segStart = float32 ((sin (float t * 1.5) * 0.5 + 0.5))
    let segEnd = min 1.0f (segStart + 0.3f)
    let segment = Scene.extractPathSegment spiralPath (segStart * length) (segEnd * length)

    Scene.create bg [
        Scene.text "8. Path Operations" 20f 40f 28f (Scene.fill SKColors.White)

        Scene.path unionPath (Scene.fill (SKColor(100uy, 200uy, 100uy, 180uy)))
        Scene.text "Union" 140f 260f 16f (Scene.fill SKColors.White)

        Scene.translate 300f 0f [ Scene.path intersectPath (Scene.fill (SKColor(200uy, 100uy, 100uy, 180uy))) ]
        Scene.text "Intersect" 440f 260f 16f (Scene.fill SKColors.White)

        Scene.translate 600f 0f [ Scene.path diffPath (Scene.fill (SKColor(100uy, 100uy, 200uy, 180uy))) ]
        Scene.text "Difference" 740f 260f 16f (Scene.fill SKColors.White)

        Scene.translate 0f 220f [ Scene.path xorPath (Scene.fill (SKColor(200uy, 200uy, 100uy, 180uy))) ]
        Scene.text "XOR" 140f 480f 16f (Scene.fill SKColors.White)

        Scene.path spiralPath (Scene.stroke SKColors.Cyan 3f)
        Scene.text $"Path length: %.1f{length}px" 450f 530f 14f (Scene.fill SKColors.White)

        Scene.path segment (Scene.stroke SKColors.Yellow 5f)
        Scene.text $"Segment: %.0f{segStart * 100f}%% → %.0f{segEnd * 100f}%%" 450f 555f 14f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
    ]

showScene "Path Operations — Union, Intersect, Difference, XOR, Measure" demo8 10

// ── Demo 9: Drawing Extensions ──

let demo9 (t: float32) =
    let bg = SKColor(25uy, 25uy, 35uy)
    let pts = [| for i in 0..20 ->
                    let x = 40f + float32 i * 35f
                    let y = 130f + float32 (sin (float x * 0.03 + float t * 2.0) * 30.0)
                    SKPoint(x, y) |]
    let polyPts = [| for i in 0..20 ->
                        let x = 40f + float32 i * 35f
                        let y = 210f + float32 (cos (float x * 0.04 + float t * 1.5) * 25.0)
                        SKPoint(x, y) |]
    let triPositions = [|
        SKPoint(100f, 310f); SKPoint(200f, 450f); SKPoint(300f, 310f)
        SKPoint(300f, 310f); SKPoint(200f, 450f); SKPoint(400f, 450f)
    |]
    let triColors = [|
        SKColors.Red; SKColors.Green; SKColors.Blue
        SKColors.Blue; SKColors.Green; SKColors.Yellow
    |]
    let sweep = (sin (float t) * 0.5 + 0.5) * 300.0 + 30.0 |> float32
    Scene.create bg [
        Scene.text "9. Drawing Extensions" 20f 40f 28f (Scene.fill SKColors.White)

        Scene.text "DrawPoints:" 20f 80f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        Scene.points pts PointMode.Points (Scene.stroke SKColors.Coral 8f |> Scene.withStrokeCap StrokeCap.Round)
        Scene.text "Points" 750f 120f 12f (Scene.fill SKColors.White)

        Scene.points pts PointMode.Lines (Scene.stroke (SKColor(100uy, 200uy, 100uy, 120uy)) 2f)
        Scene.text "Lines" 750f 140f 12f (Scene.fill (SKColor(100uy, 200uy, 100uy)))

        Scene.points polyPts PointMode.Polygon (Scene.stroke SKColors.DodgerBlue 2f)
        Scene.text "Polygon" 750f 210f 12f (Scene.fill SKColors.DodgerBlue)

        Scene.text "DrawVertices:" 20f 280f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        Scene.vertices triPositions triColors VertexMode.Triangles (Scene.fill SKColors.White)

        Scene.text "DrawArc:" 500f 280f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
        Scene.arc (SKRect(520f, 310f, 720f, 510f)) 0f sweep true (Scene.fill (SKColor(255uy, 165uy, 0uy, 200uy)))
        Scene.arc (SKRect(520f, 310f, 720f, 510f)) 0f sweep false (Scene.stroke SKColors.White 2f)
        Scene.text $"sweep = %.0f{sweep}°" 580f 540f 14f (Scene.fill SKColors.White)
    ]

showScene "Drawing Extensions — Points, Vertices, Arc" demo9 10

// ── Demo 10: 3D Perspective ──

let demo10 (t: float32) =
    let bg = SKColor(25uy, 25uy, 35uy)
    let angle = t * 45f

    Scene.create bg [
        Scene.text "10. 3D Perspective Transforms" 20f 40f 28f (Scene.fill SKColors.White)

        // Rotating card - Y axis
        let card1 = Transform.Compose [
            Transform.Translate(200f, 200f)
            Transform.Perspective(Transform3D.Compose [
                Transform3D.RotateY(angle)
            ])
            Transform.Translate(-75f, -50f)
        ]
        Scene.group (Some card1) None [
            Scene.rect 0f 0f 150f 100f (Scene.fill SKColors.DodgerBlue)
            Scene.rect 0f 0f 150f 100f (Scene.stroke SKColors.White 2f)
            Scene.text "Y-Rotate" 20f 55f 18f (Scene.fill SKColors.White)
        ]

        // Rotating card - X axis
        let card2 = Transform.Compose [
            Transform.Translate(500f, 200f)
            Transform.Perspective(Transform3D.Compose [
                Transform3D.RotateX(angle * 0.7f)
            ])
            Transform.Translate(-75f, -50f)
        ]
        Scene.group (Some card2) None [
            Scene.rect 0f 0f 150f 100f (Scene.fill SKColors.Coral)
            Scene.rect 0f 0f 150f 100f (Scene.stroke SKColors.White 2f)
            Scene.text "X-Rotate" 20f 55f 18f (Scene.fill SKColors.White)
        ]

        // Combined rotation
        let card3 = Transform.Compose [
            Transform.Translate(350f, 450f)
            Transform.Perspective(Transform3D.Compose [
                Transform3D.RotateX(angle * 0.5f)
                Transform3D.RotateY(angle * 0.3f)
                Transform3D.RotateZ(angle * 0.2f)
            ])
            Transform.Translate(-100f, -60f)
        ]
        Scene.group (Some card3) None [
            Scene.rect 0f 0f 200f 120f (Scene.fill SKColors.LimeGreen)
            Scene.rect 0f 0f 200f 120f (Scene.stroke SKColors.White 2f)
            Scene.text "X+Y+Z" 50f 65f 22f (Scene.fill SKColors.Black)
        ]

        Scene.text $"Angle: %.0f{angle}°" 20f 620f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
    ]

showScene "3D Perspective — Y-Rotate, X-Rotate, Combined XYZ" demo10 10

printfn "\n════════════════════════════════════════"
printfn "All 10 demos complete. Press Escape to close."

while running do
    Thread.Sleep(100)

printfn "Done."

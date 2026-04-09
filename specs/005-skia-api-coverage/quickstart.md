# Quickstart: Comprehensive SkiaSharp API Coverage

**Branch**: `005-skia-api-coverage` | **Date**: 2026-04-09

## Build & Test

```bash
# Build
dotnet build src/SkiaViewer/SkiaViewer.fsproj

# Run tests
dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj

# Pack
dotnet pack src/SkiaViewer/SkiaViewer.fsproj -o ~/.local/share/nuget-local/
```

## Usage Examples

### Stroke Styling

```fsharp
open SkiaViewer

// Dashed line with round caps
let dashedPaint =
    { Scene.emptyPaint with
        Stroke = Some SKColors.Red
        StrokeWidth = 3f
        StrokeCap = StrokeCap.Round
        StrokeJoin = StrokeJoin.Bevel
        PathEffect = Some (PathEffect.Dash([| 10f; 5f |], 0f)) }

let myLine = Scene.line 10f 10f 200f 100f dashedPaint
```

### Gradients and Shaders

```fsharp
// Radial gradient fill
let gradientPaint =
    { Scene.emptyPaint with
        Fill = Some SKColors.White  // fallback
        Shader = Some (Shader.RadialGradient(
            SKPoint(100f, 100f), 80f,
            [| SKColors.Red; SKColors.Blue |],
            [| 0f; 1f |],
            TileMode.Clamp)) }

let myCircle = Scene.circle 100f 100f 80f gradientPaint
```

### Blend Modes

```fsharp
// Multiply blend mode
let blendPaint =
    { Scene.emptyPaint with
        Fill = Some SKColors.Red
        BlendMode = BlendMode.Multiply }

let overlayRect = Scene.rect 50f 50f 100f 100f blendPaint
```

### Drop Shadow (Image Filter)

```fsharp
let shadowPaint =
    { Scene.emptyPaint with
        Fill = Some SKColors.Blue
        ImageFilter = Some (ImageFilter.DropShadow(5f, 5f, 3f, 3f, SKColors.Black)) }

let shadowRect = Scene.rect 100f 100f 80f 60f shadowPaint
```

### Blur (Mask Filter)

```fsharp
let blurPaint =
    { Scene.emptyPaint with
        Fill = Some SKColors.Green
        MaskFilter = Some (MaskFilter.Blur(BlurStyle.Normal, 5f)) }
```

### Color Filter (Grayscale)

```fsharp
let grayscaleMatrix = [|
    0.21f; 0.72f; 0.07f; 0f; 0f
    0.21f; 0.72f; 0.07f; 0f; 0f
    0.21f; 0.72f; 0.07f; 0f; 0f
    0f;    0f;    0f;    1f; 0f
|]

let grayscalePaint =
    { Scene.emptyPaint with
        Fill = Some SKColors.Red
        ColorFilter = Some (ColorFilter.ColorMatrix(grayscaleMatrix)) }
```

### Clipping

```fsharp
// Circular clip on a group
let circularClip =
    Clip.Path(
        [ PathCommand.AddCircle(100f, 100f, 50f, PathDirection.Clockwise) ],
        ClipOperation.Intersect, true)

let clippedGroup = Scene.groupWithClip None None circularClip [
    Scene.rect 0f 0f 200f 200f (Scene.fill SKColors.Red)
]
```

### Text with Custom Font

```fsharp
let fontPaint =
    { Scene.emptyPaint with
        Fill = Some SKColors.Black
        Font = Some { Family = "Arial"; Weight = 700; Slant = FontSlant.Upright; Width = 5 } }

let myText = Scene.text "Hello World" 50f 100f 24f fontPaint

// Measure text bounds
let bounds = Scene.measureText "Hello World" 24f (Some Scene.defaultFont)
```

### Path Operations

```fsharp
let circle1 = [ PathCommand.AddCircle(100f, 100f, 50f, PathDirection.Clockwise) ]
let circle2 = [ PathCommand.AddCircle(130f, 100f, 50f, PathDirection.Clockwise) ]

// Union of two circles
let merged = Scene.combinePaths PathOp.Union circle1 circle2
let mergedElement = Scene.path merged (Scene.fill SKColors.Blue)
```

### 3D Perspective Transform

```fsharp
let perspective3d = Transform.Perspective(
    Transform3D.Compose [
        Transform3D.Camera(0f, 0f, -576f)
        Transform3D.RotateY(45f)
    ])

let perspectiveGroup = Scene.group (Some perspective3d) None [
    Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Red)
]
```

### Picture Recording and Playback

```fsharp
let recorded = Scene.recordPicture (SKRect(0f, 0f, 200f, 200f)) [
    Scene.circle 50f 50f 30f (Scene.fill SKColors.Red)
    Scene.rect 80f 80f 40f 40f (Scene.fill SKColors.Blue)
]

// Draw picture at two positions
let scene = Scene.create SKColors.White [
    Scene.picture recorded (Some (Transform.Translate(0f, 0f)))
    Scene.picture recorded (Some (Transform.Translate(200f, 0f)))
]
```

### Region Hit Testing

```fsharp
let region = Scene.createRegionFromRect (SKRectI(10, 10, 100, 100))
let isHit = Scene.regionContains region 50 50  // true
let isMiss = Scene.regionContains region 200 200  // false
```

## File Structure

```
src/SkiaViewer/
├── Scene.fsi          # MODIFIED — all new types + extended Paint/Element/PathCommand/Transform
├── Scene.fs           # MODIFIED — type definitions + new DSL helpers + utility functions
├── SceneRenderer.fsi  # UNCHANGED
├── SceneRenderer.fs   # MODIFIED — render new element types, apply new paint properties
├── Viewer.fsi         # UNCHANGED
├── Viewer.fs          # MINOR — update Paint construction in any internal usage
├── VulkanBackend.fs   # UNCHANGED

tests/SkiaViewer.Tests/
├── SceneTests.fs           # MODIFIED — test new types and DSL helpers
├── SceneRendererTests.fs   # MODIFIED — test rendering of new elements and paint properties
├── ViewerTests.fs          # MODIFIED — update Paint construction
├── SurfaceAreaBaseline.txt # UPDATED — new API surface

scripts/
├── prelude.fsx                     # MODIFIED — update Paint helpers
├── examples/
│   ├── 04-effects-showcase.fsx     # NEW — shader, filter, blend mode demos
│   └── 05-advanced-features.fsx    # NEW — path ops, regions, runtime effects, 3D
```

# Public API Contract: Comprehensive SkiaSharp API Coverage

**Branch**: `005-skia-api-coverage` | **Date**: 2026-04-09

## Scene.fsi — New and Modified Public Types

### New DU Types

```fsharp
namespace SkiaViewer

open SkiaSharp

/// Stroke line cap style.
[<RequireQualifiedAccess>]
type StrokeCap = | Butt | Round | Square

/// Stroke line join style.
[<RequireQualifiedAccess>]
type StrokeJoin = | Miter | Round | Bevel

/// Compositing blend mode.
[<RequireQualifiedAccess>]
type BlendMode =
    | Clear | Src | Dst | SrcOver | DstOver
    | SrcIn | DstIn | SrcOut | DstOut
    | SrcATop | DstATop | Xor | Plus | Modulate
    | Screen | Overlay | Darken | Lighten
    | ColorDodge | ColorBurn | HardLight | SoftLight
    | Difference | Exclusion | Multiply
    | Hue | Saturation | Color | Luminosity

/// Shader tile mode for gradients and image fills.
[<RequireQualifiedAccess>]
type TileMode = | Clamp | Repeat | Mirror | Decal

/// Fill shader applied to an element's paint.
[<RequireQualifiedAccess>]
type Shader =
    | LinearGradient of start: SKPoint * endPoint: SKPoint * colors: SKColor[] * positions: float32[] * tileMode: TileMode
    | RadialGradient of center: SKPoint * radius: float32 * colors: SKColor[] * positions: float32[] * tileMode: TileMode
    | SweepGradient of center: SKPoint * colors: SKColor[] * positions: float32[] * startAngle: float32 * endAngle: float32
    | TwoPointConicalGradient of start: SKPoint * startRadius: float32 * endPoint: SKPoint * endRadius: float32 * colors: SKColor[] * positions: float32[] * tileMode: TileMode
    | PerlinNoiseFractalNoise of baseFrequencyX: float32 * baseFrequencyY: float32 * numOctaves: int * seed: float32
    | PerlinNoiseTurbulence of baseFrequencyX: float32 * baseFrequencyY: float32 * numOctaves: int * seed: float32
    | SolidColor of color: SKColor
    | Image of bitmap: SKBitmap * tileModeX: TileMode * tileModeY: TileMode
    | Compose of shader1: Shader * shader2: Shader * blendMode: BlendMode

/// Trim path effect mode.
[<RequireQualifiedAccess>]
type TrimMode = | Normal | Inverted

/// 1D path effect stamp style.
[<RequireQualifiedAccess>]
type Path1DStyle = | Translate | Rotate | Morph

/// Path effect applied to stroke geometry.
[<RequireQualifiedAccess>]
type PathEffect =
    | Dash of intervals: float32[] * phase: float32
    | Corner of radius: float32
    | Trim of start: float32 * stop: float32 * mode: TrimMode
    | Path1D of path: PathCommand list * advance: float32 * phase: float32 * style: Path1DStyle
    | Compose of outer: PathEffect * inner: PathEffect
    | Sum of first: PathEffect * second: PathEffect

/// High contrast inversion style.
[<RequireQualifiedAccess>]
type HighContrastInvertStyle = | NoInvert | InvertBrightness | InvertLightness

/// Color filter applied to paint output.
[<RequireQualifiedAccess>]
type ColorFilter =
    | BlendMode of color: SKColor * mode: BlendMode
    | ColorMatrix of matrix: float32[]
    | Compose of outer: ColorFilter * inner: ColorFilter
    | HighContrast of grayscale: bool * invertStyle: HighContrastInvertStyle * contrast: float32
    | Lighting of multiply: SKColor * add: SKColor
    | LumaColor

/// Blur style for mask filters.
[<RequireQualifiedAccess>]
type BlurStyle = | Normal | Solid | Outer | Inner

/// Mask filter for alpha-channel transformations.
[<RequireQualifiedAccess>]
type MaskFilter = | Blur of style: BlurStyle * sigma: float32

/// Color channel selector.
[<RequireQualifiedAccess>]
type ColorChannel = | R | G | B | A

/// Image filter for pixel-level effects.
[<RequireQualifiedAccess>]
type ImageFilter =
    | Blur of sigmaX: float32 * sigmaY: float32
    | DropShadow of dx: float32 * dy: float32 * sigmaX: float32 * sigmaY: float32 * color: SKColor
    | Dilate of radiusX: int * radiusY: int
    | Erode of radiusX: int * radiusY: int
    | Offset of dx: float32 * dy: float32
    | ColorFilter of filter: ColorFilter
    | Compose of outer: ImageFilter * inner: ImageFilter
    | Merge of filters: ImageFilter list
    | DisplacementMap of xChannel: ColorChannel * yChannel: ColorChannel * scale: float32 * displacement: ImageFilter
    | MatrixConvolution of kernelSize: int * int * kernel: float32[] * gain: float32 * bias: float32 * kernelOffset: int * int * tileMode: TileMode * convolveAlpha: bool

/// Clip operation type.
[<RequireQualifiedAccess>]
type ClipOperation = | Intersect | Difference

/// Clip region applied to a group.
[<RequireQualifiedAccess>]
type Clip =
    | Rect of rect: SKRect * operation: ClipOperation * antialias: bool
    | Path of commands: PathCommand list * operation: ClipOperation * antialias: bool

/// Font slant style.
[<RequireQualifiedAccess>]
type FontSlant = | Upright | Italic | Oblique

/// Font specification for text rendering.
type FontSpec =
    { Family: string
      Weight: int
      Slant: FontSlant
      Width: int }

/// Point rendering mode.
[<RequireQualifiedAccess>]
type PointMode = | Points | Lines | Polygon

/// Vertex rendering mode.
[<RequireQualifiedAccess>]
type VertexMode = | Triangles | TriangleStrip | TriangleFan

/// Boolean path operation.
[<RequireQualifiedAccess>]
type PathOp = | Difference | Intersect | Union | Xor | ReverseDifference

/// Path fill type.
[<RequireQualifiedAccess>]
type PathFillType = | Winding | EvenOdd | InverseWinding | InverseEvenOdd

/// Path winding direction.
[<RequireQualifiedAccess>]
type PathDirection = | Clockwise | CounterClockwise

/// 3D transformation for perspective effects.
[<RequireQualifiedAccess>]
type Transform3D =
    | RotateX of degrees: float32
    | RotateY of degrees: float32
    | RotateZ of degrees: float32
    | Translate of x: float32 * y: float32 * z: float32
    | Camera of x: float32 * y: float32 * z: float32
    | Compose of Transform3D list
```

### Modified Types

```fsharp
/// Declarative visual style applied to elements. (BREAKING — new required fields)
type Paint =
    { Fill: SKColor option
      Stroke: SKColor option
      StrokeWidth: float32
      Opacity: float32
      IsAntialias: bool
      StrokeCap: StrokeCap
      StrokeJoin: StrokeJoin
      StrokeMiter: float32
      BlendMode: BlendMode
      Shader: Shader option
      ColorFilter: ColorFilter option
      MaskFilter: MaskFilter option
      ImageFilter: ImageFilter option
      PathEffect: PathEffect option
      Font: FontSpec option }

/// Path drawing commands. (Extended with convenience commands)
[<RequireQualifiedAccess>]
type PathCommand =
    | MoveTo of x: float32 * y: float32
    | LineTo of x: float32 * y: float32
    | QuadTo of cx: float32 * cy: float32 * x: float32 * y: float32
    | CubicTo of c1x: float32 * c1y: float32 * c2x: float32 * c2y: float32 * x: float32 * y: float32
    | ArcTo of rect: SKRect * startAngle: float32 * sweepAngle: float32
    | Close
    | AddRect of rect: SKRect * direction: PathDirection
    | AddCircle of cx: float32 * cy: float32 * radius: float32 * direction: PathDirection
    | AddOval of rect: SKRect * direction: PathDirection
    | AddRoundRect of rect: SKRect * rx: float32 * ry: float32 * direction: PathDirection

/// 2D spatial transformation. (Extended with 3D perspective)
[<RequireQualifiedAccess>]
type Transform =
    | Translate of x: float32 * y: float32
    | Rotate of degrees: float32 * centerX: float32 * centerY: float32
    | Scale of scaleX: float32 * scaleY: float32 * centerX: float32 * centerY: float32
    | Matrix of SKMatrix
    | Compose of Transform list
    | Perspective of Transform3D

/// Declarative visual element. (Extended with new primitives and clip on Group)
[<RequireQualifiedAccess>]
type Element =
    | Rect of x: float32 * y: float32 * width: float32 * height: float32 * paint: Paint
    | Ellipse of cx: float32 * cy: float32 * rx: float32 * ry: float32 * paint: Paint
    | Line of x1: float32 * y1: float32 * x2: float32 * y2: float32 * paint: Paint
    | Text of text: string * x: float32 * y: float32 * fontSize: float32 * paint: Paint
    | Image of bitmap: SKBitmap * x: float32 * y: float32 * width: float32 * height: float32 * paint: Paint
    | Path of commands: PathCommand list * paint: Paint
    | Group of transform: Transform option * paint: Paint option * clip: Clip option * children: Element list
    | Points of points: SKPoint[] * mode: PointMode * paint: Paint
    | Vertices of positions: SKPoint[] * colors: SKColor[] * mode: VertexMode * paint: Paint
    | Arc of rect: SKRect * startAngle: float32 * sweepAngle: float32 * useCenter: bool * paint: Paint
    | Picture of picture: SKPicture * transform: Transform option
```

### New Scene Module Functions

```fsharp
module Scene =
    // ... existing functions updated to use new Paint ...

    /// Empty paint with sensible defaults for all new fields.
    val emptyPaint: Paint

    // New DSL helpers for paint modification
    val withStrokeCap: cap: StrokeCap -> paint: Paint -> Paint
    val withStrokeJoin: join: StrokeJoin -> paint: Paint -> Paint
    val withBlendMode: mode: BlendMode -> paint: Paint -> Paint
    val withShader: shader: Shader -> paint: Paint -> Paint
    val withColorFilter: filter: ColorFilter -> paint: Paint -> Paint
    val withMaskFilter: filter: MaskFilter -> paint: Paint -> Paint
    val withImageFilter: filter: ImageFilter -> paint: Paint -> Paint
    val withPathEffect: effect: PathEffect -> paint: Paint -> Paint
    val withFont: font: FontSpec -> paint: Paint -> Paint

    // New element constructors
    val points: pts: SKPoint[] -> mode: PointMode -> paint: Paint -> Element
    val vertices: positions: SKPoint[] -> colors: SKColor[] -> mode: VertexMode -> paint: Paint -> Element
    val arc: rect: SKRect -> startAngle: float32 -> sweepAngle: float32 -> useCenter: bool -> paint: Paint -> Element
    val picture: pic: SKPicture -> transform: Transform option -> Element
    val groupWithClip: transform: Transform option -> paint: Paint option -> clip: Clip -> children: Element list -> Element

    // Utility functions
    val measureText: text: string -> fontSize: float32 -> font: FontSpec option -> SKRect
    val defaultFont: FontSpec

    // Path operations
    val combinePaths: op: PathOp -> path1: PathCommand list -> path2: PathCommand list -> PathCommand list
    val measurePath: commands: PathCommand list -> float32
    val extractPathSegment: commands: PathCommand list -> start: float32 -> stop: float32 -> PathCommand list
    val withFillType: fillType: PathFillType -> commands: PathCommand list -> paint: Paint -> Element

    // Region utilities
    val createRegionFromRect: rect: SKRectI -> SKRegion
    val createRegionFromPath: commands: PathCommand list -> clip: SKRegion -> SKRegion
    val combineRegions: op: RegionOp -> region1: SKRegion -> region2: SKRegion -> SKRegion
    val regionContains: region: SKRegion -> x: int -> y: int -> bool

    // Picture recording
    val recordPicture: bounds: SKRect -> elements: Element list -> SKPicture
```

### RegionOp (new DU for Scene module)

```fsharp
/// Region boolean operation.
[<RequireQualifiedAccess>]
type RegionOp = | Difference | Intersect | Union | Xor | ReverseDifference | Replace
```

## SceneRenderer.fsi — No Public API Changes

SceneRenderer remains `module internal` with the same signature:

```fsharp
module internal SceneRenderer =
    val render: scene: Scene -> canvas: SKCanvas -> unit
```

## Viewer.fsi — No Changes

The Viewer API surface is unchanged. All new functionality is expressed through the Scene DSL types.

## Surface Area Baseline Impact

The `SurfaceAreaBaseline.txt` file must be updated to include all new types and Scene module functions listed above. The existing baseline is already out of date (callback API vs stream API) — this feature will update it to reflect the current + new API surface.

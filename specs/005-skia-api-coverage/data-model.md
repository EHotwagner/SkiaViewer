# Data Model: Comprehensive SkiaSharp API Coverage

**Branch**: `005-skia-api-coverage` | **Date**: 2026-04-09

## New Types

### StrokeCap (DU)

```
StrokeCap
  | Butt
  | Round
  | Square
```

Maps to: `SKStrokeCap`

### StrokeJoin (DU)

```
StrokeJoin
  | Miter
  | Round
  | Bevel
```

Maps to: `SKStrokeJoin`

### BlendMode (DU)

```
BlendMode
  | Clear | Src | Dst | SrcOver | DstOver
  | SrcIn | DstIn | SrcOut | DstOut
  | SrcATop | DstATop | Xor | Plus | Modulate
  | Screen | Overlay | Darken | Lighten
  | ColorDodge | ColorBurn | HardLight | SoftLight
  | Difference | Exclusion | Multiply
  | Hue | Saturation | Color | Luminosity
```

Maps to: `SKBlendMode`

### TileMode (DU)

```
TileMode
  | Clamp
  | Repeat
  | Mirror
  | Decal
```

Maps to: `SKShaderTileMode`

### Shader (DU)

```
Shader
  | LinearGradient of start: SKPoint * endPoint: SKPoint * colors: SKColor[] * positions: float32[] * tileMode: TileMode
  | RadialGradient of center: SKPoint * radius: float32 * colors: SKColor[] * positions: float32[] * tileMode: TileMode
  | SweepGradient of center: SKPoint * colors: SKColor[] * positions: float32[] * startAngle: float32 * endAngle: float32
  | TwoPointConicalGradient of start: SKPoint * startRadius: float32 * endPoint: SKPoint * endRadius: float32 * colors: SKColor[] * positions: float32[] * tileMode: TileMode
  | PerlinNoiseFractalNoise of baseFrequencyX: float32 * baseFrequencyY: float32 * numOctaves: int * seed: float32
  | PerlinNoiseTurbulence of baseFrequencyX: float32 * baseFrequencyY: float32 * numOctaves: int * seed: float32
  | SolidColor of color: SKColor
  | Image of bitmap: SKBitmap * tileModeX: TileMode * tileModeY: TileMode
  | Compose of shader1: Shader * shader2: Shader * blendMode: BlendMode
```

Maps to: `SKShader.Create*` factory methods

### PathEffect (DU)

```
PathEffect
  | Dash of intervals: float32[] * phase: float32
  | Corner of radius: float32
  | Trim of start: float32 * stop: float32 * mode: TrimMode
  | Path1D of path: PathCommand list * advance: float32 * phase: float32 * style: Path1DStyle
  | Compose of outer: PathEffect * inner: PathEffect
  | Sum of first: PathEffect * second: PathEffect
```

Maps to: `SKPathEffect.Create*` factory methods

### TrimMode (DU)

```
TrimMode
  | Normal
  | Inverted
```

Maps to: `SKTrimPathEffectMode`

### Path1DStyle (DU)

```
Path1DStyle
  | Translate
  | Rotate
  | Morph
```

Maps to: `SKPath1DPathEffectStyle`

### ColorFilter (DU)

```
ColorFilter
  | BlendMode of color: SKColor * mode: BlendMode
  | ColorMatrix of matrix: float32[]
  | Compose of outer: ColorFilter * inner: ColorFilter
  | HighContrast of grayscale: bool * invertStyle: HighContrastInvertStyle * contrast: float32
  | Lighting of multiply: SKColor * add: SKColor
  | LumaColor
```

Maps to: `SKColorFilter.Create*` factory methods

### HighContrastInvertStyle (DU)

```
HighContrastInvertStyle
  | NoInvert
  | InvertBrightness
  | InvertLightness
```

Maps to: `SKHighContrastConfigInvertStyle`

### BlurStyle (DU)

```
BlurStyle
  | Normal
  | Solid
  | Outer
  | Inner
```

Maps to: `SKBlurStyle`

### MaskFilter (DU)

```
MaskFilter
  | Blur of style: BlurStyle * sigma: float32
```

Maps to: `SKMaskFilter.CreateBlur`

### ImageFilter (DU)

```
ImageFilter
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
```

Maps to: `SKImageFilter.Create*` factory methods

### ColorChannel (DU)

```
ColorChannel
  | R | G | B | A
```

Maps to: `SKColorChannel`

### ClipOperation (DU)

```
ClipOperation
  | Intersect
  | Difference
```

Maps to: `SKClipOperation`

### Clip (DU)

```
Clip
  | Rect of rect: SKRect * operation: ClipOperation * antialias: bool
  | Path of commands: PathCommand list * operation: ClipOperation * antialias: bool
```

### FontSpec (Record)

```
FontSpec
  Family: string
  Weight: int          // Maps to SKFontStyleWeight values (100-900)
  Slant: FontSlant
  Width: int           // Maps to SKFontStyleWidth values (1-9)
```

### FontSlant (DU)

```
FontSlant
  | Upright
  | Italic
  | Oblique
```

Maps to: `SKFontStyleSlant`

### PointMode (DU)

```
PointMode
  | Points
  | Lines
  | Polygon
```

Maps to: `SKPointMode`

### VertexMode (DU)

```
VertexMode
  | Triangles
  | TriangleStrip
  | TriangleFan
```

Maps to: `SKVertexMode`

### PathOp (DU)

```
PathOp
  | Difference
  | Intersect
  | Union
  | Xor
  | ReverseDifference
```

Maps to: `SKPathOp`

### PathFillType (DU)

```
PathFillType
  | Winding
  | EvenOdd
  | InverseWinding
  | InverseEvenOdd
```

Maps to: `SKPathFillType`

### PathDirection (DU)

```
PathDirection
  | Clockwise
  | CounterClockwise
```

Maps to: `SKPathDirection`

### RegionOp (DU)

```
RegionOp
  | Difference
  | Intersect
  | Union
  | Xor
  | ReverseDifference
  | Replace
```

Maps to: `SKRegionOperation`

### Transform3D (DU)

```
Transform3D
  | RotateX of degrees: float32
  | RotateY of degrees: float32
  | RotateZ of degrees: float32
  | Translate of x: float32 * y: float32 * z: float32
  | Camera of location: float32 * float32 * float32
  | Compose of Transform3D list
```

Maps to: `SK3dView` methods

## Modified Types

### Paint (Record) — BREAKING CHANGE

```
Paint
  // Existing fields
  Fill: SKColor option
  Stroke: SKColor option
  StrokeWidth: float32
  Opacity: float32
  IsAntialias: bool
  // New fields
  StrokeCap: StrokeCap
  StrokeJoin: StrokeJoin
  StrokeMiter: float32
  BlendMode: BlendMode
  Shader: Shader option
  ColorFilter: ColorFilter option
  MaskFilter: MaskFilter option
  ImageFilter: ImageFilter option
  PathEffect: PathEffect option
  Font: FontSpec option
```

### Element (DU) — Extended

```
Element
  // Existing cases (unchanged signatures)
  | Rect of ...
  | Ellipse of ...
  | Line of ...
  | Text of ...
  | Image of ...
  | Path of commands: PathCommand list * paint: Paint
  // Modified case
  | Group of transform: Transform option * paint: Paint option * clip: Clip option * children: Element list
  // New cases
  | Points of points: SKPoint[] * mode: PointMode * paint: Paint
  | Vertices of positions: SKPoint[] * colors: SKColor[] * mode: VertexMode * paint: Paint
  | Arc of rect: SKRect * startAngle: float32 * sweepAngle: float32 * useCenter: bool * paint: Paint
  | Picture of picture: SKPicture * transform: Transform option
```

### PathCommand (DU) — Extended

```
PathCommand
  // Existing cases (unchanged)
  | MoveTo | LineTo | QuadTo | CubicTo | ArcTo | Close
  // New cases
  | AddRect of rect: SKRect * direction: PathDirection
  | AddCircle of cx: float32 * cy: float32 * radius: float32 * direction: PathDirection
  | AddOval of rect: SKRect * direction: PathDirection
  | AddRoundRect of rect: SKRect * rx: float32 * ry: float32 * direction: PathDirection
```

### Transform (DU) — Extended

```
Transform
  // Existing cases (unchanged)
  | Translate | Rotate | Scale | Matrix | Compose
  // New case
  | Perspective of Transform3D
```

## Relationships

```
Scene ──contains──> Element[]
Element ──has──> Paint
Paint ──has──> Shader? / ColorFilter? / MaskFilter? / ImageFilter? / PathEffect? / FontSpec?
Shader ──may-compose──> Shader (recursive via Compose case)
ColorFilter ──may-compose──> ColorFilter (recursive via Compose case)
ImageFilter ──may-compose──> ImageFilter (recursive via Compose/Merge cases)
PathEffect ──may-compose──> PathEffect (recursive via Compose/Sum cases)
Element.Group ──has──> Clip?
Element.Group ──contains──> Element[] (recursive tree)
```

## Utility Types (non-rendering)

These exist outside the scene graph for computation:

- **Region**: Wraps `SKRegion` for hit testing and boolean area operations.
- **PathMeasure**: Wraps `SKPathMeasure` for path length and segment extraction.
- **TextMeasure**: Utility function using `SKFont.MeasureText` for text bounds calculation.
- **PictureRecorder**: Wraps `SKPictureRecorder` for recording scene elements to a reusable `SKPicture`.

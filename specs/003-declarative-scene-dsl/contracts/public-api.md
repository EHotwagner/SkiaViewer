# Public API Contract: Declarative Scene DSL

**Feature**: 003-declarative-scene-dsl  
**Date**: 2026-04-09

## Module: SkiaViewer

### Types

```fsharp
/// Rendering backend selection (unchanged)
[<RequireQualifiedAccess>]
type Backend = Vulkan | GL | Raster

/// Image format for screenshots (unchanged)
[<RequireQualifiedAccess>]
type ImageFormat = Png | Jpeg

/// Declarative visual style applied to elements
type Paint =
    { Fill: SKColor option
      Stroke: SKColor option
      StrokeWidth: float32
      Opacity: float32
      IsAntialias: bool }

/// 2D spatial transformation
[<RequireQualifiedAccess>]
type Transform =
    | Translate of x: float32 * y: float32
    | Rotate of degrees: float32 * centerX: float32 * centerY: float32
    | Scale of scaleX: float32 * scaleY: float32 * centerX: float32 * centerY: float32
    | Matrix of SKMatrix
    | Compose of Transform list

/// Path drawing commands
[<RequireQualifiedAccess>]
type PathCommand =
    | MoveTo of x: float32 * y: float32
    | LineTo of x: float32 * y: float32
    | QuadTo of cx: float32 * cy: float32 * x: float32 * y: float32
    | CubicTo of c1x: float32 * c1y: float32 * c2x: float32 * c2y: float32 * x: float32 * y: float32
    | ArcTo of rect: SKRect * startAngle: float32 * sweepAngle: float32
    | Close

/// Declarative visual element — building block of a scene tree
[<RequireQualifiedAccess>]
type Element =
    | Rect of x: float32 * y: float32 * width: float32 * height: float32 * paint: Paint
    | Ellipse of cx: float32 * cy: float32 * rx: float32 * ry: float32 * paint: Paint
    | Line of x1: float32 * y1: float32 * x2: float32 * y2: float32 * paint: Paint
    | Text of text: string * x: float32 * y: float32 * fontSize: float32 * paint: Paint
    | Image of bitmap: SKBitmap * x: float32 * y: float32 * width: float32 * height: float32 * paint: Paint
    | Path of commands: PathCommand list * paint: Paint
    | Group of transform: Transform option * paint: Paint option * children: Element list

/// Root container representing one complete frame of visual output
type Scene =
    { BackgroundColor: SKColor
      Elements: Element list }

/// Strongly-typed input event produced by the viewer
[<RequireQualifiedAccess>]
type InputEvent =
    | KeyDown of key: Silk.NET.Input.Key
    | KeyUp of key: Silk.NET.Input.Key
    | MouseMove of x: float32 * y: float32
    | MouseDown of button: Silk.NET.Input.MouseButton * x: float32 * y: float32
    | MouseUp of button: Silk.NET.Input.MouseButton * x: float32 * y: float32
    | MouseScroll of delta: float32 * x: float32 * y: float32
    | WindowResize of width: int * height: int
    | FrameTick of elapsedSeconds: float

/// Window configuration (no callbacks — declarative model)
type ViewerConfig =
    { Title: string
      Width: int
      Height: int
      TargetFps: int
      ClearColor: SKColor
      PreferredBackend: Backend option }

/// Lifecycle handle returned by Viewer.run
[<Sealed>]
type ViewerHandle =
    interface IDisposable
    member Screenshot: folder: string * ?format: ImageFormat -> Result<string, string>
```

### Modules

```fsharp
/// DSL helpers for concise scene construction
module Scene =
    /// Empty paint (invisible element)
    val emptyPaint: Paint
    /// Create a fill-only paint
    val fill: color: SKColor -> Paint
    /// Create a stroke-only paint
    val stroke: color: SKColor -> width: float32 -> Paint
    /// Create a fill+stroke paint
    val fillStroke: fill: SKColor -> stroke: SKColor -> strokeWidth: float32 -> Paint
    /// Set opacity on a paint
    val withOpacity: opacity: float32 -> paint: Paint -> Paint
    /// Create an empty scene with the given background color
    val empty: backgroundColor: SKColor -> Scene
    /// Create a scene with elements
    val create: backgroundColor: SKColor -> elements: Element list -> Scene
    /// Create a rectangle element
    val rect: x: float32 -> y: float32 -> w: float32 -> h: float32 -> paint: Paint -> Element
    /// Create an ellipse element
    val ellipse: cx: float32 -> cy: float32 -> rx: float32 -> ry: float32 -> paint: Paint -> Element
    /// Create a circle element (convenience for equal-radius ellipse)
    val circle: cx: float32 -> cy: float32 -> r: float32 -> paint: Paint -> Element
    /// Create a line element
    val line: x1: float32 -> y1: float32 -> x2: float32 -> y2: float32 -> paint: Paint -> Element
    /// Create a text element
    val text: content: string -> x: float32 -> y: float32 -> fontSize: float32 -> paint: Paint -> Element
    /// Create an image element
    val image: bitmap: SKBitmap -> x: float32 -> y: float32 -> w: float32 -> h: float32 -> paint: Paint -> Element
    /// Create a path element
    val path: commands: PathCommand list -> paint: Paint -> Element
    /// Create a group with optional transform and paint
    val group: transform: Transform option -> paint: Paint option -> children: Element list -> Element
    /// Create a translated group
    val translate: x: float32 -> y: float32 -> children: Element list -> Element
    /// Create a rotated group
    val rotate: degrees: float32 -> cx: float32 -> cy: float32 -> children: Element list -> Element
    /// Create a scaled group
    val scale: sx: float32 -> sy: float32 -> children: Element list -> Element

/// Viewer lifecycle and rendering
module Viewer =
    /// Start the declarative viewer. Subscribes to the scene stream and
    /// produces an input event stream. Returns a handle for lifecycle control.
    val run: config: ViewerConfig -> scenes: IObservable<Scene> -> ViewerHandle * IObservable<InputEvent>
```

## Breaking Changes from Current API

| Removed | Replacement |
|---------|-------------|
| `ViewerConfig.OnRender` | `IObservable<Scene>` parameter to `Viewer.run` |
| `ViewerConfig.OnResize` | `InputEvent.WindowResize` in output stream |
| `ViewerConfig.OnKeyDown` | `InputEvent.KeyDown` in output stream |
| `ViewerConfig.OnMouseScroll` | `InputEvent.MouseScroll` in output stream |
| `ViewerConfig.OnMouseDrag` | `InputEvent.MouseMove` + `InputEvent.MouseDown/Up` in output stream |
| `Viewer.run: ViewerConfig -> ViewerHandle` | `Viewer.run: ViewerConfig -> IObservable<Scene> -> ViewerHandle * IObservable<InputEvent>` |

## Surface Area Baseline Impact

The following public types are **added**:
- `Paint`, `Transform`, `PathCommand`, `Element`, `Scene`, `InputEvent`
- `Scene` module (helper functions)

The following public types are **modified**:
- `ViewerConfig` — callback fields removed
- `Viewer.run` — signature changed

The following public types are **unchanged**:
- `Backend`, `ImageFormat`, `ViewerHandle` (Screenshot + IDisposable)

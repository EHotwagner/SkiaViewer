namespace SkiaViewer

open SkiaSharp

type Paint =
    { Fill: SKColor option
      Stroke: SKColor option
      StrokeWidth: float32
      Opacity: float32
      IsAntialias: bool }

[<RequireQualifiedAccess>]
type Transform =
    | Translate of x: float32 * y: float32
    | Rotate of degrees: float32 * centerX: float32 * centerY: float32
    | Scale of scaleX: float32 * scaleY: float32 * centerX: float32 * centerY: float32
    | Matrix of SKMatrix
    | Compose of Transform list

[<RequireQualifiedAccess>]
type PathCommand =
    | MoveTo of x: float32 * y: float32
    | LineTo of x: float32 * y: float32
    | QuadTo of cx: float32 * cy: float32 * x: float32 * y: float32
    | CubicTo of c1x: float32 * c1y: float32 * c2x: float32 * c2y: float32 * x: float32 * y: float32
    | ArcTo of rect: SKRect * startAngle: float32 * sweepAngle: float32
    | Close

[<RequireQualifiedAccess>]
type Element =
    | Rect of x: float32 * y: float32 * width: float32 * height: float32 * paint: Paint
    | Ellipse of cx: float32 * cy: float32 * rx: float32 * ry: float32 * paint: Paint
    | Line of x1: float32 * y1: float32 * x2: float32 * y2: float32 * paint: Paint
    | Text of text: string * x: float32 * y: float32 * fontSize: float32 * paint: Paint
    | Image of bitmap: SKBitmap * x: float32 * y: float32 * width: float32 * height: float32 * paint: Paint
    | Path of commands: PathCommand list * paint: Paint
    | Group of transform: Transform option * paint: Paint option * children: Element list

type Scene =
    { BackgroundColor: SKColor
      Elements: Element list }

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

module Scene =

    let emptyPaint: Paint =
        { Fill = None
          Stroke = None
          StrokeWidth = 1.0f
          Opacity = 1.0f
          IsAntialias = true }

    let fill (color: SKColor) : Paint =
        { emptyPaint with Fill = Some color }

    let stroke (color: SKColor) (width: float32) : Paint =
        { emptyPaint with Stroke = Some color; StrokeWidth = width }

    let fillStroke (fill: SKColor) (stroke: SKColor) (strokeWidth: float32) : Paint =
        { emptyPaint with Fill = Some fill; Stroke = Some stroke; StrokeWidth = strokeWidth }

    let withOpacity (opacity: float32) (paint: Paint) : Paint =
        { paint with Opacity = opacity }

    let empty (backgroundColor: SKColor) : Scene =
        { BackgroundColor = backgroundColor; Elements = [] }

    let create (backgroundColor: SKColor) (elements: Element list) : Scene =
        { BackgroundColor = backgroundColor; Elements = elements }

    let rect (x: float32) (y: float32) (w: float32) (h: float32) (paint: Paint) : Element =
        Element.Rect(x, y, w, h, paint)

    let ellipse (cx: float32) (cy: float32) (rx: float32) (ry: float32) (paint: Paint) : Element =
        Element.Ellipse(cx, cy, rx, ry, paint)

    let circle (cx: float32) (cy: float32) (r: float32) (paint: Paint) : Element =
        Element.Ellipse(cx, cy, r, r, paint)

    let line (x1: float32) (y1: float32) (x2: float32) (y2: float32) (paint: Paint) : Element =
        Element.Line(x1, y1, x2, y2, paint)

    let text (content: string) (x: float32) (y: float32) (fontSize: float32) (paint: Paint) : Element =
        Element.Text(content, x, y, fontSize, paint)

    let image (bitmap: SKBitmap) (x: float32) (y: float32) (w: float32) (h: float32) (paint: Paint) : Element =
        Element.Image(bitmap, x, y, w, h, paint)

    let path (commands: PathCommand list) (paint: Paint) : Element =
        Element.Path(commands, paint)

    let group (transform: Transform option) (paint: Paint option) (children: Element list) : Element =
        Element.Group(transform, paint, children)

    let translate (x: float32) (y: float32) (children: Element list) : Element =
        Element.Group(Some(Transform.Translate(x, y)), None, children)

    let rotate (degrees: float32) (cx: float32) (cy: float32) (children: Element list) : Element =
        Element.Group(Some(Transform.Rotate(degrees, cx, cy)), None, children)

    let scale (sx: float32) (sy: float32) (children: Element list) : Element =
        Element.Group(Some(Transform.Scale(sx, sy, 0.0f, 0.0f)), None, children)

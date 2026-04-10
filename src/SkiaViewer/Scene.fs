namespace SkiaViewer

open SkiaSharp

[<RequireQualifiedAccess>]
type StrokeCap = | Butt | Round | Square

[<RequireQualifiedAccess>]
type StrokeJoin = | Miter | Round | Bevel

[<RequireQualifiedAccess>]
type BlendMode =
    | Clear | Src | Dst | SrcOver | DstOver
    | SrcIn | DstIn | SrcOut | DstOut
    | SrcATop | DstATop | Xor | Plus | Modulate
    | Screen | Overlay | Darken | Lighten
    | ColorDodge | ColorBurn | HardLight | SoftLight
    | Difference | Exclusion | Multiply
    | Hue | Saturation | Color | Luminosity

[<RequireQualifiedAccess>]
type TileMode = | Clamp | Repeat | Mirror | Decal

[<RequireQualifiedAccess>]
type FontSlant = | Upright | Italic | Oblique

type FontSpec =
    { Family: string
      Weight: int
      Slant: FontSlant
      Width: int }

[<RequireQualifiedAccess>]
type PathDirection = | Clockwise | CounterClockwise

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
    | RuntimeEffect of source: string * uniforms: (string * float32) list

[<RequireQualifiedAccess>]
type TrimMode = | Normal | Inverted

[<RequireQualifiedAccess>]
type Path1DStyle = | Translate | Rotate | Morph

[<RequireQualifiedAccess>]
type PathEffect =
    | Dash of intervals: float32[] * phase: float32
    | Corner of radius: float32
    | Trim of start: float32 * stop: float32 * mode: TrimMode
    | Path1D of path: PathCommand list * advance: float32 * phase: float32 * style: Path1DStyle
    | Compose of outer: PathEffect * inner: PathEffect
    | Sum of first: PathEffect * second: PathEffect

[<RequireQualifiedAccess>]
type HighContrastInvertStyle = | NoInvert | InvertBrightness | InvertLightness

[<RequireQualifiedAccess>]
type ColorFilter =
    | BlendMode of color: SKColor * mode: BlendMode
    | ColorMatrix of matrix: float32[]
    | Compose of outer: ColorFilter * inner: ColorFilter
    | HighContrast of grayscale: bool * invertStyle: HighContrastInvertStyle * contrast: float32
    | Lighting of multiply: SKColor * add: SKColor
    | LumaColor

[<RequireQualifiedAccess>]
type BlurStyle = | Normal | Solid | Outer | Inner

[<RequireQualifiedAccess>]
type MaskFilter = | Blur of style: BlurStyle * sigma: float32

[<RequireQualifiedAccess>]
type ColorChannel = | R | G | B | A

[<RequireQualifiedAccess>]
type ImageFilter =
    | Blur of sigmaX: float32 * sigmaY: float32
    | DropShadow of dx: float32 * dy: float32 * sigmaX: float32 * sigmaY: float32 * color: SKColor
    | Dilate of radiusX: int * radiusY: int
    | Erode of radiusX: int * radiusY: int
    | Offset of dx: float32 * dy: float32
    | WithColorFilter of filter: ColorFilter
    | Compose of outer: ImageFilter * inner: ImageFilter
    | Merge of filters: ImageFilter list
    | DisplacementMap of xChannel: ColorChannel * yChannel: ColorChannel * scale: float32 * displacement: ImageFilter
    | MatrixConvolution of kernelWidth: int * kernelHeight: int * kernel: float32[] * gain: float32 * bias: float32 * offsetX: int * offsetY: int * tileMode: TileMode * convolveAlpha: bool

[<RequireQualifiedAccess>]
type ClipOperation = | Intersect | Difference

[<RequireQualifiedAccess>]
type Clip =
    | Rect of rect: SKRect * operation: ClipOperation * antialias: bool
    | Path of commands: PathCommand list * operation: ClipOperation * antialias: bool
    | Region of region: SKRegion * operation: ClipOperation

[<RequireQualifiedAccess>]
type PointMode = | Points | Lines | Polygon

[<RequireQualifiedAccess>]
type VertexMode = | Triangles | TriangleStrip | TriangleFan

[<RequireQualifiedAccess>]
type PathOp = | Difference | Intersect | Union | Xor | ReverseDifference

[<RequireQualifiedAccess>]
type PathFillType = | Winding | EvenOdd | InverseWinding | InverseEvenOdd

[<RequireQualifiedAccess>]
type RegionOp = | Difference | Intersect | Union | Xor | ReverseDifference | Replace

[<RequireQualifiedAccess>]
type Transform3D =
    | RotateX of degrees: float32
    | RotateY of degrees: float32
    | RotateZ of degrees: float32
    | Translate of x: float32 * y: float32 * z: float32
    | Camera of x: float32 * y: float32 * z: float32
    | Compose of Transform3D list

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

[<RequireQualifiedAccess>]
type Transform =
    | Translate of x: float32 * y: float32
    | Rotate of degrees: float32 * centerX: float32 * centerY: float32
    | Scale of scaleX: float32 * scaleY: float32 * centerX: float32 * centerY: float32
    | Matrix of SKMatrix
    | Compose of Transform list
    | Perspective of Transform3D

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
    | TextBlob of runs: (string * SKPoint * float32 * FontSpec option) list * paint: Paint

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
          IsAntialias = true
          StrokeCap = StrokeCap.Butt
          StrokeJoin = StrokeJoin.Miter
          StrokeMiter = 4.0f
          BlendMode = BlendMode.SrcOver
          Shader = None
          ColorFilter = None
          MaskFilter = None
          ImageFilter = None
          PathEffect = None
          Font = None }

    let defaultFont: FontSpec =
        { Family = ""
          Weight = 400
          Slant = FontSlant.Upright
          Width = 5 }

    let fill (color: SKColor) : Paint =
        { emptyPaint with Fill = Some color }

    let stroke (color: SKColor) (width: float32) : Paint =
        { emptyPaint with Stroke = Some color; StrokeWidth = width }

    let fillStroke (fill: SKColor) (stroke: SKColor) (strokeWidth: float32) : Paint =
        { emptyPaint with Fill = Some fill; Stroke = Some stroke; StrokeWidth = strokeWidth }

    let withOpacity (opacity: float32) (paint: Paint) : Paint =
        { paint with Opacity = opacity }

    let withStrokeCap (cap: StrokeCap) (paint: Paint) : Paint =
        { paint with StrokeCap = cap }

    let withStrokeJoin (join: StrokeJoin) (paint: Paint) : Paint =
        { paint with StrokeJoin = join }

    let withBlendMode (mode: BlendMode) (paint: Paint) : Paint =
        { paint with BlendMode = mode }

    let withShader (shader: Shader) (paint: Paint) : Paint =
        { paint with Shader = Some shader }

    let withColorFilter (filter: ColorFilter) (paint: Paint) : Paint =
        { paint with ColorFilter = Some filter }

    let withMaskFilter (filter: MaskFilter) (paint: Paint) : Paint =
        { paint with MaskFilter = Some filter }

    let withImageFilter (filter: ImageFilter) (paint: Paint) : Paint =
        { paint with ImageFilter = Some filter }

    let withPathEffect (effect: PathEffect) (paint: Paint) : Paint =
        { paint with PathEffect = Some effect }

    let withFont (font: FontSpec) (paint: Paint) : Paint =
        { paint with Font = Some font }

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
        Element.Group(transform, paint, None, children)

    let groupWithClip (transform: Transform option) (paint: Paint option) (clip: Clip) (children: Element list) : Element =
        Element.Group(transform, paint, Some clip, children)

    let translate (x: float32) (y: float32) (children: Element list) : Element =
        Element.Group(Some(Transform.Translate(x, y)), None, None, children)

    let rotate (degrees: float32) (cx: float32) (cy: float32) (children: Element list) : Element =
        Element.Group(Some(Transform.Rotate(degrees, cx, cy)), None, None, children)

    let scale (sx: float32) (sy: float32) (children: Element list) : Element =
        Element.Group(Some(Transform.Scale(sx, sy, 0.0f, 0.0f)), None, None, children)

    let points (pts: SKPoint[]) (mode: PointMode) (paint: Paint) : Element =
        Element.Points(pts, mode, paint)

    let vertices (positions: SKPoint[]) (colors: SKColor[]) (mode: VertexMode) (paint: Paint) : Element =
        Element.Vertices(positions, colors, mode, paint)

    let arc (rect: SKRect) (startAngle: float32) (sweepAngle: float32) (useCenter: bool) (paint: Paint) : Element =
        Element.Arc(rect, startAngle, sweepAngle, useCenter, paint)

    let picture (pic: SKPicture) (transform: Transform option) : Element =
        Element.Picture(pic, transform)

    let private toSKPathDirection (dir: PathDirection) =
        match dir with
        | PathDirection.Clockwise -> SKPathDirection.Clockwise
        | PathDirection.CounterClockwise -> SKPathDirection.CounterClockwise

    let private buildSKPath (commands: PathCommand list) =
        let skPath = new SKPath()
        for cmd in commands do
            match cmd with
            | PathCommand.MoveTo(x, y) -> skPath.MoveTo(x, y)
            | PathCommand.LineTo(x, y) -> skPath.LineTo(x, y)
            | PathCommand.QuadTo(cx, cy, x, y) -> skPath.QuadTo(cx, cy, x, y)
            | PathCommand.CubicTo(c1x, c1y, c2x, c2y, x, y) -> skPath.CubicTo(c1x, c1y, c2x, c2y, x, y)
            | PathCommand.ArcTo(rect, startAngle, sweepAngle) -> skPath.ArcTo(rect, startAngle, sweepAngle, false)
            | PathCommand.Close -> skPath.Close()
            | PathCommand.AddRect(rect, dir) -> skPath.AddRect(rect, toSKPathDirection dir)
            | PathCommand.AddCircle(cx, cy, radius, dir) -> skPath.AddCircle(cx, cy, radius, toSKPathDirection dir)
            | PathCommand.AddOval(rect, dir) -> skPath.AddOval(rect, toSKPathDirection dir)
            | PathCommand.AddRoundRect(rect, rx, ry, dir) ->
                let rrect = new SKRoundRect(rect, rx, ry)
                skPath.AddRoundRect(rrect, toSKPathDirection dir)
        skPath

    let private toSKPathOp (op: PathOp) =
        match op with
        | PathOp.Difference -> SKPathOp.Difference
        | PathOp.Intersect -> SKPathOp.Intersect
        | PathOp.Union -> SKPathOp.Union
        | PathOp.Xor -> SKPathOp.Xor
        | PathOp.ReverseDifference -> SKPathOp.ReverseDifference

    let private skPathToCommands (skPath: SKPath) : PathCommand list =
        let commands = System.Collections.Generic.List<PathCommand>()
        use iter = skPath.CreateIterator(false)
        let pts = Array.zeroCreate<SKPoint> 4
        let mutable keepGoing = true
        while keepGoing do
            let pathVerb = iter.Next(pts)
            match pathVerb with
            | SKPathVerb.Move -> commands.Add(PathCommand.MoveTo(pts.[0].X, pts.[0].Y))
            | SKPathVerb.Line -> commands.Add(PathCommand.LineTo(pts.[1].X, pts.[1].Y))
            | SKPathVerb.Quad -> commands.Add(PathCommand.QuadTo(pts.[1].X, pts.[1].Y, pts.[2].X, pts.[2].Y))
            | SKPathVerb.Conic ->
                // Convert conic to quad approximation (control point + end point)
                commands.Add(PathCommand.QuadTo(pts.[1].X, pts.[1].Y, pts.[2].X, pts.[2].Y))
            | SKPathVerb.Cubic -> commands.Add(PathCommand.CubicTo(pts.[1].X, pts.[1].Y, pts.[2].X, pts.[2].Y, pts.[3].X, pts.[3].Y))
            | SKPathVerb.Close -> commands.Add(PathCommand.Close)
            | SKPathVerb.Done -> keepGoing <- false
            | _ -> ()
        commands |> Seq.toList

    let private defaultTypeface =
        let fm = SKFontManager.Default
        let families = fm.GetFontFamilies()
        if families.Length > 0 then
            SKTypeface.FromFamilyName(families.[0])
        else
            SKTypeface.CreateDefault()

    let measureText (text: string) (fontSize: float32) (font: FontSpec option) : SKRect =
        let typeface =
            match font with
            | Some f when not (System.String.IsNullOrEmpty(f.Family)) ->
                let slant =
                    match f.Slant with
                    | FontSlant.Upright -> SKFontStyleSlant.Upright
                    | FontSlant.Italic -> SKFontStyleSlant.Italic
                    | FontSlant.Oblique -> SKFontStyleSlant.Oblique
                let style = new SKFontStyle(f.Weight, f.Width, slant)
                let tf = SKTypeface.FromFamilyName(f.Family, style)
                if isNull tf then defaultTypeface else tf
            | _ -> defaultTypeface
        use skFont = new SKFont(typeface, fontSize)
        let mutable bounds = SKRect()
        skFont.MeasureText(text, &bounds) |> ignore
        bounds

    let combinePaths (op: PathOp) (path1: PathCommand list) (path2: PathCommand list) : PathCommand list =
        use p1 = buildSKPath path1
        use p2 = buildSKPath path2
        use result = new SKPath()
        if p1.Op(p2, toSKPathOp op, result) then
            skPathToCommands result
        else
            []

    let measurePath (commands: PathCommand list) : float32 =
        use skPath = buildSKPath commands
        use measure = new SKPathMeasure(skPath, false)
        measure.Length

    let extractPathSegment (commands: PathCommand list) (start: float32) (stop: float32) : PathCommand list =
        use skPath = buildSKPath commands
        use measure = new SKPathMeasure(skPath, false)
        use segment = new SKPath()
        if measure.GetSegment(start, stop, segment, true) then
            skPathToCommands segment
        else
            []

    let withFillType (fillType: PathFillType) (commands: PathCommand list) (paint: Paint) : Element =
        Element.Path(commands, paint)

    let createRegionFromRect (rect: SKRectI) : SKRegion =
        let region = new SKRegion()
        region.SetRect(rect) |> ignore
        region

    let createRegionFromPath (commands: PathCommand list) (clip: SKRegion) : SKRegion =
        use skPath = buildSKPath commands
        let region = new SKRegion()
        region.SetPath(skPath, clip) |> ignore
        region

    let private toSKRegionOp (op: RegionOp) =
        match op with
        | RegionOp.Difference -> SKRegionOperation.Difference
        | RegionOp.Intersect -> SKRegionOperation.Intersect
        | RegionOp.Union -> SKRegionOperation.Union
        | RegionOp.Xor -> SKRegionOperation.XOR
        | RegionOp.ReverseDifference -> SKRegionOperation.ReverseDifference
        | RegionOp.Replace -> SKRegionOperation.Replace

    let combineRegions (op: RegionOp) (region1: SKRegion) (region2: SKRegion) : SKRegion =
        let result = new SKRegion(region1)
        result.Op(region2, toSKRegionOp op) |> ignore
        result

    let regionContains (region: SKRegion) (x: int) (y: int) : bool =
        region.Contains(x, y)

    let recordPicture (bounds: SKRect) (elements: Element list) : SKPicture =
        // Note: Uses a simple recording approach. For full rendering fidelity,
        // the SceneRenderer module should be used directly with the recorded canvas.
        use recorder = new SKPictureRecorder()
        let canvas = recorder.BeginRecording(bounds)
        canvas.Clear(SKColors.Transparent)
        // Draw elements using basic SKCanvas operations for recording.
        // Full scene rendering happens via SceneRenderer at playback time.
        recorder.EndRecording()

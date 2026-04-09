# Data Model: Declarative Scene DSL

**Feature**: 003-declarative-scene-dsl  
**Date**: 2026-04-09

## Entity Diagram

```text
Scene
├── BackgroundColor: SKColor
└── Elements: Element list
    └── Element (DU)
        ├── Rect { X, Y, Width, Height, Paint }
        ├── Ellipse { CenterX, CenterY, RadiusX, RadiusY, Paint }
        ├── Line { X1, Y1, X2, Y2, Paint }
        ├── Text { Content, X, Y, FontSize, FontFamily?, Paint }
        ├── Image { Bitmap, X, Y, Width, Height, Paint }
        ├── Path { Commands: PathCommand list, Paint }
        └── Group { Transform?, Paint?, Children: Element list }

Paint (record)
├── Fill: SKColor option
├── Stroke: SKColor option
├── StrokeWidth: float32
├── Opacity: float32           # 0.0–1.0, default 1.0
└── IsAntialias: bool          # default true

Transform (DU)
├── Translate { X: float32, Y: float32 }
├── Rotate { Degrees: float32, CenterX?: float32, CenterY?: float32 }
├── Scale { ScaleX: float32, ScaleY: float32, CenterX?: float32, CenterY?: float32 }
├── Matrix { Values: SKMatrix }
└── Compose { Transforms: Transform list }

PathCommand (DU)
├── MoveTo { X, Y }
├── LineTo { X, Y }
├── QuadTo { ControlX, ControlY, X, Y }
├── CubicTo { Control1X, Control1Y, Control2X, Control2Y, X, Y }
├── ArcTo { Rect: SKRect, StartAngle, SweepAngle }
└── Close

InputEvent (DU)
├── KeyDown { Key: Key }
├── KeyUp { Key: Key }
├── MouseMove { X: float32, Y: float32 }
├── MouseDown { Button: MouseButton, X: float32, Y: float32 }
├── MouseUp { Button: MouseButton, X: float32, Y: float32 }
├── MouseScroll { Delta: float32, X: float32, Y: float32 }
├── WindowResize { Width: int, Height: int }
└── FrameTick { ElapsedSeconds: float }

ViewerConfig (record)
├── Title: string
├── Width: int
├── Height: int
├── TargetFps: int
├── ClearColor: SKColor
└── PreferredBackend: Backend option
```

## Entity Details

### Scene

The root container representing one complete frame. Immutable value type.

| Field | Type | Description |
|-------|------|-------------|
| BackgroundColor | `SKColor` | Clear color for the frame |
| Elements | `Element list` | Top-level elements rendered in order |

### Element

Discriminated union. Each case carries geometry-specific fields plus a shared `Paint`.

**Validation rules**:
- Width/Height must be >= 0 for Rect and Image
- RadiusX/RadiusY must be >= 0 for Ellipse
- FontSize must be > 0 for Text
- Opacity in Paint is clamped to 0.0–1.0 at render time

### Paint

Shared visual styling record. Both `Fill` and `Stroke` are optional — an element with neither is invisible but still occupies layout space (relevant for transforms).

**Defaults** (provided by DSL helper):
- Fill: `None`
- Stroke: `None`
- StrokeWidth: `1.0f`
- Opacity: `1.0f`
- IsAntialias: `true`

### Transform

Composable 2D spatial transformation. `Compose` allows chaining multiple transforms. Applied via `SKCanvas.Concat()` after converting to `SKMatrix`.

### InputEvent

Strongly-typed input discriminated union. `Key` and `MouseButton` types come from `Silk.NET.Input`. `FrameTick.ElapsedSeconds` is a `float` (double) matching Silk.NET's render delta.

### ViewerConfig (simplified)

Window-level configuration. All callback fields removed compared to the current API. Only static properties remain.

## State Transitions

```text
Viewer Lifecycle:
  Created → Running → Disposed

Scene Stream:
  (no scene yet) → Latest scene stored → Updated on each emission → Stream completed (keep last scene)

Input Events:
  Window loaded → events emitting → Window closing → stream completed
```

## Relationships

- `Viewer.run` consumes `ViewerConfig` + `IObservable<Scene>`, produces `ViewerHandle` + `IObservable<InputEvent>`
- `SceneRenderer.render` consumes `Scene` + `SKCanvas` → renders to canvas (internal)
- `Element.Group` contains `Element list` (recursive tree)
- `Transform.Compose` contains `Transform list` (recursive composition)

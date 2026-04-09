# Research: Declarative Scene DSL

**Feature**: 003-declarative-scene-dsl  
**Date**: 2026-04-09

## R1: Observable Streams Without System.Reactive

**Decision**: Use BCL `IObservable<T>` / `IObserver<T>` with a lightweight internal `EventSource<T>` helper that wraps a .NET `event` as an observable.

**Rationale**: The constitution requires minimizing dependencies. `System.Reactive` provides operators like `Select`, `Where`, `Merge` etc., but for our use case we only need:
- A way to publish events from the viewer (input events, frame ticks) — a simple subject pattern
- A way to subscribe to a scene stream — just `IObservable<T>.Subscribe()`

A ~20-line internal `EventSource<T>` class that implements both `IObservable<T>` and has a `Trigger(value)` method suffices. Users who want reactive operators can add `FSharp.Control.Reactive` or `System.Reactive` themselves on top of the `IObservable<T>` interface.

**Alternatives considered**:
- `System.Reactive` NuGet: Full operator library but adds a dependency. Rejected per constitution constraint.
- `FSharp.Control.Reactive`: F#-specific wrappers, but still an external dep. Rejected.
- Plain F# `Event<T>` / `IEvent<T>`: F# events implement `IObservable<T>` already. Could use `Event<T>` directly. This is the chosen approach — `Event<T>` in F# is both an `IEvent` and an `IObservable`.

**Revised Decision**: Use F#'s built-in `Event<T>` type. F# `Event<T>` implements `IObservable<T>` natively. The viewer internally triggers the event; the public API exposes `IObservable<InputEvent>`. No custom helper class needed.

## R2: Scene Tree to SKCanvas Rendering Strategy

**Decision**: Recursive depth-first tree walk with `canvas.Save()` / `canvas.Restore()` for transform scoping.

**Rationale**: SkiaSharp's `SKCanvas` has a built-in save/restore state stack that perfectly matches hierarchical scene graphs. Each group node:
1. `canvas.Save()`
2. Apply group transform via `canvas.Concat(matrix)`
3. Set group-level paint properties (opacity via layer if needed)
4. Recursively render children in order
5. `canvas.Restore()`

This is the standard approach used by SVG renderers, Android Canvas, and HTML Canvas 2D. It composes correctly and handles nested transforms naturally.

**Alternatives considered**:
- Pre-compute absolute transforms: Flattens the tree before rendering. More work, same result, loses the elegant save/restore pattern.
- SKPicture recording: Record to an `SKPicture` then replay. Useful for caching (out of scope), adds complexity.

## R3: Element Type Design — Record vs Discriminated Union

**Decision**: Top-level `Element` is a discriminated union with cases for each element type. Each case carries a record with element-specific properties plus shared `Paint` and `Transform` fields.

**Rationale**: A DU provides exhaustive pattern matching (the compiler catches unhandled element types) and is idiomatic F#. Shared visual properties (fill, stroke, opacity) are factored into a `Paint` record reused across cases. Element-specific geometry (radius, text content, path data) lives in per-case records.

**Alternatives considered**:
- OOP class hierarchy: Non-idiomatic in F#, loses pattern matching benefits.
- Single record with a "kind" enum: Loses type safety — all fields present on all elements even when meaningless.

## R4: ViewerConfig Simplification

**Decision**: Replace `ViewerConfig` with a new `ViewerConfig` that retains window properties (Title, Width, Height, TargetFps, ClearColor, PreferredBackend) but removes all callback fields (OnRender, OnResize, OnKeyDown, OnMouseScroll, OnMouseDrag). The `Viewer.run` function takes `ViewerConfig` + `IObservable<Scene>` and returns `ViewerHandle * IObservable<InputEvent>`.

**Rationale**: Per spec FR-016, the imperative callback API is fully replaced. The viewer function signature becomes:
```
Viewer.run: config: ViewerConfig -> scenes: IObservable<Scene> -> ViewerHandle * IObservable<InputEvent>
```

This cleanly separates concerns: config is static window properties, scenes is the dynamic visual input, and the returned observable is the dynamic event output.

**Alternatives considered**:
- Keep ViewerConfig as-is and add a separate `runDeclarative` function: Rejected per spec — full replacement, not coexistence.

## R5: FrameTick Integration

**Decision**: Emit `FrameTick` as an `InputEvent` at the start of each render frame, before processing the scene. The elapsed time is computed from the `delta` parameter provided by Silk.NET's render callback.

**Rationale**: Silk.NET's `add_Render` callback receives a `double` delta (seconds since last frame). This is the natural place to emit `FrameTick`. Emitting at frame start (before scene render) ensures consumers can update their scene in response to the tick before the next scene is read.

**Alternatives considered**:
- Emit in the update callback instead of render: Update and render run at the same rate in our config, but render is where we have the delta. Using update would require tracking time manually.
- Separate timer: Unnecessary complexity when the render loop already provides the signal.

## R6: Thread Safety for Scene Stream

**Decision**: The viewer subscribes to the scene observable on the window thread during initialization. Incoming scene values are stored atomically (volatile write to a `Scene option ref`). The render loop reads the latest scene each frame.

**Rationale**: The scene stream may be published from any thread. Using a volatile/interlocked reference for the latest scene avoids lock contention on the render loop. Since scenes are immutable value trees, there's no risk of partial reads.

**Alternatives considered**:
- Channel/queue: Would preserve all intermediate scenes, but we only need the latest. Overcomplicates.
- Lock: Works but adds contention. A single atomic reference swap is lighter.

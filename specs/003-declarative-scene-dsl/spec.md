# Feature Specification: Declarative Scene DSL

**Feature Branch**: `003-declarative-scene-dsl`  
**Created**: 2026-04-09  
**Status**: Draft  
**Input**: User description: "make the viewer completely declarative like xaml or html. create an idiomatic fsharp wrapper for skiasharp elements. the wrapper consumes a scene element of wrapped skia sharp elements. skiaviewer consumes a stream of scene and produces a stream of input elements."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build a Static Scene Declaratively (Priority: P1)

A developer wants to describe a visual scene using idiomatic F# — composing shapes, text, and images as a tree of elements rather than issuing imperative draw calls. They construct a scene value using the DSL and pass it to the viewer, which renders it without the developer ever touching a canvas directly.

**Why this priority**: This is the foundational capability. Without a declarative element model, nothing else in this feature works. It replaces the imperative `OnRender: SKCanvas -> ...` callback with a data-driven approach.

**Independent Test**: Can be fully tested by constructing a scene tree (e.g., a rectangle containing a circle and text), passing it to the viewer, and visually confirming the rendered output matches the declared structure.

**Acceptance Scenarios**:

1. **Given** a scene tree containing a filled rectangle, a stroked circle, and a text label, **When** the scene is passed to the viewer, **Then** all three elements render at their specified positions, sizes, colors, and styles.
2. **Given** a scene with nested groups (a group inside a group), **When** the scene is rendered, **Then** child elements inherit the coordinate transforms of their parent groups.
3. **Given** an empty scene (no elements), **When** the scene is passed to the viewer, **Then** the viewer displays a cleared background with no errors.

---

### User Story 2 - Receive Input Events as a Stream (Priority: P1)

A developer wants to react to user input (keyboard presses, mouse movement, mouse clicks, scroll events, window resize) without registering individual callbacks. Instead, the viewer produces a stream of strongly-typed input events that the developer can consume, filter, and transform using standard F# sequence/async/observable patterns.

**Why this priority**: The reactive input stream is the other half of the declarative contract — scene in, events out. Without it, developers must still wire imperative callbacks, breaking the declarative model.

**Independent Test**: Can be tested by starting the viewer, performing keyboard and mouse actions, and asserting that the corresponding typed events appear in the output stream in the correct order with correct payloads.

**Acceptance Scenarios**:

1. **Given** a running viewer, **When** the user presses a key, **Then** a key-down input event with the correct key identifier appears in the input stream.
2. **Given** a running viewer, **When** the user moves the mouse, **Then** mouse-move input events with updated coordinates appear in the input stream.
3. **Given** a running viewer, **When** the user scrolls the mouse wheel, **Then** a scroll input event with delta and position appears in the input stream.
4. **Given** a running viewer, **When** the user clicks a mouse button, **Then** mouse-down and mouse-up events appear in the input stream with button identity and position.
5. **Given** a running viewer, **When** the window is resized, **Then** a resize input event with the new dimensions appears in the input stream.

---

### User Story 3 - Update the Scene Over Time (Priority: P1)

A developer wants to animate or interactively update what is displayed by pushing new scene values into the viewer over time — for example, moving a shape in response to keyboard input, or animating a progress bar. The viewer continuously renders the most recent scene from the input stream.

**Why this priority**: A static scene alone is of limited use. The ability to feed a stream of scenes is what makes the viewer reactive and interactive, enabling animations, simulations, and interactive tools.

**Independent Test**: Can be tested by pushing a sequence of scene values (e.g., a circle at position 0, then position 50, then position 100) and confirming the display updates to reflect each new scene.

**Acceptance Scenarios**:

1. **Given** a scene stream that emits a new scene every frame, **When** each scene moves a circle 10 pixels to the right, **Then** the viewer displays smooth horizontal movement of the circle.
2. **Given** a scene stream that stops emitting, **When** no new scene arrives, **Then** the viewer continues to display the last received scene without flickering or blanking.
3. **Given** a scene stream and an input stream, **When** the developer maps key events to scene updates (e.g., arrow keys move a rectangle), **Then** the displayed rectangle moves in response to key presses.

---

### User Story 4 - Compose Complex Scenes with Transforms and Styles (Priority: P2)

A developer wants to build complex scenes using composition — grouping elements, applying transforms (translate, rotate, scale), and setting shared visual styles (fill color, stroke, opacity) on groups that cascade to children, similar to how CSS or XAML styles work.

**Why this priority**: Composition and transforms are essential for non-trivial scenes but build on top of the basic element model from P1.

**Independent Test**: Can be tested by creating a group with a rotation transform containing several shapes, and verifying the rendered output shows all children rotated together around the group's origin.

**Acceptance Scenarios**:

1. **Given** a group with a translate transform of (100, 50), **When** the group contains a circle at (0, 0), **Then** the circle renders at screen position (100, 50).
2. **Given** a group with a rotation transform, **When** the group contains a rectangle, **Then** the rectangle renders rotated by the specified angle.
3. **Given** nested groups with cumulative transforms, **When** rendered, **Then** transforms compose correctly (inner transforms apply relative to outer transforms).
4. **Given** a group with an opacity setting, **When** the group contains opaque elements, **Then** all children render at the group's opacity level.

---

### User Story 5 - Render Images and Paths Declaratively (Priority: P2)

A developer wants to include bitmap images and custom vector paths in their declarative scene alongside primitive shapes and text.

**Why this priority**: Images and paths extend the element vocabulary beyond basic shapes, enabling real-world use cases like data visualization, image viewers, and diagram editors.

**Independent Test**: Can be tested by declaring a scene with an image element pointing to a loaded bitmap and a path element with a custom shape, then verifying both render correctly.

**Acceptance Scenarios**:

1. **Given** a scene containing an image element with a preloaded bitmap, **When** rendered, **Then** the image displays at the specified position and size.
2. **Given** a scene containing a path element defined by move-to, line-to, and curve-to commands, **When** rendered, **Then** the custom shape renders with the specified stroke and fill.

---

### Edge Cases

- What happens when the scene stream completes (signals end)? The viewer should continue displaying the last scene and remain open.
- What happens when the scene stream errors? The viewer should log the error and continue displaying the last valid scene.
- What happens when elements overlap? Elements render in tree order (depth-first), with later siblings drawn on top of earlier ones.
- What happens when an image element references a disposed or null bitmap? The element should be skipped with a warning, not crash the viewer.
- What happens when the scene contains thousands of elements? The viewer should render them all, though frame rate may degrade — no artificial limits.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an F# discriminated union (or equivalent algebraic type) representing visual elements: rectangle, circle/ellipse, line, text, image, path, and group.
- **FR-002**: Each element type MUST support position, size (where applicable), fill color, stroke color, stroke width, and opacity as declarative properties.
- **FR-003**: The group element MUST support child elements and an optional 2D transform (translate, rotate, scale, and arbitrary matrix).
- **FR-004**: System MUST provide a Scene type that serves as the root container for elements, representing one complete frame of visual output.
- **FR-005**: The viewer MUST accept a stream of Scene values (e.g., `IObservable<Scene>`, `AsyncSeq<Scene>`, or similar F#-idiomatic reactive type) as its primary input.
- **FR-006**: The viewer MUST produce a stream of input events (e.g., `IObservable<InputEvent>`) as its primary output, covering keyboard, mouse, scroll, and window events.
- **FR-007**: The InputEvent type MUST be a discriminated union with cases for: KeyDown, KeyUp, MouseMove, MouseDown, MouseUp, MouseScroll, WindowResize, and FrameTick.
- **FR-008**: Each input event case MUST carry relevant payload data (key identity, mouse position, button identity, scroll delta, window dimensions, elapsed time since last frame for FrameTick).
- **FR-009**: The viewer MUST render the most recently received Scene on each frame.
- **FR-010**: The viewer MUST continue displaying the last received Scene when the scene stream stops emitting.
- **FR-011**: Group transforms MUST compose hierarchically — child transforms apply relative to parent transforms.
- **FR-012**: Elements within a group MUST render in declaration order, with later elements drawn on top of earlier ones.
- **FR-013**: The DSL MUST provide idiomatic F# helper functions or computation expressions for concise scene construction.
- **FR-014**: The existing screenshot functionality MUST continue to work with the declarative viewer.
- **FR-015**: The viewer MUST support both Vulkan and GL backends as it does today — the declarative layer is backend-agnostic.
- **FR-016**: The declarative scene stream API MUST fully replace the imperative `OnRender` callback API — no imperative rendering entry point shall remain in the public API.
- **FR-017**: The viewer MUST return a handle providing `Dispose()` for shutdown and `Screenshot()` for frame capture. Scene data and input events flow through streams, not the handle.

### Key Entities

- **Element**: A visual primitive (rectangle, circle, line, text, image, path) or a composite (group) that forms the building blocks of a scene tree.
- **Scene**: The root container representing one complete frame of visual output, composed of a tree of Elements.
- **InputEvent**: A strongly-typed discriminated union representing a single user interaction or window event.
- **Transform**: A 2D spatial transformation (translate, rotate, scale, matrix) that can be applied to groups and composes hierarchically.
- **Paint**: A declarative description of visual style (fill color, stroke color, stroke width, opacity) applied to elements.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can describe and render a multi-element scene (at least 5 elements of different types) using only declarative F# code — no imperative canvas calls.
- **SC-002**: Scene updates pushed at 60 values per second render smoothly without dropped frames on a standard workstation.
- **SC-003**: Input events are delivered to the consumer within one frame of the physical input occurring.
- **SC-004**: A developer can build a simple interactive application (e.g., a draggable shape) using only the scene stream and input stream — no callbacks or mutable state inside the viewer.
- **SC-005**: The declarative API requires at least 30% fewer lines of code compared to the equivalent imperative `OnRender` callback approach for a scene with 5+ elements.
- **SC-006**: Existing screenshot functionality works without modification when using the declarative viewer.

## Clarifications

### Session 2026-04-09

- Q: Should the declarative API coexist with or replace the imperative `OnRender` callback API? → A: Replace entirely — the declarative API fully replaces the imperative API.
- Q: Should the input event stream include a frame-tick event for animation? → A: Yes — include a `FrameTick` event carrying elapsed time since last frame.
- Q: Should the viewer return a handle for lifecycle control or use pure streams? → A: Handle + streams — streams carry scene/input data; handle provides `Dispose()` and `Screenshot()`.

## Assumptions

- The scene stream and input stream will use `IObservable<T>` as the streaming abstraction, since it is built into .NET, well-supported in F#, and aligns with the reactive "streams in, streams out" model. FSharp.Control.Reactive or similar may be used for ergonomic operators.
- The declarative layer renders on top of the existing SkiaSharp + Silk.NET infrastructure — it translates the scene tree into SkiaSharp canvas calls internally, rather than replacing the rendering backend.
- The element model covers 2D graphics only; 3D elements are out of scope.
- Text rendering uses system-default fonts unless a font is explicitly specified in the text element.
- The imperative `OnRender` callback API is removed and fully replaced by the declarative scene stream API. There is no backward-compatible coexistence — consumers must migrate to the declarative model.
- Performance optimization (scene diffing, dirty-region tracking, element caching) is out of scope for the initial implementation — the full scene tree is rendered each frame.

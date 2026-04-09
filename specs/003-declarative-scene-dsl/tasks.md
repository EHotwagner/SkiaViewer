# Tasks: Declarative Scene DSL

**Input**: Design documents from `/specs/003-declarative-scene-dsl/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Included — constitution mandates test evidence for behavior-changing code (Principle III).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Project file updates and compilation order preparation

- [x] T001 Update compilation order in src/SkiaViewer/SkiaViewer.fsproj to: VulkanBackend.fs → Scene.fsi → Scene.fs → SceneRenderer.fsi → SceneRenderer.fs → Viewer.fsi → Viewer.fs
- [x] T002 [P] Add SceneTests.fs and SceneRendererTests.fs to tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj compilation order

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core type definitions and internal renderer that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Create Scene.fsi with all public type signatures: Paint, Transform, PathCommand, Element, Scene, InputEvent, and Scene module helper function signatures in src/SkiaViewer/Scene.fsi — per contracts/public-api.md
- [x] T004 Create Scene.fs implementing all types and DSL helper functions (fill, stroke, fillStroke, withOpacity, empty, create, rect, ellipse, circle, line, text, image, path, group, translate, rotate, scale) in src/SkiaViewer/Scene.fs
- [x] T005 Create SceneRenderer.fsi declaring internal render function: `val render: scene: Scene -> canvas: SKCanvas -> unit` in src/SkiaViewer/SceneRenderer.fsi
- [x] T006 Create SceneRenderer.fs implementing recursive depth-first scene tree rendering with SKCanvas.Save/Restore for transform scoping, Paint-to-SKPaint conversion, and element-type dispatch in src/SkiaViewer/SceneRenderer.fs
- [x] T007 Verify `dotnet build` succeeds with new files (Scene and SceneRenderer compile, Viewer still compiles with old API temporarily)

**Checkpoint**: All types defined, scene renderer works in isolation. Ready for user story implementation.

---

## Phase 3: User Story 1 — Build a Static Scene Declaratively (Priority: P1) MVP

**Goal**: A developer can construct a scene tree using the DSL and pass it to the viewer for rendering — no imperative canvas calls.

**Independent Test**: Construct a scene with rect, circle, text, pass single-value observable to viewer, verify rendering occurs without exceptions.

### Tests for User Story 1

- [x] T008 [P] [US1] Create tests/SkiaViewer.Tests/SceneTests.fs with unit tests: DSL helpers produce correct Element variants (rect, circle, text, group), Paint defaults are correct, empty scene has no elements
- [x] T009 [P] [US1] Create tests/SkiaViewer.Tests/SceneRendererTests.fs with rendering tests: render a scene with 5+ element types to an offscreen SKSurface and verify non-black pixels at expected positions (pixel sampling)

### Implementation for User Story 1

- [x] T010 [US1] Rewrite src/SkiaViewer/Viewer.fsi: replace ViewerConfig (remove all callback fields), change `Viewer.run` signature to `config: ViewerConfig -> scenes: IObservable<Scene> -> ViewerHandle * IObservable<InputEvent>` — ViewerHandle retains Screenshot + IDisposable
- [x] T011 [US1] Rewrite src/SkiaViewer/Viewer.fs: refactor `Viewer.run` to accept `IObservable<Scene>`, subscribe to scene stream storing latest scene atomically, render latest scene via `SceneRenderer.render` each frame instead of calling OnRender callback. Use F# `Event<InputEvent>` for input event publishing (initially empty). Use scene's BackgroundColor for canvas clear.
- [x] T012 [US1] Add structured stderr logging in src/SkiaViewer/Viewer.fs for: scene stream error (log and keep last scene), scene stream completion (log and keep last scene), disposed bitmap skip with warning
- [x] T013 [US1] Update tests/SkiaViewer.Tests/ViewerTests.fs: rewrite all existing tests to use new declarative API (ViewerConfig without callbacks, IObservable<Scene> input). Verify: multi-element scene renders frames, empty scene renders without errors, start/stop cycle works, cross-thread dispose completes, screenshot works with declarative viewer. Update surface-area baseline test for new public types.

**Checkpoint**: Viewer renders declarative scenes. Tests pass. No input events yet (US2). Screenshot works.

---

## Phase 4: User Story 2 — Receive Input Events as a Stream (Priority: P1)

**Goal**: The viewer produces a stream of strongly-typed InputEvent values covering keyboard, mouse, scroll, and window events.

**Independent Test**: Start viewer, programmatically verify input event observable is subscribable and emits WindowResize on startup.

### Tests for User Story 2

- [x] T014 [US2] Add input event tests to tests/SkiaViewer.Tests/ViewerTests.fs: subscribe to IObservable<InputEvent>, verify WindowResize event is emitted when viewer starts (framebuffer resize on load), verify event stream completes or stops after viewer disposal

### Implementation for User Story 2

- [x] T015 [US2] Wire all input events in src/SkiaViewer/Viewer.fs: in the window Load handler, subscribe to keyboard (KeyDown, KeyUp), mouse (MouseMove, MouseDown, MouseUp), scroll (MouseScroll), and framebuffer resize (WindowResize) via Silk.NET.Input, triggering the `Event<InputEvent>` for each. Expose the event's `.Publish` as the returned `IObservable<InputEvent>`.
- [x] T016 [US2] Add KeyUp event support: wire `kb.add_KeyUp` alongside existing KeyDown in src/SkiaViewer/Viewer.fs (current API only has KeyDown)

**Checkpoint**: Viewer emits typed input events. Tests pass. Combined with US1: static declarative scene + input events work.

---

## Phase 5: User Story 3 — Update the Scene Over Time (Priority: P1)

**Goal**: The viewer continuously renders the most recent scene from the input stream, enabling animation and interactive updates via FrameTick.

**Independent Test**: Push a sequence of scenes with a circle at different positions, verify viewer renders without exceptions. Verify FrameTick events are emitted.

### Tests for User Story 3

- [x] T017 [US3] Add dynamic scene tests to tests/SkiaViewer.Tests/ViewerTests.fs: push 10 distinct scenes via Event<Scene>.Trigger over 1 second, verify frame count > 0 and no exceptions. Verify FrameTick events are emitted in the input stream (subscribe and count FrameTick events over 1 second, assert count > 30).

### Implementation for User Story 3

- [x] T018 [US3] Add FrameTick emission in src/SkiaViewer/Viewer.fs: in the `win.add_Render` callback, emit `InputEvent.FrameTick(delta)` on the input event stream at the start of each frame (before scene render), using the `double` delta parameter from Silk.NET
- [x] T019 [US3] Verify scene stream resilience in src/SkiaViewer/Viewer.fs: ensure stream completion keeps last scene displayed, stream errors are logged and last valid scene is retained (test with a scene observable that errors after emitting 5 scenes)

**Checkpoint**: Full reactive loop works — scenes in, events out. FrameTick enables animation. All P1 stories complete.

---

## Phase 6: User Story 4 — Compose with Transforms and Styles (Priority: P2)

**Goal**: Groups with transforms (translate, rotate, scale) compose hierarchically, and group-level paint/opacity cascades to children.

**Independent Test**: Create a group with a translate transform containing a circle at (0,0), verify the circle renders at the translated position via pixel sampling.

### Tests for User Story 4

- [x] T020 [US4] Add transform composition tests to tests/SkiaViewer.Tests/SceneRendererTests.fs: render a translated group with a filled rect to an offscreen surface, sample pixel at translated position to verify non-transparent. Test nested group transforms compose (translate inside translate). Test rotation renders at expected angle. Test group opacity renders children with reduced alpha.

### Implementation for User Story 4

- [x] T021 [US4] Implement Transform-to-SKMatrix conversion in src/SkiaViewer/SceneRenderer.fs: Translate → SKMatrix.CreateTranslation, Rotate → SKMatrix.CreateRotation (with center), Scale → SKMatrix.CreateScale (with center), Matrix → passthrough, Compose → fold/concat matrices
- [x] T022 [US4] Implement group opacity via SKCanvas.SaveLayer in src/SkiaViewer/SceneRenderer.fs: when a group has Paint with Opacity < 1.0, use `canvas.SaveLayer(paint)` with an SKPaint whose alpha is set, then `canvas.Restore()` after children render

**Checkpoint**: Complex composed scenes with transforms and opacity work. Pixel tests verify correctness.

---

## Phase 7: User Story 5 — Render Images and Paths Declaratively (Priority: P2)

**Goal**: Image and Path elements render correctly in the declarative scene tree.

**Independent Test**: Create a scene with an Image element using a programmatically created SKBitmap and a Path element with MoveTo/LineTo commands, verify both render.

### Tests for User Story 5

- [x] T023 [US5] Add image and path rendering tests to tests/SkiaViewer.Tests/SceneRendererTests.fs: create a 10x10 red SKBitmap, render an Image element at (50,50) on a 200x200 surface, verify red pixels at (55,55). Create a Path with MoveTo(0,0) + LineTo(100,0) with a 3px stroke, verify non-transparent pixels along the line.

### Implementation for User Story 5

- [x] T024 [P] [US5] Implement Image element rendering in src/SkiaViewer/SceneRenderer.fs: draw bitmap at position/size using `canvas.DrawBitmap(bitmap, destRect, paint)`, skip with stderr warning if bitmap is null or disposed
- [x] T025 [P] [US5] Implement Path element rendering in src/SkiaViewer/SceneRenderer.fs: convert PathCommand list to SKPath (MoveTo/LineTo/QuadTo/CubicTo/ArcTo/Close), draw with `canvas.DrawPath(skPath, paint)`, dispose SKPath after use

**Checkpoint**: All element types render. Full scene vocabulary available.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Scripting accessibility, documentation, and final validation

- [x] T026 [P] Update scripts/prelude.fsx for new declarative API: replace `defaultConfig` with new ViewerConfig (no callbacks), add `emptyScene`, `simpleScene` helpers, update `screenshot` helper
- [x] T027 [P] Update scripts/examples/01-screenshot.fsx to use declarative API: create scene with Scene.create + DSL helpers, pass IObservable<Scene> to Viewer.run
- [x] T028 [P] Create scripts/examples/02-declarative-scene.fsx: interactive demo using Event<Scene> for scene updates, arrow keys to move a shape, FrameTick for animation — per quickstart.md
- [x] T029 Run `dotnet build` and `dotnet test` to verify all tests pass
- [x] T030 Run `dotnet pack` and verify NuGet package outputs to local store
- [x] T031 Validate quickstart.md code examples compile and run correctly against the implementation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational — delivers MVP (static declarative rendering)
- **US2 (Phase 4)**: Depends on US1 (viewer refactor must be complete for input wiring)
- **US3 (Phase 5)**: Depends on US2 (FrameTick is an InputEvent; input event infrastructure must exist)
- **US4 (Phase 6)**: Depends on Foundational only — can run in parallel with US1-US3 (touches SceneRenderer, not Viewer)
- **US5 (Phase 7)**: Depends on Foundational only — can run in parallel with US1-US3 (touches SceneRenderer, not Viewer)
- **Polish (Phase 8)**: Depends on US1, US2, US3 completion (scripts use full API)

### User Story Dependencies

```text
Phase 1 (Setup)
    │
Phase 2 (Foundational)
    │
    ├──────────────────────────┬──────────────────┐
    │                          │                  │
Phase 3 (US1: Static Scene)   Phase 6 (US4)     Phase 7 (US5)
    │                          (Transforms)      (Images/Paths)
Phase 4 (US2: Input Events)   [parallel]        [parallel]
    │
Phase 5 (US3: Dynamic Scene + FrameTick)
    │
Phase 8 (Polish)
```

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Type definitions before rendering logic
- Rendering before viewer integration
- Story complete before moving to next priority

### Parallel Opportunities

- **Phase 2**: T003+T004 (Scene types) can run in parallel with T005+T006 (SceneRenderer) once types are defined — but T006 depends on T004, so sequential within pairs
- **Phase 3**: T008 and T009 (tests) can run in parallel
- **Phase 6+7**: US4 (transforms) and US5 (images/paths) can run in parallel with US1-US3 since they only touch SceneRenderer
- **Phase 7**: T024 and T025 (image + path rendering) can run in parallel
- **Phase 8**: T026, T027, T028 (scripts) can all run in parallel

---

## Parallel Example: User Story 1

```text
# After Foundational phase completes:

# Launch tests in parallel:
Task: T008 "SceneTests.fs — DSL helper unit tests"
Task: T009 "SceneRendererTests.fs — rendering pixel tests"

# Then implement sequentially:
Task: T010 "Rewrite Viewer.fsi (new API surface)"
Task: T011 "Rewrite Viewer.fs (scene stream + renderer integration)"
Task: T012 "Add structured logging for scene stream errors"
Task: T013 "Update ViewerTests.fs for new API"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (.fsproj update)
2. Complete Phase 2: Foundational (types + scene renderer)
3. Complete Phase 3: User Story 1 (declarative viewer rendering)
4. **STOP and VALIDATE**: Viewer renders a multi-element declarative scene. Screenshot works. All tests pass.
5. This is a working MVP — scenes render declaratively, no callbacks needed.

### Incremental Delivery

1. Setup + Foundational → Types and renderer ready
2. US1 → Static declarative scenes render → **MVP!**
3. US2 → Input events flow as observable stream
4. US3 → Dynamic scenes + FrameTick → **Full reactive loop!**
5. US4 → Transform composition → Complex composed scenes
6. US5 → Images + paths → Full element vocabulary
7. Polish → Scripts, docs, final validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 → US2 → US3 (viewer refactor chain)
   - Developer B: US4 + US5 (SceneRenderer enhancements, independent)
3. Merge when both tracks complete, then Polish

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Constitution requires: .fsi files (handled in T003, T005, T010), test evidence (each phase), surface-area baseline (T013), structured logging (T012)
- Total: 31 tasks across 8 phases

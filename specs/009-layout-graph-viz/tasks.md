# Tasks: Layout System & Graph Visualization

**Input**: Design documents from `/specs/009-layout-graph-viz/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included — constitution requires test evidence for all behavior-changing code.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the new SkiaViewer.Layout project and test project with proper structure.

- [x] T001 Create project file src/SkiaViewer.Layout/SkiaViewer.Layout.fsproj with net10.0, ProjectReference to SkiaViewer, PackageReferences for Microsoft.Msagl 1.1.6 and Microsoft.Msagl.Drawing 1.1.6, NuGet packaging metadata
- [x] T002 Create test project file tests/SkiaViewer.Layout.Tests/SkiaViewer.Layout.Tests.fsproj with net10.0, xunit 2.9.3, ProjectReference to SkiaViewer.Layout, InternalsVisibleTo
- [x] T003 [P] Add SkiaViewer.Layout and SkiaViewer.Layout.Tests to any solution file if present
- [x] T004 [P] Create empty placeholder files for compilation order in src/SkiaViewer.Layout/SkiaViewer.Layout.fsproj: Types.fsi, Types.fs, Defaults.fsi, Defaults.fs, Layout.fsi, Layout.fs, GraphValidation.fsi, GraphValidation.fs, Graph.fsi, Graph.fs

---

## Phase 2: SkiaSharp 3.x Upgrade — User Story 5 (Priority: P0)

**Goal**: Upgrade from SkiaSharp 2.88.6 to latest SkiaSharp 3.x across all projects. All existing tests pass.

**Independent Test**: Run full existing test suite — all tests must pass. Run example scripts and verify visual output.

**⚠️ CRITICAL**: No layout or graph work can begin until this phase is complete.

### Implementation for User Story 5

- [x] T005 [US5] Update PackageReference for SkiaSharp from 2.88.6 to latest 3.x in src/SkiaViewer/SkiaViewer.fsproj
- [x] T006 [US5] Update or replace SkiaSharp.NativeAssets.Linux.NoDependencies PackageReference for 3.x in src/SkiaViewer/SkiaViewer.fsproj
- [x] T007 [P] [US5] Update PackageReference for SkiaSharp from 2.88.6 to latest 3.x in tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj
- [x] T008 [P] [US5] Update PackageReference for SkiaSharp.NativeAssets.Linux.NoDependencies for 3.x in tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj
- [x] T009 [US5] Fix SKPaint/SKFont split in src/SkiaViewer/Scene.fsi and src/SkiaViewer/Scene.fs — update Paint type and DSL helpers if font properties moved to separate SKFont object
- [x] T010 [US5] Update makeSKPaint and drawWithPaint in src/SkiaViewer/SceneRenderer.fs for SkiaSharp 3.x API (SKPaint/SKFont split, any renamed methods)
- [x] T011 [US5] Update GPU surface creation in src/SkiaViewer/VulkanBackend.fs for SkiaSharp 3.x (GRContext changes, SKSurface.Create signature changes)
- [x] T012 [US5] Update surface creation and GRContext usage in src/SkiaViewer/Viewer.fs for SkiaSharp 3.x
- [x] T013 [US5] Update src/SkiaViewer/CachedRenderer.fs for any SkiaSharp 3.x API changes (SKPicture, SKPictureRecorder)
- [x] T014 [US5] Fix all SkiaSharp 3.x compilation errors in src/SkiaViewer.Charts/ (all chart modules that use SkiaSharp APIs directly)
- [x] T015 [US5] Update NuGet references in scripts/prelude.fsx for SkiaSharp 3.x
- [x] T016 [US5] Update NuGet references in scripts/charts-prelude.fsx for SkiaSharp 3.x
- [x] T017 [US5] Fix any compilation errors in tests/SkiaViewer.Tests/ due to SkiaSharp 3.x API changes
- [x] T018 [US5] Fix any compilation errors in tests/SkiaViewer.Charts.Tests/ due to SkiaSharp 3.x API changes
- [x] T019 [US5] Run full test suite: `dotnet test` across all test projects — all existing tests must pass
- [x] T020 [US5] Verify Vulkan and GL rendering backends work by running example scripts (scripts/examples/01-screenshot.fsx, scripts/examples/02-declarative-scene.fsx)

**Checkpoint**: SkiaSharp 3.x upgrade complete. All existing tests pass. All user story work can now begin.

---

## Phase 3: User Story 1 — Layout Containers (Priority: P1) 🎯 MVP

**Goal**: Developers can arrange elements using HStack, VStack, and Dock layouts without manual coordinate math.

**Independent Test**: Create nested layout containers and verify children are positioned correctly. Resize and verify reflow.

### Implementation for User Story 1

- [x] T021 [P] [US1] Define layout type definitions in src/SkiaViewer.Layout/Types.fsi: HorizontalAlignment, VerticalAlignment, DockPosition, LayoutPadding, LayoutSizing, StackConfig, DockConfig, LayoutChild, DockChild
- [x] T022 [P] [US1] Implement layout types in src/SkiaViewer.Layout/Types.fs matching Types.fsi signatures
- [x] T023 [P] [US1] Define default values in src/SkiaViewer.Layout/Defaults.fsi: padding, sizing, stackConfig, dockConfig
- [x] T024 [P] [US1] Implement layout defaults in src/SkiaViewer.Layout/Defaults.fs matching Defaults.fsi
- [x] T025 [US1] Define layout API in src/SkiaViewer.Layout/Layout.fsi: hstack, vstack, dock, child, childWithSize, dockChild
- [x] T026 [US1] Implement hstack in src/SkiaViewer.Layout/Layout.fs — measure children, distribute horizontal space, position with alignment, return Group element with translated children
- [x] T027 [US1] Implement vstack in src/SkiaViewer.Layout/Layout.fs — same as hstack but vertical axis
- [x] T028 [US1] Implement dock in src/SkiaViewer.Layout/Layout.fs — process Top/Bottom (full width, reduce remaining height), Left/Right (reduce remaining width), Fill (remaining space)
- [x] T029 [US1] Implement child, childWithSize, dockChild helper constructors in src/SkiaViewer.Layout/Layout.fs
- [x] T030 [US1] Write layout tests in tests/SkiaViewer.Layout.Tests/LayoutTests.fs — verify hstack positions children left-to-right, vstack top-to-bottom, dock allocates edges correctly
- [x] T031 [US1] Write nesting tests in tests/SkiaViewer.Layout.Tests/LayoutTests.fs — verify nested stacks, dock with inner vstack
- [x] T032 [US1] Write edge case tests in tests/SkiaViewer.Layout.Tests/EdgeCaseTests.fs — empty container (zero children), single child, alignment variations (Left/Center/Right/Stretch)
- [x] T033 [US1] Write resize tests in tests/SkiaViewer.Layout.Tests/LayoutTests.fs — same layout with different width/height produces correctly proportioned output
- [x] T034 [US1] Create surface-area baseline test in tests/SkiaViewer.Layout.Tests/SurfaceAreaTests.fs for Types, Defaults, and Layout modules
- [x] T035 [US1] Run `dotnet test tests/SkiaViewer.Layout.Tests/` — all layout tests pass

**Checkpoint**: Layout containers fully functional. Developers can compose HStack, VStack, Dock with nesting. Independently testable.

---

## Phase 4: User Story 2 — DAG Visualization (Priority: P2)

**Goal**: Developers can render a directed acyclic graph with automatic layered layout and directed edge arrows.

**Independent Test**: Define a DAG with 5-10 nodes and directed edges, render it, verify nodes positioned without overlap and edges have arrowheads.

### Implementation for User Story 2

- [x] T036 [P] [US2] Add graph type definitions to src/SkiaViewer.Layout/Types.fsi: GraphKind, LayoutDirection, NodeShape, NodeStyle, EdgeStyle, GraphNode, GraphEdge, GraphConfig, GraphDefinition
- [x] T037 [P] [US2] Implement graph types in src/SkiaViewer.Layout/Types.fs matching Types.fsi signatures
- [x] T038 [P] [US2] Add graph defaults to src/SkiaViewer.Layout/Defaults.fsi: nodeStyle, edgeStyle, graphConfig
- [x] T039 [P] [US2] Implement graph defaults in src/SkiaViewer.Layout/Defaults.fs matching Defaults.fsi
- [x] T040 [US2] Define validation API in src/SkiaViewer.Layout/GraphValidation.fsi: validate (returns Result<unit, string list>)
- [x] T041 [US2] Implement validateNodes in src/SkiaViewer.Layout/GraphValidation.fs — check unique node IDs
- [x] T042 [US2] Implement validateEdges in src/SkiaViewer.Layout/GraphValidation.fs — check source/target reference existing node IDs
- [x] T043 [US2] Implement detectCycles in src/SkiaViewer.Layout/GraphValidation.fs — Kahn's algorithm topological sort, return Error with cycle description for directed graphs
- [x] T044 [US2] Implement validate in src/SkiaViewer.Layout/GraphValidation.fs — compose validateNodes, validateEdges, detectCycles into single Result<unit, string list>
- [x] T045 [US2] Define graph rendering API in src/SkiaViewer.Layout/Graph.fsi: render, defaultConfig, validate
- [x] T046 [US2] Implement MSAGL graph building in src/SkiaViewer.Layout/Graph.fs — convert GraphDefinition nodes/edges to MSAGL Graph, configure SugiyamaLayoutSettings with LayoutDirection/NodeSpacing/LayerSpacing
- [x] T047 [US2] Implement MSAGL layout execution in src/SkiaViewer.Layout/Graph.fs — call CalculateLayout, read back node.Center/BoundingBox and edge.Curve geometries
- [x] T048 [US2] Implement node rendering in src/SkiaViewer.Layout/Graph.fs — convert MSAGL node positions to Element tree (Rect/Ellipse per NodeShape + Text label), scale to fit within width/height bounds
- [x] T049 [US2] Implement edge rendering in src/SkiaViewer.Layout/Graph.fs — convert MSAGL edge.Curve (ICurve) to Path element (line segments, cubic beziers), apply edge styling
- [x] T050 [US2] Implement arrowhead rendering in src/SkiaViewer.Layout/Graph.fs — draw small filled triangles at edge target endpoints for directed graphs
- [x] T051 [US2] Implement Graph.render in src/SkiaViewer.Layout/Graph.fs — validate, compute layout, render nodes + edges + arrowheads, return Result<Element, string>
- [x] T052 [US2] Write validation tests in tests/SkiaViewer.Layout.Tests/GraphValidationTests.fs — duplicate node IDs, missing edge targets, cycle detection, valid DAG passes
- [x] T053 [US2] Write DAG rendering tests in tests/SkiaViewer.Layout.Tests/GraphTests.fs — verify render returns Ok for valid DAG, output is Group element with expected child count, nodes don't overlap
- [x] T054 [US2] Write edge case tests in tests/SkiaViewer.Layout.Tests/EdgeCaseTests.fs — single node no edges, disconnected components rendered
- [x] T055 [US2] Update surface-area baseline in tests/SkiaViewer.Layout.Tests/SurfaceAreaTests.fs for GraphValidation and Graph modules
- [x] T056 [US2] Run `dotnet test tests/SkiaViewer.Layout.Tests/` — all graph tests pass

**Checkpoint**: DAG visualization fully functional. Developers can define and render directed acyclic graphs with automatic layout.

---

## Phase 5: User Story 3 — Undirected & Weighted Graphs (Priority: P3)

**Goal**: Developers can render undirected graphs with weighted edges, where weight is visually distinguishable.

**Independent Test**: Define an undirected weighted graph, render it, verify no arrowheads and edge weights are visually represented.

### Implementation for User Story 3

- [x] T057 [US3] Add MDS layout support in src/SkiaViewer.Layout/Graph.fs — use MdsLayoutSettings when GraphKind is Undirected
- [x] T058 [US3] Implement weight-to-thickness mapping in src/SkiaViewer.Layout/Graph.fs — map edge weight range to line thickness (min 1px, max configurable via EdgeStyle)
- [x] T059 [US3] Implement edge label rendering in src/SkiaViewer.Layout/Graph.fs — render weight/label text at edge midpoint when EdgeStyle.ShowLabel is true
- [x] T060 [US3] Suppress arrowheads for undirected graphs in src/SkiaViewer.Layout/Graph.fs — skip arrowhead rendering when GraphKind is Undirected or ShowArrowheads is Some false
- [x] T061 [US3] Write undirected graph tests in tests/SkiaViewer.Layout.Tests/GraphTests.fs — verify render with Undirected kind produces no arrowhead elements
- [x] T062 [US3] Write weighted edge tests in tests/SkiaViewer.Layout.Tests/GraphTests.fs — verify edges with different weights produce different stroke widths, weight=0 still renders
- [x] T063 [US3] Write edge label tests in tests/SkiaViewer.Layout.Tests/GraphTests.fs — verify label text elements at edge midpoints when ShowLabel is true
- [x] T064 [US3] Write negative weight edge case test in tests/SkiaViewer.Layout.Tests/EdgeCaseTests.fs — negative weights render without error
- [x] T065 [US3] Update surface-area baseline in tests/SkiaViewer.Layout.Tests/SurfaceAreaTests.fs if public API changed
- [x] T066 [US3] Run `dotnet test tests/SkiaViewer.Layout.Tests/` — all undirected/weighted tests pass

**Checkpoint**: Undirected weighted graphs fully functional. Both DAG and undirected graph types work independently.

---

## Phase 6: User Story 4 — Layout + Graph Composition (Priority: P4)

**Goal**: Graph elements compose naturally inside layout containers. A developer can build a complete dashboard with graphs and other elements.

**Independent Test**: Embed a graph inside a VStack alongside a text header. Verify graph stays within allocated bounds.

### Implementation for User Story 4

- [x] T067 [US4] Write composition tests in tests/SkiaViewer.Layout.Tests/GraphTests.fs — verify graph Element output works as LayoutChild inside hstack/vstack/dock
- [x] T068 [US4] Write bounds verification tests in tests/SkiaViewer.Layout.Tests/GraphTests.fs — verify graph element positions stay within parent-allocated bounds at multiple sizes
- [x] T069 [US4] Write graphical composition test in tests/SkiaViewer.Layout.Tests/GraphicalTests.fs — render graph inside VStack with text header, verify visual output
- [x] T070 [US4] Run `dotnet test tests/SkiaViewer.Layout.Tests/` — all composition tests pass

**Checkpoint**: Layout + graph composition works. All four user stories are independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Scripting accessibility, packaging, final validation.

- [x] T071 [P] Create scripts/layout-prelude.fsx — load SkiaViewer.Layout.dll, expose helpers for layouts and graphs
- [x] T072 [P] Create scripts/examples/05-layouts.fsx — nested HStack/VStack/Dock demos with resize
- [x] T073 [P] Create scripts/examples/06-graphs.fsx — DAG, undirected weighted graph, and graph-inside-layout composition demos
- [x] T074 Run full test suite: `dotnet test` across all test projects (SkiaViewer.Tests, SkiaViewer.Charts.Tests, SkiaViewer.Layout.Tests)
- [x] T075 Pack SkiaViewer.Layout: `dotnet pack src/SkiaViewer.Layout/ -o ~/.local/share/nuget-local/`
- [x] T076 Verify quickstart.md examples compile and run against packed build
- [x] T077 Update SkiaViewer version numbers in .fsproj files if needed

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **SkiaSharp Upgrade (Phase 2)**: Depends on Setup — **BLOCKS all subsequent phases**
- **Layout Containers (Phase 3)**: Depends on Phase 2 completion
- **DAG Visualization (Phase 4)**: Depends on Phase 2 completion (independent of Phase 3)
- **Undirected Graphs (Phase 5)**: Depends on Phase 4 (extends graph rendering)
- **Composition (Phase 6)**: Depends on Phase 3 + Phase 4 (needs both layouts and graphs)
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US5 (P0)**: SkiaSharp upgrade — MUST complete first, blocks all others
- **US1 (P1)**: Layout containers — independent after US5
- **US2 (P2)**: DAG visualization — independent after US5 (can run parallel with US1)
- **US3 (P3)**: Undirected graphs — depends on US2 (extends graph infrastructure)
- **US4 (P4)**: Composition — depends on US1 + US2

### Within Each User Story

- .fsi signature files before .fs implementations
- Types before Defaults before Layout/Graph
- Validation before rendering (for graph stories)
- Implementation before tests
- Tests pass before checkpoint

### Parallel Opportunities

- T003, T004 can run in parallel (Setup)
- T007, T008 can run in parallel with each other (SkiaSharp upgrade in perf tests)
- T021-T024 can all run in parallel (layout types + defaults, different files)
- T036-T039 can all run in parallel (graph types + defaults, different files)
- US1 and US2 can proceed in parallel after US5 completes (different modules, no shared state)
- T071-T073 can all run in parallel (independent script files)

---

## Parallel Example: User Story 1

```bash
# Launch type definitions in parallel (different files):
Task: "Define layout types in src/SkiaViewer.Layout/Types.fsi" (T021)
Task: "Define defaults in src/SkiaViewer.Layout/Defaults.fsi" (T023)

# After types complete, launch implementations in parallel:
Task: "Implement layout types in src/SkiaViewer.Layout/Types.fs" (T022)
Task: "Implement defaults in src/SkiaViewer.Layout/Defaults.fs" (T024)
```

## Parallel Example: User Story 2

```bash
# Launch graph types and defaults in parallel:
Task: "Add graph type definitions to Types.fsi" (T036)
Task: "Add graph defaults to Defaults.fsi" (T038)

# After types, launch validation and rendering signatures in parallel:
Task: "Define validation API in GraphValidation.fsi" (T040)
Task: "Define graph rendering API in Graph.fsi" (T045)
```

---

## Implementation Strategy

### MVP First (User Story 5 + User Story 1)

1. Complete Phase 1: Setup
2. Complete Phase 2: SkiaSharp 3.x Upgrade (US5) — **CRITICAL BLOCKER**
3. Complete Phase 3: Layout Containers (US1)
4. **STOP and VALIDATE**: Test layouts independently
5. Deploy/demo if ready — layouts alone deliver significant value

### Incremental Delivery

1. Setup + SkiaSharp Upgrade → Foundation ready
2. Add Layout Containers (US1) → Test independently → **MVP!**
3. Add DAG Visualization (US2) → Test independently → Demo
4. Add Undirected Graphs (US3) → Test independently → Demo
5. Add Composition (US4) → Test independently → Demo
6. Polish + Pack → Ship

### Parallel Strategy

After US5 (SkiaSharp upgrade) completes:
- Stream A: US1 (Layout Containers)
- Stream B: US2 (DAG Visualization)
- Then: US3 extends US2, US4 merges US1 + US2

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Constitution requires .fsi files, surface-area baselines, and test evidence for all public modules

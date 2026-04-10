# Implementation Plan: Layout System & Graph Visualization

**Branch**: `009-layout-graph-viz` | **Date**: 2026-04-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/009-layout-graph-viz/spec.md`

## Summary

Add layout containers (HStack, VStack, Dock) and graph visualization (DAG, undirected, weighted edges) to SkiaViewer as a new `SkiaViewer.Layout` library. Layout concepts are ported from UILayout (C#) to F#. Graph layout computation uses MSAGL. Prerequisites include upgrading the entire project from SkiaSharp 2.88.6 to SkiaSharp 3.x.

## Technical Context

**Language/Version**: F# on .NET 10.0  
**Primary Dependencies**: SkiaSharp 3.x (upgrade from 2.88.6), Silk.NET 2.22.0, Microsoft.Msagl 1.1.6, Microsoft.Msagl.Drawing 1.1.6  
**Storage**: N/A — all data is immutable per render frame  
**Testing**: xunit 2.9.3  
**Target Platform**: Linux x64, Windows x64 (desktop)  
**Project Type**: Library (NuGet-packable)  
**Performance Goals**: 100-node DAG layout < 2s, layout reflow on resize < 500ms  
**Constraints**: F#-only codebase (constitution), no mocks in tests, .fsi signature files for all public modules  
**Scale/Scope**: Graphs up to ~100 nodes, layouts with ~10-20 children per container

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| I. Spec-First Delivery | PASS | Spec complete with testable user stories, acceptance criteria, and scope boundaries. Adds public API surface (new project), introduces new dependencies (MSAGL). |
| II. Compiler-Enforced Structural Contracts | PASS | All new public modules will have .fsi signature files. Surface-area baselines will be created. |
| III. Test Evidence Is Mandatory | PASS | Each user story has independent test criteria. Tests will run against live environment (no mocks). |
| IV. Observability and Safe Failure Handling | PASS | Graph.render returns Result<Element, string> for validation errors. Cycle detection fails fast with clear error. |
| V. Scripting Accessibility | PASS | New `layout-prelude.fsx` and numbered example scripts planned. |
| F#-only constraint | PASS | UILayout concepts ported to F# (not consumed as C#). MSAGL is a NuGet dependency (external, like SkiaSharp). |
| .fsi signature files | PASS | Planned for: Types.fsi, Layout.fsi, Graph.fsi, Defaults.fsi |
| Surface-area baselines | PASS | Will be created for all public modules. |
| dotnet pack | PASS | SkiaViewer.Layout will be packable to local NuGet store. |
| Dependency justification | PASS | MSAGL: graph layout is non-trivial, well-solved by MSAGL, MIT licensed. No other new deps. |

## Project Structure

### Documentation (this feature)

```text
specs/009-layout-graph-viz/
├── spec.md
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── layout-api.md
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
src/
├── SkiaViewer/                          # Existing — upgraded to SkiaSharp 3.x
│   ├── VulkanBackend.fs                 # Updated for SkiaSharp 3.x API changes
│   ├── Scene.fsi / Scene.fs             # Updated for SKPaint/SKFont split
│   ├── SceneRenderer.fsi / .fs          # Updated for SkiaSharp 3.x rendering API
│   ├── CachedRenderer.fsi / .fs         # Updated for SkiaSharp 3.x
│   └── Viewer.fsi / Viewer.fs           # Updated for surface creation changes
├── SkiaViewer.Charts/                   # Existing — updated for SkiaSharp 3.x
│   └── (all chart modules updated)
└── SkiaViewer.Layout/                   # NEW
    ├── SkiaViewer.Layout.fsproj
    ├── Types.fsi / Types.fs             # Layout + Graph type definitions
    ├── Defaults.fsi / Defaults.fs       # Default configs
    ├── Layout.fsi / Layout.fs           # HStack, VStack, Dock layout engine
    ├── GraphValidation.fsi / .fs        # Cycle detection, edge/node validation
    └── Graph.fsi / Graph.fs             # MSAGL integration, graph → Element rendering

tests/
├── SkiaViewer.Tests/                    # Existing — updated for SkiaSharp 3.x
├── SkiaViewer.Charts.Tests/             # Existing — updated for SkiaSharp 3.x
└── SkiaViewer.Layout.Tests/             # NEW
    ├── SkiaViewer.Layout.Tests.fsproj
    ├── LayoutTests.fs                   # HStack, VStack, Dock tests
    ├── GraphValidationTests.fs          # Cycle detection, validation tests
    ├── GraphTests.fs                    # Graph rendering tests
    ├── EdgeCaseTests.fs                 # Empty containers, single node, disconnected components
    ├── SurfaceAreaTests.fs              # Public API baseline
    └── GraphicalTests.fs                # Visual rendering verification

scripts/
├── prelude.fsx                          # Existing — updated for SkiaSharp 3.x
├── charts-prelude.fsx                   # Existing — updated
├── layout-prelude.fsx                   # NEW — loads SkiaViewer.Layout
└── examples/
    ├── 05-layouts.fsx                   # NEW — layout demos
    └── 06-graphs.fsx                    # NEW — graph visualization demos
```

**Structure Decision**: New `SkiaViewer.Layout` project following the established pattern (like SkiaViewer.Charts). References SkiaViewer core via ProjectReference. MSAGL is the only new NuGet dependency.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New NuGet dependency (MSAGL) | Sugiyama and force-directed graph layout algorithms are non-trivial to implement correctly | Implementing from scratch would be error-prone and slow for a well-solved problem |
| Separate GraphValidation module | Cycle detection and validation logic is distinct from rendering | Inlining in Graph module would mix concerns and make testing harder |

## Phase-by-Phase Implementation

### Phase 0: SkiaSharp 3.x Upgrade (P0)

Upgrade all projects from SkiaSharp 2.88.6 to latest SkiaSharp 3.x. This is the prerequisite for all subsequent work.

**Scope**:
1. Update all `.fsproj` PackageReferences from SkiaSharp 2.88.6 → 3.x
2. Replace `SkiaSharp.NativeAssets.Linux.NoDependencies` with 3.x equivalent
3. Fix `SKPaint` → `SKPaint` + `SKFont` split in Scene.fs, SceneRenderer.fs
4. Update `SKSurface.Create` calls in Viewer.fs, VulkanBackend.fs
5. Update `GRContext` usage if renamed in 3.x
6. Fix any `SKBitmap` API changes
7. Update prelude.fsx NuGet references
8. Run full test suite — all existing tests must pass
9. Run example scripts — verify visual output

**Key files to modify**:
- `src/SkiaViewer/SkiaViewer.fsproj` — package versions
- `src/SkiaViewer/Scene.fsi` / `Scene.fs` — Paint type if SKPaint/SKFont split affects DSL
- `src/SkiaViewer/SceneRenderer.fs` — `makeSKPaint`, `drawWithPaint`, font rendering
- `src/SkiaViewer/VulkanBackend.fs` — GPU surface creation
- `src/SkiaViewer/Viewer.fs` — surface creation, GRContext
- `src/SkiaViewer.Charts/*.fs` — any direct SkiaSharp API usage
- `tests/**/*.fsproj` — package versions
- `scripts/prelude.fsx` — NuGet references

### Phase 1: Layout Containers (P1)

Implement HStack, VStack, and Dock layout containers as pure functions that produce Element trees.

**Scope**:
1. Create `SkiaViewer.Layout` project with .fsproj, ProjectReference to SkiaViewer
2. Define types in `Types.fsi` / `Types.fs` — alignment enums, LayoutPadding, LayoutSizing, StackConfig, DockConfig, LayoutChild, DockChild
3. Implement `Defaults.fsi` / `Defaults.fs` — sensible defaults for all config types
4. Implement `Layout.fsi` / `Layout.fs`:
   - `hstack`: Measure children → distribute horizontal space → position with alignment → return Group element with translated children
   - `vstack`: Same vertically
   - `dock`: Process Top/Bottom (full width, reduce remaining height) → Left/Right (reduce remaining width) → Fill (remaining space)
   - Helper constructors: `child`, `childWithSize`, `dockChild`
5. Create test project `SkiaViewer.Layout.Tests`
6. Write layout tests — verify element positions, nesting, resize behavior
7. Create surface-area baseline
8. Create `layout-prelude.fsx`
9. Create `scripts/examples/05-layouts.fsx`

**Key design**: Layout functions are pure — they take config + children + available width/height and return a `Group` element with children translated to their computed positions. No mutation, no retained state.

### Phase 2: Graph Visualization — DAG (P2)

Implement DAG rendering using MSAGL's Sugiyama layout.

**Scope**:
1. Add MSAGL NuGet references to SkiaViewer.Layout.fsproj
2. Define graph types in `Types.fsi` / `Types.fs` — GraphKind, GraphNode, GraphEdge, GraphConfig, GraphDefinition, NodeStyle, EdgeStyle, NodeShape
3. Implement `GraphValidation.fsi` / `GraphValidation.fs`:
   - `validateNodes`: Check unique IDs
   - `validateEdges`: Check source/target reference existing nodes
   - `detectCycles`: Kahn's algorithm topological sort for DAG validation
   - `validate`: Compose all validations, return Result<unit, string list>
4. Implement `Graph.fsi` / `Graph.fs`:
   - Build MSAGL graph from GraphDefinition
   - Configure SugiyamaLayoutSettings with LayoutDirection, NodeSpacing, LayerSpacing
   - Run CalculateLayout
   - Convert MSAGL geometry → Element tree:
     - Nodes: Rect/Ellipse + Text label, positioned at node.Center
     - Edges: Path from edge.Curve (line segments, cubic beziers)
     - Arrowheads: Small triangles at edge endpoints for directed edges
   - Return Result<Element, string>
5. Write graph tests — DAG rendering, validation, cycle detection
6. Update surface-area baseline

### Phase 3: Undirected & Weighted Graphs (P3)

Extend graph rendering for undirected graphs with weighted edges.

**Scope**:
1. Add MDS layout support in Graph.fs — use `MdsLayoutSettings` for undirected graphs
2. Implement weight visualization:
   - Map weight range to line thickness (min 1px, max configurable)
   - Render weight labels at edge midpoints when `EdgeStyle.ShowLabel = true`
3. Suppress arrowheads for undirected graphs
4. Handle disconnected components — MSAGL handles this natively
5. Write tests for undirected graphs, weighted edges, edge cases (weight=0, negative weights)
6. Update surface-area baseline

### Phase 4: Composition & Polish (P4)

Integrate graph elements within layout containers. Create documentation and examples.

**Scope**:
1. Verify graph elements work as children in layout containers (they're just Elements — should work naturally)
2. Test graph + layout composition: graph inside VStack, graph next to DataGrid
3. Write `scripts/examples/06-graphs.fsx` with DAG + undirected + composition examples
4. Update `layout-prelude.fsx` with graph helpers
5. Final surface-area baseline update
6. Run full test suite across all projects
7. `dotnet pack` SkiaViewer.Layout to local NuGet store

## .fsi Signature Contracts

New signature files for this feature:

### Types.fsi
- All layout types: HorizontalAlignment, VerticalAlignment, DockPosition, LayoutPadding, LayoutSizing, StackConfig, DockConfig, LayoutChild, DockChild
- All graph types: GraphKind, LayoutDirection, NodeShape, NodeStyle, EdgeStyle, GraphNode, GraphEdge, GraphConfig, GraphDefinition

### Defaults.fsi
- `val padding: LayoutPadding`
- `val sizing: LayoutSizing`
- `val stackConfig: StackConfig`
- `val dockConfig: DockConfig`
- `val nodeStyle: NodeStyle`
- `val edgeStyle: EdgeStyle`
- `val graphConfig: kind: GraphKind -> GraphConfig`

### Layout.fsi
- `val hstack: config: StackConfig -> children: LayoutChild list -> width: float32 -> height: float32 -> Element`
- `val vstack: config: StackConfig -> children: LayoutChild list -> width: float32 -> height: float32 -> Element`
- `val dock: config: DockConfig -> children: DockChild list -> width: float32 -> height: float32 -> Element`
- `val child: element: Element -> LayoutChild`
- `val childWithSize: width: float32 -> height: float32 -> element: Element -> LayoutChild`
- `val dockChild: position: DockPosition -> element: Element -> DockChild`

### GraphValidation.fsi
- `val validate: graph: GraphDefinition -> Result<unit, string list>`

### Graph.fsi
- `val render: graph: GraphDefinition -> width: float32 -> height: float32 -> Result<Element, string>`
- `val defaultConfig: kind: GraphKind -> GraphConfig`
- `val validate: graph: GraphDefinition -> Result<unit, string list>`

## Constitution Re-Check (Post-Design)

| Gate | Status | Notes |
|------|--------|-------|
| I. Spec-First Delivery | PASS | Full artifact chain: spec → plan → .fsi contracts → tasks (next) |
| II. Structural Contracts | PASS | 5 new .fsi files defined. Surface-area baselines planned. |
| III. Test Evidence | PASS | Test files per module: LayoutTests, GraphValidationTests, GraphTests, EdgeCaseTests, SurfaceAreaTests, GraphicalTests |
| IV. Observability | PASS | Graph.render returns Result with error messages. Validation surfaces all issues as string list. |
| V. Scripting | PASS | layout-prelude.fsx + examples 05/06 planned |
| Dependencies | PASS | MSAGL justified (non-trivial algorithms), MIT licensed, version-pinned |
| dotnet pack | PASS | SkiaViewer.Layout packable to local NuGet |

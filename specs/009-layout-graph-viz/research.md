# Research: Layout System & Graph Visualization

**Feature**: 009-layout-graph-viz  
**Date**: 2026-04-09

## Decision 0: Upgrade SkiaSharp to 3.x

**Decision**: Upgrade the entire project from SkiaSharp 2.88.6 to the latest SkiaSharp 3.x as a prerequisite for this feature.

**Rationale**:
- SkiaSharp 3.x is the actively developed major version; 2.88.x is legacy.
- Aligns with UILayout's SkiaSharp 3.x target, reducing friction if UILayout is consumed in the future.
- Modern SkiaSharp 3.x APIs improve surface creation, native asset packaging, and GPU backend support.
- Staying on 2.x means accumulating technical debt as the ecosystem moves forward.

**Known breaking changes to address**:
- `SKBitmap` API changes (some constructors/methods renamed or removed).
- `SKSurface.Create` signature changes — GPU surface creation differs.
- Native asset package names may change (e.g., `SkiaSharp.NativeAssets.Linux.NoDependencies` may need replacement).
- `SKPaint` is split into `SKPaint` + `SKFont` in 3.x — font properties move to a separate object.
- `GRContext` renamed to `GRRecordingContext` in some APIs.

**Alternatives considered**:
- **Stay on 2.88.6**: Rejected — blocks future ecosystem compatibility and accumulates debt.
- **Upgrade later as separate feature**: Rejected — doing it now avoids building new layout/graph code against a soon-to-be-upgraded API.

**Migration approach**: Upgrade all `.fsproj` references, fix compilation errors, update rendering pipeline (VulkanBackend, SceneRenderer, CachedRenderer), update tests, verify all example scripts.

## Decision 1: UILayout Integration Strategy

**Decision**: Port UILayout's layout concepts to F# rather than consuming the C# library directly.

**Rationale**:
- UILayout is not published on NuGet. It uses shared projects (`.shproj`/`.projitems`) that compile C# source into the consuming project — this violates the constitution's "F# on .NET is the exclusive stack" constraint.
- UILayout's layout logic (HStack, VStack, Dock) is well-understood and not complex to implement. The core algorithm is: measure children → arrange within bounds.
- MPL 2.0 license permits creating derived works.

**Alternatives considered**:
- **Git submodule + shared project import**: Rejected — violates F#-only constraint (shared projects compile C# into the consuming project).
- **Fork UILayout as separate C# NuGet package**: Rejected — adds maintenance burden of a separate C# project.
- **Implement from scratch without UILayout reference**: Rejected — UILayout's design (HorizontalStack, VerticalStack, Dock, alignment enums, ChildSpacing, Padding) is a good model to follow.

**UILayout concepts to port**:
- `HorizontalStack`: Children arranged left-to-right, respecting DesiredWidth/Height and alignment.
- `VerticalStack`: Children arranged top-to-bottom.
- `Dock`: Children docked to Top/Bottom/Left/Right/Fill edges.
- Sizing: DesiredWidth, DesiredHeight, HorizontalAlignment (Left/Right/Center/Stretch), VerticalAlignment (Top/Bottom/Center/Stretch).
- Spacing: ChildSpacing (float) between children, Padding around container.
- Layout pass: `GetContentSize()` (measure) → `UpdateContentLayout()` (arrange within bounds).

## Decision 2: MSAGL for Graph Layout

**Decision**: Use MSAGL (`Microsoft.Msagl` + `Microsoft.Msagl.Drawing` v1.1.6) for graph layout computation.

**Rationale**:
- MIT licensed, maintained by Microsoft.
- .NET Standard 2.0 — compatible with .NET 10.0.
- Provides Sugiyama (layered) layout for DAGs and MDS (force-directed) for undirected graphs.
- Returns geometry (node positions, edge curves as ICurve) without any UI dependency — rendering stays in SkiaSharp.
- Well-tested on large graphs.

**Alternatives considered**:
- **Implement layout algorithms from scratch**: Rejected — Sugiyama and force-directed algorithms are non-trivial, well-solved problems.
- **QuikGraph**: Only provides data structures/algorithms, no layout. Could complement MSAGL but unnecessary.
- **GraphX**: Low activity since 2020, WPF-coupled rendering.
- **GiGraph + Graphviz**: Requires external Graphviz binary — unacceptable runtime dependency.

**MSAGL API pattern** (to wrap in F#):
```
Graph → AddEdge/AddNode → SugiyamaLayoutSettings or MdsLayoutSettings
→ CalculateLayout → Read node.Center, node.BoundingBox, edge.Curve
→ Convert to SkiaViewer Element tree (Rect for nodes, Path for edges)
```

## Decision 3: New Project or Extend Existing

**Decision**: Create a new `SkiaViewer.Layout` project for layout containers and graph visualization.

**Rationale**:
- Layout and graph visualization are distinct from charting (SkiaViewer.Charts).
- Follows the existing pattern: core library (SkiaViewer) + domain libraries (SkiaViewer.Charts, SkiaViewer.Layout).
- Layout containers are a general-purpose feature that other libraries could depend on.
- Graph visualization naturally lives alongside layout since graphs are rendered within layout bounds.

**Alternatives considered**:
- **Add to SkiaViewer core**: Rejected — would bloat the core with MSAGL dependency.
- **Add to SkiaViewer.Charts**: Rejected — graphs are not charts; mixing concerns.
- **Separate SkiaViewer.Layout and SkiaViewer.Graph**: Rejected — graph rendering depends on layout bounds, and graph elements should be layout-composable. One project is simpler.

## Decision 4: Edge Weight Visualization

**Decision**: Represent edge weights via line thickness (proportional to weight) with optional numeric labels.

**Rationale**:
- Line thickness is the most immediately perceivable visual encoding for weight.
- Labels provide exact values when needed.
- Both can be controlled via the graph edge styling API.

## Decision 5: Graph Node Rendering

**Decision**: Graph nodes render as styled rectangles with text labels by default, using the existing Element types.

**Rationale**:
- Consistent with SkiaViewer's existing Rect + Text element types.
- Custom node content can be achieved by composing elements within a Group.
- MSAGL provides bounding boxes for nodes; we render within those bounds.

## Decision 6: Cycle Detection for DAGs

**Decision**: Use topological sort (Kahn's algorithm) to detect cycles before passing to MSAGL's Sugiyama layout.

**Rationale**:
- MSAGL's Sugiyama layout may not produce meaningful results for cyclic graphs.
- Detecting cycles upfront and returning a clear error is better than silent misbehavior.
- Topological sort is O(V+E) and simple to implement in F#.

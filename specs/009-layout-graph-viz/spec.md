# Feature Specification: Layout System & Graph Visualization

**Feature Branch**: `009-layout-graph-viz`  
**Created**: 2026-04-09  
**Status**: Draft  
**Input**: User description: "add https://github.com/mikeoliphant/UILayout for layouts and also add a graph visualization element for DAGs, undirected, weighted edges."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Arrange Scene Elements Using Layouts (Priority: P1)

A developer building a SkiaViewer application wants to arrange visual elements using layout containers (horizontal stacks, vertical stacks, dock layouts) so that elements automatically position and size themselves relative to each other without manual coordinate math.

**Why this priority**: Layouts are the foundational building block. Without automatic arrangement, every element requires manual positioning, which is brittle and tedious. This unlocks all higher-level composition patterns.

**Independent Test**: Can be fully tested by creating a scene with nested layout containers (e.g., a vertical stack containing two horizontal stacks of rectangles) and verifying that elements are positioned and sized correctly when the window is resized.

**Acceptance Scenarios**:

1. **Given** a scene definition with a vertical stack containing three child elements, **When** the scene is rendered, **Then** the children are arranged top-to-bottom with no overlap and fill the available vertical space according to their sizing rules.
2. **Given** a scene definition with a horizontal stack, **When** the window is resized, **Then** child elements reflow to fill the new width proportionally.
3. **Given** nested layout containers (e.g., a dock layout with a vertical stack in the center), **When** the scene is rendered, **Then** each container respects its parent's allocated bounds and arranges its own children accordingly.

---

### User Story 2 - Visualize a Directed Acyclic Graph (Priority: P2)

A developer wants to render a DAG (directed acyclic graph) to visualize dependency trees, task pipelines, or data flow diagrams. The graph should automatically lay out nodes and draw directed edges between them.

**Why this priority**: DAGs are the most common graph type in software tooling and data visualization. Supporting them first delivers the highest value for the most use cases.

**Independent Test**: Can be fully tested by defining a DAG with 5-10 nodes and directed edges, rendering it, and verifying that nodes are positioned without overlap, edges follow the correct direction, and the overall layout is readable (e.g., top-to-bottom or left-to-right flow).

**Acceptance Scenarios**:

1. **Given** a DAG definition with nodes and directed edges, **When** the graph is rendered, **Then** nodes are automatically positioned in a layered layout with edges drawn as arrows pointing from source to target.
2. **Given** a DAG with a node that has multiple incoming edges, **When** rendered, **Then** all incoming edges are visually distinguishable and do not overlap each other at the target node.
3. **Given** a DAG with 50+ nodes, **When** rendered, **Then** the layout completes and displays without nodes overlapping.

---

### User Story 3 - Visualize an Undirected Graph with Weighted Edges (Priority: P3)

A developer wants to render an undirected graph where edges have weights, useful for network topology diagrams, social graphs, or similarity maps. Edge weights should be visually represented (e.g., line thickness or label).

**Why this priority**: Undirected weighted graphs extend the graph visualization to cover network and relationship use cases. This builds on the graph rendering infrastructure established for DAGs.

**Independent Test**: Can be fully tested by defining an undirected graph with weighted edges, rendering it, and verifying that nodes are positioned using a force-directed or similar layout, edges connect the correct nodes without arrows, and edge weights are visually distinguishable.

**Acceptance Scenarios**:

1. **Given** an undirected graph definition with weighted edges, **When** rendered, **Then** edges are drawn without arrowheads and edge weight is visually represented (e.g., thicker lines for higher weight or a numeric label).
2. **Given** an undirected graph with varying edge weights, **When** rendered, **Then** a viewer can visually distinguish high-weight edges from low-weight edges.
3. **Given** a graph where two nodes are connected by an edge with weight 0, **When** rendered, **Then** the edge is still drawn but with minimal visual prominence.

---

### User Story 4 - Combine Layouts with Graph Visualization (Priority: P4)

A developer wants to embed a graph visualization inside a layout container alongside other elements (e.g., a title bar above a graph, or a graph next to a data grid). The graph should respect the bounds allocated by its parent layout.

**Why this priority**: Composition of layouts and graph elements is the natural end-goal but depends on both the layout system and graph rendering being functional first.

**Independent Test**: Can be fully tested by creating a vertical stack with a text element on top and a graph element below, then verifying the graph renders within its allocated bounds and does not overlap the text.

**Acceptance Scenarios**:

1. **Given** a layout container with a graph element as one of its children, **When** rendered, **Then** the graph is clipped to and laid out within the bounds assigned by the parent container.
2. **Given** a window resize, **When** a graph is inside a layout, **Then** the graph re-lays out its nodes to fit the new available space.

---

### User Story 5 - Upgrade to SkiaSharp 3.x (Priority: P0)

The project upgrades from SkiaSharp 2.88.6 to the latest SkiaSharp 3.x release across all projects (SkiaViewer, SkiaViewer.Charts, tests, perf tests). All existing functionality continues to work after the upgrade.

**Why this priority**: SkiaSharp 3.x is the current major version with active development. Upgrading unblocks compatibility with UILayout and other modern SkiaSharp-based libraries, and ensures the project stays on a supported version. Must happen before layout and graph work begins.

**Independent Test**: Can be fully tested by running the full existing test suite after the upgrade and verifying all tests pass. Existing example scripts should produce the same visual output.

**Acceptance Scenarios**:

1. **Given** the project references SkiaSharp 3.x, **When** the full test suite runs, **Then** all existing tests pass.
2. **Given** the project references SkiaSharp 3.x, **When** existing example scripts are executed, **Then** they produce the same visual output as before.
3. **Given** the SkiaSharp upgrade, **When** the Vulkan and GL rendering backends are used, **Then** both backends render correctly.

---

### Edge Cases

- What happens when a layout container has zero children? It should render as empty space with no errors.
- What happens when a graph has a single node and no edges? It should render the node centered in the available area.
- What happens when a graph has disconnected components? All components should be laid out and visible, not just the largest connected component.
- What happens when a DAG definition contains a cycle? The system should detect the cycle and report an error rather than entering an infinite loop.
- What happens when edge weights are negative? The system should treat negative weights as valid values and render them (weight affects visual representation only, not layout correctness).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide layout containers that automatically arrange child elements in horizontal stacks, vertical stacks, and dock layouts.
- **FR-002**: Layout containers MUST support nesting (layouts within layouts) to arbitrary depth.
- **FR-003**: Layout containers MUST recalculate child positions and sizes when the available space changes (e.g., window resize).
- **FR-004**: System MUST provide a graph visualization element that renders directed acyclic graphs with automatic node positioning.
- **FR-005**: System MUST provide a graph visualization element that renders undirected graphs.
- **FR-006**: Graph edges MUST support a weight attribute that is visually represented (e.g., line thickness, label, or both).
- **FR-007**: Directed graph edges MUST display directional indicators (arrowheads) pointing from source to target.
- **FR-008**: Undirected graph edges MUST NOT display directional indicators.
- **FR-009**: Graph visualization MUST automatically lay out nodes without overlap using an appropriate algorithm (e.g., layered layout for DAGs, force-directed for undirected graphs).
- **FR-010**: System MUST detect cycles in a graph declared as a DAG and report an error.
- **FR-011**: Graph nodes MUST support customizable labels and visual styling (color, size).
- **FR-012**: Graph edges MUST support customizable visual styling (color, thickness, dash pattern).
- **FR-013**: Graph visualization elements MUST be composable within layout containers, respecting parent-allocated bounds.
- **FR-014**: Layout and graph elements MUST integrate with the existing declarative scene DSL.
- **FR-015**: System MUST render disconnected graph components without discarding any component.

### Key Entities

- **LayoutContainer**: Represents an arrangement strategy (horizontal stack, vertical stack, dock) with child elements, spacing, and alignment properties.
- **GraphDefinition**: Represents a graph structure consisting of nodes and edges, with a graph type (directed/undirected) and layout preferences.
- **GraphNode**: Represents a vertex in a graph with an identifier, label, and visual style.
- **GraphEdge**: Represents a connection between two nodes with optional weight, direction, and visual style.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can arrange 10+ elements in nested layouts without specifying any absolute coordinates and achieve the expected visual result.
- **SC-002**: A DAG with 100 nodes and 150 edges renders with no overlapping nodes and completes layout within 2 seconds.
- **SC-003**: An undirected weighted graph with 50 nodes renders with visually distinguishable edge weights.
- **SC-004**: Resizing the window causes layouts and embedded graphs to re-layout within 500 milliseconds, maintaining readability.
- **SC-005**: Graph elements embedded in layout containers stay within their allocated bounds at all tested window sizes.
- **SC-006**: A developer can define a complete layout-with-graph scene using only the declarative DSL, without imperative positioning code.

## Assumptions

- The project will upgrade from SkiaSharp 2.88.6 to SkiaSharp 3.x as a prerequisite for this feature.
- UILayout (https://github.com/mikeoliphant/UILayout) layout concepts will be ported to F# (UILayout is C#, not on NuGet, and uses shared projects incompatible with the F#-only constitution constraint).
- MSAGL (Microsoft Automatic Graph Layout) will be used for graph layout computation, providing node positions and edge curves. SkiaViewer renders the computed geometry via SkiaSharp — no MSAGL UI dependencies are used.
- Graph visualization is read-only in this iteration; interactive features (dragging nodes, zooming, panning) are out of scope.
- The existing scene DSL element types (Rect, Text, etc.) can be used as graph node content.
- Performance targets assume rendering on a modern desktop system.

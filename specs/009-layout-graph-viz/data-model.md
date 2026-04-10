# Data Model: Layout System & Graph Visualization

**Feature**: 009-layout-graph-viz  
**Date**: 2026-04-09

## Layout Types

### LayoutAlignment

```
HorizontalAlignment: Left | Center | Right | Stretch
VerticalAlignment: Top | Center | Bottom | Stretch
```

### LayoutPadding

```
LayoutPadding:
  Left: float32
  Top: float32
  Right: float32
  Bottom: float32
```

### LayoutSizing

```
LayoutSizing:
  DesiredWidth: float32 option    -- None = fill available
  DesiredHeight: float32 option   -- None = fill available
  MinWidth: float32 option
  MinHeight: float32 option
  MaxWidth: float32 option
  MaxHeight: float32 option
```

### LayoutElement (extends Scene.Element DU)

```
LayoutElement:
  | HStack of config: StackConfig * children: LayoutChild list
  | VStack of config: StackConfig * children: LayoutChild list
  | Dock of config: DockConfig * children: DockChild list

StackConfig:
  Spacing: float32
  Padding: LayoutPadding
  HAlign: HorizontalAlignment    -- alignment of the stack itself
  VAlign: VerticalAlignment

DockConfig:
  Padding: LayoutPadding
  LastChildFill: bool            -- dock's last child fills remaining space

LayoutChild:
  Element: Element               -- any Scene.Element
  Sizing: LayoutSizing
  HAlign: HorizontalAlignment
  VAlign: VerticalAlignment

DockChild:
  Element: Element
  Dock: DockPosition             -- Top | Bottom | Left | Right | Fill
  Sizing: LayoutSizing
```

### DockPosition

```
DockPosition: Top | Bottom | Left | Right | Fill
```

## Graph Types

### GraphKind

```
GraphKind: Directed | Undirected
```

### GraphNode

```
GraphNode:
  Id: string                     -- unique identifier
  Label: string                  -- display text
  Style: NodeStyle option        -- visual customization
```

### NodeStyle

```
NodeStyle:
  FillColor: SKColor option
  StrokeColor: SKColor option
  Width: float32 option          -- overrides auto-sizing
  Height: float32 option
  FontSize: float32 option
  Shape: NodeShape               -- Rectangle | Ellipse | RoundedRect
```

### NodeShape

```
NodeShape: Rectangle | Ellipse | RoundedRect of cornerRadius: float32
```

### GraphEdge

```
GraphEdge:
  Source: string                 -- source node Id
  Target: string                 -- target node Id
  Weight: float option           -- None = unweighted
  Label: string option           -- optional edge label
  Style: EdgeStyle option        -- visual customization
```

### EdgeStyle

```
EdgeStyle:
  Color: SKColor option
  Thickness: float32 option      -- overrides weight-based thickness
  DashPattern: float32[] option  -- e.g., [| 5f; 3f |]
  ShowLabel: bool                -- display weight/label on edge
```

### GraphConfig

```
GraphConfig:
  Kind: GraphKind
  LayoutDirection: LayoutDirection   -- for DAGs
  NodeSpacing: float32               -- minimum distance between nodes
  LayerSpacing: float32              -- distance between layers (DAGs)
  DefaultNodeStyle: NodeStyle
  DefaultEdgeStyle: EdgeStyle
  ShowArrowheads: bool option        -- None = auto (true for Directed)

LayoutDirection: TopToBottom | LeftToRight | BottomToTop | RightToLeft
```

### GraphDefinition

```
GraphDefinition:
  Config: GraphConfig
  Nodes: GraphNode list
  Edges: GraphEdge list
```

## Entity Relationships

```
GraphDefinition 1──* GraphNode     (contains nodes)
GraphDefinition 1──* GraphEdge     (contains edges)
GraphEdge *──1 GraphNode.Id        (Source references a node)
GraphEdge *──1 GraphNode.Id        (Target references a node)
GraphConfig 1──1 GraphDefinition   (configuration for the graph)

LayoutElement *──* Element         (layout contains scene elements)
LayoutElement *──* LayoutElement   (layouts nest recursively)
GraphDefinition ──> Element        (graph renders as Element tree)
```

## Validation Rules

- GraphNode.Id must be unique within a GraphDefinition.
- GraphEdge.Source and GraphEdge.Target must reference existing node Ids.
- For Directed graphs: if cycles are detected, return Error with cycle path.
- GraphEdge.Weight: any float value is valid (negative, zero, positive).
- LayoutSizing: MinWidth <= MaxWidth, MinHeight <= MaxHeight when both specified.
- LayoutPadding: all values >= 0.
- StackConfig.Spacing >= 0.

## State Transitions

None — all types are immutable. A new GraphDefinition or LayoutElement is created for each render frame. Layout computation and graph layout are pure functions: input definition → output Element tree.

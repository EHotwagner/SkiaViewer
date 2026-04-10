# Contract: SkiaViewer.Layout Public API

**Feature**: 009-layout-graph-viz  
**Date**: 2026-04-09  
**Type**: F# library (consumed via project reference or NuGet)

## Module: SkiaViewer.Layout.Types

All types defined in `Types.fsi`. Consumers import via `open SkiaViewer.Layout`.

### Layout Types (public)

```fsharp
type HorizontalAlignment = Left | Center | Right | Stretch
type VerticalAlignment = Top | Center | Bottom | Stretch
type DockPosition = Top | Bottom | Left | Right | Fill
type NodeShape = Rectangle | Ellipse | RoundedRect of cornerRadius: float32
type GraphKind = Directed | Undirected
type LayoutDirection = TopToBottom | LeftToRight | BottomToTop | RightToLeft

type LayoutPadding = { Left: float32; Top: float32; Right: float32; Bottom: float32 }
type LayoutSizing = { DesiredWidth: float32 option; DesiredHeight: float32 option; MinWidth: float32 option; MinHeight: float32 option; MaxWidth: float32 option; MaxHeight: float32 option }
type StackConfig = { Spacing: float32; Padding: LayoutPadding; HAlign: HorizontalAlignment; VAlign: VerticalAlignment }
type DockConfig = { Padding: LayoutPadding; LastChildFill: bool }
type LayoutChild = { Element: Element; Sizing: LayoutSizing; HAlign: HorizontalAlignment; VAlign: VerticalAlignment }
type DockChild = { Element: Element; Dock: DockPosition; Sizing: LayoutSizing }
type NodeStyle = { FillColor: SKColor option; StrokeColor: SKColor option; Width: float32 option; Height: float32 option; FontSize: float32 option; Shape: NodeShape }
type EdgeStyle = { Color: SKColor option; Thickness: float32 option; DashPattern: float32[] option; ShowLabel: bool }
type GraphNode = { Id: string; Label: string; Style: NodeStyle option }
type GraphEdge = { Source: string; Target: string; Weight: float option; Label: string option; Style: EdgeStyle option }
type GraphConfig = { Kind: GraphKind; LayoutDirection: LayoutDirection; NodeSpacing: float32; LayerSpacing: float32; DefaultNodeStyle: NodeStyle; DefaultEdgeStyle: EdgeStyle; ShowArrowheads: bool option }
type GraphDefinition = { Config: GraphConfig; Nodes: GraphNode list; Edges: GraphEdge list }
```

## Module: SkiaViewer.Layout.Defaults

```fsharp
module Defaults =
    val padding: LayoutPadding
    val sizing: LayoutSizing
    val stackConfig: StackConfig
    val dockConfig: DockConfig
    val nodeStyle: NodeStyle
    val edgeStyle: EdgeStyle
    val graphConfig: kind: GraphKind -> GraphConfig
```

## Module: SkiaViewer.Layout.Layout

```fsharp
module Layout =
    /// Arrange children in a horizontal stack within the given bounds.
    val hstack: config: StackConfig -> children: LayoutChild list -> width: float32 -> height: float32 -> Element

    /// Arrange children in a vertical stack within the given bounds.
    val vstack: config: StackConfig -> children: LayoutChild list -> width: float32 -> height: float32 -> Element

    /// Arrange children in a dock layout within the given bounds.
    val dock: config: DockConfig -> children: DockChild list -> width: float32 -> height: float32 -> Element

    /// Create a LayoutChild with default sizing and alignment.
    val child: element: Element -> LayoutChild

    /// Create a LayoutChild with specified sizing.
    val childWithSize: width: float32 -> height: float32 -> element: Element -> LayoutChild

    /// Create a DockChild.
    val dockChild: position: DockPosition -> element: Element -> DockChild
```

## Module: SkiaViewer.Layout.Graph

```fsharp
module Graph =
    /// Render a graph definition as a Scene Element within the given bounds.
    /// Returns Error if the graph is invalid (e.g., DAG with cycle, missing node references).
    val render: graph: GraphDefinition -> width: float32 -> height: float32 -> Result<Element, string>

    /// Create a default graph config for the given kind.
    val defaultConfig: kind: GraphKind -> GraphConfig

    /// Validate a graph definition without rendering.
    val validate: graph: GraphDefinition -> Result<unit, string list>
```

## Stability Guarantees

- All types are immutable records and discriminated unions.
- Functions are pure: same input produces same output.
- `Graph.render` returns `Result` to surface validation errors without exceptions.
- Breaking changes require spec amendment and surface-area baseline update.

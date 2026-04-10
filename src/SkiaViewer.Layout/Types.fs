namespace SkiaViewer.Layout

open SkiaSharp
open SkiaViewer

[<RequireQualifiedAccess>]
type HorizontalAlignment = Left | Center | Right | Stretch

[<RequireQualifiedAccess>]
type VerticalAlignment = Top | Center | Bottom | Stretch

[<RequireQualifiedAccess>]
type DockPosition = Top | Bottom | Left | Right | Fill

[<RequireQualifiedAccess>]
type NodeShape = Rectangle | Ellipse | RoundedRect of cornerRadius: float32

[<RequireQualifiedAccess>]
type GraphKind = Directed | Undirected

[<RequireQualifiedAccess>]
type LayoutDirection = TopToBottom | LeftToRight | BottomToTop | RightToLeft

type LayoutPadding =
    { Left: float32
      Top: float32
      Right: float32
      Bottom: float32 }

type LayoutSizing =
    { DesiredWidth: float32 option
      DesiredHeight: float32 option
      MinWidth: float32 option
      MinHeight: float32 option
      MaxWidth: float32 option
      MaxHeight: float32 option }

type StackConfig =
    { Spacing: float32
      Padding: LayoutPadding
      HAlign: HorizontalAlignment
      VAlign: VerticalAlignment }

type DockConfig =
    { Padding: LayoutPadding
      LastChildFill: bool }

type LayoutChild =
    { Element: Element
      Sizing: LayoutSizing
      HAlign: HorizontalAlignment
      VAlign: VerticalAlignment }

type DockChild =
    { Element: Element
      Dock: DockPosition
      Sizing: LayoutSizing }

type NodeStyle =
    { FillColor: SKColor option
      StrokeColor: SKColor option
      Width: float32 option
      Height: float32 option
      FontSize: float32 option
      Shape: NodeShape }

type EdgeStyle =
    { Color: SKColor option
      Thickness: float32 option
      DashPattern: float32[] option
      ShowLabel: bool }

type GraphNode =
    { Id: string
      Label: string
      Style: NodeStyle option }

type GraphEdge =
    { Source: string
      Target: string
      Weight: float option
      Label: string option
      Style: EdgeStyle option }

type GraphConfig =
    { Kind: GraphKind
      LayoutDirection: LayoutDirection
      NodeSpacing: float32
      LayerSpacing: float32
      DefaultNodeStyle: NodeStyle
      DefaultEdgeStyle: EdgeStyle
      ShowArrowheads: bool option }

type GraphDefinition =
    { Config: GraphConfig
      Nodes: GraphNode list
      Edges: GraphEdge list }

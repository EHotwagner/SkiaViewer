namespace SkiaViewer.Layout

open SkiaSharp

module Defaults =

    let padding : LayoutPadding =
        { Left = 0f; Top = 0f; Right = 0f; Bottom = 0f }

    let sizing : LayoutSizing =
        { DesiredWidth = None
          DesiredHeight = None
          MinWidth = None
          MinHeight = None
          MaxWidth = None
          MaxHeight = None }

    let stackConfig : StackConfig =
        { Spacing = 0f
          Padding = padding
          HAlign = HorizontalAlignment.Left
          VAlign = VerticalAlignment.Top }

    let dockConfig : DockConfig =
        { Padding = padding
          LastChildFill = true }

    let nodeStyle : NodeStyle =
        { FillColor = Some (SKColor(0xFFuy, 0xFFuy, 0xFFuy))
          StrokeColor = Some (SKColor(0x00uy, 0x00uy, 0x00uy))
          Width = None
          Height = None
          FontSize = None
          Shape = NodeShape.Rectangle }

    let edgeStyle : EdgeStyle =
        { Color = Some (SKColor(0x00uy, 0x00uy, 0x00uy))
          Thickness = None
          DashPattern = None
          ShowLabel = false }

    let graphConfig (kind: GraphKind) : GraphConfig =
        { Kind = kind
          LayoutDirection = LayoutDirection.TopToBottom
          NodeSpacing = 30f
          LayerSpacing = 50f
          DefaultNodeStyle = nodeStyle
          DefaultEdgeStyle = edgeStyle
          ShowArrowheads = None }

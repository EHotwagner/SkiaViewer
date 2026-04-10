namespace SkiaViewer.Layout

open SkiaSharp
open SkiaViewer
open Microsoft.Msagl.Core.Layout
open Microsoft.Msagl.Core.Geometry
open Microsoft.Msagl.Core.Geometry.Curves
open Microsoft.Msagl.Layout.Layered
open Microsoft.Msagl.Layout.MDS

module Graph =

    let defaultConfig (kind: GraphKind) : GraphConfig =
        Defaults.graphConfig kind

    let validate (graph: GraphDefinition) : Result<unit, string list> =
        GraphValidation.validate graph

    let private defaultNodeWidth = 80f
    let private defaultNodeHeight = 40f
    let private defaultFontSize = 12f

    let private resolveNodeStyle (defaults: NodeStyle) (nodeStyle: NodeStyle option) =
        match nodeStyle with
        | Some s -> s
        | None -> defaults

    let private resolveEdgeStyle (defaults: EdgeStyle) (edgeStyle: EdgeStyle option) =
        match edgeStyle with
        | Some s -> s
        | None -> defaults

    let private buildGeometryGraph (graph: GraphDefinition) : GeometryGraph * System.Collections.Generic.Dictionary<string, Node> =
        let gg = new GeometryGraph()
        let nodeMap = System.Collections.Generic.Dictionary<string, Node>()

        for n in graph.Nodes do
            let style = resolveNodeStyle graph.Config.DefaultNodeStyle n.Style
            let w = style.Width |> Option.defaultValue defaultNodeWidth |> float
            let h = style.Height |> Option.defaultValue defaultNodeHeight |> float
            let boundary =
                match style.Shape with
                | NodeShape.Rectangle -> CurveFactory.CreateRectangle(w, h, new Point(0.0, 0.0))
                | NodeShape.Ellipse -> CurveFactory.CreateEllipse(w / 2.0, h / 2.0, new Point(0.0, 0.0))
                | NodeShape.RoundedRect r ->
                    CurveFactory.CreateRectangleWithRoundedCorners(w, h, float r, float r, new Point(0.0, 0.0))
            let msaglNode = new Node(boundary)
            gg.Nodes.Add(msaglNode)
            nodeMap.[n.Id] <- msaglNode

        for e in graph.Edges do
            if nodeMap.ContainsKey e.Source && nodeMap.ContainsKey e.Target then
                let edge = new Edge(nodeMap.[e.Source], nodeMap.[e.Target])
                gg.Edges.Add(edge)

        (gg, nodeMap)

    let private layoutGraph (gg: GeometryGraph) (config: GraphConfig) =
        match config.Kind with
        | GraphKind.Directed ->
            let settings = SugiyamaLayoutSettings()
            settings.NodeSeparation <- float config.NodeSpacing
            settings.LayerSeparation <- float config.LayerSpacing
            match config.LayoutDirection with
            | LayoutDirection.TopToBottom ->
                settings.Transformation <- PlaneTransformation.Rotation(0.0)
            | LayoutDirection.LeftToRight ->
                settings.Transformation <- PlaneTransformation.Rotation(System.Math.PI / 2.0)
            | LayoutDirection.BottomToTop ->
                settings.Transformation <- PlaneTransformation.Rotation(System.Math.PI)
            | LayoutDirection.RightToLeft ->
                settings.Transformation <- PlaneTransformation.Rotation(-System.Math.PI / 2.0)
            let layout = new LayeredLayout(gg, settings)
            layout.Run()
        | GraphKind.Undirected ->
            let settings = MdsLayoutSettings()
            let layout = new MdsGraphLayout(settings, gg)
            layout.Run()
            // MDS doesn't route edges — create straight line segments
            for e in gg.Edges do
                if isNull e.Curve then
                    e.Curve <- new LineSegment(e.Source.Center, e.Target.Center)

    let private scaleAndTranslate (gg: GeometryGraph) (width: float32) (height: float32) =
        let bb = gg.BoundingBox
        let graphW = float32 bb.Width
        let graphH = float32 bb.Height
        let margin = 10f
        let availW = width - 2f * margin
        let availH = height - 2f * margin
        let scaleX = if graphW > 0f then availW / graphW else 1f
        let scaleY = if graphH > 0f then availH / graphH else 1f
        let scale = min scaleX scaleY
        let offsetX = margin + (availW - graphW * scale) / 2f - float32 bb.Left * scale
        let offsetY = margin + (availH - graphH * scale) / 2f - float32 bb.Bottom * scale
        (scale, offsetX, offsetY)

    let private transformPoint (scale: float32) (offsetX: float32) (offsetY: float32) (p: Point) =
        (float32 p.X * scale + offsetX, float32 p.Y * scale + offsetY)

    let private renderNode (scale: float32) (offsetX: float32) (offsetY: float32) (graphNode: GraphNode) (msaglNode: Node) (defaultStyle: NodeStyle) : Element list =
        let style = resolveNodeStyle defaultStyle graphNode.Style
        let cx, cy = transformPoint scale offsetX offsetY msaglNode.Center
        let w = float32 msaglNode.BoundingBox.Width * scale
        let h = float32 msaglNode.BoundingBox.Height * scale
        let x = cx - w / 2f
        let y = cy - h / 2f

        let fillColor = style.FillColor |> Option.defaultValue SKColors.White
        let strokeColor = style.StrokeColor |> Option.defaultValue SKColors.Black
        let fontSize = style.FontSize |> Option.defaultValue defaultFontSize

        let fillPaint = Scene.fill fillColor
        let strokePaint = Scene.stroke strokeColor 1f

        let shapeElements =
            match style.Shape with
            | NodeShape.Rectangle ->
                [ Scene.rect x y w h fillPaint
                  Scene.rect x y w h strokePaint ]
            | NodeShape.Ellipse ->
                let rx = w / 2f
                let ry = h / 2f
                [ Element.Ellipse(cx, cy, rx, ry, fillPaint)
                  Element.Ellipse(cx, cy, rx, ry, strokePaint) ]
            | NodeShape.RoundedRect r ->
                let commands = [ PathCommand.AddRoundRect(SKRect(x, y, x + w, y + h), r, r, PathDirection.Clockwise) ]
                [ Scene.path commands fillPaint
                  Scene.path commands strokePaint ]

        let textElement =
            let textPaint = Scene.fill SKColors.Black
            [ Scene.text graphNode.Label (cx - w / 4f) (cy + fontSize / 3f) fontSize textPaint ]

        shapeElements @ textElement

    let private computeWeightThickness (weight: float option) (style: EdgeStyle) (allWeights: float list) : float32 =
        match style.Thickness with
        | Some t -> t
        | None ->
            match weight with
            | None -> 1f
            | Some w ->
                if allWeights.IsEmpty then 1f
                else
                    let minW = allWeights |> List.min
                    let maxW = allWeights |> List.max
                    let range = maxW - minW
                    if range <= 0.0 then 2f
                    else
                        let normalized = (w - minW) / range
                        1f + float32 normalized * 5f // 1px to 6px

    let private renderEdge (scale: float32) (offsetX: float32) (offsetY: float32) (edge: Edge) (graphEdge: GraphEdge) (defaultStyle: EdgeStyle) (showArrowheads: bool) (allWeights: float list) : Element list =
        let style = resolveEdgeStyle defaultStyle graphEdge.Style
        let color = style.Color |> Option.defaultValue SKColors.Black
        let thickness = computeWeightThickness graphEdge.Weight style allWeights
        let paint =
            let basePaint = Scene.stroke color thickness
            match style.DashPattern with
            | Some pattern ->
                { basePaint with PathEffect = Some (PathEffect.Dash(pattern, 0f)) }
            | None -> basePaint

        let curve = edge.Curve
        if isNull curve then []
        else
            let commands = ResizeArray<PathCommand>()
            match curve with
            | :? LineSegment as ls ->
                let (sx, sy) = transformPoint scale offsetX offsetY ls.Start
                let (ex, ey) = transformPoint scale offsetX offsetY ls.End
                commands.Add(PathCommand.MoveTo(sx, sy))
                commands.Add(PathCommand.LineTo(ex, ey))
            | :? Curve as cc ->
                let mutable first = true
                for seg in cc.Segments do
                    match seg with
                    | :? LineSegment as ls ->
                        let (sx, sy) = transformPoint scale offsetX offsetY ls.Start
                        let (ex, ey) = transformPoint scale offsetX offsetY ls.End
                        if first then
                            commands.Add(PathCommand.MoveTo(sx, sy))
                            first <- false
                        commands.Add(PathCommand.LineTo(ex, ey))
                    | :? CubicBezierSegment as cb ->
                        let (sx, sy) = transformPoint scale offsetX offsetY (cb.B 0)
                        let (c1x, c1y) = transformPoint scale offsetX offsetY (cb.B 1)
                        let (c2x, c2y) = transformPoint scale offsetX offsetY (cb.B 2)
                        let (ex, ey) = transformPoint scale offsetX offsetY (cb.B 3)
                        if first then
                            commands.Add(PathCommand.MoveTo(sx, sy))
                            first <- false
                        commands.Add(PathCommand.CubicTo(c1x, c1y, c2x, c2y, ex, ey))
                    | _ -> ()
            | _ -> ()

            let edgeElements =
                if commands.Count > 0 then
                    [ Scene.path (commands |> Seq.toList) paint ]
                else []

            let arrowElements =
                if showArrowheads && commands.Count > 0 then
                    // Draw arrowhead at end of edge
                    let lastCmd = commands.[commands.Count - 1]
                    let (tipX, tipY, prevX, prevY) =
                        match lastCmd with
                        | PathCommand.LineTo(ex, ey) ->
                            if commands.Count >= 2 then
                                let prev = commands.[commands.Count - 2]
                                match prev with
                                | PathCommand.MoveTo(px, py) | PathCommand.LineTo(px, py) -> (ex, ey, px, py)
                                | _ -> (ex, ey, ex - 1f, ey)
                            else (ex, ey, ex - 1f, ey)
                        | PathCommand.CubicTo(_, _, c2x, c2y, ex, ey) -> (ex, ey, c2x, c2y)
                        | _ -> (0f, 0f, 0f, 0f)

                    let dx = tipX - prevX
                    let dy = tipY - prevY
                    let len = sqrt (dx * dx + dy * dy)
                    if len > 0f then
                        let arrowLen = 8f * scale
                        let arrowWidth = 4f * scale
                        let nx = dx / len
                        let ny = dy / len
                        let baseX = tipX - nx * arrowLen
                        let baseY = tipY - ny * arrowLen
                        let perpX = -ny
                        let perpY = nx
                        let p1x = baseX + perpX * arrowWidth
                        let p1y = baseY + perpY * arrowWidth
                        let p2x = baseX - perpX * arrowWidth
                        let p2y = baseY - perpY * arrowWidth
                        let arrowCmds = [
                            PathCommand.MoveTo(tipX, tipY)
                            PathCommand.LineTo(p1x, p1y)
                            PathCommand.LineTo(p2x, p2y)
                            PathCommand.Close
                        ]
                        [ Scene.path arrowCmds (Scene.fill color) ]
                    else []
                else []

            let labelElements =
                let showLabel = style.ShowLabel
                if showLabel && commands.Count >= 2 then
                    let labelText =
                        match graphEdge.Label with
                        | Some l -> l
                        | None ->
                            match graphEdge.Weight with
                            | Some w -> $"%.1f{w}"
                            | None -> ""
                    if labelText <> "" then
                        // Find midpoint of edge
                        let (midX, midY) =
                            let first = commands.[0]
                            let last = commands.[commands.Count - 1]
                            let (sx, sy) =
                                match first with
                                | PathCommand.MoveTo(x, y) -> (x, y)
                                | _ -> (0f, 0f)
                            let (ex, ey) =
                                match last with
                                | PathCommand.LineTo(x, y) -> (x, y)
                                | PathCommand.CubicTo(_, _, _, _, x, y) -> (x, y)
                                | _ -> (sx, sy)
                            ((sx + ex) / 2f, (sy + ey) / 2f)
                        let labelFontSize = 10f
                        [ Scene.text labelText midX (midY - 3f) labelFontSize (Scene.fill color) ]
                    else []
                else []

            edgeElements @ arrowElements @ labelElements

    let render (graph: GraphDefinition) (width: float32) (height: float32) : Result<Element, string> =
        match GraphValidation.validate graph with
        | Error errors -> Error (System.String.Join("; ", errors))
        | Ok () ->
            if graph.Nodes.IsEmpty then
                Ok (Scene.group None None [])
            else
                let (gg, nodeMap) = buildGeometryGraph graph
                layoutGraph gg graph.Config
                let (scale, offsetX, offsetY) = scaleAndTranslate gg width height

                let showArrowheads =
                    match graph.Config.ShowArrowheads with
                    | Some v -> v
                    | None -> graph.Config.Kind = GraphKind.Directed

                let allWeights =
                    graph.Edges
                    |> List.choose (fun e -> e.Weight)

                // Render edges first (behind nodes)
                let edgeElements =
                    graph.Edges
                    |> List.mapi (fun i ge ->
                        let msaglEdges = gg.Edges |> Seq.toArray
                        if i < msaglEdges.Length then
                            renderEdge scale offsetX offsetY msaglEdges.[i] ge graph.Config.DefaultEdgeStyle showArrowheads allWeights
                        else [])
                    |> List.concat

                // Render nodes
                let nodeElements =
                    graph.Nodes
                    |> List.collect (fun gn ->
                        if nodeMap.ContainsKey gn.Id then
                            renderNode scale offsetX offsetY gn nodeMap.[gn.Id] graph.Config.DefaultNodeStyle
                        else [])

                Ok (Scene.group None None (edgeElements @ nodeElements))

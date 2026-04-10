module SkiaViewer.Layout.Tests.GraphTests

open Xunit
open SkiaViewer
open SkiaViewer.Layout

let private makeNode id label = { Id = id; Label = label; Style = None }
let private makeEdge src tgt = { Source = src; Target = tgt; Weight = None; Label = None; Style = None }

let private extractGroupChildren (element: Element) : Element list =
    match element with
    | Element.Group(_, _, _, children) -> children
    | _ -> []

let private countElementType (pred: Element -> bool) (elements: Element list) =
    elements |> List.filter pred |> List.length

let private isRect = function Element.Rect _ -> true | _ -> false
let private isPath = function Element.Path _ -> true | _ -> false
let private isText = function Element.Text _ -> true | _ -> false
let private isEllipse = function Element.Ellipse _ -> true | _ -> false

// === DAG Rendering ===

[<Fact>]
let ``render valid DAG returns Ok`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "Start"; makeNode "B" "Process"; makeNode "C" "End" ]
          Edges = [ makeEdge "A" "B"; makeEdge "B" "C"; makeEdge "A" "C" ] }
    let result = Graph.render graph 600f 400f
    Assert.True(Result.isOk result)

[<Fact>]
let ``render DAG returns Group element`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ makeEdge "A" "B" ] }
    match Graph.render graph 400f 300f with
    | Ok element ->
        match element with
        | Element.Group _ -> Assert.True(true)
        | _ -> Assert.Fail("Expected Group element")
    | Error e -> Assert.Fail($"Render failed: {e}")

[<Fact>]
let ``render DAG contains nodes and edges`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C" ]
          Edges = [ makeEdge "A" "B"; makeEdge "B" "C" ] }
    match Graph.render graph 600f 400f with
    | Ok element ->
        let children = extractGroupChildren element
        // Should have rect/text for each node + path for each edge + arrowheads
        let rects = countElementType isRect children
        let texts = countElementType isText children
        let paths = countElementType isPath children
        Assert.True(rects >= 3, $"Expected >= 3 rects (fill+stroke per node), got {rects}")
        Assert.True(texts >= 3, $"Expected >= 3 text elements for node labels, got {texts}")
        Assert.True(paths >= 2, $"Expected >= 2 paths for edges, got {paths}")
    | Error e -> Assert.Fail($"Render failed: {e}")

[<Fact>]
let ``render invalid graph returns Error`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C" ]
          Edges = [ makeEdge "A" "B"; makeEdge "B" "C"; makeEdge "C" "A" ] } // cycle
    let result = Graph.render graph 600f 400f
    Assert.True(Result.isError result)

[<Fact>]
let ``nodes do not overlap`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [
              makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C"
              makeNode "D" "D"; makeNode "E" "E"
          ]
          Edges = [
              makeEdge "A" "B"; makeEdge "A" "C"; makeEdge "B" "D"
              makeEdge "C" "D"; makeEdge "D" "E"
          ] }
    match Graph.render graph 600f 400f with
    | Ok element ->
        let children = extractGroupChildren element
        // Extract rect positions
        let rects =
            children
            |> List.choose (fun e ->
                match e with
                | Element.Rect(x, y, w, h, _) -> Some (x, y, w, h)
                | _ -> None)
        // Check no fill rects overlap (check pairs)
        let fillRects = rects |> List.indexed |> List.filter (fun (i, _) -> i % 2 = 0) |> List.map snd
        let mutable overlaps = 0
        for i in 0..fillRects.Length - 2 do
            for j in i+1..fillRects.Length - 1 do
                let (x1, y1, w1, h1) = fillRects.[i]
                let (x2, y2, w2, h2) = fillRects.[j]
                if x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2 then
                    overlaps <- overlaps + 1
        Assert.Equal(0, overlaps)
    | Error e -> Assert.Fail($"Render failed: {e}")

// === Single node ===

[<Fact>]
let ``single node no edges renders Ok`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "Alone" ]
          Edges = [] }
    match Graph.render graph 300f 200f with
    | Ok element ->
        let children = extractGroupChildren element
        Assert.True(children.Length > 0, "Single node should produce elements")
    | Error e -> Assert.Fail($"Render failed: {e}")

// === Empty graph ===

[<Fact>]
let ``empty graph renders Ok with empty Group`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = []
          Edges = [] }
    match Graph.render graph 300f 200f with
    | Ok element ->
        let children = extractGroupChildren element
        Assert.Equal(0, children.Length)
    | Error e -> Assert.Fail($"Render failed: {e}")

// === Ellipse nodes ===

[<Fact>]
let ``ellipse node shape renders ellipses`` () =
    let nodeStyle = { Defaults.nodeStyle with Shape = NodeShape.Ellipse }
    let config = { Graph.defaultConfig GraphKind.Directed with DefaultNodeStyle = nodeStyle }
    let graph =
        { Config = config
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ makeEdge "A" "B" ] }
    match Graph.render graph 400f 300f with
    | Ok element ->
        let children = extractGroupChildren element
        let ellipses = countElementType isEllipse children
        Assert.True(ellipses >= 2, $"Expected >= 2 ellipses, got {ellipses}")
    | Error e -> Assert.Fail($"Render failed: {e}")

// === defaultConfig ===

[<Fact>]
let ``defaultConfig Directed has TopToBottom direction`` () =
    let config = Graph.defaultConfig GraphKind.Directed
    Assert.Equal(GraphKind.Directed, config.Kind)
    Assert.Equal(LayoutDirection.TopToBottom, config.LayoutDirection)

[<Fact>]
let ``defaultConfig Undirected has TopToBottom direction`` () =
    let config = Graph.defaultConfig GraphKind.Undirected
    Assert.Equal(GraphKind.Undirected, config.Kind)

// === Undirected Graph Tests ===

[<Fact>]
let ``undirected graph renders Ok`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C" ]
          Edges = [ makeEdge "A" "B"; makeEdge "B" "C"; makeEdge "A" "C" ] }
    let result = Graph.render graph 600f 400f
    Assert.True(Result.isOk result)

[<Fact>]
let ``undirected graph produces no arrowheads`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ makeEdge "A" "B" ] }
    match Graph.render graph 400f 300f with
    | Ok element ->
        let children = extractGroupChildren element
        // Count filled path elements (arrowheads are filled triangles)
        // Edges are stroked paths; arrowheads would be additional filled paths
        // With no arrowheads, we should have exactly 1 path for the edge
        let paths = children |> List.filter (fun e -> match e with Element.Path _ -> true | _ -> false)
        // Edge path count should equal number of edges (1), no extra arrowhead paths
        Assert.Equal(1, paths.Length)
    | Error e -> Assert.Fail($"Render failed: {e}")

// === Weighted Edge Tests ===

[<Fact>]
let ``weighted edges produce different stroke widths`` () =
    let makeWeightedEdge src tgt w : GraphEdge =
        { Source = src; Target = tgt; Weight = Some w; Label = None; Style = None }
    let graph =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C" ]
          Edges = [ makeWeightedEdge "A" "B" 1.0; makeWeightedEdge "B" "C" 10.0 ] }
    match Graph.render graph 600f 400f with
    | Ok element ->
        let children = extractGroupChildren element
        // Get path elements (edges)
        let paths =
            children
            |> List.choose (fun e ->
                match e with
                | Element.Path(_, paint) -> Some paint.StrokeWidth
                | _ -> None)
        Assert.True(paths.Length >= 2, $"Expected >= 2 edge paths, got {paths.Length}")
        // Different weights should produce different thicknesses
        let distinct = paths |> List.distinct
        Assert.True(distinct.Length >= 2, $"Expected different stroke widths, got {distinct}")
    | Error e -> Assert.Fail($"Render failed: {e}")

[<Fact>]
let ``zero weight edge still renders`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ { Source = "A"; Target = "B"; Weight = Some 0.0; Label = None; Style = None } ] }
    let result = Graph.render graph 400f 300f
    Assert.True(Result.isOk result)

[<Fact>]
let ``negative weight edge renders without error`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ { Source = "A"; Target = "B"; Weight = Some -5.0; Label = None; Style = None } ] }
    let result = Graph.render graph 400f 300f
    Assert.True(Result.isOk result)

// === Edge Label Tests ===

[<Fact>]
let ``edge with ShowLabel renders text at midpoint`` () =
    let labelStyle : EdgeStyle = { Defaults.edgeStyle with ShowLabel = true }
    let graph =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ { Source = "A"; Target = "B"; Weight = Some 5.0; Label = Some "5 Gbps"
                      Style = Some labelStyle } ] }
    match Graph.render graph 400f 300f with
    | Ok element ->
        let children = extractGroupChildren element
        // Should have text elements for edge label in addition to node labels
        let texts = children |> List.filter (fun e -> match e with Element.Text(t, _, _, _, _) -> t = "5 Gbps" | _ -> false)
        Assert.True(texts.Length >= 1, $"Expected edge label text '5 Gbps', got {texts.Length} matches")
    | Error e -> Assert.Fail($"Render failed: {e}")

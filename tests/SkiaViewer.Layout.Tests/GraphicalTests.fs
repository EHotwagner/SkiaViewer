module SkiaViewer.Layout.Tests.GraphicalTests

open Xunit
open SkiaSharp
open SkiaViewer
open SkiaViewer.Layout

let private makeNode id label : GraphNode = { Id = id; Label = label; Style = None }
let private makeEdge src tgt : GraphEdge = { Source = src; Target = tgt; Weight = None; Label = None; Style = None }

let private extractGroupChildren (element: Element) : Element list =
    match element with
    | Element.Group(_, _, _, children) -> children
    | _ -> []

// === US4: Composition Tests ===

[<Fact>]
let ``graph element works as LayoutChild in vstack`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ makeEdge "A" "B" ] }
    match Graph.render graph 580f 200f with
    | Ok graphElement ->
        let header = Scene.text "Title" 10f 30f 24f (Scene.fill SKColors.Black)
        let page =
            Layout.vstack { Defaults.stackConfig with Spacing = 10f } [
                Layout.childWithSize 580f 40f header
                Layout.child graphElement
            ] 600f 300f
        let children = extractGroupChildren page
        Assert.Equal(2, children.Length)
    | Error e -> Assert.Fail($"Graph render failed: {e}")

[<Fact>]
let ``graph element works as LayoutChild in hstack`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ makeEdge "A" "B" ] }
    match Graph.render graph 200f 200f with
    | Ok graphElement ->
        let sidebar = Scene.rect 0f 0f 100f 200f (Scene.fill SKColors.LightGray)
        let row =
            Layout.hstack Defaults.stackConfig [
                Layout.childWithSize 100f 200f sidebar
                Layout.child graphElement
            ] 400f 200f
        let children = extractGroupChildren row
        Assert.Equal(2, children.Length)
    | Error e -> Assert.Fail($"Graph render failed: {e}")

[<Fact>]
let ``graph element works inside dock layout`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C" ]
          Edges = [ makeEdge "A" "B"; makeEdge "B" "C" ] }
    match Graph.render graph 400f 300f with
    | Ok graphElement ->
        let header = Scene.rect 0f 0f 500f 40f (Scene.fill SKColors.DarkBlue)
        let topChild = { Layout.dockChild DockPosition.Top header
                         with Sizing = { Defaults.sizing with DesiredHeight = Some 40f } }
        let docked =
            Layout.dock Defaults.dockConfig [
                topChild
                Layout.dockChild DockPosition.Fill graphElement
            ] 500f 400f
        let children = extractGroupChildren docked
        Assert.Equal(2, children.Length)
    | Error e -> Assert.Fail($"Graph render failed: {e}")

[<Fact>]
let ``graph inside vstack with text header renders full element tree`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "Start"; makeNode "B" "End" ]
          Edges = [ makeEdge "A" "B" ] }
    match Graph.render graph 580f 350f with
    | Ok graphElement ->
        let title = Scene.text "Network Topology" 10f 30f 24f (Scene.fill SKColors.Black)
        let page =
            Layout.vstack { Defaults.stackConfig with Spacing = 10f; Padding = { Left = 10f; Top = 10f; Right = 10f; Bottom = 10f } } [
                Layout.childWithSize 580f 40f title
                Layout.child graphElement
            ] 600f 400f
        // The composed element should be a valid Group
        match page with
        | Element.Group(_, _, _, children) ->
            Assert.Equal(2, children.Length)
            // Each child is a translated Group
            for child in children do
                match child with
                | Element.Group(Some (Transform.Translate _), _, _, _) -> ()
                | _ -> Assert.Fail($"Expected translated group child, got {child}")
        | _ -> Assert.Fail("Expected Group element for composed layout")
    | Error e -> Assert.Fail($"Graph render failed: {e}")

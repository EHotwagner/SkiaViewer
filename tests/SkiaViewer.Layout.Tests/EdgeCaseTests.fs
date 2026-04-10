module SkiaViewer.Layout.Tests.EdgeCaseTests

open Xunit
open SkiaSharp
open SkiaViewer
open SkiaViewer.Layout

let private dummyElement = Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)

let private extractGroupChildren (element: Element) : Element list =
    match element with
    | Element.Group(_, _, _, children) -> children
    | _ -> []

let private extractTranslation (element: Element) : (float32 * float32) option =
    match element with
    | Element.Group(Some (Transform.Translate(x, y)), _, _, _) -> Some (x, y)
    | _ -> None

// === Empty container ===

[<Fact>]
let ``hstack with zero children produces empty group`` () =
    let result = Layout.hstack Defaults.stackConfig [] 300f 100f
    let group = extractGroupChildren result
    Assert.Empty(group)

[<Fact>]
let ``vstack with zero children produces empty group`` () =
    let result = Layout.vstack Defaults.stackConfig [] 200f 300f
    let group = extractGroupChildren result
    Assert.Empty(group)

[<Fact>]
let ``dock with zero children produces empty group`` () =
    let result = Layout.dock Defaults.dockConfig [] 300f 200f
    let group = extractGroupChildren result
    Assert.Empty(group)

// === Single child ===

[<Fact>]
let ``hstack with single child positions at origin`` () =
    let result = Layout.hstack Defaults.stackConfig [ Layout.childWithSize 50f 50f dummyElement ] 300f 100f
    let group = extractGroupChildren result
    Assert.Equal(1, group.Length)
    let (x, y) = (extractTranslation group.[0]).Value
    Assert.Equal(0f, x)
    Assert.Equal(0f, y)

[<Fact>]
let ``vstack with single child positions at origin`` () =
    let result = Layout.vstack Defaults.stackConfig [ Layout.childWithSize 50f 50f dummyElement ] 200f 300f
    let group = extractGroupChildren result
    Assert.Equal(1, group.Length)
    let (x, y) = (extractTranslation group.[0]).Value
    Assert.Equal(0f, x)
    Assert.Equal(0f, y)

// === Alignment variations ===

[<Fact>]
let ``vstack center alignment centers child horizontally`` () =
    let child = { Layout.childWithSize 50f 30f dummyElement with HAlign = HorizontalAlignment.Center }
    let result = Layout.vstack Defaults.stackConfig [ child ] 200f 100f
    let group = extractGroupChildren result
    let (x, _) = (extractTranslation group.[0]).Value
    Assert.Equal(75f, x) // (200 - 50) / 2

[<Fact>]
let ``vstack right alignment positions child at right`` () =
    let child = { Layout.childWithSize 50f 30f dummyElement with HAlign = HorizontalAlignment.Right }
    let result = Layout.vstack Defaults.stackConfig [ child ] 200f 100f
    let group = extractGroupChildren result
    let (x, _) = (extractTranslation group.[0]).Value
    Assert.Equal(150f, x) // 200 - 50

[<Fact>]
let ``hstack center alignment centers child vertically`` () =
    let child = { Layout.childWithSize 50f 30f dummyElement with VAlign = VerticalAlignment.Center }
    let result = Layout.hstack Defaults.stackConfig [ child ] 200f 100f
    let group = extractGroupChildren result
    let (_, y) = (extractTranslation group.[0]).Value
    Assert.Equal(35f, y) // (100 - 30) / 2

[<Fact>]
let ``hstack bottom alignment positions child at bottom`` () =
    let child = { Layout.childWithSize 50f 30f dummyElement with VAlign = VerticalAlignment.Bottom }
    let result = Layout.hstack Defaults.stackConfig [ child ] 200f 100f
    let group = extractGroupChildren result
    let (_, y) = (extractTranslation group.[0]).Value
    Assert.Equal(70f, y) // 100 - 30

// === Graph edge cases ===

let private makeNode id label : GraphNode = { Id = id; Label = label; Style = None }
let private makeEdge src tgt : GraphEdge = { Source = src; Target = tgt; Weight = None; Label = None; Style = None }

[<Fact>]
let ``disconnected components render without error`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C"; makeNode "D" "D" ]
          Edges = [ makeEdge "A" "B"; makeEdge "C" "D" ] } // Two disconnected pairs
    let result = Graph.render graph 600f 400f
    Assert.True(Result.isOk result)

module SkiaViewer.Layout.Tests.GraphValidationTests

open Xunit
open SkiaViewer.Layout

let private makeNode id label = { Id = id; Label = label; Style = None }
let private makeEdge src tgt = { Source = src; Target = tgt; Weight = None; Label = None; Style = None }

[<Fact>]
let ``valid DAG passes validation`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C" ]
          Edges = [ makeEdge "A" "B"; makeEdge "B" "C"; makeEdge "A" "C" ] }
    let result = GraphValidation.validate graph
    Assert.True(Result.isOk result)

[<Fact>]
let ``duplicate node IDs returns error`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "First"; makeNode "A" "Second"; makeNode "B" "B" ]
          Edges = [ makeEdge "A" "B" ] }
    let result = GraphValidation.validate graph
    match result with
    | Error errors ->
        Assert.True(errors |> List.exists (fun e -> e.Contains "Duplicate"))
    | Ok () -> Assert.Fail("Expected validation error for duplicate IDs")

[<Fact>]
let ``missing edge source returns error`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ makeEdge "X" "B" ] }
    let result = GraphValidation.validate graph
    match result with
    | Error errors ->
        Assert.True(errors |> List.exists (fun e -> e.Contains "X"))
    | Ok () -> Assert.Fail("Expected validation error for missing source")

[<Fact>]
let ``missing edge target returns error`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B" ]
          Edges = [ makeEdge "A" "Z" ] }
    let result = GraphValidation.validate graph
    match result with
    | Error errors ->
        Assert.True(errors |> List.exists (fun e -> e.Contains "Z"))
    | Ok () -> Assert.Fail("Expected validation error for missing target")

[<Fact>]
let ``cycle in directed graph returns error`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C" ]
          Edges = [ makeEdge "A" "B"; makeEdge "B" "C"; makeEdge "C" "A" ] }
    let result = GraphValidation.validate graph
    match result with
    | Error errors ->
        Assert.True(errors |> List.exists (fun e -> e.Contains "cycle"))
    | Ok () -> Assert.Fail("Expected cycle detection error")

[<Fact>]
let ``cycle in undirected graph does not trigger cycle detection`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [ makeNode "A" "A"; makeNode "B" "B"; makeNode "C" "C" ]
          Edges = [ makeEdge "A" "B"; makeEdge "B" "C"; makeEdge "C" "A" ] }
    let result = GraphValidation.validate graph
    Assert.True(Result.isOk result)

[<Fact>]
let ``empty graph passes validation`` () =
    let graph =
        { Config = Graph.defaultConfig GraphKind.Directed
          Nodes = []
          Edges = [] }
    let result = GraphValidation.validate graph
    Assert.True(Result.isOk result)

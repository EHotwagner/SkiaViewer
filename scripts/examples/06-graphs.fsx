/// Graph Visualization Demos
/// Demonstrates DAG, undirected weighted graph, and graph-inside-layout composition.
///
/// Usage: dotnet fsi scripts/examples/06-graphs.fsx

#load "../layout-prelude.fsx"
open SkiaSharp
open SkiaViewer
open SkiaViewer.Layout

// --- Demo 1: Simple DAG ---
let dag =
    { Config = Graph.defaultConfig GraphKind.Directed
      Nodes = [
          { Id = "A"; Label = "Start"; Style = None }
          { Id = "B"; Label = "Process"; Style = None }
          { Id = "C"; Label = "Decision"; Style = None }
          { Id = "D"; Label = "End"; Style = None }
      ]
      Edges = [
          { Source = "A"; Target = "B"; Weight = None; Label = None; Style = None }
          { Source = "B"; Target = "C"; Weight = None; Label = None; Style = None }
          { Source = "C"; Target = "D"; Weight = None; Label = None; Style = None }
          { Source = "A"; Target = "D"; Weight = None; Label = None; Style = None }
      ] }

match Graph.render dag 600f 400f with
| Ok element -> printfn "DAG rendered: %d children" (match element with Element.Group(_, _, _, c) -> c.Length | _ -> 0)
| Error msg -> printfn "DAG error: %s" msg

// --- Demo 2: Weighted Undirected Graph ---
let labelStyle : EdgeStyle = { Defaults.edgeStyle with ShowLabel = true }
let network =
    { Config = Graph.defaultConfig GraphKind.Undirected
      Nodes = [
          { Id = "server1"; Label = "Server 1"; Style = Some { Defaults.nodeStyle with FillColor = Some SKColors.LightBlue } }
          { Id = "server2"; Label = "Server 2"; Style = None }
          { Id = "server3"; Label = "Server 3"; Style = None }
      ]
      Edges = [
          { Source = "server1"; Target = "server2"; Weight = Some 10.0; Label = Some "10 Gbps"; Style = Some labelStyle }
          { Source = "server2"; Target = "server3"; Weight = Some 1.0; Label = Some "1 Gbps"; Style = Some labelStyle }
          { Source = "server1"; Target = "server3"; Weight = Some 5.0; Label = Some "5 Gbps"; Style = Some labelStyle }
      ] }

match Graph.render network 600f 400f with
| Ok element -> printfn "Network graph rendered: %d children" (match element with Element.Group(_, _, _, c) -> c.Length | _ -> 0)
| Error msg -> printfn "Network error: %s" msg

// --- Demo 3: Graph inside Layout ---
match Graph.render dag 580f 350f with
| Ok graphElement ->
    let title = Scene.text "Process Flow" 10f 30f 24f (Scene.fill SKColors.Black)
    let page =
        Layout.vstack { Defaults.stackConfig with Spacing = 10f; Padding = { Left = 10f; Top = 10f; Right = 10f; Bottom = 10f } } [
            Layout.childWithSize 580f 40f title
            Layout.child graphElement
        ] 600f 400f
    let scene = { BackgroundColor = SKColors.White; Elements = [ page ] }
    printfn "Composed layout+graph scene created with %d top-level elements." scene.Elements.Length
| Error msg -> printfn "Composition error: %s" msg

// --- Demo 4: Validation error ---
let cyclic =
    { Config = Graph.defaultConfig GraphKind.Directed
      Nodes = [ { Id = "A"; Label = "A"; Style = None }; { Id = "B"; Label = "B"; Style = None } ]
      Edges = [
          { Source = "A"; Target = "B"; Weight = None; Label = None; Style = None }
          { Source = "B"; Target = "A"; Weight = None; Label = None; Style = None }
      ] }

match Graph.render cyclic 400f 300f with
| Ok _ -> printfn "Unexpected: cyclic graph should fail"
| Error msg -> printfn "Correctly caught cycle: %s" msg

printfn "All graph demos completed."

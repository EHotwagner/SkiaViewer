/// Layout & Graph Visualization — Vulkan Window Demo
/// Renders layout containers and graph visualizations in a live Vulkan window.
/// Press 1-5 to switch demos. Escape to quit.
///
/// Run: dotnet fsi scripts/examples/07-layout-graph-window.fsx

#load "../layout-prelude.fsx"
open Prelude

open System
open System.Threading
open SkiaSharp
open SkiaViewer
open SkiaViewer.Layout

// ── Vulkan config ──
let config : ViewerConfig =
    { Title = "SkiaViewer — Layout & Graph Demo"
      Width = 900
      Height = 650
      TargetFps = 60
      ClearColor = SKColors.White
      PreferredBackend = Some Backend.Vulkan }

// ── Demo 1: Dock layout ──
let dockDemo () =
    let topBar =
        Scene.group None None [
            Scene.rect 0f 0f 900f 50f (Scene.fill (SKColor(0x22uy, 0x22uy, 0x44uy)))
            Scene.text "Dock Layout Demo" 20f 35f 22f (Scene.fill SKColors.White)
        ]
    let sidebar =
        Scene.group None None [
            Scene.rect 0f 0f 180f 600f (Scene.fill (SKColor(0x33uy, 0x33uy, 0x55uy)))
            Scene.text "Sidebar" 20f 30f 16f (Scene.fill SKColors.LightGray)
            Scene.rect 10f 50f 160f 40f (Scene.fill (SKColor(0x55uy, 0x55uy, 0x88uy)))
            Scene.text "Nav Item 1" 20f 78f 14f (Scene.fill SKColors.White)
            Scene.rect 10f 100f 160f 40f (Scene.fill (SKColor(0x55uy, 0x55uy, 0x88uy)))
            Scene.text "Nav Item 2" 20f 128f 14f (Scene.fill SKColors.White)
            Scene.rect 10f 150f 160f 40f (Scene.fill (SKColor(0x55uy, 0x55uy, 0x88uy)))
            Scene.text "Nav Item 3" 20f 178f 14f (Scene.fill SKColors.White)
        ]
    let content =
        Scene.group None None [
            Scene.rect 0f 0f 720f 600f (Scene.fill (SKColor(0xF0uy, 0xF0uy, 0xF5uy)))
            Scene.text "Main Content Area" 30f 40f 20f (Scene.fill (SKColor(0x33uy, 0x33uy, 0x33uy)))
            Scene.rect 20f 60f 300f 200f (Scene.fill SKColors.White)
            Scene.rect 20f 60f 300f 200f (Scene.stroke (SKColor(0xCCuy, 0xCCuy, 0xCCuy)) 1f)
            Scene.text "Card 1" 40f 100f 16f (Scene.fill (SKColor(0x44uy, 0x44uy, 0x44uy)))
            Scene.rect 340f 60f 300f 200f (Scene.fill SKColors.White)
            Scene.rect 340f 60f 300f 200f (Scene.stroke (SKColor(0xCCuy, 0xCCuy, 0xCCuy)) 1f)
            Scene.text "Card 2" 360f 100f 16f (Scene.fill (SKColor(0x44uy, 0x44uy, 0x44uy)))
        ]
    let page =
        Layout.dock Defaults.dockConfig [
            { Layout.dockChild DockPosition.Top topBar with Sizing = { Defaults.sizing with DesiredHeight = Some 50f } }
            { Layout.dockChild DockPosition.Left sidebar with Sizing = { Defaults.sizing with DesiredWidth = Some 180f } }
            Layout.dockChild DockPosition.Fill content
        ] 900f 650f
    Scene.create SKColors.White [ page ]

// ── Demo 2: Nested stacks ──
let stackDemo () =
    let makeBox w h (color: SKColor) label =
        Scene.group None None [
            Scene.rect 0f 0f w h (Scene.fill color)
            Scene.rect 0f 0f w h (Scene.stroke (SKColor(0x00uy, 0x00uy, 0x00uy, 0x33uy)) 1f)
            Scene.text label 8f 20f 12f (Scene.fill SKColors.White)
        ]
    let row1 =
        Layout.hstack { Defaults.stackConfig with Spacing = 12f } [
            Layout.childWithSize 150f 80f (makeBox 150f 80f SKColors.SteelBlue "Blue")
            Layout.childWithSize 150f 80f (makeBox 150f 80f SKColors.Coral "Coral")
            Layout.childWithSize 150f 80f (makeBox 150f 80f SKColors.SeaGreen "Green")
            Layout.childWithSize 150f 80f (makeBox 150f 80f SKColors.Goldenrod "Gold")
        ] 800f 80f
    let row2 =
        Layout.hstack { Defaults.stackConfig with Spacing = 12f } [
            Layout.childWithSize 250f 120f (makeBox 250f 120f SKColors.DarkSlateBlue "Wide Panel")
            Layout.childWithSize 250f 120f (makeBox 250f 120f SKColors.IndianRed "Another")
        ] 800f 120f
    let title = Scene.text "Nested HStack / VStack Layout" 0f 28f 24f (Scene.fill (SKColor(0x22uy, 0x22uy, 0x44uy)))
    let page =
        Layout.vstack { Defaults.stackConfig with Spacing = 20f; Padding = { Left = 50f; Top = 30f; Right = 50f; Bottom = 30f } } [
            Layout.childWithSize 800f 35f title
            Layout.childWithSize 800f 80f row1
            Layout.childWithSize 800f 120f row2
        ] 900f 650f
    Scene.create (SKColor(0xF8uy, 0xF8uy, 0xFCuy)) [ page ]

// ── Demo 3: DAG ──
let dagDemo () =
    let dag : GraphDefinition =
        { Config = { Graph.defaultConfig GraphKind.Directed with NodeSpacing = 40f; LayerSpacing = 70f }
          Nodes = [
              { Id = "src"; Label = "Source"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0x4Cuy, 0xAFuy, 0x50uy)); Shape = NodeShape.RoundedRect 8f } }
              { Id = "parse"; Label = "Parse"; Style = None }
              { Id = "validate"; Label = "Validate"; Style = None }
              { Id = "transform"; Label = "Transform"; Style = None }
              { Id = "optimize"; Label = "Optimize"; Style = None }
              { Id = "emit"; Label = "Emit"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0x21uy, 0x96uy, 0xF3uy)); Shape = NodeShape.RoundedRect 8f } }
          ]
          Edges = [
              { Source = "src"; Target = "parse"; Weight = None; Label = None; Style = None }
              { Source = "parse"; Target = "validate"; Weight = None; Label = None; Style = None }
              { Source = "parse"; Target = "transform"; Weight = None; Label = None; Style = None }
              { Source = "validate"; Target = "optimize"; Weight = None; Label = None; Style = None }
              { Source = "transform"; Target = "optimize"; Weight = None; Label = None; Style = None }
              { Source = "optimize"; Target = "emit"; Weight = None; Label = None; Style = None }
          ] }
    let title = Scene.text "DAG — Compiler Pipeline" 0f 28f 24f (Scene.fill (SKColor(0x22uy, 0x22uy, 0x44uy)))
    match Graph.render dag 800f 520f with
    | Ok graphElement ->
        let page =
            Layout.vstack { Defaults.stackConfig with Spacing = 15f; Padding = { Left = 50f; Top = 30f; Right = 50f; Bottom = 30f } } [
                Layout.childWithSize 800f 35f title
                Layout.child graphElement
            ] 900f 650f
        Scene.create (SKColor(0xFAuy, 0xFAuy, 0xFEuy)) [ page ]
    | Error msg ->
        Scene.create SKColors.White [ Scene.text ($"Error: {msg}") 50f 100f 20f (Scene.fill SKColors.Red) ]

// ── Demo 4: Weighted undirected graph ──
let networkDemo () =
    let labelEdge : EdgeStyle = { Defaults.edgeStyle with ShowLabel = true; Color = Some (SKColor(0x55uy, 0x55uy, 0x77uy)) }
    let network : GraphDefinition =
        { Config = { Graph.defaultConfig GraphKind.Undirected with NodeSpacing = 60f }
          Nodes = [
              { Id = "dc1"; Label = "DC-West"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0xE3uy, 0xF2uy, 0xFDuy)); Shape = NodeShape.Ellipse; Width = Some 100f; Height = Some 50f } }
              { Id = "dc2"; Label = "DC-East"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0xE8uy, 0xF5uy, 0xE9uy)); Shape = NodeShape.Ellipse; Width = Some 100f; Height = Some 50f } }
              { Id = "dc3"; Label = "DC-EU"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0xFCuy, 0xE4uy, 0xECuy)); Shape = NodeShape.Ellipse; Width = Some 100f; Height = Some 50f } }
              { Id = "cdn"; Label = "CDN"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0xFFuy, 0xF3uy, 0xE0uy)); Shape = NodeShape.Ellipse; Width = Some 90f; Height = Some 45f } }
              { Id = "lb"; Label = "Load Bal"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0xF3uy, 0xE5uy, 0xF5uy)); Shape = NodeShape.Ellipse; Width = Some 90f; Height = Some 45f } }
          ]
          Edges = [
              { Source = "dc1"; Target = "dc2"; Weight = Some 10.0; Label = Some "10 Gbps"; Style = Some labelEdge }
              { Source = "dc2"; Target = "dc3"; Weight = Some 5.0; Label = Some "5 Gbps"; Style = Some labelEdge }
              { Source = "dc1"; Target = "dc3"; Weight = Some 2.0; Label = Some "2 Gbps"; Style = Some labelEdge }
              { Source = "dc1"; Target = "cdn"; Weight = Some 8.0; Label = Some "8 Gbps"; Style = Some labelEdge }
              { Source = "dc2"; Target = "lb"; Weight = Some 6.0; Label = Some "6 Gbps"; Style = Some labelEdge }
              { Source = "cdn"; Target = "lb"; Weight = Some 3.0; Label = Some "3 Gbps"; Style = Some labelEdge }
          ] }
    let title = Scene.text "Network Topology — Weighted Undirected Graph" 0f 28f 24f (Scene.fill (SKColor(0x22uy, 0x22uy, 0x44uy)))
    match Graph.render network 800f 520f with
    | Ok graphElement ->
        let page =
            Layout.vstack { Defaults.stackConfig with Spacing = 15f; Padding = { Left = 50f; Top = 30f; Right = 50f; Bottom = 30f } } [
                Layout.childWithSize 800f 35f title
                Layout.child graphElement
            ] 900f 650f
        Scene.create (SKColor(0xFAuy, 0xFAuy, 0xFEuy)) [ page ]
    | Error msg ->
        Scene.create SKColors.White [ Scene.text ($"Error: {msg}") 50f 100f 20f (Scene.fill SKColors.Red) ]

// ── Demo 5: Composition — Graph + Layout ──
let compositionDemo () =
    let miniDag : GraphDefinition =
        { Config = { Graph.defaultConfig GraphKind.Directed with NodeSpacing = 25f; LayerSpacing = 45f }
          Nodes = [
              { Id = "A"; Label = "Input"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0xBBuy, 0xDEuy, 0xFBuy)) } }
              { Id = "B"; Label = "Process"; Style = None }
              { Id = "C"; Label = "Output"; Style = Some { Defaults.nodeStyle with FillColor = Some (SKColor(0xC8uy, 0xE6uy, 0xC9uy)) } }
          ]
          Edges = [
              { Source = "A"; Target = "B"; Weight = None; Label = None; Style = None }
              { Source = "B"; Target = "C"; Weight = None; Label = None; Style = None }
          ] }
    let miniUndirected : GraphDefinition =
        { Config = Graph.defaultConfig GraphKind.Undirected
          Nodes = [
              { Id = "X"; Label = "Node X"; Style = Some { Defaults.nodeStyle with Shape = NodeShape.Ellipse; FillColor = Some (SKColor(0xFFuy, 0xCDuy, 0xD2uy)) } }
              { Id = "Y"; Label = "Node Y"; Style = Some { Defaults.nodeStyle with Shape = NodeShape.Ellipse; FillColor = Some (SKColor(0xD1uy, 0xC4uy, 0xE9uy)) } }
              { Id = "Z"; Label = "Node Z"; Style = Some { Defaults.nodeStyle with Shape = NodeShape.Ellipse; FillColor = Some (SKColor(0xFFuy, 0xECuy, 0xB3uy)) } }
          ]
          Edges = [
              { Source = "X"; Target = "Y"; Weight = None; Label = None; Style = None }
              { Source = "Y"; Target = "Z"; Weight = None; Label = None; Style = None }
              { Source = "X"; Target = "Z"; Weight = None; Label = None; Style = None }
          ] }
    let title = Scene.text "Composition — Graphs inside Layout Containers" 0f 28f 24f (Scene.fill (SKColor(0x22uy, 0x22uy, 0x44uy)))
    let dagResult = Graph.render miniDag 380f 400f
    let undirectedResult = Graph.render miniUndirected 380f 400f
    match dagResult, undirectedResult with
    | Ok dagEl, Ok undEl ->
        let dagPanel =
            Scene.group None None [
                Scene.rect 0f 0f 390f 420f (Scene.fill SKColors.White)
                Scene.rect 0f 0f 390f 420f (Scene.stroke (SKColor(0xDDuy, 0xDDuy, 0xDDuy)) 1f)
                Scene.text "Directed (DAG)" 10f 22f 14f (Scene.fill (SKColor(0x66uy, 0x66uy, 0x66uy)))
                Scene.translate 5f 30f [ dagEl ]
            ]
        let undPanel =
            Scene.group None None [
                Scene.rect 0f 0f 390f 420f (Scene.fill SKColors.White)
                Scene.rect 0f 0f 390f 420f (Scene.stroke (SKColor(0xDDuy, 0xDDuy, 0xDDuy)) 1f)
                Scene.text "Undirected" 10f 22f 14f (Scene.fill (SKColor(0x66uy, 0x66uy, 0x66uy)))
                Scene.translate 5f 30f [ undEl ]
            ]
        let row =
            Layout.hstack { Defaults.stackConfig with Spacing = 20f } [
                Layout.childWithSize 390f 420f dagPanel
                Layout.childWithSize 390f 420f undPanel
            ] 800f 420f
        let page =
            Layout.vstack { Defaults.stackConfig with Spacing = 20f; Padding = { Left = 50f; Top = 30f; Right = 50f; Bottom = 30f } } [
                Layout.childWithSize 800f 35f title
                Layout.childWithSize 800f 420f row
            ] 900f 650f
        Scene.create (SKColor(0xF5uy, 0xF5uy, 0xFAuy)) [ page ]
    | Error e, _ | _, Error e ->
        Scene.create SKColors.White [ Scene.text ($"Error: {e}") 50f 100f 20f (Scene.fill SKColors.Red) ]

// ── Pre-render all scenes at startup (MSAGL can't run on UI thread) ──
let demoNames = [| "Dock Layout"; "Nested Stacks"; "DAG Pipeline"; "Network Topology"; "Composition" |]
printfn "Pre-rendering all demos..."
let addHud (idx: int) (baseScene: Scene) =
    let hud = [
        Scene.rect 0f 610f 900f 40f (Scene.fill (SKColor(0x00uy, 0x00uy, 0x00uy, 0xCCuy)))
        Scene.text (sprintf "[1-5] Switch Demo   [Esc] Quit   |   Demo %d: %s" (idx + 1) demoNames.[idx]) 20f 636f 14f (Scene.fill (SKColor(0xCCuy, 0xCCuy, 0xCCuy)))
    ]
    { baseScene with Elements = baseScene.Elements @ hud }

let preRendered =
    [| dockDemo; stackDemo; dagDemo; networkDemo; compositionDemo |]
    |> Array.mapi (fun i fn ->
        printfn "  Rendering demo %d: %s" (i + 1) demoNames.[i]
        addHud i (fn ()))
printfn "All demos pre-rendered."

let mutable currentDemo = 0
let mutable running = true
let sceneEvent = Event<Scene>()

let (viewerHandle, inputs) = Viewer.run config sceneEvent.Publish

let _sub = inputs.Subscribe(fun event ->
    match event with
    | InputEvent.KeyDown key ->
        match key with
        | Silk.NET.Input.Key.Number1 -> currentDemo <- 0; sceneEvent.Trigger(preRendered.[0])
        | Silk.NET.Input.Key.Number2 -> currentDemo <- 1; sceneEvent.Trigger(preRendered.[1])
        | Silk.NET.Input.Key.Number3 -> currentDemo <- 2; sceneEvent.Trigger(preRendered.[2])
        | Silk.NET.Input.Key.Number4 -> currentDemo <- 3; sceneEvent.Trigger(preRendered.[3])
        | Silk.NET.Input.Key.Number5 -> currentDemo <- 4; sceneEvent.Trigger(preRendered.[4])
        | Silk.NET.Input.Key.Escape -> running <- false
        | _ -> ()
    | _ -> ())

sceneEvent.Trigger(preRendered.[0])

printfn "Layout & Graph Demo running with Vulkan backend."
printfn "Press 1-5 to switch demos, Escape to quit."
printfn "  1: Dock Layout"
printfn "  2: Nested Stacks"
printfn "  3: DAG Pipeline"
printfn "  4: Network Topology (weighted undirected)"
printfn "  5: Composition (graphs inside layouts)"

while running do
    Thread.Sleep(100)

printfn "Done."

# Quickstart: SkiaViewer.Layout

**Feature**: 009-layout-graph-viz  
**Date**: 2026-04-09

## Prerequisites

- .NET 10.0 SDK
- SkiaViewer (project reference or NuGet)

## Layout Example

```fsharp
#load "prelude.fsx"
#load "layout-prelude.fsx"
open SkiaViewer
open SkiaViewer.Layout

// Create elements
let header = Scene.rect 0f 0f 400f 50f (Scene.fill SKColors.DarkBlue)
let sidebar = Scene.rect 0f 0f 100f 300f (Scene.fill SKColors.DarkGray)
let content = Scene.rect 0f 0f 300f 300f (Scene.fill SKColors.White)

// Arrange in a dock layout
let layout =
    Layout.dock Defaults.dockConfig [
        Layout.dockChild DockPosition.Top header
        Layout.dockChild DockPosition.Left sidebar
        Layout.dockChild DockPosition.Fill content
    ] 400f 350f

let scene = { BackgroundColor = SKColors.Black; Elements = [ layout ] }
```

## Nested Stacks Example

```fsharp
let buttons =
    Layout.hstack { Defaults.stackConfig with Spacing = 10f } [
        Layout.childWithSize 80f 30f (Scene.rect 0f 0f 80f 30f (Scene.fill SKColors.Green))
        Layout.childWithSize 80f 30f (Scene.rect 0f 0f 80f 30f (Scene.fill SKColors.Red))
        Layout.childWithSize 80f 30f (Scene.rect 0f 0f 80f 30f (Scene.fill SKColors.Blue))
    ] 300f 30f

let page =
    Layout.vstack { Defaults.stackConfig with Spacing = 20f } [
        Layout.childWithSize 300f 50f header
        Layout.child buttons
        Layout.child content
    ] 300f 400f
```

## DAG Visualization Example

```fsharp
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
| Ok element ->
    let scene = { BackgroundColor = SKColors.White; Elements = [ element ] }
    Viewer.run defaultConfig (singleScene scene) |> ignore
| Error msg ->
    printfn "Graph error: %s" msg
```

## Weighted Undirected Graph Example

```fsharp
let network =
    { Config = Graph.defaultConfig GraphKind.Undirected
      Nodes = [
          { Id = "server1"; Label = "Server 1"; Style = Some { Defaults.nodeStyle with FillColor = Some SKColors.LightBlue } }
          { Id = "server2"; Label = "Server 2"; Style = None }
          { Id = "server3"; Label = "Server 3"; Style = None }
      ]
      Edges = [
          { Source = "server1"; Target = "server2"; Weight = Some 10.0; Label = Some "10 Gbps"; Style = None }
          { Source = "server2"; Target = "server3"; Weight = Some 1.0; Label = Some "1 Gbps"; Style = None }
          { Source = "server1"; Target = "server3"; Weight = Some 5.0; Label = Some "5 Gbps"; Style = None }
      ] }

match Graph.render network 600f 400f with
| Ok element ->
    let scene = { BackgroundColor = SKColors.White; Elements = [ element ] }
    Viewer.run defaultConfig (singleScene scene) |> ignore
| Error msg ->
    printfn "Graph error: %s" msg
```

## Graph Inside Layout

```fsharp
let title = Scene.text "Network Topology" 10f 30f 24f (Scene.fill SKColors.Black)

match Graph.render network 580f 350f with
| Ok graphElement ->
    let page =
        Layout.vstack { Defaults.stackConfig with Spacing = 10f; Padding = { Left = 10f; Top = 10f; Right = 10f; Bottom = 10f } } [
            Layout.childWithSize 580f 40f title
            Layout.child graphElement
        ] 600f 400f
    let scene = { BackgroundColor = SKColors.White; Elements = [ page ] }
    Viewer.run defaultConfig (singleScene scene) |> ignore
| Error msg ->
    printfn "Graph error: %s" msg
```

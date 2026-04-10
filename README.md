> **Note:** This project uses [Spec Kit](https://github.com/github/spec-kit) for specification-driven development.
> Development is guided by a project constitution — see [.specify/memory/constitution.md](.specify/memory/constitution.md) for the
> governing principles and architectural constraints.

# SkiaViewer

A hardened Silk.NET + SkiaSharp viewer for .NET 10.0. Renders SkiaSharp surfaces to Vulkan GPU or OpenGL-backed windows, with a declarative scene DSL, layout containers, graph visualization, charting, thread-safe lifecycle management, and frame-level exception recovery.

https://github.com/EHotwagner/SkiaViewer/releases/download/v1.1.3/demo-reel.mp4

<details>
<summary>Demo reel scenes</summary>

| Time | Scene |
|------|-------|
| 0-8s | Animated geometry burst with glowing particles and spinning shapes |
| 8-18s | Shader showcase: radial, sweep, conical gradients + Perlin noise |
| 18-26s | Full-screen SkSL plasma shader (GPU per-pixel computation) |
| 26-38s | Live analytics dashboard: line, pie, bar, area, radar, scatter charts |
| 38-46s | Scrolling DataGrid + animated candlestick OHLC chart |
| 46-54s | DAG graph visualization, 3D perspective cards, drop shadows |
| 54-60s | Grand finale with starburst and radial glow |

</details>

## Installation

```
dotnet add package SkiaViewer
```

## Quick Start

```fsharp
open SkiaViewer
open SkiaSharp
open System

let config : ViewerConfig =
    { Title = "Hello SkiaViewer"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.Black
      PreferredBackend = None }

let sceneEvent = Event<Scene>()

let scene =
    Scene.create SKColors.CornflowerBlue [
        Scene.rect 50f 50f 200f 100f (Scene.fill SKColors.White)
        Scene.circle 400f 300f 80f (Scene.fill SKColors.Coral)
        Scene.text "Hello, SkiaViewer!" 50f 250f 32f (Scene.fill SKColors.Yellow)
    ]

let (viewer, inputs) = Viewer.run config sceneEvent.Publish
use viewer = viewer

sceneEvent.Trigger(scene)

use _sub = inputs.Subscribe(fun evt ->
    match evt with
    | InputEvent.KeyDown key -> printfn $"Key: {key}"
    | _ -> ())

Console.ReadLine() |> ignore
```

## Documentation

Full documentation is available at **https://EHotwagner.github.io/SkiaViewer/**

To build and preview locally:
```
dotnet tool restore
dotnet fsdocs watch
```
Then open http://localhost:8901.

## Features

- **Declarative scene DSL** — build scenes with composable F# types and pipeline operators
- **Vulkan GPU backend** — automatic Vulkan initialization with GL raster fallback
- **Reactive streams** — push scenes via `IObservable<Scene>`, receive input via `IObservable<InputEvent>`
- **Background thread** — `Viewer.run` returns immediately; window runs on a dedicated thread
- **Screenshot capture** — save rendered frames as PNG or JPEG
- **Full effects pipeline** — shaders, color/mask/image filters, blend modes, path effects, 3D transforms
- **Thread-safe lifecycle** — cross-thread shutdown with `IDisposable`, 5-second timeout
- **Frame-level recovery** — catches `ObjectDisposedException`, `NullReferenceException`, `ArgumentNullException` per frame

### SkiaViewer.Charts

The companion `SkiaViewer.Charts` library adds data visualization elements that compose directly into the scene DSL:

```
dotnet add package SkiaViewer.Charts
```

- **Line chart** — multi-series with auto-scaled axes, grid lines, and legends
- **Bar chart** — grouped or stacked categorical bars
- **Pie / donut chart** — proportional arc slices with optional donut hole
- **Scatter plot** — individual data points with per-series styling
- **Area chart** — filled regions under data lines with stacking
- **Histogram** — frequency distribution with configurable bin count
- **Candlestick chart** — OHLC financial data with up/down coloring
- **Radar / spider chart** — multi-variable comparison on radial axes
- **DataGrid** — sortable, scrollable tabular data with text, numeric, and boolean columns

```fsharp
open SkiaViewer.Charts

let chart =
    LineChart.lineChart (LineChart.defaultConfig 600f 400f)
        [ { Name = "Sales"; Points = [ { X = 1.0; Y = 10.0 }; { X = 2.0; Y = 25.0 } ] } ]

let grid =
    DataGrid.dataGrid (DataGrid.defaultConfig 500f 300f)
        { Columns = [ DataGrid.textColumn "Name"; DataGrid.numericColumn "Score" ]
          Rows = [ [ CellValue.TextValue "Alice"; CellValue.NumericValue 95.0 ] ] }

let scene = Scene.create SKColors.White [ chart; grid ]
```

See [Charting](https://EHotwagner.github.io/SkiaViewer/charting.html) and [DataGrid](https://EHotwagner.github.io/SkiaViewer/datagrid.html) for full documentation.

### SkiaViewer.Layout

Layout containers and graph visualization that compose into the scene DSL:

```
dotnet add package SkiaViewer.Layout
```

- **HStack / VStack** — horizontal and vertical stacking with spacing, padding, and alignment
- **Dock** — edge-docked layout (Top, Bottom, Left, Right, Fill)
- **DAG visualization** — directed acyclic graphs with automatic Sugiyama layered layout via MSAGL
- **Undirected graphs** — force-directed MDS layout with weighted edges and labels
- **Cycle detection** — Kahn's algorithm validates DAGs before rendering
- **Composition** — graph elements nest naturally inside layout containers

```fsharp
open SkiaViewer.Layout

let page =
    Layout.dock Defaults.dockConfig [
        { Layout.dockChild DockPosition.Top header
          with Sizing = { Defaults.sizing with DesiredHeight = Some 50f } }
        Layout.dockChild DockPosition.Fill content
    ] 800f 600f

let dag =
    { Config = Graph.defaultConfig GraphKind.Directed
      Nodes = [ { Id = "A"; Label = "Start"; Style = None }
                { Id = "B"; Label = "End"; Style = None } ]
      Edges = [ { Source = "A"; Target = "B"; Weight = None; Label = None; Style = None } ] }

match Graph.render dag 600f 400f with
| Ok graphElement -> // use graphElement in any scene or layout
| Error msg -> printfn "Validation error: %s" msg
```

## Known Issues

See [Known Issues](https://EHotwagner.github.io/SkiaViewer/known-issues.html) for current limitations.

## License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.

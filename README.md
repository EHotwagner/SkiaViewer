> **Note:** This project uses [Spec Kit](https://github.com/github/spec-kit) for specification-driven development.
> Development is guided by a project constitution — see [.specify/memory/constitution.md](.specify/memory/constitution.md) for the
> governing principles and architectural constraints.

# SkiaViewer

A hardened Silk.NET + SkiaSharp OpenGL viewer for .NET 10.0. Renders SkiaSharp surfaces to an OpenGL-backed window via texture upload, with a declarative scene DSL, Vulkan GPU backend, thread-safe lifecycle management, cross-thread shutdown, and frame-level exception recovery.

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

## Known Issues

See [Known Issues](https://EHotwagner.github.io/SkiaViewer/known-issues.html) for current limitations.

## License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.

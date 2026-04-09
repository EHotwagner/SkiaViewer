(**
---
title: Getting Started
category: Overview
categoryindex: 1
index: 3
description: Install SkiaViewer, configure a window, and render your first scene.
---
*)

(**
# Getting Started with SkiaViewer

SkiaViewer provides a declarative, stream-based API for rendering 2D graphics
in an OpenGL-backed window. You describe scenes as immutable data, push them
through an `IObservable<Scene>`, and receive input events back through another
observable stream.

## Installation

Add SkiaViewer to your project via NuGet:
*)

(*** do-not-eval ***)
// dotnet add package SkiaViewer

(**
## Window Configuration

Configure the viewer window with a `ViewerConfig` record:
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaSharp.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/Silk.NET.Input.Common.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

open SkiaViewer
open SkiaSharp

(**
`ViewerConfig` contains only static window properties — no callbacks.
Scene data and input events flow through streams.
*)

(*** do-not-eval ***)
let config : ViewerConfig =
    { Title = "My SkiaViewer App"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.Black
      PreferredBackend = None }

(**
<div class="alert alert-info">
<strong>Tip:</strong> Set <code>PreferredBackend</code> to <code>None</code> for auto-detection
(Vulkan with GL fallback), or force a specific backend with <code>Some Backend.Vulkan</code>
or <code>Some Backend.GL</code>.
</div>

| Field | Type | Description |
|---|---|---|
| `Title` | `string` | Window title bar text |
| `Width` | `int` | Initial window width in logical pixels |
| `Height` | `int` | Initial window height in logical pixels |
| `TargetFps` | `int` | Target frames per second |
| `ClearColor` | `SKColor` | Default background clear color |
| `PreferredBackend` | `Backend option` | `None` = auto-detect, `Some Vulkan`, `Some GL` |

## Your First Window

`Viewer.run` starts a window on a background thread. It takes a config and an
`IObservable<Scene>`, and returns a `ViewerHandle` plus an
`IObservable<InputEvent>`:
*)

(*** do-not-eval ***)
open System

let sceneEvent = Event<Scene>()

let firstScene =
    Scene.create SKColors.CornflowerBlue [
        Scene.rect 50f 50f 200f 100f (Scene.fill SKColors.White)
        Scene.circle 400f 300f 80f (Scene.fill SKColors.Coral)
        Scene.text "Hello, SkiaViewer!" 50f 250f 32f (Scene.fill SKColors.Yellow)
    ]

let (handle, inputs) = Viewer.run config sceneEvent.Publish
use _handle = handle

// Push a scene to render
sceneEvent.Trigger(firstScene)

// Subscribe to input events
use _sub = inputs.Subscribe(fun evt ->
    match evt with
    | InputEvent.KeyDown key -> printfn $"Key pressed: {key}"
    | InputEvent.FrameTick elapsed -> () // called every frame
    | _ -> ())

// Keep the app running
Console.ReadLine() |> ignore

(**
## Key Concepts

- **Scenes are immutable data.** A `Scene` is a record with a background color and
  a list of `Element` values. You build new scenes each frame rather than mutating state.

- **Reactive streams.** You push scenes through `IObservable<Scene>` and receive
  `InputEvent` values from the returned observable. This decouples your application
  logic from the rendering thread.

- **Background thread.** The window runs on a dedicated background thread. `Viewer.run`
  returns immediately — the caller controls when to block or proceed.

- **Lifecycle via `IDisposable`.** The `ViewerHandle` implements `IDisposable`.
  Disposing it triggers a cross-thread shutdown with a 5-second timeout.

## Next Steps

- [Declarative Scene DSL](declarative-scene-dsl.html) — composable scene construction
- [Drawing Primitives & Paint](drawing-primitives.html) — all element types and paint styling
- [Input Handling](input-handling.html) — keyboard, mouse, and window events
*)

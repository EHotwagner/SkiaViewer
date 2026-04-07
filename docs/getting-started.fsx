(**
---
title: Getting Started
category: Overview
categoryindex: 1
index: 1
description: Create your first SkiaViewer window and render a simple scene.
---
*)

(**
# Getting Started with SkiaViewer

SkiaViewer opens a hardware-accelerated window on a background thread and gives you a
SkiaSharp canvas to draw on each frame. This guide walks through creating a minimal viewer.

## Installation

Add the NuGet package to your project:

```
dotnet add package SkiaViewer
```

## Minimal Example

A SkiaViewer application needs a `ViewerConfig` record and a call to `Viewer.run`.
The returned `IDisposable` keeps the window alive — disposing it shuts the window down.
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

(**
Open the necessary namespaces:
*)

open SkiaViewer
open SkiaSharp
open Silk.NET.Maths

(**
## Defining a Configuration

Every viewer is configured with a `ViewerConfig` record. At minimum, you need to supply
a title, dimensions, frame rate, clear color, and callbacks. Callbacks you don't need can
be set to no-ops:
*)

(*** do-not-eval ***)
let config =
    { Title = "Hello SkiaViewer"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.CornflowerBlue
      OnRender = fun canvas fbSize ->
          use paint = new SKPaint(Color = SKColors.White, TextSize = 32.0f, IsAntialias = true)
          canvas.DrawText("Hello, SkiaViewer!", 50.0f, 80.0f, paint)
      OnResize = fun _ _ -> ()
      OnKeyDown = fun _ -> ()
      OnMouseScroll = fun _ _ _ -> ()
      OnMouseDrag = fun _ _ -> () }

(**
## Running the Viewer

Call `Viewer.run` to launch the window on a background thread. The function returns
immediately with an `IDisposable` handle:
*)

(*** do-not-eval ***)
let viewer = Viewer.run config
// The window is now open and rendering.
// In a console app, block the main thread:
System.Threading.Thread.Sleep(5000)
// Call viewer.Dispose() to gracefully shut down the window.

(**
<div class="alert alert-info">
<strong>Tip:</strong> The viewer runs on a background thread, so your main thread remains free.
In a console application, you'll need to block (e.g., with <code>Thread.Sleep</code> or
<code>Console.ReadLine()</code>) to keep the process alive.
</div>

## ViewerConfig Fields

| Field | Type | Purpose |
|---|---|---|
| `Title` | `string` | Window title bar text |
| `Width` | `int` | Initial window width (logical pixels) |
| `Height` | `int` | Initial window height (logical pixels) |
| `TargetFps` | `int` | Target frames per second |
| `ClearColor` | `SKColor` | Background color cleared each frame |
| `OnRender` | `SKCanvas -> Vector2D<int> -> unit` | Draw your scene here |
| `OnResize` | `int -> int -> unit` | Called on window resize |
| `OnKeyDown` | `Key -> unit` | Called on key press |
| `OnMouseScroll` | `float32 -> float32 -> float32 -> unit` | Scroll delta + mouse position |
| `OnMouseDrag` | `float32 -> float32 -> unit` | Drag delta (dx, dy) |

## Next Steps

- [Drawing Primitives](drawing-primitives.html) — learn to draw shapes, text, and gradients
- [Input Handling](input-handling.html) — respond to keyboard and mouse input
- [Architecture Overview](architecture.html) — understand the rendering pipeline
*)

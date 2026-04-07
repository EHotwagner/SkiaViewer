(**
---
title: Input Handling
category: Tutorials
categoryindex: 2
index: 3
description: Handle keyboard, mouse scroll, and drag input in SkiaViewer.
---
*)

(**
# Input Handling

SkiaViewer exposes three input callbacks on `ViewerConfig`: `OnKeyDown`, `OnMouseScroll`,
and `OnMouseDrag`. These are wired to Silk.NET's input system and called on the window thread.

<div class="alert alert-warning">
<strong>Thread safety:</strong> Input callbacks run on the window's background thread. If you
modify shared mutable state from these callbacks, use <code>Interlocked</code> operations or
locks to avoid races with your main thread.
</div>
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

open SkiaViewer
open SkiaSharp
open Silk.NET.Input
open System.Threading

(**
## Keyboard Input

The `OnKeyDown` callback receives a `Silk.NET.Input.Key` value each time a key is pressed.
Use pattern matching to respond to specific keys:
*)

(*** do-not-eval ***)
let mutable message = "Press a key..."

let onKeyDown (key: Key) =
    match key with
    | Key.Escape -> message <- "Escape pressed — quitting"
    | Key.Space  -> message <- "Space pressed!"
    | k          -> message <- $"Key: {k}"

(**
## Mouse Scroll

`OnMouseScroll` provides the scroll delta and the mouse position at the time of the event.
This is useful for implementing zoom:
*)

(*** do-not-eval ***)
let mutable scale = 1.0f

let onScroll (delta: float32) (mouseX: float32) (mouseY: float32) =
    let zoomFactor = 1.0f + delta * 0.1f
    scale <- scale * zoomFactor

(**
## Mouse Drag

`OnMouseDrag` fires during a left-button drag with the movement delta in pixels.
This is ideal for panning:
*)

(*** do-not-eval ***)
let mutable offsetX = 0.0f
let mutable offsetY = 0.0f

let onDrag (dx: float32) (dy: float32) =
    offsetX <- offsetX + dx
    offsetY <- offsetY + dy

(**
## Complete Interactive Example

Here is a full example combining all input types. The viewer displays a draggable,
zoomable rectangle with keyboard feedback:
*)

(*** do-not-eval ***)
let interactiveConfig =
    { Title = "Interactive Input Demo"
      Width = 600
      Height = 400
      TargetFps = 60
      ClearColor = SKColors.DarkSlateGray
      OnRender = fun canvas _ ->
          canvas.Save() |> ignore
          canvas.Translate(offsetX, offsetY)
          canvas.Scale(scale)

          use rectPaint = new SKPaint(Color = SKColors.DodgerBlue, IsAntialias = true)
          canvas.DrawRect(100.0f, 100.0f, 200.0f, 150.0f, rectPaint)

          canvas.Restore()

          use textPaint = new SKPaint(Color = SKColors.White, TextSize = 18.0f, IsAntialias = true)
          canvas.DrawText(message, 10.0f, 30.0f, textPaint)
          canvas.DrawText($"Scale: %.2f{scale}  Offset: (%.0f{offsetX}, %.0f{offsetY})", 10.0f, 55.0f, textPaint)
      OnResize = fun _ _ -> ()
      OnKeyDown = onKeyDown
      OnMouseScroll = onScroll
      OnMouseDrag = onDrag }

let viewer = Viewer.run interactiveConfig
System.Threading.Thread.Sleep(10000)
viewer.Dispose()

(**
## Next Steps

- [Getting Started](getting-started.html) — basic viewer setup
- [Architecture Overview](architecture.html) — threading model and rendering pipeline
- [API Reference](reference/index.html)
*)

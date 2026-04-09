(**
---
title: Input Handling
category: Tutorials
categoryindex: 2
index: 5
description: Keyboard, mouse, and window events via reactive input streams.
---
*)

(**
# Input Handling

SkiaViewer delivers input events through an `IObservable<InputEvent>` stream
returned by `Viewer.run`. Events are emitted on the window thread and include
keyboard, mouse, window resize, and per-frame timing events.

## Setup
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaSharp.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/Silk.NET.Input.Common.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

open SkiaViewer
open SkiaSharp
open Silk.NET.Input

(**
## The InputEvent Type

`InputEvent` is a discriminated union covering all supported input events:
*)

(*** do-not-eval ***)
type InputEventCases =
    | KeyDown of key: Key
    | KeyUp of key: Key
    | MouseMove of x: float32 * y: float32
    | MouseDown of button: MouseButton * x: float32 * y: float32
    | MouseUp of button: MouseButton * x: float32 * y: float32
    | MouseScroll of delta: float32 * x: float32 * y: float32
    | WindowResize of width: int * height: int
    | FrameTick of elapsedSeconds: float

(**
## Subscribing to Events

`Viewer.run` returns a tuple of `ViewerHandle * IObservable<InputEvent>`.
Subscribe to the observable to handle events:
*)

(*** do-not-eval ***)
open System

let config : ViewerConfig =
    { Title = "Input Demo"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.Black
      PreferredBackend = None }

let sceneEvent = Event<Scene>()
let (viewer, inputs) = Viewer.run config sceneEvent.Publish
use viewer = viewer

use _sub = inputs.Subscribe(fun evt ->
    match evt with
    | InputEvent.KeyDown key ->
        printfn $"Key down: {key}"

    | InputEvent.KeyUp key ->
        printfn $"Key up: {key}"

    | InputEvent.MouseMove(x, y) ->
        printfn $"Mouse at ({x}, {y})"

    | InputEvent.MouseDown(button, x, y) ->
        printfn $"Mouse {button} down at ({x}, {y})"

    | InputEvent.MouseUp(button, x, y) ->
        printfn $"Mouse {button} up at ({x}, {y})"

    | InputEvent.MouseScroll(delta, x, y) ->
        printfn $"Scroll {delta} at ({x}, {y})"

    | InputEvent.WindowResize(w, h) ->
        printfn $"Window resized to {w}x{h}"

    | InputEvent.FrameTick elapsed ->
        () // fired every frame with elapsed seconds since start
)

(**
<div class="alert alert-info">
<strong>Tip:</strong> <code>FrameTick</code> fires every frame and carries the elapsed time
since the viewer started. Use it for animations by computing scene updates based on the
elapsed time value.
</div>

## Interactive Example: Zoom and Pan

This example builds a simple interactive viewer with keyboard zoom and mouse pan:
*)

(*** do-not-eval ***)
let mutable zoom = 1.0f
let mutable panX = 0f
let mutable panY = 0f

let updateScene () =
    sceneEvent.Trigger(
        Scene.create SKColors.DarkSlateGray [
            Scene.translate panX panY [
                Scene.scale zoom zoom [
                    Scene.rect 100f 100f 200f 150f (Scene.fill SKColors.CornflowerBlue)
                    Scene.circle 300f 250f 60f (Scene.fill SKColors.Coral)
                    Scene.text $"Zoom: {zoom:F1}x" 100f 350f 20f (Scene.fill SKColors.White)
                ]
            ]
        ])

updateScene ()

use _inputSub = inputs.Subscribe(fun evt ->
    match evt with
    | InputEvent.MouseScroll(delta, _, _) ->
        zoom <- zoom + delta * 0.1f |> max 0.1f |> min 10f
        updateScene ()

    | InputEvent.KeyDown Key.Left -> panX <- panX - 20f; updateScene ()
    | InputEvent.KeyDown Key.Right -> panX <- panX + 20f; updateScene ()
    | InputEvent.KeyDown Key.Up -> panY <- panY - 20f; updateScene ()
    | InputEvent.KeyDown Key.Down -> panY <- panY + 20f; updateScene ()

    | InputEvent.KeyDown Key.Escape ->
        (viewer :> IDisposable).Dispose()

    | _ -> ())

Console.ReadLine() |> ignore

(**
## Animation with FrameTick

Use `FrameTick` to drive smooth animations. The `elapsedSeconds` value increases
monotonically, so derive positions from it rather than from incremental deltas:
*)

(*** do-not-eval ***)
use _animSub = inputs.Subscribe(fun evt ->
    match evt with
    | InputEvent.FrameTick elapsed ->
        let t = float32 elapsed
        let x = 200f + 100f * cos(t * 2f)
        let y = 200f + 100f * sin(t * 2f)
        sceneEvent.Trigger(
            Scene.create SKColors.Black [
                Scene.circle x y 20f (Scene.fill SKColors.Coral)
            ])
    | _ -> ())

(**
## Thread Safety

<div class="alert alert-warning">
<strong>Important:</strong> Input events fire on the window thread. If your event handler
modifies shared mutable state, use appropriate synchronization. For simple cases, mutable
variables with <code>Interlocked</code> operations or a lock are sufficient.
</div>

## Scene Stream Error Recovery

If the scene observable emits an `OnError`, the viewer preserves the last valid scene
and continues rendering. The window does not crash — it simply keeps displaying the
last successfully received scene.

## Next Steps

- [Screenshots](screenshots.html) — capture rendered frames to files
- [Architecture Overview](architecture.html) — threading model details
*)

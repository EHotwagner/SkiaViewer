/// Example: Interactive Declarative Scene
/// Demonstrates the reactive scene stream + input event pattern.
/// Use arrow keys to move a circle. Press Escape to exit.
///
/// Run: dotnet fsi scripts/examples/02-declarative-scene.fsx

#load "../prelude.fsx"
open Prelude

open System
open System.Threading
open SkiaSharp
open SkiaViewer

let sceneEvent = Event<Scene>()

let (viewer, inputs) = Viewer.run defaultConfig sceneEvent.Publish
use viewer = viewer

let mutable cx = 400f
let mutable cy = 300f
let mutable running = true

let buildScene () =
    Scene.create SKColors.DarkSlateGray [
        // Grid lines for reference
        Scene.line 0f 300f 800f 300f (Scene.stroke (SKColor(100uy, 100uy, 100uy)) 1f)
        Scene.line 400f 0f 400f 600f (Scene.stroke (SKColor(100uy, 100uy, 100uy)) 1f)
        // Movable circle
        Scene.circle cx cy 30f (Scene.fillStroke SKColors.Coral SKColors.White 2f)
        // Position label
        Scene.text (sprintf "Position: %.0f, %.0f" cx cy) 10f 30f 20f (Scene.fill SKColors.White)
        Scene.text "Arrow keys to move, Escape to quit" 10f 580f 16f (Scene.fill (SKColor(180uy, 180uy, 180uy)))
    ]

use _sub = inputs.Subscribe(fun event ->
    match event with
    | InputEvent.KeyDown key ->
        match key with
        | Silk.NET.Input.Key.Left  -> cx <- cx - 10f
        | Silk.NET.Input.Key.Right -> cx <- cx + 10f
        | Silk.NET.Input.Key.Up    -> cy <- cy - 10f
        | Silk.NET.Input.Key.Down  -> cy <- cy + 10f
        | Silk.NET.Input.Key.Escape -> running <- false
        | _ -> ()
        sceneEvent.Trigger(buildScene ())
    | _ -> ())

// Push initial scene
sceneEvent.Trigger(buildScene ())

printfn "Interactive scene running. Use arrow keys to move the circle."

// Keep running until Escape
while running do
    Thread.Sleep(100)

printfn "Done."

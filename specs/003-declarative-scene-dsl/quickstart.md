# Quickstart: Declarative Scene DSL

**Feature**: 003-declarative-scene-dsl  
**Date**: 2026-04-09

## Basic Usage

```fsharp
open SkiaViewer
open SkiaSharp

// 1. Define window configuration
let config =
    { Title = "My App"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.CornflowerBlue
      PreferredBackend = None }

// 2. Create a scene stream (static scene example)
let scenes =
    // A simple scene emitted once
    let scene =
        Scene.create SKColors.DarkSlateBlue [
            Scene.rect 50f 50f 200f 100f (Scene.fill SKColors.Coral)
            Scene.circle 400f 200f 80f (Scene.fill SKColors.LimeGreen)
            Scene.text "Hello, Declarative!" 50f 350f 32f (Scene.fill SKColors.White)
        ]
    // Wrap in an observable that emits once
    { new System.IObservable<Scene> with
        member _.Subscribe(observer) =
            observer.OnNext(scene)
            { new System.IDisposable with member _.Dispose() = () } }

// 3. Run the viewer
let (handle, inputEvents) = Viewer.run config scenes

// 4. Subscribe to input events
inputEvents.Subscribe(fun event ->
    match event with
    | InputEvent.KeyDown key -> printfn "Key: %A" key
    | InputEvent.MouseMove(x, y) -> printfn "Mouse: %.0f, %.0f" x y
    | InputEvent.FrameTick elapsed -> () // animation tick
    | _ -> ()
) |> ignore

// 5. Take a screenshot
System.Threading.Thread.Sleep(1000)
match handle.Screenshot("/tmp/screenshots") with
| Ok path -> printfn "Saved: %s" path
| Error msg -> eprintfn "Failed: %s" msg

// 6. Clean up
(handle :> System.IDisposable).Dispose()
```

## Interactive Scene (Animation)

```fsharp
open SkiaViewer
open SkiaSharp

let config =
    { Title = "Interactive"
      Width = 800; Height = 600; TargetFps = 60
      ClearColor = SKColors.Black; PreferredBackend = None }

// Use an Event<Scene> to push scene updates
let sceneEvent = Event<Scene>()

let (handle, inputs) = Viewer.run config sceneEvent.Publish

// Reactive loop: move a circle with arrow keys
let mutable cx = 400f
let mutable cy = 300f

inputs.Subscribe(fun event ->
    match event with
    | InputEvent.KeyDown key ->
        match key with
        | Silk.NET.Input.Key.Left  -> cx <- cx - 10f
        | Silk.NET.Input.Key.Right -> cx <- cx + 10f
        | Silk.NET.Input.Key.Up    -> cy <- cy - 10f
        | Silk.NET.Input.Key.Down  -> cy <- cy + 10f
        | _ -> ()
        sceneEvent.Trigger(
            Scene.create SKColors.Black [
                Scene.circle cx cy 30f (Scene.fill SKColors.Red)
                Scene.text (sprintf "%.0f, %.0f" cx cy) 10f 30f 20f (Scene.fill SKColors.White)
            ])
    | InputEvent.FrameTick _ ->
        // Could also drive animation here
        ()
    | _ -> ()
) |> ignore

// Push initial scene
sceneEvent.Trigger(
    Scene.create SKColors.Black [
        Scene.circle cx cy 30f (Scene.fill SKColors.Red)
        Scene.text "Use arrow keys" 10f 30f 20f (Scene.fill SKColors.White)
    ])
```

## Composition with Transforms

```fsharp
// Grouped elements with transforms
let scene =
    Scene.create SKColors.White [
        // A rotated group
        Scene.rotate 45f 200f 200f [
            Scene.rect 150f 150f 100f 100f (Scene.fill SKColors.Blue)
            Scene.text "Rotated" 160f 210f 16f (Scene.fill SKColors.White)
        ]
        // A translated group
        Scene.translate 400f 100f [
            Scene.circle 0f 0f 50f (Scene.fillStroke SKColors.Red SKColors.Black 2f)
            Scene.line -50f 0f 50f 0f (Scene.stroke SKColors.Black 1f)
        ]
    ]
```

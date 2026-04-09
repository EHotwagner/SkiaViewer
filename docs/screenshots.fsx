(**
---
title: Screenshots
category: Tutorials
categoryindex: 2
index: 7
description: Capture rendered frames to PNG or JPEG files.
---
*)

(**
# Screenshots

The `ViewerHandle` returned by `Viewer.run` provides a `Screenshot` method that
captures the current rendered frame and saves it as an image file.

## Setup
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaSharp.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

open SkiaViewer
open SkiaSharp

(**
## Taking a Screenshot

`Screenshot` takes a folder path and an optional image format. It returns
`Result<string, string>` — `Ok` with the file path on success, or `Error`
with a message on failure:
*)

(*** do-not-eval ***)
open System
open System.Threading

let config : ViewerConfig =
    { Title = "Screenshot Demo"
      Width = 800; Height = 600
      TargetFps = 60
      ClearColor = SKColors.Black
      PreferredBackend = None }

let scene =
    Scene.create SKColors.CornflowerBlue [
        Scene.rect 50f 50f 200f 150f (Scene.fill SKColors.White)
        Scene.text "Captured!" 70f 140f 28f (Scene.fill SKColors.Black)
    ]

let sceneObs =
    { new IObservable<Scene> with
        member _.Subscribe(observer) =
            observer.OnNext(scene)
            { new IDisposable with member _.Dispose() = () } }

let (viewer, _) = Viewer.run config sceneObs
use viewer = viewer

// Wait for rendering to stabilize
Thread.Sleep(1000)

(**
### PNG (default)
*)

(*** do-not-eval ***)
match viewer.Screenshot("/tmp/screenshots") with
| Ok path -> printfn $"Saved to: {path}"
| Error msg -> eprintfn $"Failed: {msg}"

(**
### JPEG
*)

(*** do-not-eval ***)
match viewer.Screenshot("/tmp/screenshots", ImageFormat.Jpeg) with
| Ok path -> printfn $"Saved JPEG to: {path}"
| Error msg -> eprintfn $"Failed: {msg}"

(**
## Behavior Details

| Aspect | Behavior |
|---|---|
| **File naming** | Timestamped filename (e.g., `screenshot-20260409-143022.png`) |
| **Folder creation** | Creates the target folder (including nested directories) if it doesn't exist |
| **After disposal** | Returns `Error` after the viewer has been disposed |
| **JPEG quality** | Fixed at quality 80 |
| **Thread safety** | Safe to call from any thread |

## Screenshot with Vulkan Backend

Screenshots work with both GL and Vulkan backends. The backend selection is
transparent — the same `Screenshot` API works regardless:
*)

(*** do-not-eval ***)
let vulkanConfig = { config with PreferredBackend = Some Backend.Vulkan }

let (vulkanViewer, _) = Viewer.run vulkanConfig sceneObs
use vulkanViewer = vulkanViewer
Thread.Sleep(1000)

match vulkanViewer.Screenshot("/tmp/screenshots") with
| Ok path -> printfn $"Vulkan screenshot: {path}"
| Error msg -> eprintfn $"Vulkan screenshot failed: {msg}"

(**
## Next Steps

- [Architecture Overview](architecture.html) — how the rendering pipeline works
- [Known Issues](known-issues.html) — current limitations
*)

/// Example: Screenshot Capture
/// Demonstrates capturing screenshots in PNG and JPEG formats
/// using the declarative scene API.
///
/// Run: dotnet fsi scripts/examples/01-screenshot.fsx

#load "../prelude.fsx"
open Prelude

open System
open System.Threading
open SkiaSharp
open SkiaViewer

// Create a colorful scene
let scene =
    Scene.create SKColors.DarkSlateBlue [
        Scene.rect 50f 50f 200f 100f (Scene.fill SKColors.Coral)
        Scene.circle 400f 200f 80f (Scene.fill SKColors.LimeGreen)
        Scene.text "SkiaViewer Screenshot Demo" 50f 350f 32f (Scene.fill SKColors.White)
        Scene.line 50f 400f 500f 400f (Scene.stroke SKColors.Gold 2f)
    ]

let (viewer, _inputs) = Viewer.run defaultConfig (singleScene scene)
use viewer = viewer
Thread.Sleep(1000) // Wait for rendering to start

let outputDir = "/tmp/skiaviewer-examples"

// PNG screenshot (default)
match screenshot viewer outputDir with
| Ok path -> printfn "PNG saved: %s" path
| Error msg -> eprintfn "PNG failed: %s" msg

// JPEG screenshot
match screenshotJpeg viewer outputDir with
| Ok path -> printfn "JPEG saved: %s" path
| Error msg -> eprintfn "JPEG failed: %s" msg

printfn "Done. Screenshots saved to %s" outputDir

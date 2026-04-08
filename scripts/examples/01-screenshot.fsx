/// Example: Screenshot Capture
/// Demonstrates capturing screenshots in PNG and JPEG formats.
///
/// Run: dotnet fsi scripts/examples/01-screenshot.fsx

#load "../prelude.fsx"
open Prelude

open System
open System.Threading
open SkiaSharp

// Create a viewer that draws a colorful scene
let config =
    defaultConfig (fun canvas fbSize ->
        use bgPaint = new SKPaint(Color = SKColors.DarkSlateBlue)
        canvas.DrawRect(0.0f, 0.0f, float32 fbSize.X, float32 fbSize.Y, bgPaint)

        use rectPaint = new SKPaint(Color = SKColors.Coral, IsAntialias = true)
        canvas.DrawRect(50.0f, 50.0f, 200.0f, 100.0f, rectPaint)

        use circlePaint = new SKPaint(Color = SKColors.LimeGreen, IsAntialias = true)
        canvas.DrawCircle(400.0f, 200.0f, 80.0f, circlePaint)

        use textPaint = new SKPaint(Color = SKColors.White, TextSize = 32.0f, IsAntialias = true)
        canvas.DrawText("SkiaViewer Screenshot Demo", 50.0f, 350.0f, textPaint))

use viewer = SkiaViewer.Viewer.run config
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

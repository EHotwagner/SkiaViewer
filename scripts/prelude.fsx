/// SkiaViewer FSI Prelude
/// Load this script in F# Interactive to use the SkiaViewer library interactively.
///
/// Usage:
///   dotnet fsi scripts/prelude.fsx
///
/// Or from FSI:
///   #load "scripts/prelude.fsx"

#r "src/SkiaViewer/bin/Debug/net10.0/SkiaViewer.dll"

open System
open System.Threading
open SkiaSharp
open Silk.NET.Maths
open SkiaViewer

/// Take a screenshot of a running viewer and save it to the specified folder.
/// Returns the file path on success.
let screenshot (viewer: ViewerHandle) (folder: string) =
    viewer.Screenshot(folder)

/// Take a screenshot in JPEG format.
let screenshotJpeg (viewer: ViewerHandle) (folder: string) =
    viewer.Screenshot(folder, ImageFormat.Jpeg)

/// Create a default viewer config with a custom render callback.
let defaultConfig (onRender: SKCanvas -> Vector2D<int> -> unit) : ViewerConfig =
    { Title = "SkiaViewer"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.CornflowerBlue
      OnRender = onRender
      OnResize = fun _ _ -> ()
      OnKeyDown = fun _ -> ()
      OnMouseScroll = fun _ _ _ -> ()
      OnMouseDrag = fun _ _ -> ()
      PreferredBackend = None }

printfn "SkiaViewer prelude loaded. Use 'defaultConfig', 'screenshot', 'screenshotJpeg'."

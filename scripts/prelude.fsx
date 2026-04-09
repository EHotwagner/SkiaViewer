/// SkiaViewer FSI Prelude
/// Load this script in F# Interactive to use the SkiaViewer library interactively.
///
/// Usage:
///   dotnet fsi scripts/prelude.fsx
///
/// Or from FSI:
///   #load "scripts/prelude.fsx"

#r "nuget: SkiaSharp, 2.88.6"
#r "nuget: SkiaSharp.NativeAssets.Linux.NoDependencies, 2.88.6"
#r "nuget: Silk.NET.Windowing, 2.22.0"
#r "nuget: Silk.NET.OpenGL, 2.22.0"
#r "nuget: Silk.NET.Input, 2.22.0"
#r "nuget: Silk.NET.Vulkan, 2.22.0"
#r "../src/SkiaViewer/bin/Debug/net10.0/SkiaViewer.dll"

open System
open System.Threading
open SkiaSharp
open SkiaViewer

/// Default viewer config with sensible defaults.
let defaultConfig : ViewerConfig =
    { Title = "SkiaViewer"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.CornflowerBlue
      PreferredBackend = None }

/// Create an empty scene with the given background color.
let emptyScene (color: SKColor) = Scene.empty color

/// Create a simple scene with elements on a black background.
let simpleScene (elements: Element list) = Scene.create SKColors.Black elements

/// Wrap a single scene as an observable that emits once.
let singleScene (scene: Scene) : IObservable<Scene> =
    { new IObservable<Scene> with
        member _.Subscribe(observer) =
            observer.OnNext(scene)
            { new IDisposable with member _.Dispose() = () } }

/// Take a screenshot of a running viewer and save it to the specified folder.
let screenshot (viewer: ViewerHandle) (folder: string) =
    viewer.Screenshot(folder)

/// Take a screenshot in JPEG format.
let screenshotJpeg (viewer: ViewerHandle) (folder: string) =
    viewer.Screenshot(folder, ImageFormat.Jpeg)

printfn "SkiaViewer prelude loaded. Use 'defaultConfig', 'emptyScene', 'simpleScene', 'singleScene', 'screenshot', 'screenshotJpeg'."

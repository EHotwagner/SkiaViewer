namespace SkiaViewer

open System
open SkiaSharp

/// Rendering backend selection for the viewer.
[<RequireQualifiedAccess>]
type Backend =
    /// GPU-accelerated rendering via Vulkan and SkiaSharp's Vulkan backend.
    | Vulkan
    /// CPU raster rendering uploaded to an OpenGL texture each frame.
    | GL
    /// CPU raster rendering without windowed display (headless, reserved for future use).
    | Raster

/// Image format for screenshot output.
[<RequireQualifiedAccess>]
type ImageFormat =
    /// Lossless PNG encoding (default).
    | Png
    /// Lossy JPEG encoding at quality 80.
    | Jpeg

/// Window configuration for the declarative SkiaViewer.
/// Contains only static window properties — no callbacks.
/// Scene data and input events flow through streams.
type ViewerConfig =
    { /// Window title shown in the title bar.
      Title: string
      /// Initial window width in logical pixels.
      Width: int
      /// Initial window height in logical pixels.
      Height: int
      /// Target frames per second.
      TargetFps: int
      /// Default background clear color (used when no scene is available).
      ClearColor: SKColor
      /// Optional preferred rendering backend. None enables auto-detection.
      PreferredBackend: Backend option }

/// Handle returned by Viewer.run that provides screenshot
/// functionality and lifecycle management for the viewer window.
[<Sealed>]
type ViewerHandle =
    interface IDisposable
    /// Captures the current rendered frame and saves it as an image file.
    member Screenshot: folder: string * ?format: ImageFormat -> Result<string, string>

/// Manages a Silk.NET window on a background thread with SkiaSharp rendering.
/// Accepts a stream of declarative scenes and produces a stream of input events.
module Viewer =
    /// Start the declarative viewer. Subscribes to the scene stream and
    /// produces an input event stream. Returns a handle for lifecycle control
    /// and an observable of input events.
    val run: config: ViewerConfig -> scenes: IObservable<Scene> -> ViewerHandle * IObservable<InputEvent>

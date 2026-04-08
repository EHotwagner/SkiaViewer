namespace SkiaViewer

open System
open SkiaSharp
open Silk.NET.Maths

/// <summary>
/// Rendering backend selection for the viewer.
/// </summary>
[<RequireQualifiedAccess>]
type Backend =
    /// <summary>GPU-accelerated rendering via Vulkan and SkiaSharp's Vulkan backend.</summary>
    | Vulkan
    /// <summary>CPU raster rendering uploaded to an OpenGL texture each frame.</summary>
    | GL
    /// <summary>CPU raster rendering without windowed display (headless, reserved for future use).</summary>
    | Raster

/// <summary>
/// Image format for screenshot output.
/// </summary>
[<RequireQualifiedAccess>]
type ImageFormat =
    /// <summary>Lossless PNG encoding (default).</summary>
    | Png
    /// <summary>Lossy JPEG encoding at quality 80.</summary>
    | Jpeg

/// <summary>
/// Configuration record for the SkiaViewer visualization window.
/// </summary>
/// <remarks>
/// <para>All callback fields are invoked on the window's background thread. Ensure thread safety
/// when accessing shared mutable state from these callbacks.</para>
/// <para><c>OnRender</c> receives a SkiaSharp canvas pre-cleared with <c>ClearColor</c> and the
/// current framebuffer size. Draw operations are executed on the GPU when the Vulkan backend
/// is active, or uploaded to an OpenGL texture as a fullscreen quad when using the GL raster
/// fallback.</para>
/// <para>Backend selection: the viewer tries Vulkan first, then falls back to GL raster.
/// Set <c>PreferredBackend</c> to override auto-detection. The selected backend is logged
/// to stderr at startup.</para>
/// </remarks>
/// <example>
/// <code>
/// let config =
///     { Title = "My Viewer"
///       Width = 800
///       Height = 600
///       TargetFps = 60
///       ClearColor = SKColors.CornflowerBlue
///       OnRender = fun canvas fbSize ->
///           use paint = new SKPaint(Color = SKColors.White, TextSize = 24.0f)
///           canvas.DrawText("Hello!", 10.0f, 40.0f, paint)
///       OnResize = fun w h -> printfn "Resized to %dx%d" w h
///       OnKeyDown = fun key -> printfn "Key: %A" key
///       OnMouseScroll = fun delta x y -> printfn "Scroll %.1f at (%.0f, %.0f)" delta x y
///       OnMouseDrag = fun dx dy -> printfn "Drag (%.1f, %.1f)" dx dy
///       PreferredBackend = None }
/// </code>
/// </example>
type ViewerConfig =
    { /// <summary>Window title shown in the title bar.</summary>
      Title: string
      /// <summary>Initial window width in logical pixels.</summary>
      Width: int
      /// <summary>Initial window height in logical pixels.</summary>
      Height: int
      /// <summary>Target frames per second. Controls both the update rate and render rate.</summary>
      TargetFps: int
      /// <summary>Background clear color applied to the SkiaSharp canvas before each <c>OnRender</c> call.</summary>
      ClearColor: SKColor
      /// <summary>
      /// Called each frame with a pre-cleared <see cref="T:SkiaSharp.SKCanvas"/> and the current
      /// framebuffer size as a <see cref="T:Silk.NET.Maths.Vector2D`1"/>. Draw your scene here.
      /// </summary>
      OnRender: SKCanvas -> Vector2D<int> -> unit
      /// <summary>Called when the window is resized with the new width and height in pixels.</summary>
      OnResize: int -> int -> unit
      /// <summary>Called when a keyboard key is pressed.</summary>
      OnKeyDown: Silk.NET.Input.Key -> unit
      /// <summary>Called when the mouse scroll wheel moves. Receives the scroll delta (positive = up) and the mouse X/Y position.</summary>
      OnMouseScroll: float32 -> float32 -> float32 -> unit
      /// <summary>Called during a left-button mouse drag with horizontal and vertical movement deltas in pixels.</summary>
      OnMouseDrag: float32 -> float32 -> unit
      /// <summary>
      /// Optional preferred rendering backend. <c>None</c> enables auto-detection (Vulkan first,
      /// then GL raster fallback). <c>Some Backend.GL</c> forces the GL raster path.
      /// </summary>
      PreferredBackend: Backend option }

/// <summary>
/// Handle returned by <see cref="M:SkiaViewer.Viewer.run"/> that provides screenshot
/// functionality and lifecycle management for the viewer window.
/// </summary>
/// <remarks>
/// <para>Implements <see cref="T:System.IDisposable"/> for graceful shutdown. Disposing
/// requests the window thread to close and waits up to 5 seconds for completion.</para>
/// <para>The <c>Screenshot</c> method is thread-safe and can be called from any thread.
/// It blocks the calling thread until the image file is written to disk.</para>
/// </remarks>
[<Sealed>]
type ViewerHandle =
    interface IDisposable
    /// <summary>
    /// Captures the current rendered frame and saves it as an image file to the specified folder.
    /// </summary>
    /// <param name="folder">Destination folder path. Created automatically if it does not exist.</param>
    /// <param name="format">Image format. Defaults to <see cref="F:SkiaViewer.ImageFormat.Png"/>.</param>
    /// <returns>
    /// <c>Ok(filePath)</c> with the full path of the saved file on success,
    /// or <c>Error(message)</c> with a descriptive error message on failure.
    /// </returns>
    /// <remarks>
    /// <para>This method blocks the calling thread until the file is fully written to disk.
    /// The caller can assume the file exists immediately after the function returns <c>Ok</c>.</para>
    /// <para>The rendering loop is not interrupted during capture. The method acquires a brief
    /// lock on the render surface to take a snapshot, then encodes and writes to disk outside
    /// the lock.</para>
    /// </remarks>
    member Screenshot: folder: string * ?format: ImageFormat -> Result<string, string>

/// <summary>
/// Manages a Silk.NET window on a background thread with SkiaSharp rendering.
/// Uses Vulkan GPU-backed rendering as the primary backend with automatic
/// fallback to GL raster when Vulkan is unavailable.
/// </summary>
/// <remarks>
/// <para>The viewer runs the GLFW window loop on a dedicated background thread. When the Vulkan
/// backend is active, SkiaSharp drawing operations execute on the GPU via Vulkan command buffers
/// and frames are presented through a Vulkan swapchain. When falling back to GL raster, content
/// is rendered to an off-screen <see cref="T:SkiaSharp.SKSurface"/> and uploaded each frame to
/// an OpenGL texture.</para>
/// <para>Thread safety: the surface is protected by a lock. The window can be shut down
/// from any thread by disposing the returned handle. Frame-level exceptions in the render
/// callback are caught and logged, allowing rendering to continue.</para>
/// <para>The selected backend is logged to stderr at startup. Use
/// <see cref="T:SkiaViewer.ViewerConfig"/>'s <c>PreferredBackend</c> field to override
/// auto-detection.</para>
/// </remarks>
module Viewer =
    /// <summary>
    /// Starts the viewer window on a background thread and begins the render loop.
    /// </summary>
    /// <param name="config">The <see cref="T:SkiaViewer.ViewerConfig"/> describing window properties and callbacks.</param>
    /// <returns>
    /// A <see cref="T:SkiaViewer.ViewerHandle"/> that provides screenshot functionality
    /// and lifecycle management. Disposing it requests a graceful shutdown of the window
    /// thread, waiting up to 5 seconds for completion.
    /// </returns>
    /// <remarks>
    /// <para>The returned handle triggers a cross-thread shutdown on dispose: it sets a flag that the
    /// window's update loop checks, then waits for the window thread to exit.</para>
    /// <para>GLFW (the underlying platform) requires window creation and management on a single
    /// thread. This function handles that by running the entire window lifecycle on its own
    /// dedicated thread.</para>
    /// <para>Backend selection order: Vulkan (if available and not overridden) then GL raster.
    /// The selected backend is logged to stderr.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// let config = { ... }
    /// use viewer = Viewer.run config
    /// // Window is now running on a background thread.
    /// // Take a screenshot:
    /// match viewer.Screenshot("/tmp/screenshots") with
    /// | Ok path -> printfn "Saved to %s" path
    /// | Error msg -> eprintfn "Failed: %s" msg
    /// // Dispose to shut down:
    /// // viewer.Dispose()
    /// </code>
    /// </example>
    val run: config: ViewerConfig -> ViewerHandle

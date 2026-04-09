namespace SkiaViewer

/// <namespacedoc>
/// <summary>
/// The SkiaViewer namespace provides a hardware-accelerated 2D rendering viewer
/// that combines Silk.NET windowing with SkiaSharp drawing. The viewer accepts
/// a stream of declarative Scene values and produces a stream of InputEvent values.
/// </summary>
/// </namespacedoc>
module internal NamespaceDoc = ()

#nowarn "9"

open System
open System.IO
open System.Threading
open Silk.NET.Maths
open Silk.NET.Windowing
open Silk.NET.OpenGL
open Silk.NET.Input
open SkiaSharp

[<RequireQualifiedAccess>]
type Backend =
    | Vulkan
    | GL
    | Raster

[<RequireQualifiedAccess>]
type ImageFormat =
    | Png
    | Jpeg

type ViewerConfig =
    { Title: string
      Width: int
      Height: int
      TargetFps: int
      ClearColor: SKColor
      PreferredBackend: Backend option }

[<Sealed>]
type ViewerHandle
    internal (stop: unit -> unit,
              surfaceLock: obj,
              getSurface: unit -> SkiaSharp.SKSurface,
              getWidth: unit -> int,
              getHeight: unit -> int,
              getBackend: unit -> VulkanBackend.ActiveBackend,
              isDisposed: unit -> bool) =

    let mutable disposed = false

    member _.Screenshot(folder: string, ?format: ImageFormat) : Result<string, string> =
        if disposed then
            eprintfn "[Viewer] Screenshot failed: Viewer has been disposed"
            Error "Viewer has been disposed"
        else

        try
            let image =
                lock surfaceLock (fun () ->
                    let surf = getSurface ()
                    if obj.ReferenceEquals(surf, null) then
                        None
                    elif getWidth () <= 0 || getHeight () <= 0 then
                        None
                    else
                        match getBackend () with
                        | VulkanBackend.VulkanActive state ->
                            state.GRContext.Flush()
                            state.GRContext.Submit(true)
                        | VulkanBackend.GlRasterActive -> ()

                        let snapshot = surf.Snapshot()
                        if isNull snapshot then None
                        else
                            match getBackend () with
                            | VulkanBackend.VulkanActive _ ->
                                let info = SKImageInfo(getWidth (), getHeight (), SKColorType.Rgba8888, SKAlphaType.Premul)
                                use readbackBitmap = new SKBitmap(info)
                                let ok = snapshot.ReadPixels(info, readbackBitmap.GetPixels(), info.RowBytes, 0, 0)
                                snapshot.Dispose()
                                if ok then
                                    Some (SKImage.FromBitmap(readbackBitmap))
                                else
                                    None
                            | VulkanBackend.GlRasterActive ->
                                Some snapshot)

            match image with
            | None ->
                eprintfn "[Viewer] Screenshot failed: No active surface or framebuffer is zero-size"
                Error "No active surface or framebuffer is zero-size"
            | Some img ->
                try
                    let fmt = defaultArg format ImageFormat.Png
                    let (skFormat, ext, quality) =
                        match fmt with
                        | ImageFormat.Png -> (SKEncodedImageFormat.Png, ".png", 100)
                        | ImageFormat.Jpeg -> (SKEncodedImageFormat.Jpeg, ".jpg", 80)

                    Directory.CreateDirectory(folder) |> ignore

                    let now = DateTime.UtcNow
                    let filename = sprintf "screenshot-%s%s" (now.ToString("yyyyMMdd-HHmmss-fff")) ext
                    let fullPath = Path.Combine(folder, filename)

                    use data = img.Encode(skFormat, quality)
                    use fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write)
                    data.SaveTo(fs)

                    eprintfn "[Viewer] Screenshot saved: %s" fullPath
                    Ok fullPath
                finally
                    img.Dispose()
        with ex ->
            let msg = sprintf "%s: %s" (ex.GetType().Name) ex.Message
            eprintfn "[Viewer] Screenshot failed: %s" msg
            Error msg

    interface IDisposable with
        member this.Dispose() =
            disposed <- true
            stop ()

module Viewer =

    let private vertexShaderSrc = """#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;
out vec2 TexCoord;
void main() {
    gl_Position = vec4(aPos, 0.0, 1.0);
    TexCoord = aTexCoord;
}"""

    let private fragmentShaderSrc = """#version 330 core
in vec2 TexCoord;
out vec4 FragColor;
uniform sampler2D tex;
void main() {
    FragColor = texture(tex, TexCoord);
}"""

    let private platformRegistered = lazy (Silk.NET.Windowing.Glfw.GlfwWindowing.RegisterPlatform())

    let run (config: ViewerConfig) (scenes: IObservable<Scene>) : ViewerHandle * IObservable<InputEvent> =
        let mutable windowRef: IWindow option = None
        let surfaceLock = obj ()
        let mutable shutdownRequested = false
        let windowCompleted = new ManualResetEventSlim(false)

        // Input event publishing via F# Event<T> (implements IObservable<T>)
        let inputEvent = Event<InputEvent>()

        // Latest scene — atomically swapped from scene stream, read each frame
        let mutable latestScene: Scene option = None
        let sceneLock = obj ()

        // Shared mutable state — accessed from render thread and ViewerHandle
        let mutable surface: SKSurface = Unchecked.defaultof<_>
        let mutable surfaceWidth = 0
        let mutable surfaceHeight = 0
        let mutable activeBackend = VulkanBackend.GlRasterActive

        // Cached renderer for group-level scene diffing
        let renderCache = new RenderCache(2)

        // Scene stream subscription holder
        let mutable sceneSubscription: IDisposable = null

        let thread =
            Thread(
                ThreadStart(fun () ->
                  try
                    platformRegistered.Force()
                    let mutable opts = WindowOptions.Default
                    opts.Title <- config.Title
                    opts.Size <- Vector2D<int>(config.Width, config.Height)
                    opts.UpdatesPerSecond <- float config.TargetFps
                    opts.FramesPerSecond <- float config.TargetFps
                    opts.VSync <- false

                    let win = Window.Create opts
                    windowRef <- Some win

                    let mutable gl: GL = Unchecked.defaultof<_>
                    let mutable glReady = false
                    let mutable texture: uint32 = 0u
                    let mutable vao: uint32 = 0u
                    let mutable vbo: uint32 = 0u
                    let mutable shaderProgram: uint32 = 0u

                    let recreateSurface () =
                        if not glReady then ()
                        else

                        let fbSize = win.FramebufferSize

                        if fbSize.X > 0 && fbSize.Y > 0 then
                            let info = new SKImageInfo(fbSize.X, fbSize.Y, SKColorType.Rgba8888, SKAlphaType.Premul)
                            let newSurface =
                                match activeBackend with
                                | VulkanBackend.VulkanActive state ->
                                    VulkanBackend.createGpuSurface state fbSize.X fbSize.Y
                                | VulkanBackend.GlRasterActive ->
                                    SKSurface.Create(info)
                            let oldSurface =
                                lock surfaceLock (fun () ->
                                    let old = surface
                                    surface <- newSurface
                                    surfaceWidth <- fbSize.X
                                    surfaceHeight <- fbSize.Y
                                    old)
                            if not (obj.ReferenceEquals(oldSurface, null)) then
                                oldSurface.Dispose()
                            renderCache.Invalidate()
                            eprintfn "[Viewer] Surface created: %dx%d" fbSize.X fbSize.Y
                        else
                            let oldSurface =
                                lock surfaceLock (fun () ->
                                    let old = surface
                                    surface <- Unchecked.defaultof<_>
                                    surfaceWidth <- 0
                                    surfaceHeight <- 0
                                    old)
                            if not (obj.ReferenceEquals(oldSurface, null)) then
                                oldSurface.Dispose()
                            eprintfn "[Viewer] Framebuffer zero-size, surface cleared"

                    let compileShader (gl: GL) (shaderType: ShaderType) (source: string) =
                        let shader = gl.CreateShader(shaderType)
                        gl.ShaderSource(shader, source)
                        gl.CompileShader(shader)
                        let status = gl.GetShader(shader, ShaderParameterName.CompileStatus)
                        if status = 0 then
                            let log = gl.GetShaderInfoLog(shader)
                            eprintfn "[Viewer] Shader compile error: %s" log
                        shader

                    let setupGl () =
                        let vs = compileShader gl ShaderType.VertexShader vertexShaderSrc
                        let fs = compileShader gl ShaderType.FragmentShader fragmentShaderSrc
                        shaderProgram <- gl.CreateProgram()
                        gl.AttachShader(shaderProgram, vs)
                        gl.AttachShader(shaderProgram, fs)
                        gl.LinkProgram(shaderProgram)
                        let linkStatus = gl.GetProgram(shaderProgram, ProgramPropertyARB.LinkStatus)
                        if linkStatus = 0 then
                            let log = gl.GetProgramInfoLog(shaderProgram)
                            eprintfn "[Viewer] Shader link error: %s" log
                        gl.DeleteShader(vs)
                        gl.DeleteShader(fs)

                        let bl = [| -1.0f; -1.0f; 0.0f; 1.0f |]
                        let br = [|  1.0f; -1.0f; 1.0f; 1.0f |]
                        let tr = [|  1.0f;  1.0f; 1.0f; 0.0f |]
                        let tl = [| -1.0f;  1.0f; 0.0f; 0.0f |]
                        let vertices: float32[] = Array.concat [| bl; br; tr; bl; tr; tl |]

                        vao <- gl.GenVertexArray()
                        gl.BindVertexArray(vao)

                        vbo <- gl.GenBuffer()
                        gl.BindBuffer(GLEnum.ArrayBuffer, vbo)
                        let span = ReadOnlySpan<float32>(vertices)
                        gl.BufferData(GLEnum.ArrayBuffer, ReadOnlySpan<byte>(System.Runtime.InteropServices.MemoryMarshal.AsBytes(span).ToArray()), GLEnum.StaticDraw)

                        gl.VertexAttribPointer(0u, 2, GLEnum.Float, false, 4u * 4u, nativeint 0 |> NativeInterop.NativePtr.ofNativeInt<byte> |> NativeInterop.NativePtr.toVoidPtr)
                        gl.EnableVertexAttribArray(0u)
                        gl.VertexAttribPointer(1u, 2, GLEnum.Float, false, 4u * 4u, nativeint 8 |> NativeInterop.NativePtr.ofNativeInt<byte> |> NativeInterop.NativePtr.toVoidPtr)
                        gl.EnableVertexAttribArray(1u)

                        texture <- gl.GenTexture()
                        gl.BindTexture(GLEnum.Texture2D, texture)
                        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, int GLEnum.Linear)
                        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, int GLEnum.Linear)

                    let mutable inputCtx: Silk.NET.Input.IInputContext option = None

                    win.add_Load (fun _ ->
                        match config.PreferredBackend with
                        | Some b -> eprintfn "[Viewer] Preferred backend: %A" b
                        | None -> ()

                        let tryVulkan =
                            match config.PreferredBackend with
                            | Some Backend.GL | Some Backend.Raster -> false
                            | _ -> true

                        let vulkanState =
                            if tryVulkan then
                                try VulkanBackend.tryInit ()
                                with ex ->
                                    eprintfn "[Viewer] Vulkan initialization failed: %s" ex.Message
                                    None
                            else
                                None

                        match vulkanState with
                        | Some state ->
                            activeBackend <- VulkanBackend.VulkanActive state
                            eprintfn "[Viewer] Backend selected: Vulkan (%s)" state.DeviceName
                        | None ->
                            if tryVulkan then
                                match config.PreferredBackend with
                                | Some Backend.Vulkan ->
                                    eprintfn "[Viewer] Preferred backend Vulkan unavailable, falling back"
                                | _ -> ()
                            activeBackend <- VulkanBackend.GlRasterActive
                            eprintfn "[Viewer] Backend selected: GL raster (fallback)"

                        gl <- GL.GetApi(win)
                        gl.ClearColor(
                            float32 config.ClearColor.Red / 255.0f,
                            float32 config.ClearColor.Green / 255.0f,
                            float32 config.ClearColor.Blue / 255.0f,
                            1.0f)
                        setupGl ()
                        glReady <- true
                        recreateSurface ()
                        eprintfn "[Viewer] Window loaded, GL context ready"

                        // Wire input events
                        let input = win.CreateInput()
                        inputCtx <- Some input

                        for kb in input.Keyboards do
                            kb.add_KeyDown (fun _ key _ ->
                                try inputEvent.Trigger(InputEvent.KeyDown key) with _ -> ())
                            kb.add_KeyUp (fun _ key _ ->
                                try inputEvent.Trigger(InputEvent.KeyUp key) with _ -> ())

                        for mouse in input.Mice do
                            mouse.add_Scroll (fun _ wheel ->
                                let pos = mouse.Position
                                try inputEvent.Trigger(InputEvent.MouseScroll(wheel.Y, pos.X, pos.Y)) with _ -> ())

                            mouse.add_MouseDown (fun _ btn ->
                                let pos = mouse.Position
                                try inputEvent.Trigger(InputEvent.MouseDown(btn, pos.X, pos.Y)) with _ -> ())

                            mouse.add_MouseUp (fun _ btn ->
                                let pos = mouse.Position
                                try inputEvent.Trigger(InputEvent.MouseUp(btn, pos.X, pos.Y)) with _ -> ())

                            mouse.add_MouseMove (fun _ pos ->
                                try inputEvent.Trigger(InputEvent.MouseMove(pos.X, pos.Y)) with _ -> ())

                        // Subscribe to scene stream
                        sceneSubscription <-
                            scenes.Subscribe(
                                { new IObserver<Scene> with
                                    member _.OnNext(scene) =
                                        lock sceneLock (fun () -> latestScene <- Some scene)
                                    member _.OnError(ex) =
                                        eprintfn "[Viewer] Scene stream error: %s — keeping last valid scene" ex.Message
                                    member _.OnCompleted() =
                                        eprintfn "[Viewer] Scene stream completed — keeping last scene" }))

                    win.add_FramebufferResize (fun size ->
                        recreateSurface ()
                        try inputEvent.Trigger(InputEvent.WindowResize(size.X, size.Y)) with _ -> ())

                    win.add_Update (fun _ ->
                        if shutdownRequested then
                            eprintfn "[Viewer] Shutdown requested, closing window"
                            win.Close())

                    win.add_Render (fun delta ->
                        // Emit FrameTick at start of frame
                        try inputEvent.Trigger(InputEvent.FrameTick(delta)) with _ -> ()

                        // Hold surfaceLock for the entire render + GPU readback cycle
                        // to prevent concurrent Vulkan GRContext access from Screenshot.
                        lock surfaceLock (fun () ->
                            let snapSurface = surface
                            let snapWidth = surfaceWidth
                            let snapHeight = surfaceHeight

                            if not (obj.ReferenceEquals(snapSurface, null)) then
                                try
                                    let canvas = snapSurface.Canvas

                                    if not (obj.ReferenceEquals(canvas, null)) then
                                        // Get latest scene or use default
                                        let scene =
                                            lock sceneLock (fun () -> latestScene)

                                        match scene with
                                        | Some s ->
                                            renderCache.Render s canvas
                                        | None ->
                                            canvas.Clear config.ClearColor

                                        canvas.Flush()

                                        match activeBackend with
                                        | VulkanBackend.VulkanActive state ->
                                            state.GRContext.Flush()
                                            state.GRContext.Submit(true)
                                            use img = snapSurface.Snapshot()
                                            if not (isNull img) then
                                                let info = SKImageInfo(snapWidth, snapHeight, SKColorType.Rgba8888, SKAlphaType.Premul)
                                                use readbackBitmap = new SKBitmap(info)
                                                let ok = img.ReadPixels(info, readbackBitmap.GetPixels(), info.RowBytes, 0, 0)
                                                if ok then
                                                    let pixels = readbackBitmap.GetPixels()
                                                    gl.BindTexture(GLEnum.Texture2D, texture)
                                                    gl.TexImage2D(GLEnum.Texture2D, 0, int GLEnum.Rgba8, uint32 snapWidth, uint32 snapHeight, 0, GLEnum.Rgba, GLEnum.UnsignedByte, pixels.ToPointer())
                                                    gl.Viewport(0, 0, uint32 snapWidth, uint32 snapHeight)
                                                    gl.Clear(uint32 GLEnum.ColorBufferBit)
                                                    gl.UseProgram(shaderProgram)
                                                    gl.BindVertexArray(vao)
                                                    gl.DrawArrays(GLEnum.Triangles, 0, 6u)
                                        | VulkanBackend.GlRasterActive ->
                                            let pixmap = snapSurface.PeekPixels()
                                            if not (obj.ReferenceEquals(pixmap, null)) then
                                                let pixels = pixmap.GetPixels()
                                                gl.BindTexture(GLEnum.Texture2D, texture)
                                                gl.TexImage2D(GLEnum.Texture2D, 0, int GLEnum.Rgba8, uint32 snapWidth, uint32 snapHeight, 0, GLEnum.Rgba, GLEnum.UnsignedByte, pixels.ToPointer())
                                                gl.Viewport(0, 0, uint32 snapWidth, uint32 snapHeight)
                                                gl.Clear(uint32 GLEnum.ColorBufferBit)
                                                gl.UseProgram(shaderProgram)
                                                gl.BindVertexArray(vao)
                                                gl.DrawArrays(GLEnum.Triangles, 0, 6u)
                                with
                                | :? ObjectDisposedException as ex ->
                                    eprintfn "[Viewer] Render warning (ObjectDisposed): %s" ex.Message
                                | :? NullReferenceException as ex ->
                                    eprintfn "[Viewer] Render warning (NullReference): %s" ex.Message
                                | :? System.ArgumentNullException as ex ->
                                    eprintfn "[Viewer] Render warning (ArgumentNull): %s" ex.Message
                                | ex ->
                                    eprintfn "[Viewer] Render callback exception (%s): %s" (ex.GetType().Name) ex.Message))

                    win.add_Closing (fun _ ->
                        // Dispose render cache
                        (renderCache :> System.IDisposable).Dispose()

                        // Dispose scene subscription
                        if not (isNull sceneSubscription) then
                            sceneSubscription.Dispose()
                            sceneSubscription <- null

                        match inputCtx with
                        | Some ctx ->
                            ctx.Dispose()
                            inputCtx <- None
                        | None -> ()

                        let oldSurface =
                            lock surfaceLock (fun () ->
                                let old = surface
                                surface <- Unchecked.defaultof<_>
                                old)
                        if not (obj.ReferenceEquals(oldSurface, null)) then
                            oldSurface.Dispose()

                        match activeBackend with
                        | VulkanBackend.VulkanActive state ->
                            VulkanBackend.cleanup state
                        | VulkanBackend.GlRasterActive -> ()

                        if shaderProgram <> 0u then
                            gl.DeleteProgram(shaderProgram)
                        if vao <> 0u then
                            gl.DeleteVertexArray(vao)
                        if vbo <> 0u then
                            gl.DeleteBuffer(vbo)
                        if texture <> 0u then
                            gl.DeleteTexture(texture)
                        eprintfn "[Viewer] Resources released")

                    win.Run()
                    eprintfn "[Viewer] Window thread exiting"
                  with
                  | ex ->
                      eprintfn "[Viewer] Window thread error: %s" ex.Message
                  windowCompleted.Set())
            )

        thread.IsBackground <- true
        thread.Start()

        let stop () =
            shutdownRequested <- true
            if not (windowCompleted.Wait(TimeSpan.FromSeconds(5.0))) then
                eprintfn "[Viewer] Warning: window thread did not complete within 5s timeout"
            windowRef <- None

        let handle =
            new ViewerHandle(
                stop,
                surfaceLock,
                (fun () -> surface),
                (fun () -> surfaceWidth),
                (fun () -> surfaceHeight),
                (fun () -> activeBackend),
                (fun () -> shutdownRequested))

        (handle, inputEvent.Publish :> IObservable<InputEvent>)

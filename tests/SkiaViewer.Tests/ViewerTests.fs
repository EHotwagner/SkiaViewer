namespace SkiaViewer.Tests

open System
open System.IO
open System.Threading
open Xunit
open SkiaSharp
open SkiaViewer

/// Serialize all viewer tests — GLFW requires single-threaded window lifecycle.
[<CollectionDefinition("Viewer", DisableParallelization = true)>]
type ViewerCollection() = class end

[<Collection("Viewer")>]
type ViewerTests() =

    static let makeConfig () : ViewerConfig =
        { Title = "ViewerTest"
          Width = 400
          Height = 300
          TargetFps = 60
          ClearColor = SKColors.Black
          PreferredBackend = None }

    static let singleSceneObservable (scene: Scene) : IObservable<Scene> =
        { new IObservable<Scene> with
            member _.Subscribe(observer) =
                observer.OnNext(scene)
                { new IDisposable with member _.Dispose() = () } }

    static let testScene () =
        Scene.create SKColors.Black [
            Scene.rect 10f 10f 80f 60f (Scene.fill SKColors.CornflowerBlue)
            Scene.circle 200f 80f 40f (Scene.fill SKColors.Coral)
            Scene.line 10f 150f 300f 150f (Scene.stroke SKColors.White 2f)
            Scene.text "Hello" 10f 200f 20f (Scene.fill SKColors.Yellow)
            Scene.ellipse 300f 200f 40f 20f (Scene.fill SKColors.LimeGreen)
        ]

    [<Fact>]
    member _.``continuous rendering counts frames without exceptions`` () =
        let mutable frameCount = 0

        let sceneEvent = Event<Scene>()
        let config = makeConfig ()
        let scene = testScene ()

        let (viewer, inputs) = Viewer.run config sceneEvent.Publish
        use viewer = viewer

        // Count frames via FrameTick
        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        sceneEvent.Trigger(scene)
        Thread.Sleep(3000)

        Assert.True(frameCount > 60, $"Expected > 60 frames but got {frameCount}")

    [<Fact>]
    member _.``empty scene renders without errors`` () =
        let mutable frameCount = 0
        let scene = Scene.empty SKColors.CornflowerBlue

        let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        Thread.Sleep(2000)
        Assert.True(frameCount > 0, $"Expected frames with empty scene but got {frameCount}")

    [<Fact>]
    member _.``start stop cycle 10 times without crash`` () =
        let scene = Scene.create SKColors.Black [
            Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Orange)
        ]

        for _ in 1..10 do
            let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
            use viewer = viewer
            Thread.Sleep(500)

        Assert.True(true)

    [<Fact>]
    member _.``cross-thread dispose completes within timeout`` () =
        let mutable frameCount = 0
        let scene = testScene ()

        let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        Thread.Sleep(1000)

        let disposeTask = System.Threading.Tasks.Task.Run(fun () -> (viewer :> IDisposable).Dispose())
        let completed = disposeTask.Wait(TimeSpan.FromSeconds(2.0))

        Assert.True(completed, "Dispose should complete within 2 seconds")
        Assert.True(frameCount > 0, "Should have rendered at least some frames before dispose")

    [<Fact>]
    member _.``multi-element scene renders five element types`` () =
        let mutable frameCount = 0
        let scene = testScene ()

        let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        Thread.Sleep(3000)
        Assert.True(frameCount > 0, $"Expected frames to be rendered but got {frameCount}")

    [<Fact>]
    member _.``small window renders gracefully`` () =
        let mutable frameCount = 0
        let config = { makeConfig () with Width = 100; Height = 100 }
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.White) ]

        let (viewer, inputs) = Viewer.run config (singleSceneObservable scene)
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        Thread.Sleep(2000)
        Assert.True(frameCount > 0, $"Expected frames but got {frameCount}")

    // ── Backend tests ──

    [<Fact>]
    member _.``vulkan backend renders frames without exceptions`` () =
        let mutable frameCount = 0
        let config = { makeConfig () with PreferredBackend = Some Backend.Vulkan }
        let scene = testScene ()

        let (viewer, inputs) = Viewer.run config (singleSceneObservable scene)
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        Thread.Sleep(3000)
        Assert.True(frameCount > 60, $"Expected > 60 frames but got {frameCount}")

    [<Fact>]
    member _.``GL fallback when preferred backend is GL`` () =
        let mutable frameCount = 0
        let config = { makeConfig () with PreferredBackend = Some Backend.GL }
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Orange) ]

        let (viewer, inputs) = Viewer.run config (singleSceneObservable scene)
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        Thread.Sleep(2000)
        Assert.True(frameCount > 0, $"Expected frames via GL fallback but got {frameCount}")

    [<Fact>]
    member _.``auto-detect with PreferredBackend None renders frames`` () =
        let mutable frameCount = 0
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Green) ]

        let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        Thread.Sleep(2000)
        Assert.True(frameCount > 0, $"Expected frames with auto-detect but got {frameCount}")

    [<Fact>]
    member _.``backend selection message appears on stderr`` () =
        let mutable capturedOutput = ""
        let originalStderr = Console.Error
        use sw = new StringWriter()
        Console.SetError(sw)

        try
            let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.White) ]
            let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
            use viewer = viewer
            Thread.Sleep(2000)
            (viewer :> IDisposable).Dispose()
            capturedOutput <- sw.ToString()
        finally
            Console.SetError(originalStderr)

        Assert.Contains("Backend selected:", capturedOutput)

    // ── Input event tests ──

    [<Fact>]
    member _.``input event stream is subscribable and emits FrameTick`` () =
        let mutable frameTicks = 0
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.White) ]

        let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameTicks) |> ignore
            | _ -> ())

        Thread.Sleep(2000)
        Assert.True(frameTicks > 30, $"Expected > 30 FrameTick events but got {frameTicks}")

    [<Fact>]
    member _.``input event stream delivers multiple event types`` () =
        let mutable frameTickCount = 0
        let mutable anyEventCount = 0
        let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.White) ]

        let (viewer, inputs) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            Interlocked.Increment(&anyEventCount) |> ignore
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameTickCount) |> ignore
            | _ -> ())

        Thread.Sleep(2000)
        Assert.True(frameTickCount > 30, $"Expected > 30 FrameTick events but got {frameTickCount}")
        Assert.True(anyEventCount > 30, $"Expected > 30 total events but got {anyEventCount}")

    // ── Dynamic scene tests ──

    [<Fact>]
    member _.``pushing multiple scenes updates rendering`` () =
        let mutable frameCount = 0
        let sceneEvent = Event<Scene>()

        let (viewer, inputs) = Viewer.run (makeConfig ()) sceneEvent.Publish
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        // Push 10 distinct scenes
        for i in 0..9 do
            let x = float32 i * 20f
            sceneEvent.Trigger(
                Scene.create SKColors.Black [
                    Scene.circle x 50f 10f (Scene.fill SKColors.Red)
                ])
            Thread.Sleep(100)

        Thread.Sleep(500)
        Assert.True(frameCount > 0, $"Expected frames rendered but got {frameCount}")

    [<Fact>]
    member _.``scene stream error keeps last valid scene`` () =
        let mutable frameCount = 0

        let errorObservable =
            { new IObservable<Scene> with
                member _.Subscribe(observer) =
                    // Emit 5 valid scenes then error
                    for _ in 1..5 do
                        observer.OnNext(Scene.create SKColors.Black [
                            Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)
                        ])
                    observer.OnError(Exception("Test error"))
                    { new IDisposable with member _.Dispose() = () } }

        let (viewer, inputs) = Viewer.run (makeConfig ()) errorObservable
        use viewer = viewer

        use _sub = inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick _ -> Interlocked.Increment(&frameCount) |> ignore
            | _ -> ())

        Thread.Sleep(2000)
        // Should still render (last valid scene preserved)
        Assert.True(frameCount > 30, $"Expected continued rendering after error but got {frameCount}")

    // ── Screenshot tests ──

    [<Fact>]
    member _.``screenshot saves PNG file`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir) |> ignore

        try
            let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 200f 150f (Scene.fill SKColors.Red) ]
            let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
            use viewer = viewer
            Thread.Sleep(1000)

            let result = viewer.Screenshot(tempDir)
            match result with
            | Ok path ->
                Assert.True(File.Exists(path), $"Screenshot file should exist at {path}")
                Assert.EndsWith(".png", path)
                Assert.True(FileInfo(path).Length > 0L, "Screenshot file should not be empty")
            | Error msg ->
                Assert.Fail($"Screenshot should succeed but got Error: {msg}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot saves JPEG when format specified`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))

        try
            let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 200f 150f (Scene.fill SKColors.Magenta) ]
            let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
            use viewer = viewer
            Thread.Sleep(1000)

            let result = viewer.Screenshot(tempDir, ImageFormat.Jpeg)
            match result with
            | Ok path ->
                Assert.True(File.Exists(path))
                Assert.EndsWith(".jpg", path)
                Assert.True(FileInfo(path).Length > 0L)
            | Error msg ->
                Assert.Fail($"Screenshot should succeed but got Error: {msg}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot returns error after viewer disposal`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir) |> ignore

        try
            let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Purple) ]
            let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
            Thread.Sleep(1000)
            (viewer :> IDisposable).Dispose()

            let result = viewer.Screenshot(tempDir)
            match result with
            | Error _ -> ()
            | Ok path -> Assert.Fail($"Expected Error after disposal, but got Ok: {path}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot creates non-existent folder`` () =
        let baseDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        let nestedDir = Path.Combine(baseDir, "nested", "subfolder")

        try
            let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Orange) ]
            let (viewer, _) = Viewer.run (makeConfig ()) (singleSceneObservable scene)
            use viewer = viewer
            Thread.Sleep(1000)

            let result = viewer.Screenshot(nestedDir)
            match result with
            | Ok path ->
                Assert.True(Directory.Exists(nestedDir))
                Assert.True(File.Exists(path))
            | Error msg ->
                Assert.Fail($"Screenshot should succeed but got Error: {msg}")
        finally
            if Directory.Exists(baseDir) then Directory.Delete(baseDir, true)

    [<Fact>]
    member _.``screenshot with Vulkan backend`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))

        try
            let config = { makeConfig () with PreferredBackend = Some Backend.Vulkan }
            let scene = Scene.create SKColors.Black [ Scene.rect 0f 0f 200f 150f (Scene.fill SKColors.Red) ]
            let (viewer, _) = Viewer.run config (singleSceneObservable scene)
            use viewer = viewer
            Thread.Sleep(1000)

            let result = viewer.Screenshot(tempDir)
            match result with
            | Ok path ->
                Assert.True(File.Exists(path))
                Assert.True(FileInfo(path).Length > 0L)
            | Error msg ->
                Assert.Fail($"Screenshot with Vulkan should succeed: {msg}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    // ── Surface area baseline test ──

    [<Fact>]
    member _.``public API surface matches baseline`` () =
        let asm = typeof<ViewerConfig>.Assembly
        let publicTypes =
            asm.GetExportedTypes()
            |> Array.map (fun t -> t.FullName)
            |> Array.sort

        // Existing types
        Assert.Contains("SkiaViewer.Backend", publicTypes)
        Assert.Contains("SkiaViewer.ImageFormat", publicTypes)
        Assert.Contains("SkiaViewer.ViewerConfig", publicTypes)
        Assert.Contains("SkiaViewer.ViewerHandle", publicTypes)
        Assert.Contains("SkiaViewer.Viewer", publicTypes)

        // New declarative types
        Assert.Contains("SkiaViewer.Paint", publicTypes)
        Assert.Contains("SkiaViewer.Transform", publicTypes)
        Assert.Contains("SkiaViewer.PathCommand", publicTypes)
        Assert.Contains("SkiaViewer.Element", publicTypes)
        Assert.Contains("SkiaViewer.Scene", publicTypes)
        Assert.Contains("SkiaViewer.InputEvent", publicTypes)

        // ViewerHandle still has Screenshot and IDisposable
        let handleType = typeof<ViewerHandle>
        let screenshotMethod = handleType.GetMethod("Screenshot")
        Assert.NotNull(screenshotMethod)
        Assert.True(typeof<IDisposable>.IsAssignableFrom(handleType))

        // Viewer.run returns ViewerHandle * IObservable<InputEvent>
        let viewerModule = asm.GetType("SkiaViewer.Viewer")
        Assert.NotNull(viewerModule)
        let runMethod = viewerModule.GetMethod("run")
        Assert.NotNull(runMethod)

        // Scene module has DSL helpers
        let sceneModule = asm.GetType("SkiaViewer.Scene")
        Assert.NotNull(sceneModule)

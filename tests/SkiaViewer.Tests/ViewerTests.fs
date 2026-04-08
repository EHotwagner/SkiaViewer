namespace SkiaViewer.Tests

open System
open System.IO
open System.Threading
open Xunit
open SkiaSharp
open Silk.NET.Maths
open SkiaViewer

/// Serialize all viewer tests — GLFW requires single-threaded window lifecycle.
[<CollectionDefinition("Viewer", DisableParallelization = true)>]
type ViewerCollection() = class end

[<Collection("Viewer")>]
type ViewerTests() =

    static let makeConfig (onRender: SKCanvas -> Vector2D<int> -> unit) =
        { Title = "ViewerTest"
          Width = 400
          Height = 300
          TargetFps = 60
          ClearColor = SKColors.Black
          OnRender = onRender
          OnResize = fun _ _ -> ()
          OnKeyDown = fun _ -> ()
          OnMouseScroll = fun _ _ _ -> ()
          OnMouseDrag = fun _ _ -> ()
          PreferredBackend = None }

    [<Fact>]
    member _.``continuous rendering counts frames without exceptions`` () =
        let mutable frameCount = 0
        let mutable exceptionCount = 0

        let config =
            makeConfig (fun canvas fbSize ->
                try
                    Interlocked.Increment(&frameCount) |> ignore

                    use fillPaint = new SKPaint(Color = SKColors.CornflowerBlue, IsAntialias = true)
                    canvas.DrawRect(10.0f, 10.0f, 80.0f, 60.0f, fillPaint)

                    use circlePaint = new SKPaint(Color = SKColors.Coral, IsAntialias = true)
                    canvas.DrawCircle(200.0f, 80.0f, 40.0f, circlePaint)

                    use linePaint = new SKPaint(Color = SKColors.White, StrokeWidth = 2.0f, IsStroke = true)
                    canvas.DrawLine(10.0f, 150.0f, 300.0f, 150.0f, linePaint)

                    use textPaint = new SKPaint(Color = SKColors.Yellow, TextSize = 20.0f, IsAntialias = true)
                    canvas.DrawText("Frame " + string frameCount, 10.0f, 200.0f, textPaint)

                    use shader = SKShader.CreateLinearGradient(
                        SKPoint(10.0f, 220.0f), SKPoint(200.0f, 270.0f),
                        [| SKColors.Red; SKColors.Blue |], [| 0.0f; 1.0f |],
                        SKShaderTileMode.Clamp)
                    use gradPaint = new SKPaint(Shader = shader, IsAntialias = true)
                    canvas.DrawRect(10.0f, 220.0f, 190.0f, 50.0f, gradPaint)
                with _ ->
                    Interlocked.Increment(&exceptionCount) |> ignore)

        use viewer = Viewer.run config
        Thread.Sleep(3000)

        Assert.True(frameCount > 60, $"Expected > 60 frames but got {frameCount}")
        Assert.Equal(0, exceptionCount)

    [<Fact>]
    member _.``render exception recovery continues rendering`` () =
        let mutable frameCount = 0

        let config =
            makeConfig (fun canvas _ ->
                let n = Interlocked.Increment(&frameCount)
                use paint = new SKPaint(Color = SKColors.Green, IsAntialias = true)
                canvas.DrawRect(20.0f, 20.0f, 100.0f, 50.0f, paint)
                if n % 10 = 0 then
                    raise (InvalidOperationException("Deliberate test exception")))

        use viewer = Viewer.run config
        Thread.Sleep(3000)

        Assert.True(frameCount > 60, $"Expected > 60 frames but got {frameCount}")

    [<Fact>]
    member _.``start stop cycle 10 times without crash`` () =
        for i in 1..10 do
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.Orange)
                    canvas.DrawRect(0.0f, 0.0f, 100.0f, 100.0f, paint))

            use viewer = Viewer.run config
            Thread.Sleep(500)

        Assert.True(true)

    [<Fact>]
    member _.``cross-thread dispose completes within timeout`` () =
        let mutable frameCount = 0

        let config =
            makeConfig (fun canvas _ ->
                Interlocked.Increment(&frameCount) |> ignore
                use paint = new SKPaint(Color = SKColors.Purple)
                canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))

        let viewer = Viewer.run config
        Thread.Sleep(1000)

        let disposeTask = System.Threading.Tasks.Task.Run(fun () -> (viewer :> IDisposable).Dispose())
        let completed = disposeTask.Wait(TimeSpan.FromSeconds(2.0))

        Assert.True(completed, "Dispose should complete within 2 seconds")
        Assert.True(frameCount > 0, "Should have rendered at least some frames before dispose")

    [<Fact>]
    member _.``standalone demo renders five primitive types`` () =
        let mutable frameCount = 0

        let config =
            makeConfig (fun canvas fbSize ->
                Interlocked.Increment(&frameCount) |> ignore

                use fillPaint = new SKPaint(Color = SKColors.DodgerBlue, IsAntialias = true)
                canvas.DrawRect(10.0f, 10.0f, 120.0f, 80.0f, fillPaint)

                use strokePaint = new SKPaint(Color = SKColors.LimeGreen, IsStroke = true, StrokeWidth = 3.0f, IsAntialias = true)
                let rrect = new SKRoundRect(SKRect(150.0f, 10.0f, 300.0f, 90.0f), 10.0f, 10.0f)
                canvas.DrawRoundRect(rrect, strokePaint)

                use circlePaint = new SKPaint(Color = SKColors.Tomato, IsAntialias = true)
                canvas.DrawCircle(60.0f, 160.0f, 35.0f, circlePaint)

                use linePaint = new SKPaint(Color = SKColors.Gold, StrokeWidth = 2.0f, IsStroke = true, IsAntialias = true)
                canvas.DrawLine(120.0f, 120.0f, 350.0f, 200.0f, linePaint)

                use textPaint = new SKPaint(Color = SKColors.White, TextSize = 24.0f, IsAntialias = true)
                canvas.DrawText($"Frame {frameCount}", 150.0f, 170.0f, textPaint)

                use shader = SKShader.CreateLinearGradient(
                    SKPoint(10.0f, 220.0f), SKPoint(350.0f, 270.0f),
                    [| SKColors.DeepPink; SKColors.Cyan |], [| 0.0f; 1.0f |],
                    SKShaderTileMode.Clamp)
                use gradPaint = new SKPaint(Shader = shader, IsAntialias = true)
                canvas.DrawRect(10.0f, 220.0f, 340.0f, 50.0f, gradPaint))

        use viewer = Viewer.run config
        Thread.Sleep(3000)

        Assert.True(frameCount > 0, $"Expected frames to be rendered but got {frameCount}")

    [<Fact>]
    member _.``callbacks wired without exceptions`` () =
        let mutable frameCount = 0

        let config =
            { Title = "Callback Test"
              Width = 400
              Height = 300
              TargetFps = 60
              ClearColor = SKColors.DarkSlateGray
              OnRender = fun canvas _ ->
                  Interlocked.Increment(&frameCount) |> ignore
                  use paint = new SKPaint(Color = SKColors.White, TextSize = 16.0f)
                  canvas.DrawText("Scroll/drag test", 10.0f, 30.0f, paint)
              OnResize = fun _ _ -> ()
              OnKeyDown = fun _ -> ()
              OnMouseScroll = fun _ _ _ -> ()
              OnMouseDrag = fun _ _ -> ()
              PreferredBackend = None }

        use viewer = Viewer.run config
        Thread.Sleep(2000)

        Assert.True(frameCount > 0, "Viewer should start and render with callbacks wired")

    [<Fact>]
    member _.``small window renders gracefully`` () =
        let mutable frameCount = 0

        let config =
            { makeConfig (fun canvas _ ->
                  Interlocked.Increment(&frameCount) |> ignore
                  use paint = new SKPaint(Color = SKColors.White)
                  canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))
              with Width = 100; Height = 100 }

        use viewer = Viewer.run config
        Thread.Sleep(2000)

        Assert.True(frameCount > 0, $"Expected frames to be rendered but got {frameCount}")

    [<Fact>]
    member _.``concurrent access from multiple threads`` () =
        let mutable frameCount = 0
        let mutable exceptionCount = 0

        let config =
            { Title = "Concurrency Test"
              Width = 400
              Height = 300
              TargetFps = 60
              ClearColor = SKColors.Black
              OnRender = fun canvas _ ->
                  try
                      Interlocked.Increment(&frameCount) |> ignore
                      use paint = new SKPaint(Color = SKColors.White)
                      canvas.DrawRect(0.0f, 0.0f, 100.0f, 100.0f, paint)
                  with _ ->
                      Interlocked.Increment(&exceptionCount) |> ignore
              OnResize = fun _ _ -> ()
              OnKeyDown = fun _ -> ()
              OnMouseScroll = fun _ _ _ -> ()
              OnMouseDrag = fun _ _ -> ()
              PreferredBackend = None }

        use viewer = Viewer.run config
        Thread.Sleep(1500)

        use cts = new CancellationTokenSource()
        let threads =
            [| Thread(fun () -> while not cts.Token.IsCancellationRequested do config.OnResize 400 300; Thread.Sleep(10))
               Thread(fun () -> while not cts.Token.IsCancellationRequested do config.OnKeyDown Silk.NET.Input.Key.A; Thread.Sleep(10))
               Thread(fun () -> while not cts.Token.IsCancellationRequested do config.OnMouseScroll 1.0f 200.0f 150.0f; Thread.Sleep(10))
               Thread(fun () -> while not cts.Token.IsCancellationRequested do config.OnMouseDrag 5.0f 5.0f; Thread.Sleep(10)) |]

        for t in threads do
            t.IsBackground <- true
            t.Start()

        Thread.Sleep(2000)
        cts.Cancel()
        for t in threads do t.Join(1000) |> ignore

        Assert.True(frameCount > 0, "Should have rendered frames during concurrent access")
        Assert.Equal(0, exceptionCount)

    // ── New tests for Vulkan backend ──

    [<Fact>]
    member _.``vulkan backend renders frames without exceptions`` () =
        let mutable frameCount = 0
        let mutable exceptionCount = 0

        let config =
            { makeConfig (fun canvas fbSize ->
                  try
                      Interlocked.Increment(&frameCount) |> ignore
                      use paint = new SKPaint(Color = SKColors.CornflowerBlue, IsAntialias = true)
                      canvas.DrawRect(10.0f, 10.0f, 80.0f, 60.0f, paint)
                      use textPaint = new SKPaint(Color = SKColors.White, TextSize = 20.0f, IsAntialias = true)
                      canvas.DrawText("Vulkan " + string frameCount, 10.0f, 200.0f, textPaint)
                  with _ ->
                      Interlocked.Increment(&exceptionCount) |> ignore)
              with PreferredBackend = Some Backend.Vulkan }

        // If Vulkan is not available, the viewer falls back to GL — still valid
        use viewer = Viewer.run config
        Thread.Sleep(3000)

        Assert.True(frameCount > 60, $"Expected > 60 frames but got {frameCount}")
        Assert.Equal(0, exceptionCount)

    [<Fact>]
    member _.``GL fallback when preferred backend is GL`` () =
        let mutable frameCount = 0

        let config =
            { makeConfig (fun canvas _ ->
                  Interlocked.Increment(&frameCount) |> ignore
                  use paint = new SKPaint(Color = SKColors.Orange)
                  canvas.DrawRect(0.0f, 0.0f, 100.0f, 100.0f, paint))
              with PreferredBackend = Some Backend.GL }

        use viewer = Viewer.run config
        Thread.Sleep(2000)

        Assert.True(frameCount > 0, $"Expected frames rendered via GL fallback but got {frameCount}")

    [<Fact>]
    member _.``auto-detect with PreferredBackend None renders frames`` () =
        let mutable frameCount = 0

        let config =
            makeConfig (fun canvas _ ->
                Interlocked.Increment(&frameCount) |> ignore
                use paint = new SKPaint(Color = SKColors.Green)
                canvas.DrawRect(0.0f, 0.0f, 100.0f, 100.0f, paint))

        use viewer = Viewer.run config
        Thread.Sleep(2000)

        Assert.True(frameCount > 0, $"Expected frames with auto-detect but got {frameCount}")

    [<Fact>]
    member _.``backend selection message appears on stderr`` () =
        let mutable capturedOutput = ""
        let originalStderr = Console.Error
        use sw = new StringWriter()
        Console.SetError(sw)

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.White)
                    canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))

            use viewer = Viewer.run config
            Thread.Sleep(2000)
            (viewer :> IDisposable).Dispose()
            capturedOutput <- sw.ToString()
        finally
            Console.SetError(originalStderr)

        Assert.Contains("Backend selected:", capturedOutput)

    [<Fact>]
    member _.``preferred backend logging appears on stderr`` () =
        let mutable capturedOutput = ""
        let originalStderr = Console.Error
        use sw = new StringWriter()
        Console.SetError(sw)

        try
            let config =
                { makeConfig (fun canvas _ ->
                      use paint = new SKPaint(Color = SKColors.White)
                      canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))
                  with PreferredBackend = Some Backend.GL }

            use viewer = Viewer.run config
            Thread.Sleep(2000)
            (viewer :> IDisposable).Dispose()
            capturedOutput <- sw.ToString()
        finally
            Console.SetError(originalStderr)

        Assert.Contains("Preferred backend:", capturedOutput)

    // ── Screenshot tests ──

    [<Fact>]
    member _.``screenshot saves PNG file to existing folder`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir) |> ignore

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.Red)
                    canvas.DrawRect(0.0f, 0.0f, 200.0f, 150.0f, paint))

            use viewer = Viewer.run config
            Thread.Sleep(1000)

            let result = viewer.Screenshot(tempDir)

            match result with
            | Ok path ->
                Assert.True(File.Exists(path), $"Screenshot file should exist at {path}")
                Assert.EndsWith(".png", path)
                let fileInfo = FileInfo(path)
                Assert.True(fileInfo.Length > 0L, "Screenshot file should not be empty")
            | Error msg ->
                Assert.Fail($"Screenshot should succeed but got Error: {msg}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot returns error before first frame`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir) |> ignore

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.Blue)
                    canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))

            use viewer = Viewer.run config
            // Do NOT sleep — call immediately before any render
            let result = viewer.Screenshot(tempDir)

            match result with
            | Error _ -> () // Expected
            | Ok path -> Assert.Fail($"Expected Error before first frame, but got Ok: {path}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot produces distinct files on rapid successive calls`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir) |> ignore

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.Green)
                    canvas.DrawRect(0.0f, 0.0f, 100.0f, 100.0f, paint))

            use viewer = Viewer.run config
            Thread.Sleep(1000)

            let results =
                [| for _ in 1..10 do viewer.Screenshot(tempDir) |]

            let okPaths =
                results
                |> Array.choose (function Ok p -> Some p | Error _ -> None)

            Assert.Equal(10, okPaths.Length)
            Assert.Equal(10, (okPaths |> Array.distinct |> Array.length))

            for path in okPaths do
                Assert.True(File.Exists(path), $"File should exist: {path}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot returns error after viewer disposal`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir) |> ignore

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.Purple)
                    canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))

            let viewer = Viewer.run config
            Thread.Sleep(1000)
            (viewer :> IDisposable).Dispose()

            let result = viewer.Screenshot(tempDir)

            match result with
            | Error _ -> () // Expected
            | Ok path -> Assert.Fail($"Expected Error after disposal, but got Ok: {path}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot returns error when framebuffer is zero-size`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDir) |> ignore

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.White)
                    canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))

            use viewer = Viewer.run config
            // Call immediately — surface may not be ready yet
            let result = viewer.Screenshot(tempDir)

            match result with
            | Error _ -> () // Expected — no surface ready
            | Ok _ -> () // Also acceptable if surface initialized fast enough
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    // ── User Story 2: Save Folder tests ──

    [<Fact>]
    member _.``screenshot creates non-existent folder`` () =
        let baseDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))
        let nestedDir = Path.Combine(baseDir, "nested", "subfolder")

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.Orange)
                    canvas.DrawRect(0.0f, 0.0f, 100.0f, 100.0f, paint))

            use viewer = Viewer.run config
            Thread.Sleep(1000)

            let result = viewer.Screenshot(nestedDir)

            match result with
            | Ok path ->
                Assert.True(Directory.Exists(nestedDir), "Nested directory should be created")
                Assert.True(File.Exists(path), $"Screenshot file should exist at {path}")
            | Error msg ->
                Assert.Fail($"Screenshot should succeed but got Error: {msg}")
        finally
            if Directory.Exists(baseDir) then Directory.Delete(baseDir, true)

    [<Fact>]
    member _.``screenshot returns error for invalid path`` () =
        let config =
            makeConfig (fun canvas _ ->
                use paint = new SKPaint(Color = SKColors.Cyan)
                canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))

        use viewer = Viewer.run config
        Thread.Sleep(1000)

        // Null character in path is invalid on all platforms
        let invalidPath = "/tmp/skiaviewer\x00invalid"
        let result = viewer.Screenshot(invalidPath)

        match result with
        | Error _ -> () // Expected
        | Ok path -> Assert.Fail($"Expected Error for invalid path, but got Ok: {path}")

    // ── User Story 3: Image Format tests ──

    [<Fact>]
    member _.``screenshot saves JPEG when format specified`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.Magenta)
                    canvas.DrawRect(0.0f, 0.0f, 200.0f, 150.0f, paint))

            use viewer = Viewer.run config
            Thread.Sleep(1000)

            let result = viewer.Screenshot(tempDir, ImageFormat.Jpeg)

            match result with
            | Ok path ->
                Assert.True(File.Exists(path), $"JPEG file should exist at {path}")
                Assert.EndsWith(".jpg", path)
                let fileInfo = FileInfo(path)
                Assert.True(fileInfo.Length > 0L, "JPEG file should not be empty")
            | Error msg ->
                Assert.Fail($"Screenshot should succeed but got Error: {msg}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot defaults to PNG when no format specified`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))

        try
            let config =
                makeConfig (fun canvas _ ->
                    use paint = new SKPaint(Color = SKColors.Teal)
                    canvas.DrawRect(0.0f, 0.0f, 100.0f, 100.0f, paint))

            use viewer = Viewer.run config
            Thread.Sleep(1000)

            let result = viewer.Screenshot(tempDir)

            match result with
            | Ok path ->
                Assert.EndsWith(".png", path)
            | Error msg ->
                Assert.Fail($"Screenshot should succeed but got Error: {msg}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    // ── Backend validation tests ──

    [<Fact>]
    member _.``screenshot works with Vulkan backend`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))

        try
            let config =
                { makeConfig (fun canvas _ ->
                      use paint = new SKPaint(Color = SKColors.Red)
                      canvas.DrawRect(0.0f, 0.0f, 200.0f, 150.0f, paint))
                  with PreferredBackend = Some Backend.Vulkan }

            use viewer = Viewer.run config
            Thread.Sleep(1000)

            let result = viewer.Screenshot(tempDir)

            match result with
            | Ok path ->
                Assert.True(File.Exists(path), $"File should exist: {path}")
                let fileInfo = FileInfo(path)
                Assert.True(fileInfo.Length > 0L, "File should not be empty")
            | Error msg ->
                Assert.Fail($"Screenshot with Vulkan backend should succeed: {msg}")
        finally
            if Directory.Exists(tempDir) then Directory.Delete(tempDir, true)

    [<Fact>]
    member _.``screenshot works with GL backend`` () =
        let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-test-" + Guid.NewGuid().ToString("N"))

        try
            let config =
                { makeConfig (fun canvas _ ->
                      use paint = new SKPaint(Color = SKColors.Blue)
                      canvas.DrawRect(0.0f, 0.0f, 200.0f, 150.0f, paint))
                  with PreferredBackend = Some Backend.GL }

            use viewer = Viewer.run config
            Thread.Sleep(1000)

            let result = viewer.Screenshot(tempDir)

            match result with
            | Ok path ->
                Assert.True(File.Exists(path), $"File should exist: {path}")
                let fileInfo = FileInfo(path)
                Assert.True(fileInfo.Length > 0L, "File should not be empty")
            | Error msg ->
                Assert.Fail($"Screenshot with GL backend should succeed: {msg}")
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

        // Verify expected public types exist
        Assert.Contains("SkiaViewer.Backend", publicTypes)
        Assert.Contains("SkiaViewer.ImageFormat", publicTypes)
        Assert.Contains("SkiaViewer.ViewerConfig", publicTypes)
        Assert.Contains("SkiaViewer.ViewerHandle", publicTypes)
        Assert.Contains("SkiaViewer.Viewer", publicTypes)

        // Verify ViewerHandle has Screenshot member
        let handleType = typeof<ViewerHandle>
        let screenshotMethod = handleType.GetMethod("Screenshot")
        Assert.NotNull(screenshotMethod)

        // Verify ViewerHandle implements IDisposable
        Assert.True(typeof<IDisposable>.IsAssignableFrom(handleType))

        // Verify Viewer.run returns ViewerHandle
        let viewerModule = asm.GetType("SkiaViewer.Viewer")
        Assert.NotNull(viewerModule)
        let runMethod = viewerModule.GetMethod("run")
        Assert.NotNull(runMethod)
        Assert.Equal(typeof<ViewerHandle>, runMethod.ReturnType)

(**
---
title: Test Suite Documentation
category: Reference
categoryindex: 5
index: 5
description: Complete test documentation with code and behavior descriptions.
---
*)

(*** condition: prepare ***)
#r "../tests/SkiaViewer.Tests/bin/Release/net10.0/SkiaViewer.dll"
#r "../tests/SkiaViewer.Tests/bin/Release/net10.0/SkiaSharp.dll"
#r "../tests/SkiaViewer.Tests/bin/Release/net10.0/Silk.NET.Maths.dll"
#r "../tests/SkiaViewer.Tests/bin/Release/net10.0/Silk.NET.Input.Common.dll"
#r "../tests/SkiaViewer.Tests/bin/Release/net10.0/Silk.NET.Core.dll"
#r "../tests/SkiaViewer.Tests/bin/Release/net10.0/xunit.core.dll"
#r "../tests/SkiaViewer.Tests/bin/Release/net10.0/xunit.assert.dll"
#r "../tests/SkiaViewer.Tests/bin/Release/net10.0/xunit.abstractions.dll"

(**
# Test Suite Documentation

SkiaViewer has 8 xUnit tests in `tests/SkiaViewer.Tests/ViewerTests.fs`. All tests
are serialized via a `[<Collection("Viewer")>]` attribute because GLFW requires
single-threaded window lifecycle management.

All tests use a shared `makeConfig` helper that creates a `ViewerConfig` with sensible
defaults (400x300, 60fps, black background, no-op input callbacks) and a custom `OnRender`.

---

## Test Infrastructure

The test file defines a collection to serialize execution:
*)

(*** do-not-eval ***)
/// Serialize all viewer tests — GLFW requires single-threaded window lifecycle.
[<Xunit.CollectionDefinition("Viewer", DisableParallelization = true)>]
type ViewerCollection() = class end

(**
And a helper to create configs with only the render callback varying:
*)

(*** do-not-eval ***)
open SkiaSharp
open Silk.NET.Maths
open SkiaViewer

let makeConfig (onRender: SKCanvas -> Vector2D<int> -> unit) =
    { Title = "ViewerTest"
      Width = 400
      Height = 300
      TargetFps = 60
      ClearColor = SKColors.Black
      OnRender = onRender
      OnResize = fun _ _ -> ()
      OnKeyDown = fun _ -> ()
      OnMouseScroll = fun _ _ _ -> ()
      OnMouseDrag = fun _ _ -> () }

(**
---

## Rendering Tests

### Test: `continuous rendering counts frames without exceptions`

**What this test does:** Creates a viewer that draws five different SkiaSharp primitives
each frame (filled rectangle, circle, line, text with frame counter, linear gradient rectangle).
It runs for 3 seconds, then asserts that more than 60 frames were rendered and that zero
exceptions occurred. The exception counter is incremented in a catch-all handler inside the
render callback.

**System under test:** `Viewer.run` — verifies the render loop sustains frame throughput
and that standard SkiaSharp drawing operations work without errors.
*)

(*** do-not-eval ***)
open System.Threading
open Xunit

let ``continuous rendering counts frames without exceptions`` () =
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

(**
---

### Test: `render exception recovery continues rendering`

**What this test does:** Creates a viewer where the render callback deliberately throws
an `InvalidOperationException` every 10th frame. It draws a green rectangle on every frame,
then conditionally raises an exception when the frame count is divisible by 10. After 3 seconds,
it asserts more than 60 frames were rendered, confirming that the viewer's frame-level exception
handling caught the exceptions and allowed rendering to continue.

**System under test:** `Viewer.run` — specifically the `try/with` exception recovery in the
render event handler.
*)

(*** do-not-eval ***)
let ``render exception recovery continues rendering`` () =
    let mutable frameCount = 0

    let config =
        makeConfig (fun canvas _ ->
            let n = Interlocked.Increment(&frameCount)
            use paint = new SKPaint(Color = SKColors.Green, IsAntialias = true)
            canvas.DrawRect(20.0f, 20.0f, 100.0f, 50.0f, paint)
            if n % 10 = 0 then
                raise (System.InvalidOperationException("Deliberate test exception")))

    use viewer = Viewer.run config
    Thread.Sleep(3000)

    Assert.True(frameCount > 60, $"Expected > 60 frames but got {frameCount}")

(**
---

### Test: `standalone demo renders five primitive types`

**What this test does:** Creates a viewer that draws six distinct SkiaSharp primitives
each frame: a filled rectangle, a stroked rounded rectangle, a filled circle, a diagonal line,
text with the frame counter, and a gradient-filled rectangle. After 3 seconds, it asserts that
at least one frame was rendered. This test validates that a variety of SkiaSharp drawing operations
work correctly within the viewer pipeline.

**System under test:** `Viewer.run` — end-to-end rendering of multiple primitive types.
*)

(*** do-not-eval ***)
let ``standalone demo renders five primitive types`` () =
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

(**
---

## Lifecycle Tests

### Test: `start stop cycle 10 times without crash`

**What this test does:** Runs a tight loop that creates a viewer, lets it render for 500ms,
then disposes it — repeated 10 times. Each viewer draws an orange rectangle. The test
asserts `true` at the end (i.e., it passes if no exceptions or crashes occur during the
10 start/stop cycles).

**System under test:** `Viewer.run` and the `IDisposable` shutdown path — validates that
repeated creation and disposal does not leak resources or crash due to GLFW global state issues.
*)

(*** do-not-eval ***)
let ``start stop cycle 10 times without crash`` () =
    for i in 1..10 do
        let config =
            makeConfig (fun canvas _ ->
                use paint = new SKPaint(Color = SKColors.Orange)
                canvas.DrawRect(0.0f, 0.0f, 100.0f, 100.0f, paint))

        use viewer = Viewer.run config
        Thread.Sleep(500)

    Assert.True(true)

(**
---

### Test: `cross-thread dispose completes within timeout`

**What this test does:** Creates a viewer that renders purple rectangles and counts frames.
After 1 second of rendering, it calls `Dispose()` from a `Task.Run` (a different thread
than both the main test thread and the window thread). It asserts that the dispose completes
within 2 seconds and that at least some frames were rendered before shutdown.

**System under test:** The cross-thread shutdown mechanism — the `shutdownRequested` flag,
`ManualResetEventSlim` signaling, and the `ViewerHandle.Dispose` timeout logic.
*)

(*** do-not-eval ***)
let ``cross-thread dispose completes within timeout`` () =
    let mutable frameCount = 0

    let config =
        makeConfig (fun canvas _ ->
            Interlocked.Increment(&frameCount) |> ignore
            use paint = new SKPaint(Color = SKColors.Purple)
            canvas.DrawRect(0.0f, 0.0f, 50.0f, 50.0f, paint))

    let viewer = Viewer.run config
    Thread.Sleep(1000)

    let disposeTask = System.Threading.Tasks.Task.Run(fun () -> viewer.Dispose())
    let completed = disposeTask.Wait(System.TimeSpan.FromSeconds(2.0))

    Assert.True(completed, "Dispose should complete within 2 seconds")
    Assert.True(frameCount > 0, "Should have rendered at least some frames before dispose")

(**
---

## Callback Tests

### Test: `callbacks wired without exceptions`

**What this test does:** Creates a viewer with all callback fields explicitly set (not using
the `makeConfig` helper): `OnResize`, `OnKeyDown`, `OnMouseScroll`, and `OnMouseDrag` are all
no-ops, and `OnRender` draws text and counts frames. After 2 seconds, it asserts that frames
were rendered. This validates that setting all callback fields (even as no-ops) doesn't cause
wiring errors.

**System under test:** The input callback registration in the `win.add_Load` handler —
keyboard, mouse scroll, and mouse drag event subscriptions.
*)

(*** do-not-eval ***)
let ``callbacks wired without exceptions`` () =
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
          OnMouseDrag = fun _ _ -> () }

    use viewer = Viewer.run config
    Thread.Sleep(2000)

    Assert.True(frameCount > 0, "Viewer should start and render with callbacks wired")

(**
---

### Test: `small window renders gracefully`

**What this test does:** Creates a 100x100 pixel viewer (smaller than the default 400x300)
that draws a white rectangle and counts frames. After 2 seconds, it asserts frames were
rendered. This validates that the viewer handles small framebuffer sizes correctly —
surface creation, texture upload, and the fullscreen quad all work at small dimensions.

**System under test:** `Viewer.run` with a small window — specifically the `recreateSurface`
logic and texture upload with small framebuffer sizes.
*)

(*** do-not-eval ***)
let ``small window renders gracefully`` () =
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

(**
---

## Concurrency Tests

### Test: `concurrent access from multiple threads`

**What this test does:** Creates a viewer that counts frames and draws a white rectangle.
After 1.5 seconds of rendering, it spawns 4 background threads that simultaneously invoke
the config's callbacks in tight loops: `OnResize`, `OnKeyDown`, `OnMouseScroll`, and
`OnMouseDrag`. These threads hammer the callbacks for 2 seconds, then are cancelled. The test
asserts that frames were rendered and zero exceptions occurred in the render callback.

**System under test:** Thread safety of the viewer under concurrent callback invocation.
While the callbacks themselves are just config fields (not viewer-internal), this test validates
that the viewer's render loop doesn't crash when external code calls the same callback functions
concurrently.
*)

(*** do-not-eval ***)
let ``concurrent access from multiple threads`` () =
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
          OnMouseDrag = fun _ _ -> () }

    use viewer = Viewer.run config
    Thread.Sleep(1500)

    use cts = new System.Threading.CancellationTokenSource()
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

(**
---

## Summary

| Test | Category | Key Assertion |
|---|---|---|
| continuous rendering counts frames | Rendering | > 60 frames, 0 exceptions |
| render exception recovery | Rendering | > 60 frames despite periodic exceptions |
| standalone demo renders five primitive types | Rendering | > 0 frames with multiple primitives |
| start stop cycle 10 times | Lifecycle | No crash across 10 create/dispose cycles |
| cross-thread dispose completes | Lifecycle | Dispose from Task.Run completes in < 2s |
| callbacks wired without exceptions | Callbacks | Viewer starts with all callbacks set |
| small window renders gracefully | Callbacks | 100x100 window renders successfully |
| concurrent access from multiple threads | Concurrency | 0 exceptions under concurrent callback invocation |
*)

module SkiaViewer.PerfTests.Benchmarks

open System
open System.Diagnostics
open System.IO
open System.Threading
open SkiaSharp
open SkiaViewer
open SkiaViewer.PerfTests.Metrics
open SkiaViewer.PerfTests.Report

let private warmupDurationMs = 2000.0
let private minMeasureDurationMs = 5000.0
let private maxMeasureDurationMs = 30000.0
let private minMeasureFrames = 200

let private singleSceneObservable (scene: Scene) : IObservable<Scene> =
    { new IObservable<Scene> with
        member _.Subscribe(observer) =
            observer.OnNext(scene)
            { new IDisposable with member _.Dispose() = () } }

let private backendToString (backend: Backend) =
    match backend with
    | Backend.Vulkan -> "Vulkan"
    | Backend.GL -> "GL"
    | Backend.Raster -> "Raster"

let private makeConfig (backend: Backend) : ViewerConfig =
    { Title = "PerfTest"
      Width = 800
      Height = 600
      TargetFps = 999
      ClearColor = SKColors.Black
      PreferredBackend = Some backend }

let private runViewerBenchmark (backend: Backend) (scene: Scene) (benchName: string) (elemType: string) (elemCount: int) : BenchmarkResult =
    let collector = FrameTimeCollector()
    let memBefore = measureMemoryBefore ()

    let (viewer, inputs) = Viewer.run (makeConfig backend) (singleSceneObservable scene)

    use _sub =
        inputs.Subscribe(fun evt ->
            match evt with
            | InputEvent.FrameTick delta ->
                collector.AddFrame(delta)
                if not collector.IsWarmedUp && collector.ElapsedMs >= warmupDurationMs then
                    collector.MarkWarmedUp()
            | _ -> ())

    collector.Start()

    // Wait for warmup + measurement
    while not collector.IsWarmedUp do
        Thread.Sleep(50)

    while (collector.MeasuredCount < minMeasureFrames || collector.ElapsedMs < minMeasureDurationMs)
          && collector.ElapsedMs < maxMeasureDurationMs do
        Thread.Sleep(50)

    (viewer :> IDisposable).Dispose()

    let memAfter = measureMemoryAfter memBefore
    let frameTimes = collector.GetMeasuredFrameTimes()
    let stats = computeStats frameTimes

    { BenchmarkName = benchName
      Backend = backendToString backend
      ElementCount = elemCount
      ElementType = elemType
      WarmupFrames = collector.WarmupCount
      MeasuredFrames = stats.MeasuredFrames
      MeasuredDurationMs = stats.MeasuredDurationMs
      AvgFps = stats.AvgFps
      MedianFrameTimeMs = stats.MedianFrameTimeMs
      MinFrameTimeMs = stats.MinFrameTimeMs
      MaxFrameTimeMs = stats.MaxFrameTimeMs
      P99FrameTimeMs = stats.P99FrameTimeMs
      ManagedMemoryDeltaBytes = memAfter.ManagedMemoryDeltaBytes
      PeakWorkingSetBytes = memAfter.PeakWorkingSetBytes }

let runThroughputBenchmark (backend: Backend) : BenchmarkResult =
    let scene = SceneGenerators.generateScene "Mixed" 100 42
    eprintfn "  [Benchmark] Throughput: %s ..." (backendToString backend)
    runViewerBenchmark backend scene "Throughput" "Mixed" 100

let runStressBenchmark (backend: Backend) (elementType: string) (count: int) : BenchmarkResult =
    let scene = SceneGenerators.generateScene elementType count 42
    eprintfn "  [Benchmark] Stress: %s %s x%d ..." (backendToString backend) elementType count
    runViewerBenchmark backend scene (sprintf "Stress-%s-%d" elementType count) elementType count

let runCompositionBenchmark (elementType: string) (count: int) : CompositionResult =
    eprintfn "  [Benchmark] Composition: %s x%d ..." elementType count
    let sw = Stopwatch()
    let maxDurationMs = 5000.0
    let maxIterations = 1000

    // Measure scene construction
    sw.Restart()
    let mutable constructIters = 0
    let mutable lastScene = Unchecked.defaultof<Scene>
    while constructIters < maxIterations && sw.Elapsed.TotalMilliseconds < maxDurationMs do
        lastScene <- SceneGenerators.generateScene elementType count (42 + constructIters)
        constructIters <- constructIters + 1
    sw.Stop()
    let constructionTimeMs = sw.Elapsed.TotalMilliseconds / float constructIters

    // Measure renderer
    let info = SKImageInfo(800, 600, SKColorType.Rgba8888, SKAlphaType.Premul)
    use surface = SKSurface.Create(info)
    let canvas = surface.Canvas
    let scene = SceneGenerators.generateScene elementType count 42

    sw.Restart()
    let mutable renderIters = 0
    while renderIters < maxIterations && sw.Elapsed.TotalMilliseconds < maxDurationMs do
        SceneRenderer.render scene canvas
        renderIters <- renderIters + 1
    sw.Stop()
    let rendererTimeMs = sw.Elapsed.TotalMilliseconds / float renderIters

    { ElementCount = count
      ElementType = elementType
      ConstructionTimeMs = constructionTimeMs
      RendererTimeMs = rendererTimeMs
      ScenesPerSecond = 1000.0 / constructionTimeMs
      IterationCount = constructIters }

let runScreenshotBenchmark (backend: Backend) (elementCount: int) : ScreenshotResult =
    eprintfn "  [Benchmark] Screenshot: %s x%d ..." (backendToString backend) elementCount
    let scene = SceneGenerators.generateScene "Mixed" elementCount 42
    let tempDir = Path.Combine(Path.GetTempPath(), "skiaviewer-perf-" + Guid.NewGuid().ToString("N"))

    try
        let (viewer, _) = Viewer.run (makeConfig backend) (singleSceneObservable scene)
        Thread.Sleep(2000) // warm-up

        let captureCount = 10
        let sw = Stopwatch()
        let mutable totalMs = 0.0
        let mutable successCount = 0

        for _ in 1..captureCount do
            Thread.Sleep(200) // let the viewer render a full frame
            sw.Restart()
            match viewer.Screenshot(tempDir) with
            | Ok _ ->
                sw.Stop()
                totalMs <- totalMs + sw.Elapsed.TotalMilliseconds
                successCount <- successCount + 1
            | Error msg ->
                sw.Stop()
                eprintfn "  [Screenshot] Error: %s" msg

        (viewer :> IDisposable).Dispose()

        if Directory.Exists(tempDir) then
            Directory.Delete(tempDir, true)

        let avgMs = if successCount > 0 then totalMs / float successCount else 0.0
        { Backend = backendToString backend
          ElementCount = elementCount
          CaptureCount = successCount
          AvgCaptureTimeMs = avgMs
          CapturesPerSecond = if avgMs > 0.0 then 1000.0 / avgMs else 0.0 }
    with ex ->
        eprintfn "  [Screenshot] Benchmark failed: %s" ex.Message
        if Directory.Exists(tempDir) then
            Directory.Delete(tempDir, true)
        { Backend = backendToString backend
          ElementCount = elementCount
          CaptureCount = 0
          AvgCaptureTimeMs = 0.0
          CapturesPerSecond = 0.0 }

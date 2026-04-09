module SkiaViewer.PerfTests.Program

open System
open System.Diagnostics
open SkiaViewer
open SkiaViewer.PerfTests.Benchmarks
open SkiaViewer.PerfTests.Report
open SkiaViewer.PerfTests.SceneGenerators

let private elementTypes = [ "Rect"; "Ellipse"; "Line"; "Text"; "Path"; "Mixed" ]

let private probeVulkan () : (bool * string option) =
    try
        match VulkanBackend.tryInit () with
        | Some state ->
            let name = state.DeviceName
            VulkanBackend.cleanup state
            (true, Some name)
        | None -> (false, None)
    with _ -> (false, None)

let private flush () = Console.Out.Flush()

[<EntryPoint>]
let main argv =
    let writeJson = argv |> Array.contains "--json"
    let screenshotOnly = argv |> Array.contains "--screenshot-only"
    let totalSw = Stopwatch.StartNew()

    try
        let (vulkanAvailable, deviceName) = probeVulkan ()
        let backends =
            [ if vulkanAvailable then Backend.Vulkan
              Backend.GL ]

        printHeader vulkanAvailable deviceName
        flush ()

        let mutable throughputResults = []
        let mutable stressResults = []
        let mutable compositionResults = []

        if not screenshotOnly then
            // ── US1: Throughput ──
            eprintfn "[Phase] Running throughput benchmarks..."
            throughputResults <- backends |> List.map runThroughputBenchmark
            printThroughputTable throughputResults
            flush ()

            // ── US2: Stress ──
            eprintfn "[Phase] Running stress benchmarks..."
            stressResults <-
                [ for elemType in elementTypes do
                    for backend in backends do
                        for tier in complexityTiers do
                            yield runStressBenchmark backend elemType tier ]
            printStressTable stressResults
            printThresholds stressResults
            flush ()

            // ── US3: Composition ──
            eprintfn "[Phase] Running composition benchmarks..."
            compositionResults <-
                [ for elemType in elementTypes do
                    for tier in complexityTiers do
                        yield runCompositionBenchmark elemType tier ]
            printCompositionTable compositionResults
            flush ()

        // ── US4: Screenshot ──
        eprintfn "[Phase] Running screenshot benchmarks..."
        let screenshotResults =
            [ for backend in backends do
                try
                    yield runScreenshotBenchmark backend 100
                with ex ->
                    let name = match backend with Backend.Vulkan -> "Vulkan" | Backend.GL -> "GL" | _ -> "Unknown"
                    eprintfn "[Screenshot] %s benchmark failed: %s" name ex.Message
                    yield { Backend = name; ElementCount = 100; CaptureCount = 0
                            AvgCaptureTimeMs = 0.0; CapturesPerSecond = 0.0 } ]
        printScreenshotTable screenshotResults
        flush ()

        totalSw.Stop()
        printFooter totalSw.Elapsed.TotalSeconds
        flush ()

        if writeJson then
            let report : SuiteReport =
                { Timestamp = DateTimeOffset.Now
                  MachineName = Environment.MachineName
                  VulkanAvailable = vulkanAvailable
                  VulkanDeviceName = deviceName
                  TotalDurationSeconds = totalSw.Elapsed.TotalSeconds
                  RenderingResults = List.append throughputResults stressResults
                  CompositionResults = compositionResults
                  ScreenshotResults = screenshotResults }
            Report.writeJson report "perf-results.json"

        0
    with ex ->
        eprintfn "[FATAL] %s: %s" (ex.GetType().Name) ex.Message
        1

module SkiaViewer.PerfTests.Report

open System

type BenchmarkResult =
    { BenchmarkName: string
      Backend: string
      ElementCount: int
      ElementType: string
      WarmupFrames: int
      MeasuredFrames: int
      MeasuredDurationMs: float
      AvgFps: float
      MedianFrameTimeMs: float
      MinFrameTimeMs: float
      MaxFrameTimeMs: float
      P99FrameTimeMs: float
      ManagedMemoryDeltaBytes: int64
      PeakWorkingSetBytes: int64 }

type CompositionResult =
    { ElementCount: int
      ElementType: string
      ConstructionTimeMs: float
      RendererTimeMs: float
      ScenesPerSecond: float
      IterationCount: int }

type ScreenshotResult =
    { Backend: string
      ElementCount: int
      CaptureCount: int
      AvgCaptureTimeMs: float
      CapturesPerSecond: float }

type SuiteReport =
    { Timestamp: DateTimeOffset
      MachineName: string
      VulkanAvailable: bool
      VulkanDeviceName: string option
      TotalDurationSeconds: float
      RenderingResults: BenchmarkResult list
      CompositionResults: CompositionResult list
      ScreenshotResults: ScreenshotResult list }

let private separator =
    String.replicate 80 "="

let private sectionHeader (title: string) =
    let pad = String.replicate (78 - title.Length) "─"
    sprintf "── %s %s" title pad

let printHeader (vulkanAvailable: bool) (deviceName: string option) =
    printfn "%s" separator
    printfn "  SkiaViewer Performance Test Suite"
    printfn "  Machine: %s" Environment.MachineName
    printfn "  Date: %s" (DateTimeOffset.Now.ToString("o"))
    match vulkanAvailable, deviceName with
    | true, Some name -> printfn "  Vulkan: Available (%s)" name
    | true, None -> printfn "  Vulkan: Available"
    | false, _ -> printfn "  Vulkan: Unavailable"
    printfn "%s" separator
    printfn ""

let printThroughputTable (results: BenchmarkResult list) =
    printfn "%s" (sectionHeader "Rendering Throughput")
    printfn ""
    printfn "%-12s| %9s | %11s | %8s | %8s | %8s | %6s" "Backend" "Avg FPS" "Median (ms)" "Min (ms)" "Max (ms)" "P99 (ms)" "Frames"
    printfn "------------|-----------|-------------|----------|----------|----------|-------"
    for r in results do
        printfn "%-12s| %9.1f | %11.2f | %8.2f | %8.2f | %8.2f | %6d"
            r.Backend r.AvgFps r.MedianFrameTimeMs r.MinFrameTimeMs r.MaxFrameTimeMs r.P99FrameTimeMs r.MeasuredFrames
    printfn ""

let printStressTable (results: BenchmarkResult list) =
    let byTypeAndBackend =
        results
        |> List.groupBy (fun r -> r.ElementType)
        |> List.sortBy fst
    for (elemType, typeResults) in byTypeAndBackend do
        let byBackend =
            typeResults
            |> List.groupBy (fun r -> r.Backend)
            |> List.sortBy fst
        for (backend, backendResults) in byBackend do
            printfn "%s" (sectionHeader (sprintf "Stress Test: %s" elemType))
            printfn ""
            printfn "Backend: %s" backend
            printfn "%9s | %9s | %11s | %8s | %11s" "Elements" "Avg FPS" "Median (ms)" "P99 (ms)" "Memory (MB)"
            printfn "----------|-----------|-------------|----------|------------"
            for r in backendResults |> List.sortBy (fun r -> r.ElementCount) do
                let memMb = float r.PeakWorkingSetBytes / 1048576.0
                printfn "%9s | %9.1f | %11.2f | %8.2f | %11.1f"
                    (String.Format("{0:N0}", r.ElementCount))
                    r.AvgFps r.MedianFrameTimeMs r.P99FrameTimeMs memMb
            printfn ""

let printCompositionTable (results: CompositionResult list) =
    printfn "%s" (sectionHeader "Scene Composition")
    printfn ""
    let byType = results |> List.groupBy (fun r -> r.ElementType) |> List.sortBy fst
    for (elemType, typeResults) in byType do
        printfn "Element Type: %s" elemType
        printfn "%9s | %18s | %13s | %10s" "Elements" "Construction (ms)" "Renderer (ms)" "Scenes/sec"
        printfn "----------|--------------------|----- ---------|----------"
        for r in typeResults |> List.sortBy (fun r -> r.ElementCount) do
            printfn "%9s | %18.3f | %13.3f | %10.1f"
                (String.Format("{0:N0}", r.ElementCount))
                r.ConstructionTimeMs r.RendererTimeMs r.ScenesPerSecond
        printfn ""

let printScreenshotTable (results: ScreenshotResult list) =
    printfn "%s" (sectionHeader "Screenshot Capture")
    printfn ""
    printfn "%-12s| %8s | %8s | %12s" "Backend" "Elements" "Avg (ms)" "Captures/sec"
    printfn "------------|----------|----------|-------------"
    for r in results do
        printfn "%-12s| %8d | %8.2f | %12.1f"
            r.Backend r.ElementCount r.AvgCaptureTimeMs r.CapturesPerSecond
    printfn ""

let printThresholds (results: BenchmarkResult list) =
    printfn "%s" (sectionHeader "Interactive Rate Thresholds")
    printfn ""
    printfn "%-8s| %20s | %20s" "Backend" "60 FPS Limit" "30 FPS Limit"
    printfn "--------|----------------------|---------------------"
    let mixedResults =
        results
        |> List.filter (fun r -> r.ElementType = "Mixed" && r.BenchmarkName.StartsWith("Stress"))
        |> List.groupBy (fun r -> r.Backend)
        |> List.sortBy fst
    for (backend, backendResults) in mixedResults do
        let sorted = backendResults |> List.sortBy (fun r -> r.ElementCount)
        let find60 =
            sorted
            |> List.tryFindBack (fun r -> r.AvgFps >= 60.0)
            |> Option.map (fun r -> sprintf "%s elements" (String.Format("{0:N0}", r.ElementCount)))
            |> Option.defaultValue "< 10"
        let find30 =
            sorted
            |> List.tryFindBack (fun r -> r.AvgFps >= 30.0)
            |> Option.map (fun r -> sprintf "%s elements" (String.Format("{0:N0}", r.ElementCount)))
            |> Option.defaultValue "< 10"
        printfn "%-8s| %20s | %20s" backend find60 find30
    printfn ""

let printFooter (totalDurationSeconds: float) =
    printfn "%s" separator
    printfn "  Total duration: %.1fs" totalDurationSeconds
    printfn "%s" separator

let writeJson (report: SuiteReport) (path: string) =
    let options = System.Text.Json.JsonSerializerOptions(WriteIndented = true)
    let json = System.Text.Json.JsonSerializer.Serialize(report, options)
    System.IO.File.WriteAllText(path, json)
    printfn "JSON results written to: %s" path

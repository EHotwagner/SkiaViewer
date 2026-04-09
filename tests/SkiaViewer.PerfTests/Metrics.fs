module SkiaViewer.PerfTests.Metrics

open System
open System.Diagnostics

type FrameStats =
    { AvgFps: float
      MedianFrameTimeMs: float
      MinFrameTimeMs: float
      MaxFrameTimeMs: float
      P99FrameTimeMs: float
      MeasuredFrames: int
      MeasuredDurationMs: float }

type MemorySnapshot =
    { ManagedMemoryDeltaBytes: int64
      PeakWorkingSetBytes: int64 }

type FrameTimeCollector() =
    let warmupFrames = ResizeArray<float>()
    let measuredFrames = ResizeArray<float>()
    let mutable isWarmedUp = false
    let sw = Stopwatch()

    member _.Start() = sw.Restart()

    member _.MarkWarmedUp() =
        isWarmedUp <- true
        sw.Restart()

    member _.IsWarmedUp = isWarmedUp

    member _.AddFrame(deltaSeconds: float) =
        if isWarmedUp then
            measuredFrames.Add(deltaSeconds * 1000.0)
        else
            warmupFrames.Add(deltaSeconds * 1000.0)

    member _.WarmupCount = warmupFrames.Count
    member _.MeasuredCount = measuredFrames.Count
    member _.ElapsedMs = sw.Elapsed.TotalMilliseconds

    member _.GetMeasuredFrameTimes() =
        measuredFrames |> Seq.toList

let computeStats (frameTimes: float list) : FrameStats =
    match frameTimes with
    | [] ->
        { AvgFps = 0.0; MedianFrameTimeMs = 0.0; MinFrameTimeMs = 0.0
          MaxFrameTimeMs = 0.0; P99FrameTimeMs = 0.0; MeasuredFrames = 0; MeasuredDurationMs = 0.0 }
    | times ->
        let sorted = times |> List.sort |> Array.ofList
        let count = sorted.Length
        let totalMs = times |> List.sum
        let avgFrameTimeMs = totalMs / float count
        let avgFps = if avgFrameTimeMs > 0.0 then 1000.0 / avgFrameTimeMs else 0.0
        let median = sorted.[count / 2]
        let minFt = sorted.[0]
        let maxFt = sorted.[count - 1]
        let p99Idx = min (count - 1) (int (float count * 0.99))
        let p99 = sorted.[p99Idx]
        { AvgFps = avgFps
          MedianFrameTimeMs = median
          MinFrameTimeMs = minFt
          MaxFrameTimeMs = maxFt
          P99FrameTimeMs = p99
          MeasuredFrames = count
          MeasuredDurationMs = totalMs }

let measureMemoryBefore () : int64 =
    GC.Collect()
    GC.WaitForPendingFinalizers()
    GC.Collect()
    GC.GetTotalMemory(true)

let measureMemoryAfter (beforeBytes: int64) : MemorySnapshot =
    let afterBytes = GC.GetTotalMemory(false)
    let proc = Process.GetCurrentProcess()
    { ManagedMemoryDeltaBytes = afterBytes - beforeBytes
      PeakWorkingSetBytes = proc.PeakWorkingSet64 }

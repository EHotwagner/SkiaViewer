# Data Model: Performance Test Suite

**Feature**: 004-perf-test-suite  
**Date**: 2026-04-09

## Entities

### BenchmarkResult

Captures the outcome of a single benchmark run against one backend at one complexity tier.

**Fields**:
- `BenchmarkName: string` — identifier for the benchmark (e.g., "Throughput", "Stress-Rect-1000")
- `Backend: string` — rendering backend used ("Vulkan" or "GL")
- `ElementCount: int` — number of scene elements (0 for throughput baseline)
- `ElementType: string` — element type tested ("Rect", "Ellipse", "Line", "Text", "Path", "Mixed")
- `WarmupFrames: int` — frames discarded during warm-up
- `MeasuredFrames: int` — frames included in measurement
- `MeasuredDurationMs: float` — total measurement duration in milliseconds
- `AvgFps: float` — average frames per second
- `MedianFrameTimeMs: float` — median frame time
- `MinFrameTimeMs: float` — minimum frame time
- `MaxFrameTimeMs: float` — maximum frame time
- `P99FrameTimeMs: float` — 99th percentile frame time
- `ManagedMemoryDeltaBytes: int64` — managed heap change during benchmark
- `PeakWorkingSetBytes: int64` — process peak working set at benchmark end

### CompositionResult

Captures scene construction and renderer translation performance (CPU-only, no window).

**Fields**:
- `ElementCount: int` — scene complexity
- `ElementType: string` — element type tested
- `ConstructionTimeMs: float` — time to build the Scene via DSL
- `RendererTimeMs: float` — time to render Scene to SKCanvas
- `ScenesPerSecond: float` — throughput (1000 / constructionTimeMs)
- `IterationCount: int` — number of iterations measured

### ScreenshotResult

Captures screenshot pipeline performance.

**Fields**:
- `Backend: string` — rendering backend
- `ElementCount: int` — scene complexity
- `CaptureCount: int` — number of screenshots taken
- `AvgCaptureTimeMs: float` — average time per screenshot
- `CapturesPerSecond: float` — throughput

### SuiteReport

Top-level container for a full benchmark run.

**Fields**:
- `Timestamp: DateTimeOffset` — when the suite ran
- `MachineName: string` — hostname
- `VulkanAvailable: bool` — whether Vulkan initialized
- `VulkanDeviceName: string option` — GPU name if Vulkan available
- `TotalDurationSeconds: float` — wall-clock suite duration
- `RenderingResults: BenchmarkResult list` — throughput and stress results
- `CompositionResults: CompositionResult list` — scene construction results
- `ScreenshotResults: ScreenshotResult list` — capture performance results

## Relationships

- `SuiteReport` contains all result types as lists
- `BenchmarkResult` entries are grouped by benchmark name, then by backend, then by element count
- `CompositionResult` is independent of backend (CPU-only)
- `ScreenshotResult` is per-backend, per-complexity-tier

## State Transitions

No state machines. Benchmarks execute linearly: warmup -> measure -> report. The suite runs all benchmarks sequentially, accumulates results, and outputs the report once at the end.

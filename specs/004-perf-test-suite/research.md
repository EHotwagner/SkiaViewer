# Research: Performance Test Suite

**Feature**: 004-perf-test-suite  
**Date**: 2026-04-09

## R1: Benchmark Timing Approach

**Decision**: Use `System.Diagnostics.Stopwatch` for high-resolution timing within benchmarks. Measure frame times by subscribing to `InputEvent.FrameTick` events (which carry `elapsedSeconds: float` delta). For CPU-only benchmarks (scene construction, renderer translation), use `Stopwatch` directly around the measured code.

**Rationale**: `Stopwatch` is the standard .NET high-resolution timer. `FrameTick` delta is already emitted by the viewer on every render frame, providing accurate per-frame timing without additional instrumentation. No external benchmarking framework (BenchmarkDotNet) is needed — this is a live GPU performance characterization, not a microbenchmark.

**Alternatives considered**:
- BenchmarkDotNet: Excellent for CPU microbenchmarks, but doesn't fit the windowed GPU rendering model. It controls iteration count and warmup, which conflicts with real-time frame measurement.
- `DateTime.UtcNow`: Low resolution (~15ms on Windows), unsuitable for frame-level timing.

## R2: Test Project Architecture

**Decision**: Create a new console project `tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj` that references `SkiaViewer.fsproj`. The project is a standalone executable (not xUnit) that runs benchmarks sequentially and prints results to stdout. This avoids xUnit's timeout constraints and parallelism issues with GLFW.

**Rationale**: Performance tests need to control timing precisely, run for extended durations, and manage a single GLFW window lifecycle. xUnit is designed for pass/fail unit tests, not for benchmark reporting. A console app gives full control over warmup, measurement duration, and output format.

**Alternatives considered**:
- xUnit with `[<Fact>]` tests: Poor fit — timeout defaults, parallel execution conflicts, no native benchmark reporting.
- F# script (.fsx): Would work but lacks project-level dependency management and isn't packable/testable in CI.

## R3: Scene Generation Strategy for Stress Tests

**Decision**: Generate scenes programmatically using the existing `Scene` DSL. For each complexity tier, create elements with randomized (but seeded) positions and colors to ensure visual diversity without affecting reproducibility. Element types tested independently: Rect, Ellipse, Line, Text, Path. Mixed test uses equal distribution of all types.

**Rationale**: The Scene DSL is the public API — benchmarking through it measures the real user-facing path. Seeded randomness ensures reproducible element placement across runs. Testing each element type independently isolates per-type rendering costs.

**Alternatives considered**:
- Hardcoded scene definitions: Not scalable to 100,000 elements; impractical to maintain.
- External scene files (JSON/XML): Adds serialization overhead and format complexity outside the scope of this feature.

## R4: Warm-up and Measurement Duration

**Decision**: Each benchmark runs a warm-up phase of 2 seconds (frames discarded) followed by a measurement phase of at least 5 seconds or 200 frames, whichever is longer. This matches FR-008 and FR-012.

**Rationale**: GPU drivers, JIT compilation, and OS scheduling stabilize within the first 1-2 seconds. 5 seconds of steady-state measurement yields statistically meaningful frame time distributions. The 200-frame minimum ensures enough samples even at low FPS (e.g., 100k-element stress test).

**Alternatives considered**:
- Fixed frame count only: Penalizes slow benchmarks (100k elements at 5 FPS = 40 seconds for 200 frames). Duration cap prevents runaway tests.
- No warm-up: First frames include JIT, driver init, and GPU pipeline setup, skewing results.

## R5: Memory Measurement

**Decision**: Use `GC.GetTotalMemory(true)` before and after each benchmark to capture managed heap delta. Report peak working set via `System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64` for total process memory including native allocations (SkiaSharp, Vulkan driver).

**Rationale**: SkiaSharp and Vulkan allocate significant native memory not visible to `GC.GetTotalMemory`. Peak working set captures the full picture. The managed heap delta shows F# allocation overhead from scene construction.

**Alternatives considered**:
- `GC.GetTotalMemory` alone: Misses native allocations from SkiaSharp/Vulkan.
- Platform-specific memory APIs: Unnecessary; `Process.PeakWorkingSet64` is cross-platform on .NET.

## R6: Output Format

**Decision**: Print results to stdout in a structured text format with clear section headers, aligned columns for metrics, and a summary table. Optionally write a machine-readable JSON file alongside for programmatic analysis.

**Rationale**: Console output enables quick visual inspection. JSON enables comparison tooling (diff between runs). The developer asked to "see what is possible" — human-readable output is primary.

**Alternatives considered**:
- CSV: Less readable, harder to include metadata.
- Only JSON: Not human-scannable at a glance.

## R7: Backend Failure Handling

**Decision**: Wrap Vulkan initialization in a try-catch at the benchmark runner level. If Vulkan fails, log the failure, skip all Vulkan benchmarks, and continue with GL benchmarks. Report which backends were tested and which were skipped.

**Rationale**: Matches FR-010 and the edge case in the spec. The existing `VulkanBackend.tryInit()` already returns `Option` — the benchmark runner respects `None` as "skip Vulkan."

**Alternatives considered**:
- Fail the entire suite: Defeats the purpose of testing "what is possible" — GL results are still valuable.
- Retry Vulkan: If init fails, retrying rarely helps (driver issue, not transient).

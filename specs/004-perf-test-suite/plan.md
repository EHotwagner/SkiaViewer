# Implementation Plan: Performance Test Suite

**Branch**: `004-perf-test-suite` | **Date**: 2026-04-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/004-perf-test-suite/spec.md`

## Summary

Build a standalone F# console application that benchmarks the SkiaViewer rendering pipeline across Vulkan and GL backends. The suite measures rendering throughput, stress-tests scene complexity scaling (10 to 100,000 elements), profiles CPU-bound scene composition, and benchmarks screenshot capture. Results are printed as formatted tables to stdout with optional JSON output.

## Technical Context

**Language/Version**: F# on .NET 10.0  
**Primary Dependencies**: SkiaViewer (project reference), SkiaSharp 2.88.6, Silk.NET 2.22.0 (Windowing, OpenGL, Input, Vulkan)  
**Storage**: Stdout for results; optional JSON file output  
**Testing**: Self-validating (benchmark results are the output); existing xUnit tests remain unchanged  
**Target Platform**: Linux (developer machine with Vulkan driver)  
**Project Type**: Console application (benchmark runner)  
**Performance Goals**: Suite completes within 10 minutes; results reproducible within 10% variance  
**Constraints**: Single GLFW window at a time (Silk.NET limitation); sequential benchmark execution  
**Scale/Scope**: 5 complexity tiers x 6 element types x 2 backends = ~60 stress test data points

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| I. Spec-First Delivery | PASS | Spec, plan, research, data-model all produced before implementation |
| II. Compiler-Enforced Structural Contracts | PASS | New project is a console app (not a library). No new public API surface on SkiaViewer. No `.fsi` changes needed — `SceneRenderer` is already `internal` and accessible via `InternalsVisibleTo` |
| III. Test Evidence | PASS | The benchmark suite IS the test evidence — it exercises all rendering paths. Existing xUnit tests remain for regression. No mocks used — live Vulkan/GL rendering |
| IV. Observability | PASS | Suite prints structured diagnostics (backend status, timing, memory). Failures logged with context |
| V. Scripting Accessibility | N/A | Console app, not a library. No new public API to expose via FSI |
| F# exclusive stack | PASS | Pure F# |
| `.fsi` for public modules | N/A | No new public modules |
| Surface-area baselines | N/A | No public API changes |
| `dotnet pack` | N/A | Console app, IsPackable=false |

**Post-Phase-1 re-check**: All gates still pass. No public API changes introduced. The perf test project references the existing library; it does not extend the public surface.

## Project Structure

### Documentation (this feature)

```text
specs/004-perf-test-suite/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── cli-output.md   # Console output format contract
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
tests/
├── SkiaViewer.Tests/           # Existing xUnit tests (unchanged)
│   ├── SceneTests.fs
│   ├── SceneRendererTests.fs
│   ├── ViewerTests.fs
│   └── SkiaViewer.Tests.fsproj
└── SkiaViewer.PerfTests/       # NEW: Performance benchmark suite
    ├── SceneGenerators.fs      # Seeded scene generation for each element type and tier
    ├── Metrics.fs              # Frame time collection, percentile calculation, memory measurement
    ├── Benchmarks.fs           # Benchmark runners (throughput, stress, composition, screenshot)
    ├── Report.fs               # Stdout table formatting and optional JSON output
    ├── Program.fs              # Entry point, argument parsing, orchestration
    └── SkiaViewer.PerfTests.fsproj
```

**Structure Decision**: Separate console project under `tests/` (alongside existing test project). This keeps benchmarks out of the library while reusing the same project reference pattern. Five source files organized by responsibility: scene generation, metrics collection, benchmark execution, output formatting, and orchestration.

## Complexity Tracking

No constitution violations. No complexity justifications needed.

## Implementation Phases

### Phase 1: Project Scaffold and Metrics Infrastructure

**Files**: `SkiaViewer.PerfTests.fsproj`, `Metrics.fs`, `SceneGenerators.fs`

1. Create `tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj`:
   - OutputType: Exe, TargetFramework: net10.0, IsPackable: false
   - ProjectReference to `../../src/SkiaViewer/SkiaViewer.fsproj`
   - Same Silk.NET and SkiaSharp package references as main project
   - Add to `SkiaViewer.slnx`

2. Implement `Metrics.fs`:
   - `FrameTimeCollector`: Accumulates frame times from `FrameTick` events, supports warm-up discard
   - `computeStats`: Takes frame time list, returns avg/median/min/max/p99 FPS and frame times
   - `measureMemory`: Captures managed heap delta and peak working set
   - Percentile calculation via sorted array index

3. Implement `SceneGenerators.fs`:
   - `generateScene: elementType:string -> count:int -> seed:int -> Scene`
   - Element types: "Rect", "Ellipse", "Line", "Text", "Path", "Mixed"
   - Complexity tiers: `[| 10; 100; 1_000; 10_000; 100_000 |]`
   - Seeded `System.Random` for reproducible element placement within 800x600 viewport

### Phase 2: Benchmark Runners

**Files**: `Benchmarks.fs`

1. `runThroughputBenchmark: backend:Backend -> BenchmarkResult`:
   - Start viewer with a standard 100-element mixed scene
   - Subscribe to FrameTick, collect frame times
   - Warm-up 2s, measure 5s+
   - Return BenchmarkResult

2. `runStressBenchmark: backend:Backend -> elementType:string -> count:int -> BenchmarkResult`:
   - Generate scene via SceneGenerators
   - Same warm-up/measure pattern
   - Record memory alongside frame times

3. `runCompositionBenchmark: elementType:string -> count:int -> CompositionResult`:
   - No window needed — CPU-only
   - Time scene construction via Scene DSL (1000 iterations or 5 seconds)
   - Time SceneRenderer.render to a headless SKCanvas (1000 iterations or 5 seconds)
   - Report per-iteration times and throughput

4. `runScreenshotBenchmark: backend:Backend -> count:int -> ScreenshotResult`:
   - Start viewer with scene at given complexity
   - Call ViewerHandle.Screenshot in a loop (50 captures)
   - Measure per-capture time

### Phase 3: Report Formatting and Orchestration

**Files**: `Report.fs`, `Program.fs`

1. Implement `Report.fs`:
   - `printHeader: vulkanAvailable:bool -> deviceName:string option -> unit`
   - `printThroughputTable: results:BenchmarkResult list -> unit`
   - `printStressTable: results:BenchmarkResult list -> unit`
   - `printCompositionTable: results:CompositionResult list -> unit`
   - `printScreenshotTable: results:ScreenshotResult list -> unit`
   - `printThresholds: results:BenchmarkResult list -> unit` (find 60/30 FPS element count limits)
   - `writeJson: report:SuiteReport -> path:string -> unit` (optional JSON output)

2. Implement `Program.fs`:
   - Parse `--json` argument
   - Detect Vulkan availability (via `VulkanBackend.tryInit`)
   - Run benchmarks sequentially:
     1. Throughput (Vulkan, then GL)
     2. Stress per element type per backend per tier
     3. Composition (CPU-only, no backend needed)
     4. Screenshot (both backends)
   - Print results as each section completes
   - Print summary with interactive rate thresholds
   - Write JSON if `--json` passed
   - Exit 0 on success, 1 on fatal error

### Phase 4: Integration and Validation

1. Add project to solution: `dotnet sln SkiaViewer.slnx add tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj`
2. Verify `dotnet build -c Release` succeeds for entire solution
3. Run the suite on the target machine
4. Verify:
   - Both backends produce results (or Vulkan is gracefully skipped)
   - Frame counts are > 0 for all benchmarks
   - Stress test shows decreasing FPS with increasing element count
   - Composition benchmark shows increasing time with complexity
   - Screenshot benchmark produces valid capture times
   - Output format matches `contracts/cli-output.md`
5. Add `scripts/examples/03-perf-suite.fsx` documenting how to run the suite

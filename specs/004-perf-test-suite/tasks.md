# Tasks: Performance Test Suite

**Input**: Design documents from `/specs/004-perf-test-suite/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/cli-output.md, quickstart.md

**Tests**: Not explicitly requested in the feature specification. Benchmark results ARE the test evidence. Existing xUnit tests remain unchanged.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create the perf test project and configure build infrastructure

- [x] T001 Create project file at tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj with OutputType Exe, TargetFramework net10.0, IsPackable false, project reference to ../../src/SkiaViewer/SkiaViewer.fsproj, and same Silk.NET/SkiaSharp package references as main project
- [x] T002 Add tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj to SkiaViewer.slnx via `dotnet sln add`
- [x] T003 Create stub Program.fs at tests/SkiaViewer.PerfTests/Program.fs with main entry point that prints "SkiaViewer Performance Test Suite" and exits 0
- [x] T004 Verify `dotnet build -c Release` succeeds for entire solution including new project

**Checkpoint**: Project compiles and runs as empty console app

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure modules that ALL benchmarks depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 [P] Implement Metrics module at tests/SkiaViewer.PerfTests/Metrics.fs: FrameTimeCollector type that accumulates float frame deltas with configurable warm-up discard count; computeStats function that takes float list and returns record with avgFps, medianFrameTimeMs, minFrameTimeMs, maxFrameTimeMs, p99FrameTimeMs, measuredFrames, measuredDurationMs; measureMemory function that captures GC.GetTotalMemory delta and Process.PeakWorkingSet64
- [x] T006 [P] Implement SceneGenerators module at tests/SkiaViewer.PerfTests/SceneGenerators.fs: generateScene function taking elementType (string: "Rect", "Ellipse", "Line", "Text", "Path", "Mixed"), count (int), and seed (int), returning a Scene using the SkiaViewer.Scene DSL; elements randomly positioned within 800x600 viewport using seeded System.Random; complexityTiers constant = [| 10; 100; 1_000; 10_000; 100_000 |]
- [x] T007 [P] Implement Report module at tests/SkiaViewer.PerfTests/Report.fs: printHeader function (vulkanAvailable, deviceName option); printThroughputTable function (BenchmarkResult list); printStressTable function (BenchmarkResult list grouped by element type); printCompositionTable function (CompositionResult list); printScreenshotTable function (ScreenshotResult list); printThresholds function that finds max element count sustaining 60 FPS and 30 FPS per backend; printFooter function (total duration). Output format must match contracts/cli-output.md
- [x] T008 Add compile order in SkiaViewer.PerfTests.fsproj: SceneGenerators.fs, Metrics.fs, Report.fs (then Benchmarks.fs and Program.fs will be added in later phases)

**Checkpoint**: Foundation modules compile. SceneGenerators can produce scenes at all tiers. Metrics can compute stats from sample data. Report can format tables.

---

## Phase 3: User Story 1 - Measure Rendering Throughput (Priority: P1) MVP

**Goal**: Measure maximum rendering throughput of Vulkan and GL backends with a standard scene and report FPS, frame times, and consistency metrics side-by-side.

**Independent Test**: Run the suite and verify it prints a throughput table with valid FPS/frame time numbers for each available backend.

### Implementation for User Story 1

- [x] T009 [US1] Implement runThroughputBenchmark function in tests/SkiaViewer.PerfTests/Benchmarks.fs: takes Backend parameter; creates ViewerConfig (800x600, target 999 FPS to uncap); generates standard 100-element mixed scene via SceneGenerators; starts Viewer.run with scene observable; subscribes to FrameTick events collecting frame deltas into FrameTimeCollector; warm-up 2 seconds then measure minimum 5 seconds or 200 frames; disposes viewer; returns BenchmarkResult record from computeStats
- [x] T010 [US1] Add Benchmarks.fs to compile order in SkiaViewer.PerfTests.fsproj (after Report.fs, before Program.fs)
- [x] T011 [US1] Update Program.fs at tests/SkiaViewer.PerfTests/Program.fs: detect Vulkan availability via VulkanBackend.tryInit (clean up state after probe); print header via Report.printHeader; run throughput benchmark for Vulkan (if available) and GL; collect results; print throughput table via Report.printThroughputTable; print footer with total duration; exit 0 on success, 1 on fatal error
- [x] T012 [US1] Build and run: `dotnet run -c Release --project tests/SkiaViewer.PerfTests` — verify throughput table prints with valid FPS > 0 for both backends (or GL only if Vulkan unavailable), side-by-side comparison visible

**Checkpoint**: Suite prints a throughput comparison table for Vulkan vs GL. MVP is functional.

---

## Phase 4: User Story 2 - Stress Test with Increasing Scene Complexity (Priority: P1)

**Goal**: Progressively increase scene element count across element types and record how performance degrades, revealing practical upper bounds per backend.

**Independent Test**: Run the suite and verify stress tables show decreasing FPS as element count increases, with per-element-type and mixed breakdowns.

### Implementation for User Story 2

- [x] T013 [P] [US2] Implement runStressBenchmark function in tests/SkiaViewer.PerfTests/Benchmarks.fs: takes Backend, elementType (string), and count (int); generates scene via SceneGenerators.generateScene; same viewer lifecycle and measurement pattern as throughput; additionally captures memory via Metrics.measureMemory; returns BenchmarkResult with memory fields populated
- [x] T014 [US2] Update Program.fs to run stress benchmarks after throughput: iterate over element types ["Rect"; "Ellipse"; "Line"; "Text"; "Path"; "Mixed"], for each type iterate over complexityTiers [10; 100; 1_000; 10_000; 100_000], run for each available backend; print stress tables via Report.printStressTable grouped by element type
- [x] T015 [US2] Implement printThresholds in Report.fs: analyze stress results to find the maximum element count that sustains >= 60 FPS and >= 30 FPS for each backend (interpolate between tiers); print the Interactive Rate Thresholds table per contracts/cli-output.md
- [x] T016 [US2] Update Program.fs to call Report.printThresholds after all stress benchmarks complete
- [x] T017 [US2] Build and run full suite — verify stress tables show expected pattern (FPS decreases with more elements), thresholds table identifies 60/30 FPS limits

**Checkpoint**: Suite shows complete complexity scaling data with interactive rate thresholds per backend.

---

## Phase 5: User Story 3 - Scene Composition and Update Performance (Priority: P2)

**Goal**: Measure CPU-bound scene construction and renderer translation overhead independent of GPU rendering time.

**Independent Test**: Run the suite and verify composition table shows construction and renderer times increasing with complexity, with scenes-per-second throughput.

### Implementation for User Story 3

- [x] T018 [US3] Implement runCompositionBenchmark function in tests/SkiaViewer.PerfTests/Benchmarks.fs: takes elementType and count; no viewer window needed; uses Stopwatch to time SceneGenerators.generateScene calls (1000 iterations or 5 seconds); creates headless SKCanvas via SKSurface.Create(SKImageInfo(800, 600)) and times SceneRenderer.render calls (1000 iterations or 5 seconds); returns CompositionResult with per-iteration averages and scenes-per-second
- [x] T019 [US3] Update Program.fs to run composition benchmarks after stress tests: iterate element types and complexity tiers; print composition table via Report.printCompositionTable
- [x] T020 [US3] Build and run — verify composition table shows increasing construction/renderer times with complexity; scenes-per-second decreases as expected

**Checkpoint**: Suite reports CPU-bound scene composition overhead at all complexity tiers.

---

## Phase 6: User Story 4 - Screenshot Capture Performance (Priority: P3)

**Goal**: Measure screenshot pipeline throughput at varying scene complexities per backend.

**Independent Test**: Run the suite and verify screenshot table shows capture times and captures-per-second for each backend.

### Implementation for User Story 4

- [x] T021 [US4] Implement runScreenshotBenchmark function in tests/SkiaViewer.PerfTests/Benchmarks.fs: takes Backend and elementCount; starts viewer with scene at given complexity; waits 2 seconds for warm-up; calls ViewerHandle.Screenshot in temp directory 50 times using Stopwatch per capture; calculates average capture time and captures-per-second; cleans up temp directory; disposes viewer; returns ScreenshotResult
- [x] T022 [US4] Update Program.fs to run screenshot benchmarks after composition: run for each backend with 100 elements; print screenshot table via Report.printScreenshotTable
- [x] T023 [US4] Build and run — verify screenshot table shows valid capture times > 0 for each backend

**Checkpoint**: Suite reports screenshot pipeline throughput per backend.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final output, optional JSON, documentation, and validation

- [x] T024 [P] Implement writeJson function in Report.fs: serialize SuiteReport to JSON using System.Text.Json; write to perf-results.json in current directory
- [x] T025 [P] Update Program.fs to parse --json command-line argument; if present, call Report.writeJson after all benchmarks complete
- [x] T026 [P] Create scripts/examples/03-perf-suite.fsx documenting how to run the performance suite from the command line
- [x] T027 Verify complete suite output matches contracts/cli-output.md format: header, throughput table, stress tables, composition table, screenshot table, thresholds table, footer
- [x] T028 Run full suite end-to-end on target machine — verify all benchmarks complete within 10 minutes, both backends produce results, output is human-readable

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (project must compile) — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 2 — establishes viewer benchmark pattern
- **User Story 2 (Phase 4)**: Depends on Phase 2. Extends Benchmarks.fs pattern from US1, but can be implemented independently
- **User Story 3 (Phase 5)**: Depends on Phase 2 only (CPU-only, no viewer). Fully independent from US1/US2
- **User Story 4 (Phase 6)**: Depends on Phase 2. Uses viewer pattern similar to US1, but independent
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (Throughput)**: After Foundational — No dependencies on other stories
- **US2 (Stress)**: After Foundational — Independent of US1 (shares Benchmarks.fs file but different functions). Recommended after US1 since it extends the same patterns
- **US3 (Composition)**: After Foundational — Fully independent (CPU-only, different benchmark approach)
- **US4 (Screenshot)**: After Foundational — Independent (uses viewer but different measurement approach)

### Within Each User Story

- Benchmark function implementation before Program.fs integration
- Program.fs integration before validation run

### Parallel Opportunities

- T005, T006, T007 can all run in parallel (different files, no dependencies)
- US3 (Composition) can run in parallel with US1 or US2 (completely different code paths)
- T024, T025, T026 can all run in parallel (different files)

---

## Parallel Example: Phase 2 (Foundational)

```bash
# Launch all foundational modules together:
Task: "Implement Metrics module in tests/SkiaViewer.PerfTests/Metrics.fs"
Task: "Implement SceneGenerators module in tests/SkiaViewer.PerfTests/SceneGenerators.fs"
Task: "Implement Report module in tests/SkiaViewer.PerfTests/Report.fs"
```

## Parallel Example: User Stories 1 + 3

```bash
# US1 and US3 can proceed in parallel after Foundational:
# Developer A: US1 (Throughput - needs viewer)
Task: "Implement runThroughputBenchmark in Benchmarks.fs"
# Developer B: US3 (Composition - CPU only, no viewer)
Task: "Implement runCompositionBenchmark in Benchmarks.fs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T008)
3. Complete Phase 3: User Story 1 — Throughput (T009-T012)
4. **STOP and VALIDATE**: Run suite, verify Vulkan vs GL throughput comparison
5. This alone answers "what FPS can this machine achieve?"

### Incremental Delivery

1. Setup + Foundational -> Project compiles
2. Add US1 (Throughput) -> "What's the max FPS?" (MVP!)
3. Add US2 (Stress) -> "How does performance scale with complexity?"
4. Add US3 (Composition) -> "What's the CPU overhead of scene construction?"
5. Add US4 (Screenshot) -> "How fast can we capture frames?"
6. Polish -> JSON output, documentation, format validation

### Recommended Execution Order (Single Developer)

Phase 1 -> Phase 2 -> Phase 3 (US1) -> Phase 4 (US2) -> Phase 5 (US3) -> Phase 6 (US4) -> Phase 7

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- GLFW requires single-threaded window lifecycle — benchmarks that open viewers MUST run sequentially
- US3 (Composition) is the only story that doesn't need a window — it can truly run in parallel
- Benchmarks.fs is shared across US1, US2, US4 but each adds independent functions — no conflicts if developed sequentially
- The suite opens/closes the viewer for each benchmark to avoid state leakage
- Commit after each phase completion for clean git history

# Feature Specification: Performance Test Suite

**Feature Branch**: `004-perf-test-suite`  
**Created**: 2026-04-09  
**Status**: Draft  
**Input**: User description: "create and run a performance oriented test suite to see what is possible on this machine. vulkan driver and silk.net prerequisites are installed."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Measure Rendering Throughput (Priority: P1)

A developer wants to know the maximum rendering throughput of the Vulkan and OpenGL backends on their current machine. They run the performance test suite and receive a report showing frames per second, frame times, and consistency metrics for each rendering backend under controlled conditions.

**Why this priority**: Understanding raw rendering throughput is the most fundamental performance metric and directly answers "what is possible on this machine."

**Independent Test**: Can be fully tested by running the suite against a fixed scene and measuring frame output rate. Delivers a clear throughput baseline for both backends.

**Acceptance Scenarios**:

1. **Given** the test suite is invoked, **When** it runs the throughput benchmark with a standard scene, **Then** it reports average FPS, median frame time, and 99th-percentile frame time for each rendering backend (Vulkan, OpenGL).
2. **Given** the throughput benchmark completes, **When** results are displayed, **Then** each backend's metrics are presented side-by-side for easy comparison.
3. **Given** the machine has a working Vulkan driver, **When** the Vulkan throughput test runs, **Then** it completes without errors and produces valid numeric results.

---

### User Story 2 - Stress Test with Increasing Scene Complexity (Priority: P1)

A developer wants to understand how rendering performance degrades as scene complexity increases. The test suite progressively increases the number of rendered elements (rectangles, ellipses, lines, text, paths) and records performance at each tier, revealing the practical upper bounds of the rendering pipeline.

**Why this priority**: Equally critical to raw throughput — knowing the scaling behavior under load reveals practical limits and bottlenecks.

**Independent Test**: Can be tested by generating scenes with 10, 100, 1,000, 10,000, and 100,000 elements and recording frame times at each level. Delivers a complexity-vs-performance curve.

**Acceptance Scenarios**:

1. **Given** the stress test is invoked, **When** it runs through increasing element counts, **Then** it records frame time metrics at each complexity tier.
2. **Given** the stress test completes, **When** results are reported, **Then** the developer can see at which element count performance drops below interactive rates (e.g., below 30 FPS).
3. **Given** scenes with mixed element types (rectangles, ellipses, text, paths), **When** the stress test runs, **Then** it tests each element type independently and in combination.

---

### User Story 3 - Scene Composition and Update Performance (Priority: P2)

A developer wants to measure how quickly the system can process scene updates — the time from constructing a new scene to having it ready for rendering. This covers the declarative scene DSL construction and the scene renderer translation overhead, independent of GPU rendering time.

**Why this priority**: Scene composition is a CPU-bound operation that can become a bottleneck in dynamic applications. Understanding this overhead helps determine how frequently scenes can be updated.

**Independent Test**: Can be tested by timing scene construction and renderer processing for scenes of varying complexity, without requiring a visible window. Delivers scene update throughput numbers.

**Acceptance Scenarios**:

1. **Given** the composition benchmark is invoked, **When** it constructs scenes of varying sizes using the Scene DSL, **Then** it reports construction time per scene and scenes-per-second throughput.
2. **Given** scenes are constructed, **When** the scene renderer processes them, **Then** the renderer translation time is measured and reported separately from GPU render time.

---

### User Story 4 - Screenshot Capture Performance (Priority: P3)

A developer wants to know how quickly screenshots can be captured from the running viewer. This measures the overhead of the screenshot pipeline, which is important for applications that need to export frames or produce recordings.

**Why this priority**: Screenshot capture is a secondary feature, but its performance matters for batch export and recording use cases.

**Independent Test**: Can be tested by triggering repeated screenshot captures during a rendering session and measuring capture latency. Delivers screenshots-per-second throughput.

**Acceptance Scenarios**:

1. **Given** the viewer is rendering a standard scene, **When** the screenshot benchmark triggers repeated captures, **Then** it reports average capture time and captures-per-second throughput.
2. **Given** screenshots are captured at different scene complexities, **When** results are reported, **Then** capture time scaling with complexity is visible.

---

### Edge Cases

- What happens when the Vulkan backend fails to initialize (driver issue)? The suite should gracefully skip Vulkan tests and report the failure.
- How does performance behave when the system is under memory pressure? The suite should report memory usage alongside frame metrics.
- What happens with zero-element (empty) scenes? Should still produce valid timing measurements.
- How does performance differ between the first few frames (warm-up) and steady-state? The suite should discard warm-up frames from final metrics.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST measure and report frames-per-second (FPS) for both Vulkan and OpenGL rendering backends.
- **FR-002**: System MUST measure and report frame time statistics: average, median, minimum, maximum, and 99th percentile.
- **FR-003**: System MUST test rendering performance across at least five scene complexity tiers (e.g., 10, 100, 1,000, 10,000, 100,000 elements).
- **FR-004**: System MUST test each scene element type (rectangle, ellipse, line, text, path) independently and in mixed combinations.
- **FR-005**: System MUST measure scene DSL construction time separately from GPU rendering time.
- **FR-006**: System MUST measure scene renderer translation overhead separately.
- **FR-007**: System MUST measure screenshot capture throughput.
- **FR-008**: System MUST discard a configurable warm-up period before recording final metrics to ensure steady-state measurement.
- **FR-009**: System MUST report peak memory usage during each benchmark.
- **FR-010**: System MUST gracefully handle backend initialization failures by skipping the affected backend and continuing with remaining tests.
- **FR-011**: System MUST produce a structured summary report upon completion that includes all measured metrics organized by benchmark category and backend.
- **FR-012**: System MUST run each benchmark for a sufficient duration to produce statistically meaningful results (minimum 100 frames or 5 seconds, whichever is longer).

### Key Entities

- **Benchmark**: A named, repeatable performance measurement targeting a specific aspect of the rendering pipeline (throughput, stress, composition, screenshot).
- **Metric**: A quantitative measurement captured during a benchmark run (FPS, frame time, memory usage, scene construction time).
- **Complexity Tier**: A predefined level of scene complexity defined by element count and type distribution.
- **Benchmark Report**: The collected results from a full suite run, organized by benchmark and backend.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The full test suite completes within 10 minutes on a standard developer machine.
- **SC-002**: Results for each benchmark are reproducible within a 10% variance across consecutive runs on the same machine.
- **SC-003**: The suite identifies the maximum element count at which each backend sustains 60 FPS and 30 FPS interactive rates.
- **SC-004**: The developer can compare Vulkan vs. OpenGL performance from a single suite run without manual calculation.
- **SC-005**: Scene composition benchmarks demonstrate how many scene updates per second the system supports at each complexity tier.
- **SC-006**: All benchmarks that complete successfully produce valid, non-zero numeric results.

## Assumptions

- Vulkan drivers and Silk.NET prerequisites are installed and functional on the target machine (as stated by user).
- The test suite runs on a machine with a display server or virtual framebuffer available for window creation.
- Performance measurements reflect single-machine, single-user conditions (no competing GPU workloads).
- The existing SkiaViewer rendering pipeline (Viewer.run, VulkanBackend, SceneRenderer) is the system under test — no external rendering engines are in scope.
- The test suite is intended for developer use (local benchmarking), not for CI/CD integration at this stage.
- Results are reported to the console or a local file; no remote reporting or dashboarding is in scope.

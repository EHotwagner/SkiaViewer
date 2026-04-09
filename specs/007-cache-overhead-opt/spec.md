# Feature Specification: Cache Overhead Optimization

**Feature Branch**: `007-cache-overhead-opt`  
**Created**: 2026-04-09  
**Status**: Draft  
**Input**: User description: "Optimize scene diff caching to reduce fully-animated overhead from 18.5% to under 5% via reference-equality fast paths, reduced structural comparison cost, and recording overhead reduction"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Low Overhead for Fully-Animated Scenes (Priority: P1)

As a developer building fully-animated visualizations where every Group element changes every frame, I need the caching system to impose minimal overhead compared to uncached rendering, so that enabling caching does not penalize scenes that cannot benefit from it.

**Why this priority**: The current caching implementation adds 18.5% overhead for fully-animated scenes (0.61ms -> 0.73ms per frame for 20 groups x 10 children). This violates the <=5% overhead target from the original caching spec. Fixing this is critical because developers should not need to manually toggle caching based on scene behavior.

**Independent Test**: Render a fully-animated scene (all groups change every frame) with caching enabled vs disabled. Measure frame time and verify overhead is under 5%.

**Acceptance Scenarios**:

1. **Given** a scene where all Group elements change every frame, **When** rendered with caching enabled, **Then** the per-frame time is no more than 5% slower than rendering with caching disabled.
2. **Given** a scene where all Group elements change every frame, **When** the cache detects all misses for multiple consecutive frames, **Then** the system reduces comparison work on subsequent frames rather than continuing full structural equality checks.
3. **Given** a mix of static and animated groups, **When** some groups are consistently cache misses, **Then** the system still efficiently caches the static groups without penalizing them.

---

### User Story 2 - Reference-Equality Fast Path (Priority: P1)

As a developer reusing the same scene object or Group child lists across frames, I need the cache to detect object identity before performing deep structural comparison, so that the common case of passing unchanged objects is nearly free.

**Why this priority**: Deep structural equality on large Element lists is the primary source of overhead. Reference equality (`Object.ReferenceEquals`) is an O(1) check that can short-circuit the O(n) structural comparison for the majority of real-world cases where developers reuse object references.

**Independent Test**: Create a scene where Group children are the same object references across frames. Verify the cache lookup completes in O(1) without structural comparison.

**Acceptance Scenarios**:

1. **Given** a Group whose children list is the same object reference as the previous frame, **When** the cache performs lookup, **Then** it uses reference equality and skips structural comparison entirely.
2. **Given** a Group whose children list is a newly allocated list with identical contents, **When** reference equality fails, **Then** it falls back to structural equality and still finds the cache entry.
3. **Given** a scene where the scene object itself is the same reference as the previous frame, **When** the cache renders, **Then** it skips per-element comparison entirely and replays all cached entries directly.

---

### User Story 3 - Reduced Recording Overhead on Cache Misses (Priority: P2)

As a developer with scenes that have frequent cache misses, I need the recording process for new cache entries to be as lightweight as possible, so that the cost of a miss is close to the cost of direct rendering.

**Why this priority**: Each cache miss currently records children into a new pre-recorded draw command object. If the recording overhead is significantly more than direct rendering, misses become expensive. Minimizing recording cost reduces the gap between cached and uncached paths.

**Independent Test**: Measure the time to render a Group's children directly vs recording them. Verify recording overhead is under 10% of direct rendering time.

**Acceptance Scenarios**:

1. **Given** a cache miss for a Group element, **When** the children are recorded, **Then** the recording time is no more than 10% slower than rendering the same children directly to the canvas.
2. **Given** a cache miss, **When** the recording bounds are calculated, **Then** reasonable bounds are used without expensive per-element bounding box computation.

---

### Edge Cases

- What happens when a scene alternates between two completely different layouts every frame? The cache should quickly converge to retaining both layouts rather than thrashing.
- How does the system handle scenes with thousands of top-level Groups? The per-element comparison cost should scale linearly, not quadratically.
- What happens when a Group's children change by a single element deep in the list? Structural equality should detect the change but the comparison cost should be bounded.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST use object reference equality as the first comparison step before falling back to structural equality for cache lookups.
- **FR-002**: System MUST provide a whole-scene reference-equality fast path that skips all per-element comparison when the same scene object is rendered consecutively.
- **FR-003**: System MUST NOT impose more than 5% overhead on fully-animated scenes (where all Group elements change every frame) compared to uncached rendering.
- **FR-004**: System MUST maintain the existing caching behavior for mostly-static scenes (no regression in cache hit rates or speedup).
- **FR-005**: System MUST use bounded recording regions for cache miss recording rather than unbounded or overly large regions.
- **FR-006**: System MUST maintain pixel-identical output between cached and uncached rendering paths (no regression from the existing guarantee).
- **FR-007**: System MUST preserve the existing runtime toggle, cache statistics, and generation-based eviction behavior.

### Key Entities

- **ElementListIdentity**: A comparison strategy that checks reference equality first, then falls back to structural equality for cache key lookups.
- **SceneFingerprint**: A lightweight identifier for the previous scene used to enable whole-scene fast path skipping.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Fully-animated scenes (all groups change every frame) render with no more than 5% overhead when caching is enabled vs disabled, measured over 300+ frames.
- **SC-002**: Mostly-static scenes maintain at least the same speedup as the current implementation (>=4x for 200-element scenes with 20 static groups).
- **SC-003**: Identical-scene rendering (same object reference) maintains or improves the current speedup (>=2x).
- **SC-004**: Visual output remains pixel-identical between cached and uncached rendering for all test scenes.

## Assumptions

- The primary overhead sources in the current implementation are: (1) structural equality comparison on Element lists used as dictionary keys, (2) recording overhead from large bounding regions, and (3) eviction sweep cost on every frame.
- Developers commonly reuse object references for unchanged scene elements rather than reconstructing equivalent objects each frame.
- The benchmark test (20 groups x 10 children = 200 elements, 800x600 raster surface) is representative of typical workloads.
- The 18.5% overhead measured in the benchmark is consistent and reproducible across runs.

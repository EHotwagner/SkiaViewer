# Feature Specification: Scene Diff Caching

**Feature Branch**: `006-scene-diff-caching`  
**Created**: 2026-04-09  
**Status**: Draft  
**Input**: User description: "Implement scene diffing on the scene stream to optimize rendering performance by caching unchanged subtrees and avoiding redundant SkiaSharp object recreation"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Cached Rendering of Static Subtrees (Priority: P1)

As a developer building an interactive visualization with SkiaViewer, I have scenes where only a small portion of the element tree changes between frames (e.g., a moving cursor over a complex static background). The rendering system should detect that unchanged subtrees don't need to be re-rendered and reuse cached rendering results instead, so my application maintains smooth frame rates even with complex scenes.

**Why this priority**: This is the core value proposition. Most real-world scenes have significant portions that remain stable frame-to-frame. Caching unchanged subtrees eliminates the majority of redundant work (object allocation, conversion, and draw calls) and delivers the largest performance improvement.

**Independent Test**: Can be tested by creating a scene stream where a complex background (many elements) remains constant while a single foreground element changes each frame. Measure frame time reduction compared to uncached rendering.

**Acceptance Scenarios**:

1. **Given** a scene stream where a group of elements is structurally identical across consecutive frames, **When** the renderer processes the new frame, **Then** the unchanged group is rendered from a cached result rather than being re-converted and re-drawn element by element.
2. **Given** a scene stream where every element changes between frames, **When** the renderer processes the new frame, **Then** all elements are rendered fresh with no caching overhead that degrades performance below the current baseline.
3. **Given** a cached subtree whose parent transform changes, **When** the renderer processes the frame, **Then** the cached content is replayed with the new transform applied, without re-rendering the subtree contents.

---

### User Story 2 - Paint Object Memoization (Priority: P2)

As a developer using SkiaViewer, I create scenes with many elements sharing the same paint styles (fills, strokes, shaders, filters). The system should avoid recreating identical paint objects every frame, reducing allocation pressure and improving rendering throughput.

**Why this priority**: Paint conversion is one of the most frequent per-element costs. Since many elements share identical paint definitions (especially within groups), memoizing paint objects across frames provides broad savings with relatively low complexity.

**Independent Test**: Can be tested by creating a scene with many elements sharing identical paint records and measuring the reduction in object allocations and frame time.

**Acceptance Scenarios**:

1. **Given** a scene where multiple elements use structurally identical paint definitions, **When** the scene is rendered, **Then** the underlying paint resources are created once and reused across those elements within the same frame.
2. **Given** a paint definition that changes between frames, **When** the new frame is rendered, **Then** a new paint resource is created and the stale cached entry is eventually released.
3. **Given** a paint definition with complex shaders or filters, **When** it is structurally identical to a previously cached paint, **Then** the cached version is reused without re-creating the shader or filter pipeline.

---

### User Story 3 - Transparent Caching Behavior (Priority: P2)

As a developer using SkiaViewer, I want the caching system to be completely transparent -- my scenes should render identically whether caching is active or not. I should not need to change how I construct scenes or annotate elements for caching to work.

**Why this priority**: If caching introduces visual artifacts or requires changes to the scene construction API, adoption is blocked and trust in the rendering pipeline is undermined.

**Independent Test**: Can be tested by rendering the same scene sequence with caching enabled and disabled, then comparing the output pixel-by-pixel.

**Acceptance Scenarios**:

1. **Given** any valid scene, **When** rendered with caching enabled, **Then** the visual output is pixel-identical to rendering without caching.
2. **Given** a scene constructed using the existing DSL functions, **When** the caching system is active, **Then** no changes to the scene construction code are required.
3. **Given** a scene with overlapping elements and blend modes, **When** a cached subtree is replayed, **Then** blending is applied correctly as if the subtree were rendered fresh.

---

### User Story 4 - Cache Memory Management (Priority: P3)

As a developer running long-lived SkiaViewer applications, I need the caching system to manage memory responsibly. Cached entries for scenes or subtrees that are no longer in use should be released, preventing unbounded memory growth.

**Why this priority**: Without memory management, caching becomes a memory leak in long-running applications. While not needed for initial correctness, it is essential for production use.

**Independent Test**: Can be tested by streaming a long sequence of unique scenes and monitoring memory usage to confirm it stays bounded.

**Acceptance Scenarios**:

1. **Given** a long-running scene stream with changing content, **When** cached entries are no longer referenced by recent scenes, **Then** those entries are eventually released from memory.
2. **Given** a scene stream that alternates between a small set of distinct scenes, **When** the cache is operating, **Then** memory usage remains stable over time rather than growing.

---

### Edge Cases

- What happens when the scene stream emits the exact same scene object reference consecutively? The system should recognize identity equality and skip all rendering work.
- How does the system handle a scene where a group's children are reordered but otherwise identical? Reordered children should be treated as a changed subtree.
- What happens when the window is resized and the rendering surface is recreated? All cached rendering results should be invalidated since the surface dimensions changed.
- How does the system behave when scene elements reference external mutable resources (e.g., SKImage, SKPicture)? The cache should use structural equality of the immutable DSL types; external resource mutation is the caller's responsibility to handle by creating a new element.

## Clarifications

### Session 2026-04-09

- Q: At what tree level does diffing and caching operate? → A: Only Group elements are cached as subtrees; leaf elements are always re-rendered.
- Q: How should cache memory be bounded? → A: Generation-based — only retain entries referenced by the last N scenes (e.g., last 2 scenes).
- Q: Should caching be togglable at runtime? → A: Yes, developers can enable/disable caching programmatically at runtime.
- Q: Should the caching system expose diagnostics? → A: Yes, lightweight counters — cache hit/miss/eviction counts per frame.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST detect when a Group element is structurally identical to the corresponding Group in the previously rendered scene.
- **FR-002**: System MUST cache rendering results (as pre-recorded draw commands) for unchanged Group elements and replay them instead of re-rendering their children. Individual leaf elements are always re-rendered.
- **FR-003**: System MUST correctly apply parent transforms, clips, and paint overrides when replaying cached subtrees.
- **FR-004**: System MUST memoize converted paint resources across elements within a frame and across frames when the paint definition is structurally identical.
- **FR-005**: System MUST produce pixel-identical output regardless of whether caching is active or inactive.
- **FR-006**: System MUST invalidate all cached rendering results when the rendering surface dimensions change.
- **FR-007**: System MUST use generation-based eviction, retaining only cache entries referenced by the last N scenes (e.g., last 2). Entries not referenced in recent generations MUST be released.
- **FR-008**: System MUST NOT require changes to the existing scene DSL API or scene construction patterns.
- **FR-009**: System MUST NOT degrade rendering performance for scenes where all elements change every frame (fully animated scenes) compared to the current uncached baseline.
- **FR-010**: System MUST provide a runtime toggle allowing developers to programmatically enable or disable caching. When disabled, the system MUST render using the uncached code path.
- **FR-011**: System MUST expose lightweight per-frame cache diagnostics: hit count, miss count, and eviction count. These counters MUST be queryable by the developer without affecting rendering performance.

### Key Entities

- **CachedSubtree**: Represents a pre-recorded rendering result for a Group element, associated with the structural identity of the group and its children.
- **PaintCache**: A lookup structure mapping structurally identical paint definitions to their converted rendering resources.
- **SceneSnapshot**: The most recently rendered scene, retained for comparison with incoming scenes.
- **CacheStatistics**: Per-frame counters (hits, misses, evictions) exposing cache behavior to the developer for tuning.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Scenes with 80%+ unchanged elements between frames render in at least 50% less time compared to the uncached baseline.
- **SC-002**: Scenes where 100% of elements change every frame render within 5% of the uncached baseline time (minimal overhead from cache comparison).
- **SC-003**: Memory usage remains bounded via generation-based eviction (last N scenes) and does not grow unboundedly over a sustained run of 10,000+ unique scene frames.
- **SC-004**: Visual output is pixel-identical between cached and uncached rendering for all test scenes in the existing test suite.

## Assumptions

- Structural equality of F# record and discriminated union types is sufficient for detecting unchanged elements, since the scene DSL is built on immutable types.
- The existing `Scene.recordPicture` and `Element.Picture` mechanisms can serve as the underlying caching format for pre-recorded subtree rendering.
- External mutable resources (SKImage, SKPicture passed into the DSL) are treated as opaque by the cache; mutation of these resources without creating new DSL elements is the caller's responsibility.
- The performance test suite (004-perf-test-suite) provides adequate benchmarking infrastructure to measure the impact of caching.
- Cache invalidation on surface resize is sufficient; no other external events require full cache invalidation.

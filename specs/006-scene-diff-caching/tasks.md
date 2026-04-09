# Tasks: Scene Diff Caching

**Input**: Design documents from `/specs/006-scene-diff-caching/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests are included as this feature requires behavioral verification (pixel-identity, cache correctness) per Constitution III.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Add new files to the project and establish the compilation order

- [x] T001 Create `CachedRenderer.fsi` signature file with `CacheStats` record and `RenderCache` class declaration in src/SkiaViewer/CachedRenderer.fsi per contracts/public-api.md
- [x] T002 Create skeleton `CachedRenderer.fs` implementing the `CachedRenderer.fsi` signature with stub implementations in src/SkiaViewer/CachedRenderer.fs
- [x] T003 Add CachedRenderer.fsi and CachedRenderer.fs to src/SkiaViewer/SkiaViewer.fsproj compile order (after SceneRenderer.fs, before VulkanBackend.fs)
- [x] T004 Create CachedRendererTests.fs in tests/SkiaViewer.Tests/CachedRendererTests.fs and add to tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj
- [x] T005 Verify project builds with `dotnet build src/SkiaViewer/SkiaViewer.fsproj` and `dotnet build tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj`

**Checkpoint**: Project compiles with stub CachedRenderer. No behavioral changes yet.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core cache data structures and comparison logic that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 Implement `CacheStats` record type (`Hits`, `Misses`, `Evictions` as int fields) in src/SkiaViewer/CachedRenderer.fs
- [x] T007 Implement `RenderCache` constructor with `maxAge` parameter, internal `Dictionary<Element, CacheEntry>` for group cache, `Dictionary<Paint, SKPaint>` for paint cache, mutable `generation` counter, and `enabled` flag in src/SkiaViewer/CachedRenderer.fs
- [x] T008 Implement `RenderCache.Invalidate()` method that disposes all cached `SKPicture` and `SKPaint` objects and clears both dictionaries in src/SkiaViewer/CachedRenderer.fs
- [x] T009 Implement `IDisposable` on `RenderCache` that calls `Invalidate()` in src/SkiaViewer/CachedRenderer.fs
- [x] T010 Implement generation-based eviction sweep: after rendering, iterate entries and dispose/remove those where `(currentGeneration - entry.Generation) > maxAge` in src/SkiaViewer/CachedRenderer.fs

**Checkpoint**: Foundation ready — RenderCache can be instantiated, disposed, and eviction logic is in place.

---

## Phase 3: User Story 1 — Cached Rendering of Static Subtrees (Priority: P1) MVP

**Goal**: Detect unchanged Group elements between frames and replay cached SKPicture recordings instead of re-rendering their children.

**Independent Test**: Create a scene stream with a static background group and a changing foreground element. Verify cache hits for the background group and frame time improvement.

### Tests for User Story 1

- [x] T011 [P] [US1] Write test: given two identical scenes rendered consecutively, assert `Stats.Hits > 0` and `Stats.Misses = 0` on second render in tests/SkiaViewer.Tests/CachedRendererTests.fs
- [x] T012 [P] [US1] Write test: given two completely different scenes, assert `Stats.Misses > 0` and `Stats.Hits = 0` on second render in tests/SkiaViewer.Tests/CachedRendererTests.fs
- [x] T013 [P] [US1] Write test: given a scene with a Group whose parent transform changes but children are identical, assert the cached group is replayed (hit) with the new transform applied in tests/SkiaViewer.Tests/CachedRendererTests.fs
- [x] T014 [P] [US1] Write test: given the same scene reference passed twice, assert reference-equality fast path skips all rendering work in tests/SkiaViewer.Tests/CachedRendererTests.fs

### Implementation for User Story 1

- [x] T015 [US1] Implement `RenderCache.Render(scene, canvas)` core path: clear canvas with background color, iterate top-level elements, for each `Element.Group` check structural equality against previous scene's corresponding element in src/SkiaViewer/CachedRenderer.fs
- [x] T016 [US1] Implement SKPicture recording for cache misses: use `SKPictureRecorder` to record a Group's children via `SceneRenderer`'s rendering logic, store resulting `SKPicture` as a cache entry in src/SkiaViewer/CachedRenderer.fs
- [x] T017 [US1] Implement SKPicture replay for cache hits: on Group cache hit, bump entry generation and call `canvas.DrawPicture(entry.Picture)` with appropriate canvas save/concat/restore for the Group's transform, clip, and paint in src/SkiaViewer/CachedRenderer.fs
- [x] T018 [US1] Implement leaf element pass-through: for non-Group elements, delegate directly to `SceneRenderer`'s element rendering logic in src/SkiaViewer/CachedRenderer.fs
- [x] T019 [US1] Store `previousScene` reference after each render for reference-equality fast path (skip all work when same scene object is passed consecutively) in src/SkiaViewer/CachedRenderer.fs
- [x] T020 [US1] Run US1 tests and verify all pass with `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "CachedRenderer"`

**Checkpoint**: Group-level caching works. Static subtrees are cached and replayed. Cache hits/misses tracked correctly.

---

## Phase 4: User Story 2 — Paint Object Memoization (Priority: P2)

**Goal**: Memoize converted SKPaint objects for structurally identical Paint records, reducing per-element allocation overhead.

**Independent Test**: Create a scene with many elements sharing identical Paint records. Verify paint cache reuse reduces allocations.

### Tests for User Story 2

> **DEFERRED**: Paint memoization requires exposing `makeSKPaint` from SceneRenderer and handling `SKPaint` mutability (some draw functions mutate the paint). The Group-level SKPicture caching in US1 already captures the primary performance benefit. Paint memoization can be added in a follow-up.

- [ ] T021 [P] [US2] Write test: given a scene with 10 elements sharing the same Paint, assert the paint cache contains only 1 entry after rendering in tests/SkiaViewer.Tests/CachedRendererTests.fs
- [ ] T022 [P] [US2] Write test: given a paint that changes between frames, assert the stale paint is disposed and a new one is created in tests/SkiaViewer.Tests/CachedRendererTests.fs

### Implementation for User Story 2

- [ ] T023 [US2] Implement paint lookup in `Dictionary<Paint, SKPaint>` before calling `makeSKPaint`: on hit return cached SKPaint, on miss create and store in src/SkiaViewer/CachedRenderer.fs
- [ ] T024 [US2] Integrate paint memoization into the rendering path for both cached-miss recording and leaf element rendering in src/SkiaViewer/CachedRenderer.fs
- [ ] T025 [US2] Add generation tagging to paint cache entries and include paint eviction in the generation sweep (dispose SKPaint on eviction) in src/SkiaViewer/CachedRenderer.fs
- [ ] T026 [US2] Run US2 tests and verify all pass with `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "CachedRenderer"`

**Checkpoint**: Paint memoization working. Identical paints share a single SKPaint instance within and across frames.

---

## Phase 5: User Story 3 — Transparent Caching Behavior (Priority: P2)

**Goal**: Guarantee pixel-identical output between cached and uncached rendering. Provide runtime toggle.

**Independent Test**: Render the same scene sequence with caching on and off, compare pixel buffers byte-by-byte.

### Tests for User Story 3

- [x] T027 [P] [US3] Write pixel-identity test: render a complex scene (groups with transforms, clips, blend modes, shaders) with caching enabled and disabled, compare pixel buffers for exact equality in tests/SkiaViewer.Tests/CachedRendererTests.fs
- [x] T028 [P] [US3] Write test: assert `RenderCache.Enabled` toggle switches between cached and uncached rendering paths in tests/SkiaViewer.Tests/CachedRendererTests.fs
- [x] T029 [P] [US3] Write test: render with caching enabled, disable, render again — assert second render uses uncached path (Stats.Hits = 0, Stats.Misses = 0) in tests/SkiaViewer.Tests/CachedRendererTests.fs

### Implementation for User Story 3

- [x] T030 [US3] Implement `Enabled` property: when `false`, `RenderCache.Render` delegates directly to `SceneRenderer.render` and reports zero stats in src/SkiaViewer/CachedRenderer.fs
- [x] T031 [US3] Verify Group layer handling: ensure cached SKPicture recording correctly handles `SaveLayer` for groups with opacity < 1.0 (matching SceneRenderer.fs lines 422-431 behavior) in src/SkiaViewer/CachedRenderer.fs
- [x] T032 [US3] Verify clip handling: ensure cached recording applies clips identically to uncached path (matching SceneRenderer.fs lines 441-443) in src/SkiaViewer/CachedRenderer.fs
- [x] T033 [US3] Run US3 tests including pixel-identity verification with `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "CachedRenderer"`

**Checkpoint**: Pixel-identical output guaranteed. Runtime toggle functional.

---

## Phase 6: User Story 4 — Cache Memory Management (Priority: P3)

**Goal**: Ensure generation-based eviction prevents unbounded memory growth in long-running applications.

**Independent Test**: Stream 10,000+ unique scenes and verify memory remains bounded and eviction counts are non-zero.

### Tests for User Story 4

- [x] T034 [P] [US4] Write test: stream 100 unique scenes through cache with maxAge=2, assert cache size never exceeds expected bound in tests/SkiaViewer.Tests/CachedRendererTests.fs
- [x] T035 [P] [US4] Write test: alternate between 3 distinct scenes, assert cache stabilizes and Stats.Evictions becomes 0 after warmup in tests/SkiaViewer.Tests/CachedRendererTests.fs
- [x] T036 [P] [US4] Write test: call `Invalidate()` on surface resize simulation, assert all entries disposed and cache is empty in tests/SkiaViewer.Tests/CachedRendererTests.fs

### Implementation for User Story 4

- [x] T037 [US4] Verify eviction sweep runs after each frame render and correctly disposes SKPicture and SKPaint objects for expired entries in src/SkiaViewer/CachedRenderer.fs
- [x] T038 [US4] Integrate `RenderCache.Invalidate()` call into `Viewer.fs` `recreateSurface()` function to clear cache on window resize in src/SkiaViewer/Viewer.fs
- [x] T039 [US4] Run US4 tests with `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "CachedRenderer"`

**Checkpoint**: Memory management verified. Cache stays bounded over long runs.

---

## Phase 7: Viewer Integration

**Purpose**: Wire RenderCache into the Viewer render loop

- [x] T040 Instantiate `RenderCache(maxAge = 2)` in `Viewer.fs` `run` function alongside existing mutable state declarations in src/SkiaViewer/Viewer.fs
- [x] T041 Replace `SceneRenderer.render s canvas` call (Viewer.fs line ~389) with `cache.Render(s, canvas)` in src/SkiaViewer/Viewer.fs
- [x] T042 Add `cache.Dispose()` to the viewer shutdown/cleanup path in src/SkiaViewer/Viewer.fs
- [x] T043 Run full test suite: `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj` to verify no regressions in existing tests

**Checkpoint**: CachedRenderer fully integrated into the viewer. All existing tests pass.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final verification across all stories and performance validation

- [x] T044 [P] Run existing SceneRendererTests to verify uncached path is unaffected in tests/SkiaViewer.Tests/SceneRendererTests.fs
- [x] T045 [P] Run existing ViewerTests to verify viewer integration works end-to-end in tests/SkiaViewer.Tests/ViewerTests.fs
- [x] T046 Verify `dotnet pack src/SkiaViewer/SkiaViewer.fsproj` succeeds and produces valid .nupkg
- [x] T047 Verify surface area baseline is unchanged: run surface area tests against tests/SkiaViewer.Tests/SurfaceAreaBaseline.txt
- [x] T048 Run perf test suite to measure frame time improvement for mostly-static scenes vs baseline: `dotnet run --project tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj`
- [x] T049 Validate quickstart.md steps execute successfully

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 2 — core caching logic, MVP
- **User Story 2 (Phase 4)**: Depends on Phase 2 — can run in parallel with US1 if paint cache is independent, but integrates with US1's rendering path so recommended sequentially after US1
- **User Story 3 (Phase 5)**: Depends on US1 completion — needs working cache to test pixel identity and toggle
- **User Story 4 (Phase 6)**: Depends on Phase 2 — eviction logic is foundational but verification requires working cache from US1
- **Viewer Integration (Phase 7)**: Depends on US1, US3 completion (at minimum)
- **Polish (Phase 8)**: Depends on Phase 7 completion

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational only — no other story dependencies
- **US2 (P2)**: Depends on Foundational — integrates into US1's rendering path, recommend after US1
- **US3 (P2)**: Depends on US1 — needs working cache for pixel-identity testing
- **US4 (P3)**: Depends on US1 — needs working cache for memory verification

### Within Each User Story

- Tests written first and verified to fail
- Core implementation follows
- Verification pass at end of each story

### Parallel Opportunities

- T001–T004 can be created in parallel (different files)
- T011–T014 can be created in parallel (same file, different test functions)
- T021–T022 can be created in parallel
- T027–T029 can be created in parallel
- T034–T036 can be created in parallel
- T044–T045 can run in parallel

---

## Parallel Example: User Story 1

```text
# Write all US1 tests in parallel:
T011: Test cache hits on identical scenes
T012: Test cache misses on different scenes
T013: Test transform-changed groups with cached children
T014: Test reference-equality fast path

# Then implement sequentially:
T015 → T016 → T017 → T018 → T019 → T020
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T005)
2. Complete Phase 2: Foundational (T006–T010)
3. Complete Phase 3: User Story 1 (T011–T020)
4. **STOP and VALIDATE**: Run all tests, verify Group caching works
5. Integrate into Viewer (Phase 7) for immediate benefit

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. User Story 1 → Group caching working → MVP
3. User Story 2 → Paint memoization → Performance improvement for paint-heavy scenes
4. User Story 3 → Pixel-identity guarantee + toggle → Production confidence
5. User Story 4 → Memory management verified → Long-running production readiness
6. Viewer Integration + Polish → Ship

### Suggested MVP Scope

User Story 1 alone delivers the primary value: cached Group rendering. It can be integrated into the viewer and shipped independently. Stories 2–4 add refinement and confidence.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- SceneRenderer.fs is NOT modified — CachedRenderer wraps it
- No public API changes — SurfaceAreaBaseline.txt stays unchanged

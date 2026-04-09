# Tasks: Cache Overhead Optimization

**Input**: Design documents from `/specs/007-cache-overhead-opt/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Existing tests in CachedRendererTests.fs validate all changes. No new test files needed — the benchmark test measures the performance target.

**Organization**: Tasks grouped by user story. All tasks modify `src/SkiaViewer/CachedRenderer.fs` only.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

---

## Phase 1: Setup

**Purpose**: Prepare the internal data structures

- [x] T001 Define `CacheSlot` private type with `ChildrenRef: Element list`, `Picture: SKPicture`, `mutable Generation: int` in src/SkiaViewer/CachedRenderer.fs
- [x] T002 Replace `Dictionary<Element list, CacheEntry>` field with `mutable slots: CacheSlot option[]` (initial capacity 32) and `mutable previousScene: Scene voption` in src/SkiaViewer/CachedRenderer.fs
- [x] T003 Add `mutable recordBounds: SKRect` field initialized to `SKRect(0f, 0f, 4096f, 4096f)` in src/SkiaViewer/CachedRenderer.fs
- [x] T004 Verify project builds with `dotnet build src/SkiaViewer/SkiaViewer.fsproj`

**Checkpoint**: CachedRenderer compiles with new data structures. Not yet functional.

---

## Phase 2: User Story 1 + 2 — Reference-Equality Fast Paths and Low Overhead (Priority: P1)

**Goal**: Implement scene-level and element-level reference-equality fast paths to reduce fully-animated overhead to <=5%.

**Independent Test**: Run benchmark test, verify fully-animated overhead <=5% and mostly-static speedup >=4x.

### Implementation

- [x] T005 [US1] Implement scene-level fast path in `Render`: if `Object.ReferenceEquals(previousScene, scene)`, replay all cached slots by iterating the slots array, bumping generations, and replaying pictures — skip per-element comparison entirely in src/SkiaViewer/CachedRenderer.fs
- [x] T006 [US1] Implement position-based element rendering loop: for each element at index `i` in `scene.Elements`, if Group then check slot `i` with reference equality on children list first (`Object.ReferenceEquals(children, slot.ChildrenRef)`), then structural equality fallback in src/SkiaViewer/CachedRenderer.fs
- [x] T007 [US2] On cache miss, record children using `recordBounds` instead of `SKRect(-1e6, -1e6, 1e6, 1e6)` in src/SkiaViewer/CachedRenderer.fs
- [x] T008 [US1] On cache hit (reference or structural), bump slot generation and replay picture with transform/clip/opacity handling in src/SkiaViewer/CachedRenderer.fs
- [x] T009 [US1] On cache miss, create new `CacheSlot` at position `i`, disposing any existing slot's picture first. Store children reference for future reference-equality checks in src/SkiaViewer/CachedRenderer.fs
- [x] T010 [US1] Ensure slots array is resized if `scene.Elements.Length` exceeds current capacity in src/SkiaViewer/CachedRenderer.fs
- [x] T011 [US1] Store `previousScene <- ValueSome scene` after rendering for next frame's scene-level fast path in src/SkiaViewer/CachedRenderer.fs
- [x] T012 [US1] Update `Invalidate()` to clear slots array and reset previousScene in src/SkiaViewer/CachedRenderer.fs
- [x] T013 [US1] Update eviction sweep to iterate slots array: for each `Some slot` where `(generation - slot.Generation) > maxAge`, dispose picture and set slot to `None` in src/SkiaViewer/CachedRenderer.fs
- [x] T014 Run all CachedRenderer tests: `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "CachedRenderer"` to verify correctness in tests/SkiaViewer.Tests/CachedRendererTests.fs

**Checkpoint**: All existing tests pass. Position-based slots with reference-equality fast paths operational.

---

## Phase 3: User Story 3 — Reduced Recording Overhead (Priority: P2)

**Goal**: Minimize recording cost for cache misses by using bounded recording regions.

**Independent Test**: Verify recording overhead is minimal by checking benchmark fully-animated results.

### Implementation

- [x] T015 [US3] Add `SetRecordBounds` method or update `recordBounds` in `Invalidate` based on surface resize pattern (match the surface dimensions when available) in src/SkiaViewer/CachedRenderer.fs
- [x] T016 Run benchmark test: `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "Benchmark" --logger "console;verbosity=detailed"` and verify fully-animated overhead <=5% in tests/SkiaViewer.Tests/CachedRendererTests.fs

**Checkpoint**: Performance targets met. All tests pass.

---

## Phase 4: Polish & Validation

**Purpose**: Full regression check and performance validation

- [x] T017 Run full test suite: `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj` to verify no regressions (112+ tests)
- [x] T018 Verify `dotnet pack src/SkiaViewer/SkiaViewer.fsproj` succeeds
- [x] T019 Verify surface area baseline unchanged: `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "surface"`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **US1+US2 (Phase 2)**: Depends on Phase 1 — core optimization
- **US3 (Phase 3)**: Depends on Phase 2 — recording bounds refinement
- **Polish (Phase 4)**: Depends on Phase 3

### Parallel Opportunities

- T001-T003 modify different parts of the same type definition — sequential within the file
- T005-T013 are sequential (all modify the same `Render` method)
- T017-T019 can run in parallel (different test filters)

---

## Implementation Strategy

### MVP First

1. Phase 1: Data structure setup
2. Phase 2: Reference-equality fast paths + position-based slots
3. Run benchmark — verify <=5% overhead
4. Phase 3-4: Polish

### Single File Scope

All implementation is in `src/SkiaViewer/CachedRenderer.fs`. No new files, no `.fsi` changes, no other file modifications.

---

## Notes

- All tasks modify the same file — must execute sequentially
- Existing 11 tests validate correctness; benchmark test validates performance
- No new test files needed
- CachedRenderer.fsi signature is unchanged

# Implementation Plan: Scene Diff Caching

**Branch**: `006-scene-diff-caching` | **Date**: 2026-04-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-scene-diff-caching/spec.md`

## Summary

Add an internal caching layer that detects structurally unchanged `Group` elements between consecutive scene frames and replays pre-recorded `SKPicture` draw commands instead of re-rendering. Includes paint object memoization, generation-based eviction, a runtime toggle, and per-frame cache statistics. The existing public API surface is unchanged.

## Technical Context

**Language/Version**: F# on .NET 10.0  
**Primary Dependencies**: SkiaSharp 2.88.6 (SKPictureRecorder, SKPicture), Silk.NET 2.22.0  
**Storage**: In-memory dictionaries (render-thread only)  
**Testing**: xunit 2.9.3, SkiaViewer.PerfTests  
**Target Platform**: Linux (primary), cross-platform via .NET  
**Project Type**: Library  
**Performance Goals**: >=50% frame time reduction for 80%+ static scenes; <=5% overhead for fully-animated scenes  
**Constraints**: Pixel-identical output; no public API changes; single-threaded cache access (render thread only)  
**Scale/Scope**: Hundreds to low thousands of elements per scene; dozens of cached Group entries typical

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| I. Spec-First Delivery | **PASS** | Spec, clarifications, and plan complete. This feature adds public API surface (`CacheStats` type) — but it is internal, not public. No `.fsi` public surface change. |
| II. Compiler-Enforced Structural Contracts | **PASS** | New `CachedRenderer.fsi` will be created as internal signature. No changes to existing `.fsi` files. Surface area baseline unchanged. |
| III. Test Evidence Is Mandatory | **PASS** | Plan includes `CachedRendererTests.fs` with pixel-identity, hit/miss, eviction, and toggle tests. Perf tests validate performance criteria. |
| IV. Observability and Safe Failure Handling | **PASS** | FR-011 requires per-frame cache statistics (hits/misses/evictions). Cache failures degrade gracefully to uncached rendering. |
| V. Scripting Accessibility | **PASS** | No new public API exposed. Existing `prelude.fsx` and example scripts remain valid. No scripting updates needed. |
| Engineering Constraints | **PASS** | F# only. New `.fsi` for new module. No new external dependencies. Packable via `dotnet pack`. |

### Post-Design Re-check

| Gate | Status | Notes |
|------|--------|-------|
| II. `.fsi` contracts | **PASS** | `CachedRenderer.fsi` defined in contracts/public-api.md. Internal module — no public surface change. |
| III. Test evidence | **PASS** | Test strategy covers pixel-identity (behavioral), cache statistics (functional), memory bounds (non-functional). |
| IV. Observability | **PASS** | `CacheStats` record exposes hit/miss/eviction counts. Errors in recording fall through to uncached path with stderr diagnostic. |

## Project Structure

### Documentation (this feature)

```text
specs/006-scene-diff-caching/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0: Research decisions
├── data-model.md        # Phase 1: Entity definitions
├── quickstart.md        # Phase 1: Implementation guide
├── contracts/
│   └── public-api.md    # Phase 1: API contract
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/SkiaViewer/
├── Scene.fsi              # Unchanged
├── Scene.fs               # Unchanged
├── SceneRenderer.fsi      # Unchanged
├── SceneRenderer.fs       # Unchanged (internal renderElement reused by CachedRenderer)
├── CachedRenderer.fsi     # NEW — internal signature for RenderCache + CacheStats
├── CachedRenderer.fs      # NEW — cache logic, diffing, recording, eviction
├── VulkanBackend.fs       # Unchanged
├── Viewer.fsi             # Unchanged
├── Viewer.fs              # MODIFIED — use RenderCache, invalidate on resize, dispose on shutdown
└── SkiaViewer.fsproj      # MODIFIED — add CachedRenderer files to compile order

tests/SkiaViewer.Tests/
├── SceneTests.fs              # Unchanged
├── SceneRendererTests.fs      # Unchanged
├── ViewerTests.fs             # Unchanged
├── CachedRendererTests.fs     # NEW — cache-specific tests
├── SurfaceAreaBaseline.txt    # Unchanged (no public API changes)
└── SkiaViewer.Tests.fsproj    # MODIFIED — add CachedRendererTests.fs
```

**Structure Decision**: Single project, no new projects. `CachedRenderer` is an internal module within the existing `SkiaViewer` library, positioned in the compile order after `SceneRenderer` (which it depends on) and before `Viewer` (which uses it).

## Complexity Tracking

No constitution violations. No complexity justifications needed.

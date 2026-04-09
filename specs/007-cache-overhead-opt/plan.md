# Implementation Plan: Cache Overhead Optimization

**Branch**: `007-cache-overhead-opt` | **Date**: 2026-04-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-cache-overhead-opt/spec.md`

## Summary

Optimize the CachedRenderer's internal data structures to reduce fully-animated scene overhead from 18.5% to under 5%. Replace dictionary-based cache with position-indexed slots, add reference-equality fast paths at both scene and element level, and use bounded recording regions. No public API changes.

## Technical Context

**Language/Version**: F# on .NET 10.0  
**Primary Dependencies**: SkiaSharp 2.88.6 (SKPictureRecorder, SKPicture)  
**Storage**: In-memory position-indexed array (render-thread only)  
**Testing**: xunit 2.9.3, existing CachedRendererTests + benchmark  
**Target Platform**: Linux (primary), cross-platform via .NET  
**Project Type**: Library  
**Performance Goals**: <=5% overhead for fully-animated; >=4.8x speedup for mostly-static; pixel-identical output  
**Constraints**: No public API changes; single file modification (CachedRenderer.fs); all existing tests must pass  
**Scale/Scope**: Single file refactor of ~130 lines

## Constitution Check

| Gate | Status | Notes |
|------|--------|-------|
| I. Spec-First Delivery | **PASS** | Spec and plan complete. No public API changes — internal optimization only. |
| II. Compiler-Enforced Structural Contracts | **PASS** | No `.fsi` changes. `CachedRenderer.fsi` signature unchanged. Surface area baseline unchanged. |
| III. Test Evidence Is Mandatory | **PASS** | Existing tests validate correctness. Benchmark test validates performance targets. No new test files needed. |
| IV. Observability and Safe Failure Handling | **PASS** | `CacheStats` output unchanged. Diagnostics preserved. |
| V. Scripting Accessibility | **PASS** | No public API changes. Scripts unaffected. |
| Engineering Constraints | **PASS** | F# only. No new dependencies. No `.fsi` changes. Packable. |

### Post-Design Re-check

All gates remain **PASS**. This is a purely internal refactor.

## Project Structure

### Documentation (this feature)

```text
specs/007-cache-overhead-opt/
├── spec.md
├── plan.md              # This file
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── public-api.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Created by /speckit.tasks
```

### Source Code (repository root)

```text
src/SkiaViewer/
├── CachedRenderer.fsi     # Unchanged
├── CachedRenderer.fs      # MODIFIED — replace Dictionary with position-indexed slots,
│                          #   add reference-equality fast paths, bounded recording regions
└── (all other files unchanged)

tests/SkiaViewer.Tests/
├── CachedRendererTests.fs  # Unchanged (existing tests + benchmark validate changes)
└── (all other files unchanged)
```

**Structure Decision**: Single file modification. No new files, no project structure changes.

## Complexity Tracking

No constitution violations. No complexity justifications needed.

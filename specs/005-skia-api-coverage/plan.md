# Implementation Plan: Comprehensive SkiaSharp API Coverage

**Branch**: `005-skia-api-coverage` | **Date**: 2026-04-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-skia-api-coverage/spec.md`

## Summary

Extend the declarative scene DSL with comprehensive SkiaSharp 2.88.6 API coverage: stroke styling (cap, join, miter), path effects (dash, corner, trim, 1D, compose, sum), shaders (radial, sweep, conical, Perlin noise, color, image, compose), all 29 blend modes, color filters, mask filters, image filters, canvas clipping, text/font system, path operations (boolean ops, measurement, fill types), canvas drawing extensions (points, vertices, arc), picture recording/playback, regions, runtime effects (SkSL), color space management, and 3D perspective transformations. All features are expressed as immutable F# discriminated unions and records, rendered by extending the existing SceneRenderer.

## Technical Context

**Language/Version**: F# on .NET 10.0
**Primary Dependencies**: SkiaSharp 2.88.6, Silk.NET.Windowing 2.22.0, Silk.NET.OpenGL 2.22.0, Silk.NET.Input 2.22.0, Silk.NET.Vulkan 2.22.0
**Storage**: N/A
**Testing**: xUnit (existing test project `tests/SkiaViewer.Tests`)
**Target Platform**: Linux (with Vulkan/GL auto-detection)
**Project Type**: Library (NuGet-packable)
**Performance Goals**: 60 fps scene rendering maintained; new paint properties must not degrade render loop
**Constraints**: No new NuGet dependencies. All APIs confirmed available in SkiaSharp 2.88.6 (see research.md). No backward compatibility for Paint record.
**Scale/Scope**: Single project, ~2 modified files (Scene.fs/fsi, SceneRenderer.fs), ~30 new DU types, ~20 new Scene module functions

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Pre-Design | Post-Design |
|-----------|------|------------|-------------|
| I. Spec-First | Spec exists, plan traces to spec requirements | PASS | PASS — spec (68 FRs), research, data-model, contracts, quickstart all written |
| II. Compiler-Enforced Contracts | New public types require `.fsi` updates; surface-area baseline update | PLANNED | PASS — Scene.fsi contract defined with all new types; baseline update planned |
| III. Test Evidence | Each user story requires automated tests | PLANNED | PLANNED — deferred to `/speckit.tasks` |
| IV. Observability | Structured diagnostics for errors | PLANNED | PASS — runtime effect CPU error raises exception; disposed bitmap logging preserved; SkSL compile errors reported |
| V. Scripting Accessibility | Prelude + example scripts updated | PLANNED | PASS — prelude.fsx update + 2 new example scripts (04, 05) planned |
| Engineering | No new dependencies | PASS | PASS — all features use existing SkiaSharp 2.88.6 APIs |
| Engineering | Every public `.fs` has `.fsi` | PASS | PASS — Scene.fsi extended; SceneRenderer.fsi internal unchanged; Viewer.fsi unchanged |
| Engineering | Packable via `dotnet pack` | PASS | PASS — no changes to packaging |

## Project Structure

### Documentation (this feature)

```text
specs/005-skia-api-coverage/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── public-api.md    # Public API contract (.fsi definitions)
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/SkiaViewer/
├── VulkanBackend.fs          # UNCHANGED
├── Scene.fsi                 # MODIFIED — ~30 new DU types, extended Paint/Element/PathCommand/Transform, new Scene module functions
├── Scene.fs                  # MODIFIED — type definitions + DSL helpers + utility functions (region, path ops, text measure, picture recording)
├── SceneRenderer.fsi         # UNCHANGED (internal module)
├── SceneRenderer.fs          # MODIFIED — render new element types, apply new paint properties (shader, filters, effects, clip, blend mode, stroke style)
├── Viewer.fsi                # UNCHANGED
├── Viewer.fs                 # MINOR — update any internal Paint construction to include new fields

tests/SkiaViewer.Tests/
├── SceneTests.fs             # MODIFIED — test new types, DSL helpers, utility functions
├── SceneRendererTests.fs     # MODIFIED — pixel-level tests for new rendering features
├── ViewerTests.fs            # MODIFIED — update Paint construction in tests
├── SurfaceAreaBaseline.txt   # UPDATED — reflect current + new API surface

scripts/
├── prelude.fsx                     # MODIFIED — update paint helpers for new fields
├── examples/
│   ├── 01-screenshot.fsx           # MODIFIED — update Paint usage
│   ├── 02-declarative-scene.fsx    # MODIFIED — update Paint usage
│   ├── 03-perf-suite.fsx           # UNCHANGED
│   ├── 04-effects-showcase.fsx     # NEW — shaders, filters, blend modes, path effects
│   └── 05-advanced-features.fsx    # NEW — path ops, regions, runtime effects, 3D, text/font

docs/
├── drawing-primitives.fsx          # MODIFIED — add new drawing examples
├── tests.fsx                       # MODIFIED — add new test documentation
```

**Structure Decision**: Single project (existing). All new types are added to `Scene.fs`/`Scene.fsi`. Renderer extensions go in `SceneRenderer.fs`. No new source files needed — the existing module structure is sufficient. Compilation order unchanged: `VulkanBackend.fs` → `Scene.fsi` → `Scene.fs` → `SceneRenderer.fsi` → `SceneRenderer.fs` → `Viewer.fsi` → `Viewer.fs`.

## Complexity Tracking

No constitution violations to justify.

## Design Decisions

### D1: Paint Record — All Fields Required

The Paint record is extended with 10 new required fields. No backward compatibility. `emptyPaint` provides sensible defaults (StrokeCap.Butt, StrokeJoin.Miter, StrokeMiter=4f, BlendMode.SrcOver, all optional fields = None). All existing call sites must be updated.

### D2: New Types as Sibling DUs in Scene.fs

All new types (Shader, PathEffect, ColorFilter, MaskFilter, ImageFilter, Clip, FontSpec, etc.) are defined as top-level types in `Scene.fs`/`Scene.fsi`, following the existing pattern where Paint, Transform, PathCommand, Element are all top-level namespace types.

### D3: Clip on Group Element

Clipping is added as an optional `clip: Clip option` parameter on the `Element.Group` case. This follows SkiaSharp's canvas model where clip is applied after save and before drawing children.

### D4: Element DU Extended with New Cases

New drawing primitives (Points, Vertices, Arc, Picture) are added as new Element DU cases. This is a breaking change but consistent with the no-backward-compatibility decision.

### D5: Utility Functions in Scene Module

Non-rendering operations (text measurement, path boolean ops, path measurement, region creation/testing, picture recording) are exposed as functions in the `Scene` module rather than as types. They return computed values, not visual elements.

### D6: Transform Extended with Perspective

3D perspective transforms are integrated into the existing Transform DU as a new `Perspective of Transform3D` case. The renderer converts Transform3D to an SKMatrix via SK3dView.

### D7: Runtime Effect Error on CPU

When a runtime effect shader/filter is encountered during rendering and the backend is CPU (Raster), the renderer raises `System.NotSupportedException` with a descriptive message.

## Implementation Phasing

The implementation should follow this priority order from the spec:

**Phase A (P1 — Core Effects)**:
1. New type definitions in Scene.fsi/Scene.fs (all DU types)
2. Paint record extension + emptyPaint update + `with*` helpers
3. Stroke styling rendering (StrokeCap, StrokeJoin, StrokeMiter)
4. Blend mode rendering
5. Shader system (all 8 shader types + rendering)
6. Path effects (all 6 types + rendering)
7. Update all existing code (tests, scripts, docs, Viewer.fs) for new Paint fields

**Phase B (P2 — Filters & Text)**:
8. Color filters (6 types + rendering)
9. Mask filters (blur + rendering)
10. Image filters (10 types + rendering)
11. Canvas clipping (Group.clip + rendering)
12. Text/font system (FontSpec, SKFont/SKTypeface in renderer, text measurement)
13. Update tests for Phase B features

**Phase C (P3 — Advanced)**:
14. Path operations (boolean ops, measurement, fill types, convenience commands)
15. Canvas drawing extensions (Points, Vertices, Arc element types + rendering)
16. Picture recording/playback
17. Regions (utility functions)
18. Runtime effects (SkSL compilation, uniform setting, CPU error)
19. Color space management
20. 3D perspective transforms (Transform3D, SK3dView in renderer)
21. Update tests, scripts, docs, surface-area baseline for Phase C

## Key Files and Line Counts (Estimates)

| File | Action | Estimated Lines |
|------|--------|-----------------|
| Scene.fsi | MODIFIED | +250 (new types) |
| Scene.fs | MODIFIED | +400 (types + helpers + utilities) |
| SceneRenderer.fs | MODIFIED | +350 (new rendering logic) |
| Viewer.fs | MINOR | +5 (Paint construction update) |
| SceneTests.fs | MODIFIED | +300 (new type tests) |
| SceneRendererTests.fs | MODIFIED | +400 (pixel-level rendering tests) |
| ViewerTests.fs | MINOR | +10 (Paint updates) |
| SurfaceAreaBaseline.txt | UPDATED | full rewrite |
| prelude.fsx | MODIFIED | +30 (new helpers) |
| 04-effects-showcase.fsx | NEW | ~100 |
| 05-advanced-features.fsx | NEW | ~100 |

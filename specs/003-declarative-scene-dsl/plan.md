# Implementation Plan: Declarative Scene DSL

**Branch**: `003-declarative-scene-dsl` | **Date**: 2026-04-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-declarative-scene-dsl/spec.md`

## Summary

Replace the imperative `OnRender` callback API with a fully declarative scene-stream architecture. The viewer accepts an `IObservable<Scene>` of composable F# element trees and produces an `IObservable<InputEvent>` of typed input events. An internal scene renderer translates the element tree into SkiaSharp canvas calls each frame. The `ViewerHandle` retains `Dispose()` and `Screenshot()` for lifecycle management.

## Technical Context

**Language/Version**: F# on .NET 10.0  
**Primary Dependencies**: SkiaSharp 2.88.6, Silk.NET.Windowing 2.22.0, Silk.NET.OpenGL 2.22.0, Silk.NET.Input 2.22.0, Silk.NET.Vulkan 2.22.0  
**Storage**: N/A  
**Testing**: xUnit (existing test project `tests/SkiaViewer.Tests`)  
**Target Platform**: Linux (with Vulkan/GL auto-detection)  
**Project Type**: Library (NuGet-packable)  
**Performance Goals**: 60 fps scene rendering, input event delivery within 1 frame  
**Constraints**: No new NuGet dependencies — `IObservable<T>`/`IObserver<T>` are BCL types; a lightweight internal `EventSource<T>` replaces the need for `System.Reactive`  
**Scale/Scope**: Single project, ~4 new files, ~2 modified files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Pre-Design | Post-Design |
|-----------|------|------------|-------------|
| I. Spec-First | Spec exists, plan traces to spec requirements | PASS | PASS — spec, research, data-model, contracts all written |
| II. Compiler-Enforced Contracts | New public modules require `.fsi` files; surface-area baseline update | PLANNED | PASS — `Scene.fsi` (public), `SceneRenderer.fsi` (internal) defined in contracts; baseline test update planned |
| III. Test Evidence | Each user story requires automated tests | PLANNED | PLANNED — deferred to `/speckit.tasks` |
| IV. Observability | Structured diagnostics for errors | PLANNED | PASS — research R6 defines stream error handling; edge cases specify logging for disposed bitmaps, stream errors |
| V. Scripting Accessibility | Prelude + example scripts updated | PLANNED | PASS — quickstart shows new API; `prelude.fsx` + `02-declarative-scene.fsx` in structure |
| Engineering | No new dependencies | PASS | PASS — confirmed BCL-only (`IObservable<T>`, F# `Event<T>`) |
| Engineering | Every public `.fs` has `.fsi` | PLANNED | PASS — `Scene.fsi`, `Viewer.fsi` defined; `SceneRenderer.fsi` internal |
| Engineering | Packable via `dotnet pack` | PASS | PASS — no changes to packaging |

## Project Structure

### Documentation (this feature)

```text
specs/003-declarative-scene-dsl/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── public-api.md    # Public API contract
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/SkiaViewer/
├── VulkanBackend.fs          # Unchanged (internal)
├── Scene.fsi                 # NEW — public types: Element, Scene, Paint, Transform, InputEvent + DSL module
├── Scene.fs                  # NEW — type definitions + idiomatic helper functions
├── SceneRenderer.fsi         # NEW — internal module signature
├── SceneRenderer.fs          # NEW — walks scene tree, emits SKCanvas calls
├── Viewer.fsi                # MODIFIED — new declarative API surface (replaces OnRender)
├── Viewer.fs                 # MODIFIED — scene stream subscription, input event publishing

tests/SkiaViewer.Tests/
├── ViewerTests.fs            # MODIFIED — update tests for new API
├── SceneTests.fs             # NEW — DSL construction, element composition
├── SceneRendererTests.fs     # NEW — rendering correctness (pixel-level)

scripts/
├── prelude.fsx               # MODIFIED — new API helpers
├── examples/
│   ├── 01-screenshot.fsx     # MODIFIED — use declarative API
│   └── 02-declarative-scene.fsx  # NEW — interactive scene demo
```

**Structure Decision**: Single project (existing). New files are added to `src/SkiaViewer/` with `.fsi` signatures. The `.fsproj` compilation order becomes: `VulkanBackend.fs` → `Scene.fsi` → `Scene.fs` → `SceneRenderer.fsi` → `SceneRenderer.fs` → `Viewer.fsi` → `Viewer.fs`.

## Complexity Tracking

No constitution violations to justify.

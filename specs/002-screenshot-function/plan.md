# Implementation Plan: Public Screenshot Function

**Branch**: `002-screenshot-function` | **Date**: 2026-04-08 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-screenshot-function/spec.md`

## Summary

Add a public `Screenshot` method to the SkiaViewer library that captures the current rendered frame and saves it as a PNG or JPEG image to a user-specified folder. The method is synchronous (blocks until file is written), thread-safe, and works with both Vulkan and GL raster backends. This requires promoting the internal `ViewerHandle` type to public and changing the return type of `Viewer.run` from `IDisposable` to `ViewerHandle`.

## Technical Context

**Language/Version**: F# on .NET 10.0  
**Primary Dependencies**: SkiaSharp 2.88.6, Silk.NET.Windowing 2.22.0, Silk.NET.OpenGL 2.22.0, Silk.NET.Vulkan 2.22.0  
**Storage**: File system (screenshot images written to user-specified directory)  
**Testing**: xUnit 2.9.3 via `dotnet test`  
**Target Platform**: Linux (primary), cross-platform via .NET  
**Project Type**: Library (NuGet packable)  
**Performance Goals**: Screenshot capture should not visibly disrupt the render loop  
**Constraints**: Thread-safe access to render surface; Vulkan GPU→CPU readback required for Vulkan backend  
**Scale/Scope**: Single library project, ~400 LOC main module

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| I. Spec-First Delivery | PASS | Spec and plan created before implementation. Adds public API surface (Screenshot, ImageFormat, ViewerHandle). |
| II. Compiler-Enforced Structural Contracts | PASS | Plan includes `.fsi` updates for all new/changed public types. Surface-area baseline update planned. |
| III. Test Evidence Is Mandatory | PASS | Test cases defined for each user story. No mocks — tests use live viewer. |
| IV. Observability and Safe Failure Handling | PASS | Screenshot errors return `Result.Error` with descriptive messages. Diagnostic logging via stderr for capture events. |
| V. Scripting Accessibility | PASS | FSI prelude and example script planned for screenshot API. |
| Engineering: F# exclusive | PASS | All code is F#. |
| Engineering: `.fsi` for public modules | PASS | Viewer.fsi will be updated with new types. |
| Engineering: Surface-area baselines | PASS | Baseline update planned. |
| Engineering: `dotnet pack` | PASS | Existing packable project, no changes needed. |

**Post-Phase 1 Re-check**: All gates remain PASS. The design uses `Result<string, string>` for error indication (Constitution IV), updates `.fsi` signatures (Constitution II), and plans test coverage for all user stories (Constitution III).

## Project Structure

### Documentation (this feature)

```text
specs/002-screenshot-function/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── public-api.md    # Public API contract
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
└── SkiaViewer/
    ├── VulkanBackend.fs   # Internal Vulkan backend (no changes expected)
    ├── Viewer.fsi         # Public API signatures (MODIFIED: add ImageFormat, ViewerHandle, update run)
    └── Viewer.fs          # Implementation (MODIFIED: add ImageFormat, promote ViewerHandle, implement Screenshot)

tests/
└── SkiaViewer.Tests/
    └── ViewerTests.fs     # Tests (MODIFIED: add screenshot test cases)

scripts/                   # NEW directory
├── prelude.fsx            # NEW: FSI prelude for interactive use
└── examples/
    └── 01-screenshot.fsx  # NEW: Screenshot example script
```

**Structure Decision**: Single existing project layout. No new projects needed. The screenshot function is added directly to the existing `Viewer` module and `ViewerHandle` type.

## Complexity Tracking

No constitution violations to justify.

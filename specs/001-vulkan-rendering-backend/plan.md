# Implementation Plan: Vulkan-Backed SkiaSharp Rendering

**Branch**: `001-vulkan-rendering-backend` | **Date**: 2026-04-08 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-vulkan-rendering-backend/spec.md`

## Summary

Replace the CPU raster + OpenGL texture upload pipeline in `Viewer.fs` with a Vulkan-backed SkiaSharp rendering path using `GRContext.CreateVulkan`. The Vulkan backend eliminates per-frame CPU→GPU pixel copies by creating GPU-backed `SKSurface` instances. The existing GL raster pipeline is preserved as an automatic fallback for environments without Vulkan. The consumer API (`ViewerConfig` + `Viewer.run`) gains one new optional field (`PreferredBackend`) with otherwise identical behavior.

## Technical Context

**Language/Version**: F# on .NET 10.0
**Primary Dependencies**: Silk.NET.Windowing 2.22.0, Silk.NET.OpenGL 2.22.0, Silk.NET.Input 2.22.0, SkiaSharp 2.88.6, SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.6, **Silk.NET.Vulkan 2.22.0 (new)**
**Storage**: N/A
**Testing**: xunit 2.9.3 via `dotnet test`
**Target Platform**: Linux (X11/GLFW), containerized environments with GPU passthrough
**Project Type**: Library (NuGet-packable)
**Performance Goals**: 60 fps with zero CPU→GPU pixel copies on Vulkan path
**Constraints**: Must not break existing `OnRender` callback consumers; must fall back to GL raster when Vulkan unavailable
**Scale/Scope**: Single-window viewer, single GPU

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| I. Spec-First Delivery | PASS | Spec exists with user stories, acceptance criteria, and clarifications |
| II. Compiler-Enforced .fsi Contracts | REQUIRES | `Viewer.fsi` must be updated for `Backend` type and `ViewerConfig.PreferredBackend` field. `VulkanBackend.fs` is internal (no .fsi needed). |
| II. Surface-area baselines | NOTE | No baselines exist yet (pre-existing gap). Should be addressed but not blocking for this feature. |
| III. Test Evidence | REQUIRES | Tests must cover: Vulkan init success path, GL fallback path, backend logging, resize with Vulkan, resource cleanup. Tests skip Vulkan assertions if no GPU available. |
| IV. Observability | REQUIRES | Backend selection logged at startup. Fallback reason logged. Vulkan init failures logged with device/driver details. |
| V. Scripting Accessibility | NOTE | No `scripts/prelude.fsx` exists (pre-existing gap). Not blocking for this feature but should be addressed. |
| VII. Dependencies minimized | REQUIRES | Silk.NET.Vulkan 2.22.0 justified: provides Vulkan API bindings needed for GPU-backed rendering. Same package family as existing Silk.NET deps, version-aligned. |
| VII. Packable via dotnet pack | PASS | Already configured in `.fsproj`. |

**Gate result**: PASS — no violations. Two pre-existing gaps noted (surface-area baselines, scripting accessibility) are out of scope for this feature but should be addressed as follow-up work since this feature adds public API surface (`Backend` type, `PreferredBackend` field).

### Post-Phase 1 Re-check

| Gate | Status | Notes |
|------|--------|-------|
| II. .fsi Contracts | PLANNED | Contract defined in `contracts/viewer-fsi-contract.md`. Implementation must produce updated `Viewer.fsi`. |
| III. Test Evidence | PLANNED | Test scenarios defined per user story. Implementation must produce passing tests. |
| IV. Observability | PLANNED | All diagnostic messages defined. Implementation must emit them. |

## Project Structure

### Documentation (this feature)

```text
specs/001-vulkan-rendering-backend/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: research findings
├── data-model.md        # Phase 1: type definitions and state model
├── quickstart.md        # Phase 1: migration and usage guide
├── contracts/
│   └── viewer-fsi-contract.md  # Phase 1: public API delta
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2: implementation tasks (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/SkiaViewer/
├── SkiaViewer.fsproj    # Updated: add Silk.NET.Vulkan package reference
├── Viewer.fsi           # Updated: add Backend type, PreferredBackend field
├── VulkanBackend.fs     # New (internal): Vulkan init, render, cleanup
└── Viewer.fs            # Updated: backend selection, dispatch to VulkanBackend or GL raster

tests/SkiaViewer.Tests/
├── SkiaViewer.Tests.fsproj  # Unchanged
└── ViewerTests.fs           # Updated: add backend selection and fallback tests
```

**Structure Decision**: Single project, no new public modules. `VulkanBackend.fs` is an internal module — all public API changes are confined to the existing `Viewer.fsi`/`Viewer.fs` boundary. This preserves the project's minimal structure.

## Complexity Tracking

No constitution violations requiring justification. The only new dependency (Silk.NET.Vulkan) is from the same package family already in use.

## Phase 0: Research Findings

See [research.md](research.md) for full details. Key decisions:

1. **R1**: Silk.NET.Vulkan 2.22.0 for Vulkan bindings, bridged to SkiaSharp via `GRVkBackendContext`
2. **R2**: GLFW Vulkan surface extensions for window presentation
3. **R3**: `PreferredBackend: Backend option` added to ViewerConfig (source-level break, documented migration)
4. **R4**: Fallback order Vulkan → GL raster, each wrapped in try-with
5. **R5**: Vulkan resources created in Load, destroyed in Closing
6. **R6**: MSAA enabled by default at 4x, not configurable
7. **R7**: New internal `VulkanBackend.fs` file for separation of concerns
8. **R8**: `WindowOptions.API = None` for manual Vulkan surface control

## Phase 1: Design

### Data Model

See [data-model.md](data-model.md). Key types:

- **Backend** (public DU): `Vulkan | GL | Raster`
- **ViewerConfig** (modified record): +`PreferredBackend: Backend option`
- **VulkanState** (internal): Holds Vulkan handles during viewer lifecycle
- **ActiveBackend** (internal DU): `VulkanActive of VulkanState | GlRasterActive of GL state`

### Public API Contract

See [contracts/viewer-fsi-contract.md](contracts/viewer-fsi-contract.md). Surface-area delta:

| Symbol | Change |
|--------|--------|
| `Backend` | Added (DU, 3 cases) |
| `ViewerConfig.PreferredBackend` | Added (`Backend option`) |
| `Viewer.run` | Signature unchanged |

### Architecture: Render Loop by Backend

**Vulkan path** (new):
```
Load → VulkanBackend.init(window) → VulkanState
Render → lock surface → canvas.Clear → OnRender → canvas.Flush → grContext.Flush → swapchain present
Resize → VulkanBackend.recreateSwapchain → recreate GPU-backed SKSurface
Closing → VulkanBackend.cleanup(state)
```

**GL raster path** (existing, unchanged):
```
Load → setupGl() → GL state
Render → lock surface → canvas.Clear → OnRender → canvas.Flush → PeekPixels → glTexImage2D → draw quad
Resize → recreateSurface (CPU raster)
Closing → dispose surface → delete GL objects
```

### VulkanBackend.fs Internal Module Design

```fsharp
module internal SkiaViewer.VulkanBackend

/// Attempts Vulkan initialization. Returns VulkanState on success, None on failure.
/// Logs diagnostic details to stderr.
val tryInit: window: IWindow -> VulkanState option

/// Creates a VkSurfaceKHR for the GLFW window and initializes the swapchain.
val createWindowSurface: state: VulkanState -> window: IWindow -> unit

/// Creates a GPU-backed SKSurface for the given dimensions using the GRContext.
val createGpuSurface: state: VulkanState -> width: int -> height: int -> SKSurface option

/// Flushes the GRContext and presents the current frame via swapchain.
val presentFrame: state: VulkanState -> unit

/// Recreates the swapchain for a new window size.
val recreateSwapchain: state: VulkanState -> width: int -> height: int -> unit

/// Destroys all Vulkan resources in correct teardown order.
val cleanup: state: VulkanState -> unit
```

### File Compilation Order (fsproj)

```xml
<Compile Include="VulkanBackend.fs" />  <!-- New: before Viewer.fs -->
<Compile Include="Viewer.fsi" />
<Compile Include="Viewer.fs" />
```

`VulkanBackend.fs` must compile before `Viewer.fs` since `Viewer.fs` calls into it.

### Diagnostic Messages

| Event | Message Format |
|-------|---------------|
| Vulkan init success | `[Viewer] Backend selected: Vulkan ({device name})` |
| Vulkan init failure | `[Viewer] Vulkan initialization failed: {reason}` |
| GL fallback selected | `[Viewer] Backend selected: GL raster (fallback)` |
| Preferred backend unavailable | `[Viewer] Preferred backend {name} unavailable, falling back` |
| Vulkan surface created | `[Viewer] Vulkan surface created: {w}x{h}, MSAA {n}x` |
| Device lost | `[Viewer] Fatal: GPU device lost, terminating` |

### Test Plan

| Test | Covers | Strategy |
|------|--------|----------|
| Vulkan backend renders frames | US1 | Run viewer with `PreferredBackend = Some Vulkan` on GPU system; verify frame count > 0. Skip if no Vulkan. |
| GL fallback when no Vulkan | US2 | Run viewer with Vulkan unavailable; verify frames rendered via GL path. |
| Backend selection logged | US3 | Capture stderr; verify backend message present. |
| PreferredBackend = None auto-detects | US4 | Run with `None`; verify a backend is selected and logged. |
| PreferredBackend = Some GL skips Vulkan | US4 | Run with `Some GL`; verify GL path used even if Vulkan available. |
| Resize with Vulkan backend | US1-AC3 | Resize window during Vulkan rendering; verify no crash. Skip if no Vulkan. |
| Existing tests still pass | FR-007 | All 8 existing tests pass with `PreferredBackend = None`. |

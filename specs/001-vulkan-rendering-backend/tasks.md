# Tasks: Vulkan-Backed SkiaSharp Rendering

**Input**: Design documents from `/specs/001-vulkan-rendering-backend/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Required per Constitution Principle III (Test Evidence Is Mandatory). Tests run against live environment — no mocks.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Add new dependency and prepare project structure for Vulkan backend

- [x] T001 Add Silk.NET.Vulkan 2.22.0 package reference to src/SkiaViewer/SkiaViewer.fsproj
- [x] T002 Add VulkanBackend.fs to compile order in src/SkiaViewer/SkiaViewer.fsproj (before Viewer.fsi, after a new Compile entry)
- [x] T003 Create empty internal module file src/SkiaViewer/VulkanBackend.fs with namespace and module declaration

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Public API type changes and internal type scaffolding that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Add Backend discriminated union type (Vulkan | GL | Raster) with RequireQualifiedAccess to src/SkiaViewer/Viewer.fs
- [x] T005 Add PreferredBackend field (Backend option) to ViewerConfig record in src/SkiaViewer/Viewer.fs
- [x] T006 Update src/SkiaViewer/Viewer.fsi to expose Backend type with XML documentation per contracts/viewer-fsi-contract.md
- [x] T007 Update src/SkiaViewer/Viewer.fsi to add PreferredBackend field to ViewerConfig with XML documentation
- [x] T008 Update all existing ViewerConfig record expressions in tests/SkiaViewer.Tests/ViewerTests.fs to include PreferredBackend = None
- [x] T009 Define internal VulkanState record type and ActiveBackend discriminated union (VulkanActive of VulkanState | GlRasterActive) in src/SkiaViewer/VulkanBackend.fs
- [x] T010 Verify project builds and all 8 existing tests pass with dotnet test

**Checkpoint**: Foundation ready — public API updated, existing behavior preserved, user story implementation can begin

---

## Phase 3: User Story 1 — GPU-Accelerated Rendering via Vulkan (Priority: P1) MVP

**Goal**: Viewer renders consumer canvas drawing on GPU via Vulkan backend with zero CPU-to-GPU pixel copies

**Independent Test**: Launch viewer with Vulkan-capable GPU, draw shapes/text, confirm GPU-backed rendering completes. Skip Vulkan-specific assertions if no GPU available.

### Implementation for User Story 1

- [x] T011 [US1] Implement VulkanBackend.tryInit in src/SkiaViewer/VulkanBackend.fs — create Vulkan instance, enumerate physical devices, select first with graphics queue, create logical device and queue, build GRVkBackendContext with vkGetProcAddr bridge, create GRContext.CreateVulkan. Return VulkanState option. Log device name on success, failure reason on error.
- [x] T012 [US1] Implement VulkanBackend.createGpuSurface in src/SkiaViewer/VulkanBackend.fs — create GPU-backed SKSurface via SKSurface.Create(grContext, budgeted, imageInfo) with MSAA sample count capped at 4x from GRContext.GetMaxSurfaceSampleCount
- [x] T013 [US1] Implement VulkanBackend.createWindowSurface in src/SkiaViewer/VulkanBackend.fs — create VkSurfaceKHR for GLFW window using Silk.NET GLFW Vulkan surface extensions, create swapchain for frame presentation
- [x] T014 [US1] Implement VulkanBackend.presentFrame in src/SkiaViewer/VulkanBackend.fs — flush GRContext and present current frame via swapchain
- [x] T015 [US1] Implement VulkanBackend.recreateSwapchain in src/SkiaViewer/VulkanBackend.fs — destroy old swapchain, create new one at given dimensions, recreate GPU-backed SKSurface
- [x] T016 [US1] Implement VulkanBackend.cleanup in src/SkiaViewer/VulkanBackend.fs — dispose GRContext, destroy swapchain, destroy surface, destroy device, destroy instance in correct teardown order
- [x] T017 [US1] Update Viewer.fs Load handler to attempt VulkanBackend.tryInit. On success, store VulkanActive state, call createWindowSurface and createGpuSurface. On failure, fall through to existing GL setup (basic try-with only — PreferredBackend dispatch logic deferred to US2 T025). Set window API to GraphicsAPI.None for Vulkan path.
- [x] T018 [US1] Update Viewer.fs Render handler to dispatch based on active backend — Vulkan path: lock surface, clear canvas, call OnRender, canvas.Flush, grContext.Flush, presentFrame. GL path: existing glTexImage2D logic unchanged.
- [x] T019 [US1] Update Viewer.fs FramebufferResize handler to dispatch based on active backend — Vulkan path: call recreateSwapchain and recreate GPU surface. GL path: existing recreateSurface.
- [x] T020 [US1] Update Viewer.fs Closing handler to dispatch cleanup based on active backend — Vulkan path: call VulkanBackend.cleanup. GL path: existing GL resource deletion.

### Tests for User Story 1

- [x] T021 [US1] Add test in tests/SkiaViewer.Tests/ViewerTests.fs: Vulkan backend renders frames without exceptions — run viewer with PreferredBackend = Some Backend.Vulkan for 3 seconds, verify frame count > 0. Mark test as skipped if Vulkan init fails (no GPU available).
- [x] T022 [US1] Add test in tests/SkiaViewer.Tests/ViewerTests.fs: window resize with Vulkan backend does not crash — resize during Vulkan rendering, verify continued frame rendering. Skip if no Vulkan.
- [x] T023 [US1] Verify all 8 original tests still pass with PreferredBackend = None (regression check)

**Checkpoint**: User Story 1 complete — Vulkan GPU rendering works end-to-end. This is the MVP.

---

## Phase 4: User Story 2 — Automatic Fallback to Raster Pipeline (Priority: P2)

**Goal**: Viewer automatically falls back to GL raster pipeline when Vulkan is unavailable, with no consumer code changes

**Independent Test**: Run viewer in environment without Vulkan drivers, verify it renders via GL raster path identically to current behavior.

### Implementation for User Story 2

- [x] T024 [US2] Harden fallback logic in Viewer.fs Load handler — ensure all Vulkan failure modes (init exception, null device, queue not found) are caught and logged with reason before proceeding to GL setupGl path. Ensure fallback is seamless with no consumer-visible error.
- [x] T025 [US2] Implement backend priority order in Viewer.fs — respect PreferredBackend: None tries Vulkan then GL; Some Vulkan tries Vulkan only then falls back; Some GL skips Vulkan entirely; Some Raster reserved for future headless.

### Tests for User Story 2

- [x] T026 [US2] Add test in tests/SkiaViewer.Tests/ViewerTests.fs: GL fallback when preferred backend is GL — run viewer with PreferredBackend = Some Backend.GL, verify frames rendered (exercises GL raster path regardless of Vulkan availability)
- [x] T027 [US2] Add test in tests/SkiaViewer.Tests/ViewerTests.fs: auto-detect with PreferredBackend = None renders frames — verify viewer starts and renders successfully regardless of which backend is selected

**Checkpoint**: User Stories 1 and 2 complete — Vulkan rendering with automatic GL fallback

---

## Phase 5: User Story 3 — Backend Selection Logging (Priority: P3)

**Goal**: Viewer logs which rendering backend was selected at startup with diagnostic details

**Independent Test**: Launch viewer, capture stderr, verify backend selection message is present.

### Implementation for User Story 3

- [x] T028 [US3] Add diagnostic eprintfn messages in Viewer.fs Load handler per plan.md message table — "[Viewer] Backend selected: Vulkan ({device name})" on Vulkan success, "[Viewer] Vulkan initialization failed: {reason}" on failure, "[Viewer] Backend selected: GL raster (fallback)" on GL fallback
- [x] T029 [US3] Add device-lost diagnostic in Viewer.fs Render handler — "[Viewer] Fatal: GPU device lost, terminating" before raising error on Vulkan device loss

### Tests for User Story 3

- [x] T030 [US3] Add test in tests/SkiaViewer.Tests/ViewerTests.fs: backend selection message appears on stderr — capture stderr during viewer startup, verify it contains "Backend selected:" message

**Checkpoint**: User Stories 1, 2, and 3 complete — full Vulkan rendering with fallback and observability

---

## Phase 6: User Story 4 — Consumer Backend Preference (Priority: P3)

**Goal**: Consumers can optionally specify a preferred rendering backend via ViewerConfig

**Independent Test**: Set PreferredBackend to specific values and verify the correct backend is used.

### Implementation for User Story 4

- [x] T031 [US4] Implement preferred backend override logging in Viewer.fs — when PreferredBackend is Some, log "[Viewer] Preferred backend: {name}" before attempting init. When preferred is unavailable, log "[Viewer] Preferred backend {name} unavailable, falling back"

### Tests for User Story 4

- [x] T032 [US4] Add test in tests/SkiaViewer.Tests/ViewerTests.fs: preferred backend logging — run viewer with PreferredBackend = Some Backend.GL, capture stderr, verify it contains "[Viewer] Preferred backend: GL" message (distinct from T026 which tests rendering; this tests the preference logging from T031)

**Checkpoint**: All user stories complete

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, .fsi refinement, and final validation

- [x] T033 Update Viewer.fsi XML documentation — update ViewerConfig remarks to mention backend selection, update Viewer module summary to mention Vulkan primary with GL fallback, update Viewer.run remarks to document backend selection order and logging
- [x] T034 Update Viewer.fs namespacedoc module summary to reflect Vulkan-backed rendering as primary path
- [x] T035 [P] Run full test suite (dotnet test) and verify all tests pass in current environment
- [x] T036 [P] Build and verify dotnet pack succeeds for src/SkiaViewer/SkiaViewer.fsproj
- [x] T037 Validate quickstart.md migration steps against actual implementation — verify PreferredBackend = None works as documented

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — MVP target
- **US2 (Phase 4)**: Depends on Phase 2. Refines fallback logic from US1, best done after US1.
- **US3 (Phase 5)**: Depends on Phase 2. Can start after Phase 2 but benefits from US1/US2 being complete.
- **US4 (Phase 6)**: Depends on Phase 2. Lightweight; can run after US2 (which implements the backend priority logic).
- **Polish (Phase 7)**: Depends on all user stories being complete.

### User Story Dependencies

- **US1 (P1)**: After Foundational. No story dependencies. **MVP scope.**
- **US2 (P2)**: After Foundational. Refines logic introduced in US1; recommended to implement after US1.
- **US3 (P3)**: After Foundational. Adds logging to paths created in US1/US2; recommended after US2.
- **US4 (P3)**: After Foundational. Extends backend selection from US2; recommended after US2.

### Within Each User Story

- Implementation tasks are sequential (each builds on prior)
- Tests can run after implementation for that story completes
- Story checkpoint validates independent functionality

### Parallel Opportunities

- T001, T002, T003 are sequential (project file changes)
- T004, T005 modify same file (sequential), then T006, T007 modify same file (sequential)
- T009 (VulkanBackend.fs) can parallel with T004-T008 (Viewer.fs/Viewer.fsi changes)
- Within US1: T011-T016 (VulkanBackend.fs functions) are sequential; T017-T020 (Viewer.fs integration) are sequential after T011-T016
- US3 and US4 can run in parallel after US2 completes (different concerns)
- T035 and T036 (Polish) can run in parallel

---

## Parallel Example: User Story 1

```text
# After Phase 2 complete, begin US1 sequentially:
T011 → T012 → T013 → T014 → T015 → T016  (VulkanBackend.fs functions, sequential build-up)
T017 → T018 → T019 → T020                  (Viewer.fs integration, sequential after VulkanBackend complete)
T021, T022                                   (Tests, after implementation)
T023                                         (Regression check)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T010)
3. Complete Phase 3: User Story 1 (T011-T023)
4. **STOP and VALIDATE**: Vulkan GPU rendering works, existing tests pass
5. This delivers the core value: GPU-accelerated rendering with no pixel copies

### Incremental Delivery

1. Setup + Foundational → Project compiles, existing tests pass
2. Add US1 → GPU rendering works → MVP
3. Add US2 → Automatic fallback → Production-ready for mixed environments
4. Add US3 + US4 → Observability and configuration → Feature-complete
5. Polish → Documentation and packaging → Ship-ready

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Constitution requires .fsi updates (Phase 2) and test evidence (each story phase)
- Tests must run against live environment per Constitution Principle III — Vulkan tests skip if no GPU
- Commit after each task or logical group
- Stop at any checkpoint to validate independently

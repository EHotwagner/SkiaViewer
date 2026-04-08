# Tasks: Public Screenshot Function

**Input**: Design documents from `/specs/002-screenshot-function/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Constitution III mandates test evidence for all behavior-changing code. Test tasks are included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup

**Purpose**: No new projects needed. Prepare directory structure for FSI scripts (Constitution V).

- [x] T001 Create directory structure: `scripts/` and `scripts/examples/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define new public types and change `Viewer.run` return type. MUST be complete before user story implementation.

**CRITICAL**: No user story work can begin until this phase is complete.

- [x] T002 Define `ImageFormat` discriminated union (`Png | Jpeg`) with `[<RequireQualifiedAccess>]` in `src/SkiaViewer/Viewer.fs` (add before `ViewerConfig` type)
- [x] T003 Promote `ViewerHandle` from `private` to public in `src/SkiaViewer/Viewer.fs` — move it outside the `Viewer` module to namespace level, implement `IDisposable`, add placeholder `Screenshot` member that returns `Error "Not yet implemented"`
- [x] T004 Update `Viewer.run` return type from `IDisposable` to `ViewerHandle` in `src/SkiaViewer/Viewer.fs` — the `ViewerHandle` constructor must capture `surfaceLock`, `surface` ref, `activeBackend` ref, `surfaceWidth`/`surfaceHeight` refs, and `shutdownRequested` flag
- [x] T005 Update `src/SkiaViewer/Viewer.fsi` signature file: add `ImageFormat` type, add `ViewerHandle` type with `IDisposable` interface and `Screenshot` member signature, change `Viewer.run` return type to `ViewerHandle`
- [x] T006 Verify the project compiles with `dotnet build src/SkiaViewer/SkiaViewer.fsproj` and existing tests pass with `dotnet test tests/SkiaViewer.Tests/`

**Checkpoint**: Foundation ready — all existing tests pass, new types are public, `Viewer.run` returns `ViewerHandle`

---

## Phase 3: User Story 1 — Capture Screenshot on Demand (Priority: P1) MVP

**Goal**: Developer can call `Screenshot(folder)` on a running viewer and get a PNG file saved to an existing folder, blocking until the file is written.

**Independent Test**: Run viewer with known drawing, call `Screenshot("/tmp/test-screenshots")`, verify a `.png` file appears with valid image content.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T007 [P] [US1] Add test `screenshot saves PNG file to existing folder` in `tests/SkiaViewer.Tests/ViewerTests.fs` — run viewer for 1s, call `Screenshot(tempDir)`, assert `Ok` result, assert file exists at returned path, assert file has `.png` extension, assert file size > 0
- [x] T008 [P] [US1] Add test `screenshot returns error before first frame` in `tests/SkiaViewer.Tests/ViewerTests.fs` — create viewer, immediately call `Screenshot(tempDir)` before any render, assert `Error` result
- [x] T009 [P] [US1] Add test `screenshot produces distinct files on rapid successive calls` in `tests/SkiaViewer.Tests/ViewerTests.fs` — run viewer for 1s, call `Screenshot` 10 times in a loop, assert 10 distinct `Ok` file paths, assert all 10 files exist
- [x] T009a [P] [US1] Add test `screenshot returns error after viewer disposal` in `tests/SkiaViewer.Tests/ViewerTests.fs` — create viewer, render 1s, dispose, call `Screenshot(tempDir)`, assert `Error` result
- [x] T009b [P] [US1] Add test `screenshot returns error when framebuffer is zero-size` in `tests/SkiaViewer.Tests/ViewerTests.fs` — create viewer with minimum size, call `Screenshot` immediately after creation before surface is ready, assert `Error` result

### Implementation for User Story 1

- [x] T010 [US1] Implement core `Screenshot` method on `ViewerHandle` in `src/SkiaViewer/Viewer.fs`:
  - Lock `surfaceLock`, check surface is not null (return `Error` if null)
  - Check `surfaceWidth`/`surfaceHeight` > 0 (return `Error` if zero-size)
  - For Vulkan backend: flush `GRContext`, `Submit(true)`, then `Snapshot()` + `ReadPixels()` to get CPU-accessible `SKBitmap`
  - For GL raster backend: `Snapshot()` to get `SKImage`
  - Release lock after snapshot
  - Generate filename: `screenshot-YYYYMMDD-HHmmss-fff.png` using UTC `DateTime.UtcNow`
  - Encode image as PNG via `SKImage.Encode(SKEncodedImageFormat.Png, 100)`
  - Write `SKData` to file via `System.IO.File.WriteAllBytes`
  - Return `Ok(fullPath)`
  - Wrap entire operation in try/with, return `Error(message)` on any exception
  - Log screenshot events to stderr: `[Viewer] Screenshot saved: {path}` or `[Viewer] Screenshot failed: {reason}`
- [x] T011 [US1] Update `src/SkiaViewer/Viewer.fsi` if the `Screenshot` signature changed during implementation
- [x] T012 [US1] Run all tests with `dotnet test tests/SkiaViewer.Tests/` — all US1 tests must pass, all existing tests must still pass

**Checkpoint**: User Story 1 is fully functional — screenshots can be captured and saved as PNG to existing folders

---

## Phase 4: User Story 2 — Configure Save Folder (Priority: P2)

**Goal**: Developer specifies a save folder path. Non-existent folders are created automatically. Invalid/inaccessible paths return descriptive errors.

**Independent Test**: Call `Screenshot` with a non-existent nested folder path, verify folder is created and file is saved. Call with an invalid path, verify `Error` result with descriptive message.

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T013 [P] [US2] Add test `screenshot creates non-existent folder` in `tests/SkiaViewer.Tests/ViewerTests.fs` — run viewer for 1s, call `Screenshot(tempDir + "/nested/subfolder")`, assert `Ok` result, assert directory exists, assert file exists
- [x] T014 [P] [US2] Add test `screenshot returns error for invalid path` in `tests/SkiaViewer.Tests/ViewerTests.fs` — run viewer for 1s, call `Screenshot` with a path containing null characters or other OS-invalid chars, assert `Error` result with descriptive message

### Implementation for User Story 2

- [x] T015 [US2] Add `Directory.CreateDirectory(folder)` call in the `Screenshot` method in `src/SkiaViewer/Viewer.fs` before writing the file — wrap in try/with to catch `IOException`, `UnauthorizedAccessException`, `ArgumentException` and return `Error` with descriptive message
- [x] T016 [US2] Run all tests with `dotnet test tests/SkiaViewer.Tests/` — all US1 + US2 tests must pass

**Checkpoint**: User Stories 1 AND 2 are both fully functional and independently testable

---

## Phase 5: User Story 3 — Choose Image Format (Priority: P3)

**Goal**: Developer can optionally specify `ImageFormat.Jpeg` to save as JPEG instead of the default PNG.

**Independent Test**: Call `Screenshot(folder, ImageFormat.Jpeg)`, verify saved file has `.jpg` extension and is a valid JPEG image.

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T017 [P] [US3] Add test `screenshot saves JPEG when format specified` in `tests/SkiaViewer.Tests/ViewerTests.fs` — run viewer for 1s, call `Screenshot(tempDir, ImageFormat.Jpeg)`, assert `Ok` result, assert file has `.jpg` extension, assert file size > 0
- [x] T018 [P] [US3] Add test `screenshot defaults to PNG when no format specified` in `tests/SkiaViewer.Tests/ViewerTests.fs` — run viewer for 1s, call `Screenshot(tempDir)` (no format argument), assert file has `.png` extension

### Implementation for User Story 3

- [x] T019 [US3] Update `Screenshot` method in `src/SkiaViewer/Viewer.fs` to accept optional `ImageFormat` parameter — match on format to determine `SKEncodedImageFormat` (Png→Png, Jpeg→Jpeg with quality 80) and file extension (`.png` or `.jpg`)
- [x] T020 [US3] Update `src/SkiaViewer/Viewer.fsi` if the `Screenshot` signature changed
- [x] T021 [US3] Run all tests with `dotnet test tests/SkiaViewer.Tests/` — all US1 + US2 + US3 tests must pass

**Checkpoint**: All user stories are independently functional

---

## Phase 6: User Story Validation — Both Backends

**Purpose**: Verify screenshot works identically with both Vulkan and GL backends (SC-004).

- [x] T022 [P] Add test `screenshot works with Vulkan backend` in `tests/SkiaViewer.Tests/ViewerTests.fs` — create viewer with `PreferredBackend = Some Backend.Vulkan`, render 1s, call `Screenshot`, assert `Ok` and valid file
- [x] T023 [P] Add test `screenshot works with GL backend` in `tests/SkiaViewer.Tests/ViewerTests.fs` — create viewer with `PreferredBackend = Some Backend.GL`, render 1s, call `Screenshot`, assert `Ok` and valid file
- [x] T024 Run full test suite with `dotnet test tests/SkiaViewer.Tests/`

**Checkpoint**: Screenshot works on both backends

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Constitution compliance, scripting accessibility, documentation.

- [x] T025 [P] Create FSI prelude script at `scripts/prelude.fsx` — `#r` reference to compiled SkiaViewer DLL, expose `screenshot` helper function for interactive use (Constitution V)
- [x] T026 [P] Create example script at `scripts/examples/01-screenshot.fsx` — demonstrate screenshot capture with folder path and both formats (Constitution V)
- [x] T027 [P] Update or create surface-area baseline for public API changes (Constitution II) — baseline must reflect new `ImageFormat`, `ViewerHandle`, and updated `Viewer.run` signature
- [x] T028 Run `dotnet pack src/SkiaViewer/SkiaViewer.fsproj` and verify package builds successfully
- [x] T029 Run full test suite one final time: `dotnet test tests/SkiaViewer.Tests/`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **User Stories (Phases 3-5)**: All depend on Phase 2 completion
  - US1 (P1) → US2 (P2) → US3 (P3) in priority order
  - US2 extends US1's Screenshot method (folder creation)
  - US3 extends US1's Screenshot method (format parameter)
- **Backend Validation (Phase 6)**: Depends on all user stories complete
- **Polish (Phase 7)**: Depends on Phase 6 complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **User Story 2 (P2)**: Extends US1's Screenshot implementation — depends on US1 completion
- **User Story 3 (P3)**: Extends US1's Screenshot implementation — depends on US1 completion (can run parallel with US2)

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Implementation tasks are sequential (single method being built up)
- Run full test suite after each story completes

### Parallel Opportunities

- T002 and T003 are sequential (same file: Viewer.fs)
- T007, T008, T009 can be written in parallel (separate test methods)
- T013, T014 can be written in parallel
- T017, T018 can be written in parallel
- T022, T023 can run in parallel (different test methods)
- T025, T026, T027 can run in parallel (different files)
- US2 and US3 can potentially run in parallel (both extend US1 in different ways)

---

## Parallel Example: User Story 1

```text
# Write all US1 tests in parallel:
T007: "Test screenshot saves PNG file to existing folder"
T008: "Test screenshot returns error before first frame"
T009: "Test screenshot produces distinct files on rapid calls"

# Then implement sequentially:
T010: "Implement core Screenshot method"
T011: "Update .fsi if needed"
T012: "Run all tests"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (create directories)
2. Complete Phase 2: Foundational (new types, return type change)
3. Complete Phase 3: User Story 1 (core screenshot capture)
4. **STOP and VALIDATE**: Test `Screenshot(existingFolder)` works end-to-end
5. This delivers the core value: programmatic frame capture

### Incremental Delivery

1. Setup + Foundational → Types and API shape ready
2. Add User Story 1 → Core screenshot works → MVP
3. Add User Story 2 → Folder auto-creation + error handling
4. Add User Story 3 → Format selection (PNG/JPEG)
5. Backend validation → Confirm both Vulkan and GL work
6. Polish → FSI scripts, surface-area baseline, packaging

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Constitution requires: .fsi updates (II), test evidence (III), stderr logging (IV), FSI scripts (V)
- Breaking change: `Viewer.run` return type changes from `IDisposable` to `ViewerHandle`
- All tests use live viewer (no mocks per Constitution III)
- Commit after each phase or logical group

# Feature Specification: Public Screenshot Function

**Feature Branch**: `002-screenshot-function`  
**Created**: 2026-04-08  
**Status**: Draft  
**Input**: User description: "add a public screenshot function, with save folder..."

## Clarifications

### Session 2026-04-08

- Q: Should the screenshot function block the caller until the file is saved (synchronous) or return immediately (asynchronous)? → A: Synchronous -- the function blocks the calling thread until the file is saved to disk, then returns the file path.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Capture Screenshot on Demand (Priority: P1)

A developer using the SkiaViewer library wants to programmatically capture the current rendered frame and save it as an image file to a designated folder. They call a screenshot function from their application code, and the current frame is saved to disk without interrupting the rendering loop.

**Why this priority**: This is the core value of the feature. Without the ability to capture and save a screenshot, no other functionality matters.

**Independent Test**: Can be fully tested by running a viewer with a known drawing, calling the screenshot function, and verifying that an image file appears in the specified folder with the correct content.

**Acceptance Scenarios**:

1. **Given** a running viewer displaying rendered content, **When** the developer calls the screenshot function with a save folder path, **Then** an image file is saved to that folder containing the current frame content.
2. **Given** a running viewer, **When** the developer calls the screenshot function, **Then** the rendering loop continues without visible interruption or frame drop.
3. **Given** a valid save folder path, **When** a screenshot is captured, **Then** the saved file uses a predictable, non-colliding filename (e.g., timestamp-based).

---

### User Story 2 - Configure Save Folder (Priority: P2)

A developer wants to specify a destination folder where screenshots are saved. If the folder does not exist, the system creates it automatically so the developer does not need to manage directory creation separately.

**Why this priority**: Save folder configuration is essential for organizing output, but the screenshot capture itself is the more fundamental capability.

**Independent Test**: Can be tested by specifying a non-existent folder path, calling the screenshot function, and verifying the folder is created and contains the saved image.

**Acceptance Scenarios**:

1. **Given** a save folder path that exists, **When** a screenshot is captured, **Then** the image file is saved into that folder.
2. **Given** a save folder path that does not exist, **When** a screenshot is captured, **Then** the folder is created and the image file is saved into it.
3. **Given** an invalid or inaccessible folder path (e.g., read-only location), **When** a screenshot is captured, **Then** the system reports an error without crashing the viewer.

---

### User Story 3 - Choose Image Format (Priority: P3)

A developer wants to save screenshots in a specific image format (e.g., PNG or JPEG) depending on their use case -- PNG for lossless quality, JPEG for smaller file sizes.

**Why this priority**: Format choice is a convenience enhancement. PNG as a default covers most use cases; explicit format selection is a refinement.

**Independent Test**: Can be tested by requesting screenshots in different formats and verifying the output files are valid images in the requested format.

**Acceptance Scenarios**:

1. **Given** no format is specified, **When** a screenshot is captured, **Then** the image is saved as PNG (default).
2. **Given** a specific format (e.g., JPEG) is requested, **When** a screenshot is captured, **Then** the image is saved in that format.

---

### Edge Cases

- What happens when the screenshot function is called before the first frame has rendered? The system returns an error indication.
- What happens when the screenshot function is called while the viewer window is minimized (zero-size framebuffer)? The system returns an error indication.
- What happens when the save folder path contains special characters or spaces? The system handles standard OS path conventions correctly.
- What happens when disk space is insufficient to save the image? The system returns an error indication without crashing.
- What happens when multiple screenshot calls occur in rapid succession? Each call produces a distinct file with a unique filename.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a public function to capture the current rendered frame and save it as an image file.
- **FR-002**: System MUST accept a save folder path as a parameter for screenshot destination.
- **FR-003**: System MUST create the save folder if it does not already exist.
- **FR-004**: System MUST generate unique, non-colliding filenames for each screenshot (using timestamp-based naming).
- **FR-005**: System MUST save screenshots as PNG by default.
- **FR-006**: System MUST support saving screenshots in at least PNG and JPEG formats.
- **FR-007**: System MUST capture the screenshot without blocking or interrupting the rendering loop.
- **FR-008**: System MUST be callable safely from any thread (not just the render thread).
- **FR-009**: System MUST block the calling thread until the image file is fully written to disk, then return the full file path of the saved screenshot. The caller can assume the file exists immediately after the function returns.
- **FR-010**: System MUST handle error conditions (no active surface, invalid path, I/O failure) gracefully by returning an error indication rather than throwing unhandled exceptions or crashing the viewer.

### Key Entities

- **Screenshot**: A point-in-time capture of the rendered frame, consisting of pixel data, a target file path, and an image format.
- **Save Folder**: A user-specified directory path where screenshot files are written. Created on demand if absent.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can capture and save a screenshot with a single function call.
- **SC-002**: Screenshot capture completes without any visible disruption to the rendered output.
- **SC-003**: Saved image files are valid and visually match the rendered frame at the time of capture.
- **SC-004**: The screenshot function works correctly regardless of which rendering backend (Vulkan or GL) is active.
- **SC-005**: Rapid successive screenshot calls (e.g., 10 calls within 1 second) each produce a distinct, valid image file.

## Assumptions

- The screenshot function is a library API for programmatic use, not a UI feature (no hotkey or button).
- Screenshots capture the full framebuffer at its current resolution; cropping or region selection is out of scope.
- The caller provides an absolute or relative folder path; path resolution follows standard OS conventions.
- JPEG quality uses a sensible default (e.g., 80%) without requiring the caller to specify quality parameters.
- The viewer must have rendered at least one frame before a screenshot can be captured; calls before the first render return an error indication.

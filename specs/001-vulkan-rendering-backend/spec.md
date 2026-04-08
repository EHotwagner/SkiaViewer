# Feature Specification: Vulkan-Backed SkiaSharp Rendering

**Feature Branch**: `001-vulkan-rendering-backend`  
**Created**: 2026-04-08  
**Status**: Draft  
**Input**: User description: "Replace OpenGL raster pipeline with Vulkan-backed SkiaSharp rendering"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - GPU-Accelerated Rendering via Vulkan (Priority: P1)

As a consumer application using SkiaViewer, I want the viewer to render my SkiaSharp canvas drawing on the GPU via a Vulkan backend, so that drawing operations execute on GPU hardware instead of the CPU, eliminating per-frame CPU-to-GPU pixel copies and improving rendering performance.

**Why this priority**: This is the core value of the feature. The current raster pipeline forces all drawing onto the CPU and uploads pixels to the GPU every frame. Vulkan-backed rendering removes this bottleneck entirely and is the primary reason for this change.

**Independent Test**: Can be tested by launching the viewer in an environment with a Vulkan-capable GPU, performing canvas drawing operations (shapes, text, fills), and confirming that rendering completes without CPU-to-GPU pixel transfer. The rendered output should be visually identical to the current raster path.

**Acceptance Scenarios**:

1. **Given** a system with a Vulkan-capable GPU and appropriate drivers installed, **When** the viewer starts and a consumer draws shapes and text via the OnRender callback, **Then** all drawing executes on the GPU and no per-frame CPU-to-GPU pixel copy occurs.
2. **Given** a Vulkan-backed rendering session, **When** the consumer's OnRender callback draws to the SKCanvas, **Then** the canvas is GPU-backed and the rendered result is displayed in the GLFW window.
3. **Given** a Vulkan-backed rendering session, **When** the window is resized, **Then** the GPU-backed surface is recreated at the new dimensions and rendering continues without interruption.

---

### User Story 2 - Automatic Fallback to Raster Pipeline (Priority: P2)

As a consumer application, I want the viewer to automatically fall back to the existing CPU raster rendering pipeline when Vulkan is not available, so that the viewer works in all environments regardless of GPU or driver availability.

**Why this priority**: Not all deployment environments have Vulkan drivers. Without fallback, the viewer would fail entirely in those environments. This ensures backward compatibility and broad environment support.

**Independent Test**: Can be tested by running the viewer on a system without Vulkan drivers installed (or with Vulkan initialization intentionally blocked) and verifying that the viewer falls back to the current raster+GL pipeline and renders correctly.

**Acceptance Scenarios**:

1. **Given** a system without Vulkan drivers, **When** the viewer starts, **Then** it falls back to the current CPU raster rendering pipeline and functions normally.
2. **Given** Vulkan initialization fails (e.g., no physical device with a graphics queue found), **When** the viewer starts, **Then** the fallback is automatic with no consumer code changes required.
3. **Given** the viewer falls back to the raster pipeline, **When** a consumer draws via the OnRender callback, **Then** the rendering output is identical to the current behavior.

---

### User Story 3 - Backend Selection Logging (Priority: P3)

As a developer deploying a consumer application, I want the viewer to log which rendering backend was selected at startup, so that I can diagnose rendering issues and confirm the expected backend is active.

**Why this priority**: Observability is important for troubleshooting, especially in containerized or headless environments where GPU availability may vary. This is lower priority than core rendering but important for supportability.

**Independent Test**: Can be tested by launching the viewer and checking log output for a message indicating which backend (Vulkan, GL raster, or pure raster) was selected.

**Acceptance Scenarios**:

1. **Given** the viewer starts with Vulkan available, **When** initialization completes, **Then** a log message indicates the Vulkan backend was selected.
2. **Given** the viewer starts and Vulkan is unavailable, **When** initialization falls back to raster, **Then** a log message indicates the raster fallback was selected and the reason for fallback.

---

### User Story 4 - Consumer Backend Preference (Priority: P3)

As a consumer application developer, I want to optionally specify a preferred rendering backend in the viewer configuration, so that I can force a specific backend for testing or compatibility purposes.

**Why this priority**: Optional configuration for advanced users. The default auto-detection behavior covers the primary use case; explicit selection is useful for debugging and specific deployment scenarios.

**Independent Test**: Can be tested by setting the preferred backend in ViewerConfig and verifying the viewer uses the specified backend (or falls back gracefully if the preferred backend is unavailable).

**Acceptance Scenarios**:

1. **Given** a consumer sets a preferred backend to Vulkan in ViewerConfig, **When** the viewer starts and Vulkan is available, **Then** the Vulkan backend is used.
2. **Given** a consumer sets a preferred backend to Vulkan but Vulkan is unavailable, **When** the viewer starts, **Then** it falls back to the next available backend and logs the fallback.
3. **Given** no backend preference is specified, **When** the viewer starts, **Then** it auto-detects the best available backend (Vulkan first, then GL raster).

---

### Edge Cases

- If the GPU device is lost or reset mid-session (e.g., driver crash), the viewer terminates with an error. The consumer application is responsible for restarting.
- How does the viewer handle environments where Vulkan reports a physical device but device creation fails (partial driver support)?
- What happens when the GLFW window is created before Vulkan initialization completes?
- How does the viewer handle multiple GPUs — does it select the first, or the one best suited for graphics?
- If the consumer's OnRender callback throws an exception during GPU-backed rendering, the exception propagates to the consumer. The viewer does not catch or handle it.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST create a Vulkan-backed GPU rendering context when a Vulkan-capable GPU and drivers are detected.
- **FR-002**: System MUST render all consumer SKCanvas drawing operations on the GPU when using the Vulkan backend, with no per-frame CPU-to-GPU pixel copy.
- **FR-003**: System MUST present the GPU-rendered frame to the GLFW window via the Vulkan swapchain.
- **FR-004**: System MUST fall back to the existing CPU raster rendering pipeline when Vulkan initialization fails or Vulkan is unavailable.
- **FR-005**: System MUST attempt rendering backends in priority order: Vulkan, then GL raster.
- **FR-006**: System MUST log which rendering backend was selected at startup, including the reason if a fallback occurred.
- **FR-007**: System MUST preserve the existing `OnRender` callback signature — no breaking changes to the consumer API.
- **FR-008**: System MUST support window resizing with the Vulkan backend, recreating the GPU-backed surface and swapchain at the new dimensions.
- **FR-009**: System MUST properly clean up all Vulkan resources (swapchain, surface, device, instance, rendering context) when the viewer window closes.
- **FR-010**: System SHOULD allow consumers to optionally specify a preferred rendering backend via ViewerConfig.
- **FR-011**: System MUST enable hardware multisampling (MSAA) by default at a reasonable sample count when available through the Vulkan backend. The MSAA sample count is not consumer-configurable.

### Key Entities

- **Rendering Backend**: The subsystem responsible for executing drawing operations and presenting frames. Has a type (Vulkan, GL Raster, Pure Raster) and lifecycle (initialization, per-frame rendering, cleanup).
- **ViewerConfig**: Consumer-facing configuration that controls viewer behavior. Extended with an optional backend preference field.
- **GPU Context**: The GPU-side state (instance, device, queue, rendering context) that enables hardware-accelerated rendering. Created during initialization and disposed during cleanup.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In Vulkan-capable environments, the viewer renders frames entirely on the GPU with zero per-frame CPU-to-GPU pixel transfers.
- **SC-002**: The Vulkan rendering path eliminates the per-frame `glTexImage2D` pixel upload, reducing CPU-GPU bandwidth usage. Improvement is architectural (no runtime benchmark required).
- **SC-003**: The viewer starts and renders correctly in environments without Vulkan, using the raster fallback, with no errors or crashes.
- **SC-004**: Hardware multisampling (MSAA) is available when using the Vulkan backend, improving visual quality of antialiased content.
- **SC-005**: Existing consumer applications work with a single-line migration (adding `PreferredBackend = None` to ViewerConfig). The OnRender callback signature and behavior are unchanged.
- **SC-006**: The viewer logs the selected backend at startup, enabling operators to confirm GPU acceleration is active within seconds of launch.

## Clarifications

### Session 2026-04-08

- Q: What should happen when the GPU device is lost or reset mid-session? → A: Terminate the viewer with an error; consumer must restart.
- Q: What should happen if the consumer's OnRender callback throws an exception during GPU-backed rendering? → A: Let the exception propagate to the consumer uncaught.
- Q: Should MSAA be enabled by default and is it consumer-configurable? → A: Enabled by default at a reasonable sample count; not configurable by consumer.

## Assumptions

- The host or container environment has appropriate Vulkan ICD drivers installed (e.g., vulkan-radeon, vulkan-intel, nvidia-utils) when GPU rendering is desired.
- GPU device nodes (/dev/dri/renderD128, /dev/dri/card*) are accessible to the viewer process when running in a container.
- The existing GLFW windowing integration supports Vulkan surface creation (GLFW has built-in Vulkan surface support).
- SkiaSharp 2.88.6 (already used by the project) provides a stable and functional Vulkan backend API.
- A new Vulkan bindings dependency will be added to the project.
- The current CPU raster + GL pipeline code remains available as-is for the fallback path.
- Multi-GPU selection defaults to the first device reporting a graphics-capable queue family; advanced GPU selection is out of scope for the initial implementation.
- The pure raster (headless/no-window) fallback path is out of scope for initial implementation if it does not already exist.

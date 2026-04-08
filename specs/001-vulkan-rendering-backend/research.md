# Research: Vulkan-Backed SkiaSharp Rendering

**Branch**: `001-vulkan-rendering-backend` | **Date**: 2026-04-08

## R1: Vulkan + SkiaSharp Integration via Silk.NET

**Decision**: Use Silk.NET.Vulkan 2.22.0 for Vulkan API bindings and bridge to SkiaSharp's `GRContext.CreateVulkan(GRVkBackendContext)`.

**Rationale**: The project already depends on Silk.NET 2.22.0 for windowing and OpenGL. Silk.NET.Vulkan is part of the same package family, ensuring version alignment and consistent interop patterns. The proposal includes a verified prototype demonstrating end-to-end success: Vulkan instance creation, device/queue setup, `GRVkBackendContext` construction with `vkGetProcAddr` bridging, and GPU-backed `SKSurface` creation.

**Alternatives considered**:
- **vk (raw P/Invoke)**: Lower-level, more manual marshaling, no ecosystem alignment with existing Silk.NET deps.
- **VulkanSharp**: Less actively maintained, different binding conventions than Silk.NET.

## R2: Vulkan Surface + Swapchain for GLFW Window Presentation

**Decision**: Use Silk.NET's GLFW Vulkan surface extensions (`VK_KHR_surface` + `VK_KHR_xlib_surface`) to create a Vulkan surface for the GLFW window, then manage a swapchain for frame presentation.

**Rationale**: GLFW has built-in Vulkan surface support. Silk.NET.Windowing exposes `IVkSurface` when Vulkan is the selected API. The swapchain manages double/triple buffering and presents rendered frames directly — no CPU pixel copy needed.

**Alternatives considered**:
- **Render to offscreen Vulkan surface, readback to GL texture**: Defeats the purpose; still involves a pixel copy.
- **Use a separate Vulkan window library**: Unnecessary — GLFW already supports Vulkan surfaces.

## R3: ViewerConfig Record Extension — Breaking Change Analysis

**Decision**: Add `PreferredBackend: Backend option` to `ViewerConfig`. This is a source-level breaking change (F# records require all fields at construction) but not a binary-level break.

**Rationale**: F# records cannot have optional construction syntax. Consumers must add `PreferredBackend = None` to their config literals. This is a minor, well-documented migration. The alternative (a separate config type or builder pattern) adds complexity disproportionate to the change. Since this is a 1.x → 2.0 feature change, a minor source-level break is acceptable.

**Migration guidance**: Add `PreferredBackend = None` to existing `ViewerConfig` record expressions. Behavior is identical to current (auto-detect → GL raster).

**Alternatives considered**:
- **Separate `ViewerConfig2` type**: Creates parallel types, confusing API.
- **Builder/fluent pattern**: Non-idiomatic F#, excessive for one optional field.
- **Function overload `Viewer.run` with backend param**: Doesn't compose well; config should be self-describing.

## R4: Backend Fallback Strategy and Initialization Order

**Decision**: The `run` function attempts backends in order: Vulkan → GL raster. Each attempt is wrapped in a try-with block. The selected backend is logged to stderr via `eprintfn` (consistent with existing viewer diagnostics).

**Rationale**: The proposal specifies this order (FR-005). Vulkan initialization is explicit and fails fast if drivers/devices are missing. The existing GL raster path is proven stable and remains as the fallback. The fallback is transparent to consumers — the OnRender callback receives an SKCanvas regardless of backend.

**Alternatives considered**:
- **Fail if preferred backend unavailable**: Too strict for a library; consumers may not control their deployment environment.
- **Runtime backend switching**: Excessive complexity; backends are initialized once at startup.

## R5: Vulkan Resource Lifecycle and Cleanup

**Decision**: Vulkan resources (instance, device, queue, swapchain, surface, GRContext) are created during the Load handler and destroyed during the Closing handler, mirroring the existing GL resource lifecycle.

**Rationale**: The existing `Viewer.fs` creates GL resources in `Load` and destroys them in `Closing`. The Vulkan path follows the same pattern for consistency. The GRContext must be disposed before the Vulkan device, and the device before the instance — standard Vulkan teardown order.

**Alternatives considered**:
- **Lazy Vulkan initialization on first render**: Adds latency to the first frame and complicates error handling.

## R6: MSAA Configuration

**Decision**: MSAA is enabled by default when using the Vulkan backend. The sample count is determined by querying `GRContext.GetMaxSurfaceSampleCount(SKColorType.Rgba8888)` and capping at 4x. Not consumer-configurable per clarification decision.

**Rationale**: The prototype confirmed 8x MSAA support. 4x is a good default balancing quality and performance. Higher counts can be explored later if needed.

**Alternatives considered**:
- **Max available MSAA**: May impact performance on lower-end GPUs.
- **No MSAA**: Misses a key benefit of GPU-backed rendering.

## R7: Internal Module Structure

**Decision**: Add a single new internal file `VulkanBackend.fs` containing all Vulkan-specific initialization, rendering, and cleanup logic. The existing GL code stays in `Viewer.fs`. The `run` function in `Viewer.fs` orchestrates backend selection.

**Rationale**: Separating Vulkan code into its own file keeps `Viewer.fs` manageable and makes the two code paths clearly distinct. The module is internal (no `.fsi` needed), keeping the public API surface unchanged except for the `Backend` type and `ViewerConfig` field.

**Alternatives considered**:
- **Everything in Viewer.fs**: Would make the file ~500+ lines with interleaved GL and Vulkan code.
- **Separate public modules per backend**: Over-engineering; consumers don't interact with backends directly.

## R8: Window API Selection for Vulkan

**Decision**: Use `WindowOptions` with `opts.API <- GraphicsAPI(ContextAPI.Vulkan, ...)` or set `opts.API <- GraphicsAPI.None` and manage the Vulkan surface independently via GLFW's Vulkan extensions.

**Rationale**: Silk.NET's GLFW windowing supports Vulkan natively. Setting `API = None` and creating the Vulkan surface manually gives full control over instance creation, which is required to bridge to SkiaSharp. GLFW provides `glfwCreateWindowSurface` for Vulkan surface creation from a window handle.

**Alternatives considered**:
- **Let Silk.NET manage Vulkan context**: Less control over instance/device creation, harder to bridge to SkiaSharp.

# Data Model: Vulkan-Backed SkiaSharp Rendering

**Branch**: `001-vulkan-rendering-backend` | **Date**: 2026-04-08

## Public Types

### Backend (new)

Discriminated union representing available rendering backends.

| Case | Description |
|------|-------------|
| `Vulkan` | GPU-backed rendering via Vulkan + SkiaSharp GRContext |
| `GL` | CPU raster rendering uploaded to OpenGL texture (current behavior) |
| `Raster` | CPU raster rendering without windowed display (headless/future) |

**Used by**: `ViewerConfig.PreferredBackend`

### ViewerConfig (modified)

Record type for viewer configuration. Existing fields unchanged; one field added.

| Field | Type | Status | Description |
|-------|------|--------|-------------|
| `Title` | `string` | Unchanged | Window title |
| `Width` | `int` | Unchanged | Initial width in pixels |
| `Height` | `int` | Unchanged | Initial height in pixels |
| `TargetFps` | `int` | Unchanged | Target frames per second |
| `ClearColor` | `SKColor` | Unchanged | Canvas background color |
| `OnRender` | `SKCanvas -> Vector2D<int> -> unit` | Unchanged | Per-frame render callback |
| `OnResize` | `int -> int -> unit` | Unchanged | Window resize callback |
| `OnKeyDown` | `Key -> unit` | Unchanged | Keyboard input callback |
| `OnMouseScroll` | `float32 -> float32 -> float32 -> unit` | Unchanged | Scroll wheel callback |
| `OnMouseDrag` | `float32 -> float32 -> unit` | Unchanged | Mouse drag callback |
| `PreferredBackend` | `Backend option` | **New** | Optional backend preference. `None` = auto-detect (Vulkan → GL fallback) |

**Breaking change**: Source-level only. Existing consumers must add `PreferredBackend = None` to config record expressions.

## Internal Types (not public API)

### VulkanState

Internal mutable record holding Vulkan resource handles during the viewer lifecycle.

| Field | Type | Description |
|-------|------|-------------|
| Instance | Vulkan instance handle | Root Vulkan object |
| PhysicalDevice | Physical device handle | Selected GPU |
| Device | Logical device handle | Application's view of the GPU |
| Queue | Queue handle | Graphics command queue |
| GraphicsQueueIndex | uint32 | Queue family index |
| GRContext | GRContext | SkiaSharp's Vulkan rendering context |
| Surface | VkSurfaceKHR | Window surface for presentation |
| Swapchain | Swapchain state | Frame buffering and presentation |

**Lifecycle**: Created in Load handler → used per-frame in Render handler → destroyed in Closing handler.

### ActiveBackend

Internal discriminated union tracking which backend was successfully initialized.

| Case | Payload | Description |
|------|---------|-------------|
| `VulkanActive` | VulkanState | Vulkan initialized and active |
| `GlRasterActive` | GL state (existing mutables) | GL raster path active |

**Used by**: `run` function to dispatch render/cleanup logic to the correct backend.

## State Transitions

```
Startup
  ├─ Try Vulkan init ─── Success ──→ VulkanActive
  │                  └── Failure (log reason) ──→ Try GL
  └─ Try GL init ─────── Success ──→ GlRasterActive
                     └── Failure ──→ Error (no viable backend)

Running (VulkanActive)
  ├─ OnRender ──→ GPU-backed canvas draw → grContext.Flush() → swapchain present
  ├─ Resize ───→ Recreate swapchain + GPU surface
  ├─ Device lost ──→ Terminate with error
  └─ Closing ──→ Dispose GRContext → destroy swapchain → destroy surface → destroy device → destroy instance

Running (GlRasterActive)
  ├─ OnRender ──→ CPU raster draw → glTexImage2D upload → fullscreen quad
  ├─ Resize ───→ Recreate raster surface
  └─ Closing ──→ Dispose surface → delete GL objects
```

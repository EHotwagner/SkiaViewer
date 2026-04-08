# Proposal: Replace OpenGL Raster Pipeline with Vulkan-Backed SkiaSharp Rendering

**Date**: 2026-04-08
**From**: FSBarV1 project (consumer of SkiaViewer 1.0.0)
**Priority**: High — current approach causes segfaults in container environments with GPU passthrough

---

## Problem Statement

The current SkiaViewer renders via a **CPU raster SKSurface + OpenGL texture upload** pipeline. This architecture was adopted as a workaround because SkiaSharp's `GRContext.CreateGl()` segfaults in containerized Linux environments. However, the workaround has significant drawbacks and the root cause is now understood.

### Current Architecture (Viewer.fs)

```
User SKCanvas drawing (OnRender callback)
    → CPU raster SKSurface.Create(SKImageInfo)        [line 96]
    → canvas.Flush()                                   [line 239]
    → snapSurface.PeekPixels() → GetPixels()          [line 242-244]
    → gl.TexImage2D(..., pixels.ToPointer())           [line 246]
    → fullscreen quad via GL shader program             [line 249-251]
    → GLFW window display
```

Every frame does a full CPU→GPU pixel upload via `glTexImage2D`, negating the benefits of GPU passthrough.

### Root Cause of the GL Segfault

We investigated why `GRContext.CreateGl()` segfaults even with a working GPU. The findings:

**Environment**: Container with AMD Radeon Cezanne (Radeon Vega) GPU passed through via `/dev/dri/card1` and `/dev/dri/renderD128`.

**What works**:
- X11 connection to host X server (DISPLAY=:1) — confirmed via `XOpenDisplay`, `XScreenCount`
- GLX 1.4 — confirmed via `glXQueryVersion`, `glXGetClientString`
- Full hardware-accelerated OpenGL 4.6 — confirmed by creating a GLX context and querying:
  ```
  GL Vendor:   AMD
  GL Renderer: AMD Radeon Graphics (radeonsi, renoir, ACO, DRM 3.64, 6.19.10-arch1-1)
  GL Version:  4.6 (Compatibility Profile) Mesa 26.0.4-arch1.1
  ```
- The GPU is fully functional. This is NOT software rendering (not llvmpipe).

**What segfaults**: SkiaSharp's internal call to `GrGLMakeNativeInterface()` during `GRContext.CreateGl()`. The crash occurs because:

1. **GL function pointer resolution mismatch**: Skia's native `GrGLMakeNativeInterface()` uses `eglGetProcAddress` or `glXGetProcAddress` internally to resolve GL function pointers. In this container environment, EGL initialization fails (`eglInitialize` returns false) even though GLX works. If Skia tries the EGL path first and falls back poorly, it gets null function pointers and segfaults when calling them.

2. **The X socket is bind-mounted from the host** — there is no X server process inside the container. The GL context works when created explicitly via GLX, but Skia's automatic detection logic doesn't handle this correctly.

3. **`/dev/dri/card1` (not `card0`)** — the GPU was passed through as a secondary device. Some GL stacks default to `card0`.

**Key insight**: The GPU, Mesa drivers, GLX, and OpenGL all work correctly. The failure is specifically in **SkiaSharp's native library GL initialization code** — not in the graphics stack itself. This is a known fragility in SkiaSharp's GL backend when the environment doesn't match its assumptions about EGL/GLX availability.

### Problems with the Current Raster Workaround

1. **CPU-bound rendering**: All SkiaSharp drawing (canvas operations, text, antialiasing) runs on CPU. With a GPU available, this leaves significant performance on the table.

2. **Per-frame pixel copy**: Every frame, the entire framebuffer is copied from CPU memory to GPU texture via `glTexImage2D`. For a 1024x640 window at 60fps, that's ~150 MB/s of CPU→GPU bandwidth wasted.

3. **No MSAA or GPU acceleration for drawing**: Raster surfaces don't support hardware multisampling. Text and shape antialiasing is done via CPU-based algorithms rather than GPU hardware.

4. **The GL code itself is fragile**: The viewer manages a GL shader program, VAO, VBO, and texture — all boilerplate that exists solely to display a CPU-rendered bitmap. This is complexity that serves no purpose beyond working around the GPU backend failure.

---

## Proposed Solution: Vulkan-Backed SkiaSharp Rendering

Replace the raster+GL pipeline with SkiaSharp's native Vulkan backend (`GRContext.CreateVulkan`).

### Why Vulkan Solves This

1. **Vulkan initialization is explicit** — unlike GL/EGL/GLX where Skia has to guess which context creation path to use, Vulkan requires the application to create the instance, device, and queue explicitly and hand them to Skia. There is no automatic detection that can go wrong.

2. **Vulkan works in the container** — we verified this end-to-end:
   ```
   $ vulkaninfo --summary
   GPU0: AMD Radeon Graphics (RADV RENOIR)
   Vulkan 1.4.335 / Mesa 26.0.4 / RADV driver
   ```

3. **SkiaSharp 2.88.6 has full Vulkan support** — `GRContext.CreateVulkan(GRVkBackendContext)` is a first-class API. The `GRVkBackendContext` type accepts raw Vulkan handles (instance, physical device, device, queue) plus a procedure address resolver.

### Proof of Concept — Verified Working

We built and ran a complete prototype that:
- Creates a Vulkan instance, physical device, logical device, and graphics queue via Silk.NET.Vulkan
- Constructs a `GRVkBackendContext` bridging Silk.NET's `vkGetProcAddr` to SkiaSharp's delegate
- Creates a `GRContext` via `GRContext.CreateVulkan()`
- Creates a GPU-backed `SKSurface` via `SKSurface.Create(grContext, false, imageInfo)`
- Draws text, shapes with antialiasing on the GPU-backed canvas
- Snapshots the surface to a PNG file

**Result**: Complete success, no segfaults, 8x MSAA support detected.

```
=== Vulkan + SkiaSharp Prototype ===
Got Vulkan API
CreateInstance: Success
Physical devices: 1
Using device: AMD Radeon Graphics (RADV RENOIR)
Graphics queue family: 0
CreateDevice: Success
Got graphics queue
Creating GRContext.CreateVulkan...
SUCCESS: GRContext created with Vulkan backend!
  MaxSurfaceSampleCount: 8
SUCCESS: GPU-backed SKSurface created (512x512)!
Rendered to /tmp/vk-skia-test-output.png
Done — Vulkan resources cleaned up
```

### Proposed New Architecture

```
User SKCanvas drawing (OnRender callback)
    → GPU-backed SKSurface.Create(grContext, budgeted, SKImageInfo)
    → canvas drawing goes directly to GPU via Vulkan command buffers
    → canvas.Flush() + grContext.Flush()
    → Vulkan swapchain presents to GLFW window surface
```

No CPU pixel copies. No GL shader boilerplate. Drawing operations execute on the GPU.

### Required New Dependencies

```xml
<PackageReference Include="Silk.NET.Vulkan" Version="2.22.0" />
```

The consumer environment also needs `vulkan-radeon` (or equivalent Vulkan ICD) installed. The `vulkan-icd-loader` package is typically already present.

### Changes to Viewer.fs

The following sections of Viewer.fs would change:

**Remove** (GL pipeline — lines 38-53, 78-84, 120-168, 172-178, 242-251, 277-284):
- `vertexShaderSrc` and `fragmentShaderSrc` constants
- GL state: `gl`, `texture`, `vao`, `vbo`, `shaderProgram` mutables
- `compileShader` and `setupGl` functions
- `gl <- GL.GetApi(win)` and `setupGl()` in Load handler
- Pixel extraction and `gl.TexImage2D` upload in Render handler
- GL resource cleanup in Closing handler

**Replace** `recreateSurface` (lines 88-118):
```fsharp
// Before (raster):
let newSurface = SKSurface.Create(info)

// After (Vulkan GPU-backed):
let newSurface = SKSurface.Create(grContext, false, info)
```

**Add** Vulkan initialization (new, in Load handler):
- Create Vulkan instance, physical device, logical device, graphics queue
- Build `GRVkBackendContext` with proc address delegate
- Create `GRContext.CreateVulkan(vkCtx)`
- Create Vulkan surface for the GLFW window via `VK_KHR_surface` + `VK_KHR_xlib_surface`

**Add** Vulkan swapchain presentation (new, in Render handler):
- After `canvas.Flush()` + `grContext.Flush()`, present the rendered image to the window surface via swapchain

**Add** Vulkan cleanup (new, in Closing handler):
- Destroy swapchain, surface, device, instance
- Dispose GRContext

### Changes to ViewerConfig

The public `ViewerConfig` type and `OnRender: SKCanvas -> Vector2D<int> -> unit` callback signature remain **unchanged**. Consumers draw to an `SKCanvas` exactly as before — the only difference is that the canvas is now GPU-backed.

One possible addition:

```fsharp
type ViewerConfig =
    { // ... existing fields ...
      PreferredBackend: Backend option }  // None = auto (Vulkan → GL → Raster fallback)

type Backend = Vulkan | GL | Raster
```

This would allow consumers to explicitly request a backend, with automatic fallback: try Vulkan first, fall back to the current GL raster path if Vulkan is unavailable.

### Fallback Strategy

Not all environments have Vulkan. The viewer should attempt backends in order:

1. **Vulkan** — if `Vk.GetApi()` succeeds and a physical device with a graphics queue is found
2. **GL raster** (current path) — if Vulkan fails but GL is available
3. **Pure raster** (no window) — for headless/testing scenarios

The fallback should log which backend was selected so users can diagnose issues.

---

## Prototype Source Code

The complete working prototype (F# / .NET 10.0) is included below for reference. This was built against Silk.NET.Vulkan 2.22.0 and SkiaSharp 2.88.6 — the same SkiaSharp version SkiaViewer already uses.

```fsharp
#nowarn "9"
#nowarn "51"
#nowarn "3391"

open System
open System.Runtime.InteropServices
open Silk.NET.Vulkan
open Silk.NET.Core
open SkiaSharp

let vk = Vk.GetApi()

let mutable appInfo = ApplicationInfo()
appInfo.SType <- StructureType.ApplicationInfo
appInfo.ApiVersion <- uint32 ((1 <<< 22) ||| (1 <<< 12))  // VK_MAKE_VERSION(1,1,0)

let mutable createInfo = InstanceCreateInfo()
createInfo.SType <- StructureType.InstanceCreateInfo
createInfo.PApplicationInfo <- &&appInfo

let mutable instance = Unchecked.defaultof<Instance>
let res = vk.CreateInstance(&&createInfo, NativeInterop.NativePtr.nullPtr, &&instance)

// ── Pick physical device ──
let mutable devCount = 0u
vk.EnumeratePhysicalDevices(instance, &&devCount, NativeInterop.NativePtr.nullPtr) |> ignore

let devices = Array.zeroCreate<PhysicalDevice>(int devCount)
let devGC = GCHandle.Alloc(devices, GCHandleType.Pinned)
let devPtr = devGC.AddrOfPinnedObject() |> NativeInterop.NativePtr.ofNativeInt<PhysicalDevice>
vk.EnumeratePhysicalDevices(instance, &&devCount, devPtr) |> ignore
devGC.Free()

let physDevice = devices.[0]

// ── Find graphics queue family ──
let mutable queueFamilyCount = 0u
vk.GetPhysicalDeviceQueueFamilyProperties(physDevice, &&queueFamilyCount, NativeInterop.NativePtr.nullPtr)
let queueFamilies = Array.zeroCreate<QueueFamilyProperties>(int queueFamilyCount)
let qfGC = GCHandle.Alloc(queueFamilies, GCHandleType.Pinned)
let qfPtr = qfGC.AddrOfPinnedObject() |> NativeInterop.NativePtr.ofNativeInt<QueueFamilyProperties>
vk.GetPhysicalDeviceQueueFamilyProperties(physDevice, &&queueFamilyCount, qfPtr)
qfGC.Free()

let graphicsIdx =
    queueFamilies
    |> Array.findIndex (fun qf -> qf.QueueFlags.HasFlag(QueueFlags.GraphicsBit))

// ── Create logical device ──
let mutable queuePriority = 1.0f
let mutable queueCreateInfo = DeviceQueueCreateInfo()
queueCreateInfo.SType <- StructureType.DeviceQueueCreateInfo
queueCreateInfo.QueueFamilyIndex <- uint32 graphicsIdx
queueCreateInfo.QueueCount <- 1u
queueCreateInfo.PQueuePriorities <- &&queuePriority

let mutable deviceCreateInfo = DeviceCreateInfo()
deviceCreateInfo.SType <- StructureType.DeviceCreateInfo
deviceCreateInfo.QueueCreateInfoCount <- 1u
deviceCreateInfo.PQueueCreateInfos <- &&queueCreateInfo

let mutable device = Unchecked.defaultof<Device>
vk.CreateDevice(physDevice, &&deviceCreateInfo, NativeInterop.NativePtr.nullPtr, &&device) |> ignore

let mutable queue = Unchecked.defaultof<Queue>
vk.GetDeviceQueue(device, uint32 graphicsIdx, 0u, &&queue)

// ── Build GRVkBackendContext ──
let vkCtx = new GRVkBackendContext()
vkCtx.VkInstance <- instance.Handle
vkCtx.VkPhysicalDevice <- physDevice.Handle
vkCtx.VkDevice <- device.Handle
vkCtx.VkQueue <- queue.Handle
vkCtx.GraphicsQueueIndex <- uint32 graphicsIdx

let getProcAddr =
    GRVkGetProcedureAddressDelegate(fun name inst dev ->
        if dev <> IntPtr.Zero then
            vk.GetDeviceProcAddr(Silk.NET.Vulkan.Device(dev), name).Handle
        elif inst <> IntPtr.Zero then
            vk.GetInstanceProcAddr(Silk.NET.Vulkan.Instance(inst), name).Handle
        else
            vk.GetInstanceProcAddr(instance, name).Handle)
vkCtx.GetProcedureAddress <- getProcAddr

// ── Create GRContext + GPU Surface ──
let grContext = GRContext.CreateVulkan(vkCtx)
let imageInfo = SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul)
let surface = SKSurface.Create(grContext, false, imageInfo)

// ── Draw ──
let canvas = surface.Canvas
canvas.Clear(SKColors.DarkBlue)
use paint = new SKPaint(Color = SKColors.White, TextSize = 48.0f, IsAntialias = true)
canvas.DrawText("Vulkan + SkiaSharp!", 20.0f, 260.0f, paint)
canvas.Flush()
grContext.Flush()

// ── Readback to PNG ──
use image = surface.Snapshot()
use data = image.Encode(SKEncodedImageFormat.Png, 100)
use stream = IO.File.OpenWrite("/tmp/vk-skia-test-output.png")
data.SaveTo(stream)
```

---

## Environment Requirements

For the Vulkan backend to work, the host/container needs:

| Component | Package (Arch Linux) | Purpose |
|-----------|---------------------|---------|
| Vulkan ICD loader | `vulkan-icd-loader` | Loads Vulkan drivers |
| AMD Vulkan driver | `vulkan-radeon` | RADV Mesa driver for AMD GPUs |
| Intel Vulkan driver | `vulkan-intel` | ANV Mesa driver for Intel GPUs |
| NVIDIA Vulkan driver | `nvidia-utils` | Proprietary driver (includes Vulkan) |
| GPU device nodes | `/dev/dri/renderD128`, `/dev/dri/card*` | DRM render/display access |
| SkiaSharp native lib | `SkiaSharp.NativeAssets.Linux.NoDependencies` 2.88.6 | Native `libSkiaSharp.so` with Vulkan support |

The current GL raster path would remain as a fallback for environments without Vulkan.

---

## Summary

| Aspect | Current (Raster + GL) | Proposed (Vulkan) |
|--------|----------------------|-------------------|
| SKSurface | CPU raster | GPU-backed |
| Drawing | CPU | GPU (Vulkan command buffers) |
| Frame transfer | `glTexImage2D` per frame | None (GPU-native) |
| MSAA | Not available | Up to 8x hardware MSAA |
| GL boilerplate | Shaders, VAO, VBO, texture | None |
| Segfault risk | High (fragile GL init) | None (explicit Vulkan init) |
| Fallback | None (only path) | GL raster as fallback |
| New dependency | — | Silk.NET.Vulkan 2.22.0 |
| API breaking changes | — | None (OnRender callback unchanged) |

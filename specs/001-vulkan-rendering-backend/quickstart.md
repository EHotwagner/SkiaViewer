# Quickstart: Vulkan-Backed SkiaSharp Rendering

**Branch**: `001-vulkan-rendering-backend` | **Date**: 2026-04-08

## Prerequisites

- .NET 10.0 SDK
- Vulkan ICD drivers for your GPU:
  - AMD: `vulkan-radeon`
  - Intel: `vulkan-intel`
  - NVIDIA: `nvidia-utils`
- GPU device nodes accessible: `/dev/dri/renderD128`, `/dev/dri/card*`
- Verify with: `vulkaninfo --summary`

## Existing Consumer Migration

Add `PreferredBackend = None` to your `ViewerConfig` record:

```fsharp
let config =
    { Title = "My Viewer"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.CornflowerBlue
      OnRender = fun canvas fbSize ->
          use paint = new SKPaint(Color = SKColors.White, TextSize = 24.0f)
          canvas.DrawText("Hello!", 10.0f, 40.0f, paint)
      OnResize = fun w h -> ()
      OnKeyDown = fun key -> ()
      OnMouseScroll = fun delta x y -> ()
      OnMouseDrag = fun dx dy -> ()
      PreferredBackend = None }  // Auto-detect: Vulkan → GL fallback

use viewer = Viewer.run config
```

With `PreferredBackend = None`, the viewer automatically tries Vulkan first. If Vulkan is unavailable, it falls back to the existing GL raster pipeline. No other code changes needed.

## Forcing a Specific Backend

```fsharp
// Force Vulkan (error if unavailable):
PreferredBackend = Some Backend.Vulkan

// Force GL raster (skip Vulkan):
PreferredBackend = Some Backend.GL
```

## Verifying the Active Backend

Check stderr output at startup:

```
[Viewer] Backend selected: Vulkan (AMD Radeon Graphics)
```

or

```
[Viewer] Vulkan initialization failed: No physical devices found
[Viewer] Backend selected: GL raster (fallback)
```

## Build

```bash
dotnet build src/SkiaViewer/SkiaViewer.fsproj
```

## Test

```bash
dotnet test tests/SkiaViewer.Tests/
```

Tests should pass in both Vulkan and non-Vulkan environments (fallback path exercised automatically).

## Container Deployment

Ensure these are available inside the container:
- `/dev/dri/renderD128` (GPU render node)
- `/dev/dri/card*` (GPU card node)
- Vulkan ICD loader and driver packages
- X11 socket or Wayland display for windowed mode

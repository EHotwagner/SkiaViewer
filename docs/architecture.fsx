(**
---
title: Architecture Overview
category: Design
categoryindex: 4
index: 1
description: Rendering pipeline, threading model, and component relationships.
---
*)

(**
# Architecture Overview

SkiaViewer is a thin integration layer that bridges **SkiaSharp** raster rendering with
**Silk.NET** OpenGL windowing. It runs a GLFW-backed window on a dedicated background thread,
renders 2D content to a CPU-side SkiaSharp surface, then uploads the pixel data as an OpenGL
texture each frame.

## Component Diagram

```
┌──────────────────────────────────────────────────────┐
│                   User Application                    │
│                                                      │
│  ViewerConfig { OnRender, OnKeyDown, OnMouseScroll } │
└──────────────────┬───────────────────────────────────┘
                   │ Viewer.run config
                   ▼
┌──────────────────────────────────────────────────────┐
│              Background Window Thread                 │
│                                                      │
│  ┌─────────────┐    ┌──────────────────────────┐     │
│  │  Silk.NET    │    │  SkiaSharp Raster Surface │     │
│  │  GLFW Window │    │  (SKSurface + SKCanvas)   │     │
│  └──────┬──────┘    └────────────┬─────────────┘     │
│         │                        │                    │
│         │  OpenGL Context        │  Pixel Data        │
│         ▼                        ▼                    │
│  ┌──────────────────────────────────────────────┐    │
│  │          OpenGL Texture Upload                │    │
│  │  glTexImage2D(RGBA8, pixels)                  │    │
│  └──────────────────────┬───────────────────────┘    │
│                         │                             │
│                         ▼                             │
│  ┌──────────────────────────────────────────────┐    │
│  │       Fullscreen Quad (VAO + Shader)          │    │
│  │  Vertex Shader → Fragment Shader → Display    │    │
│  └──────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────┘
```

## Threading Model

SkiaViewer uses a single dedicated background thread for the entire window lifecycle:
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

(**
1. **`Viewer.run`** spawns a background thread that:
   - Registers the GLFW platform (once, via a `lazy` value)
   - Creates a `Silk.NET.Windowing.IWindow`
   - Sets up the OpenGL context (shaders, VAO, VBO, texture)
   - Creates the SkiaSharp raster surface
   - Enters the GLFW event loop (`win.Run()`)

2. **The render loop** executes each frame on the window thread:
   - Clears the SkiaSharp canvas with `ClearColor`
   - Calls `OnRender` with the canvas and framebuffer size
   - Flushes the canvas
   - Reads pixel data via `SKSurface.PeekPixels()`
   - Uploads to the OpenGL texture via `glTexImage2D`
   - Draws a fullscreen quad with the texture

3. **Cross-thread shutdown** is achieved through a `shutdownRequested` flag.
   The window's `Update` event checks this flag and calls `win.Close()` when set.
   A `ManualResetEventSlim` signals the caller when the thread exits.

## Surface Management

The SkiaSharp `SKSurface` is protected by a `surfaceLock` object. This is necessary because:

- The surface is created/destroyed on the window thread (during load and resize)
- The render callback reads from the surface on the window thread
- Shutdown can be requested from any thread

On resize, the viewer:
1. Creates a new `SKSurface` matching the framebuffer size
2. Swaps it in under the lock
3. Disposes the old surface

<details>
<summary>What happens when the window is minimized?</summary>

When the framebuffer size is zero (minimized window), the surface is set to null under the lock.
The render callback checks for null and skips rendering, preventing zero-size surface allocation.
</details>

## OpenGL Pipeline

The OpenGL setup is minimal — just enough to display a textured quad:

| Resource | Purpose |
|---|---|
| Vertex Shader | Pass-through: maps NDC positions and texture coordinates |
| Fragment Shader | Samples the texture at interpolated UV coordinates |
| VAO + VBO | Fullscreen quad (2 triangles, 6 vertices) |
| Texture | RGBA8 texture updated each frame with SkiaSharp pixels |

The quad vertices cover the full normalized device coordinate range (-1 to 1) with texture
coordinates flipped vertically (0,1 at bottom-left to 1,0 at top-right) to match SkiaSharp's
top-left origin.

## Exception Recovery

Frame-level exceptions in the `OnRender` callback are caught and logged to stderr.
This prevents a single bad frame from crashing the window. The following exception types
are handled explicitly:

- `ObjectDisposedException` — surface disposed during render
- `NullReferenceException` — surface became null mid-render
- `ArgumentNullException` — null argument in SkiaSharp call
- All other exceptions — caught as a general fallback

## Dependencies

| Package | Version | Role |
|---|---|---|
| `Silk.NET.Windowing` | 2.22.0 | GLFW window creation and event loop |
| `Silk.NET.OpenGL` | 2.22.0 | OpenGL API bindings |
| `Silk.NET.Input` | 2.22.0 | Keyboard and mouse input |
| `SkiaSharp` | 2.88.6 | 2D raster rendering (CPU) |

<div class="alert alert-info">
<strong>Design choice:</strong> Raster rendering (CPU-side SkiaSharp) was chosen over
GPU-accelerated SkiaSharp (<code>GRContext</code>) to keep the OpenGL interop simple and
avoid shared GL context complexities. The texture upload path is efficient for typical
2D visualization workloads.
</div>

## Next Steps

- [Getting Started](getting-started.html) — create your first viewer
- [API Reference](reference/index.html) — full type and function documentation
- [Test Suite](tests.html) — see how the viewer is tested
*)

(**
---
title: Architecture Overview
category: Design
categoryindex: 4
index: 1
description: Components, threading model, rendering pipeline, and design decisions.
---
*)

(**
# Architecture Overview

SkiaViewer is a .NET 10.0 library that renders declarative 2D scenes in a GLFW window
using SkiaSharp and Silk.NET. It supports both Vulkan GPU-accelerated and OpenGL CPU-raster
rendering backends.

## Component Diagram

```
┌─────────────────────────────────────────────────────────┐
│ User Application                                        │
│                                                         │
│   IObservable<Scene> ──────┐    ┌── IObservable<Input>  │
│                            │    │                       │
└────────────────────────────┼────┼───────────────────────┘
                             │    │
                             ▼    │
┌────────────────────────────────────────────────────────┐
│ Viewer.run (background thread)                         │
│                                                        │
│   ┌──────────────┐    ┌───────────────┐                │
│   │ Scene Stream │───▶│ SceneRenderer │                │
│   │ Subscription │    │ (canvas draw) │                │
│   └──────────────┘    └───────┬───────┘                │
│                               │                        │
│                               ▼                        │
│   ┌───────────────────────────────────────────────┐    │
│   │           Surface (SKSurface)                 │    │
│   │  ┌─────────────────┬─────────────────────┐    │    │
│   │  │ VulkanBackend   │ GL Raster Fallback  │    │    │
│   │  │ (GPU SKSurface) │ (CPU SKSurface)     │    │    │
│   │  └─────────────────┴─────────────────────┘    │    │
│   └───────────────────────┬───────────────────────┘    │
│                           │                            │
│                           ▼                            │
│   ┌───────────────────────────────────────────────┐    │
│   │          OpenGL Fullscreen Quad               │    │
│   │  Vertex shader ──▶ Fragment shader ──▶ Swap   │    │
│   └───────────────────────────────────────────────┘    │
│                                                        │
│   ┌──────────────┐                                     │
│   │ Input Wiring │ ── keyboard, mouse, resize ────────▶│
│   └──────────────┘                                     │
└────────────────────────────────────────────────────────┘
```

## Source Files

| File | Visibility | Responsibility |
|---|---|---|
| `Scene.fsi` / `Scene.fs` | Public | Declarative scene DSL types and helper functions |
| `SceneRenderer.fsi` / `SceneRenderer.fs` | Internal | Renders a `Scene` to an `SKCanvas` |
| `VulkanBackend.fs` | Internal | Vulkan device/queue/GRContext initialization |
| `Viewer.fsi` / `Viewer.fs` | Public | Window lifecycle, OpenGL pipeline, input wiring |

## Threading Model
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaSharp.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

(**
`Viewer.run` spawns a dedicated background thread for the window. The calling thread
gets back a `ViewerHandle` immediately and can continue with other work.

```
Main Thread                  Window Thread
    │                             │
    ├── Viewer.run ──────────────▶├── GLFW register (lazy, once)
    │   returns immediately       ├── Window.Create()
    │                             ├── GL context setup
    │                             ├── Shader compile + link
    │                             ├── VAO/VBO fullscreen quad
    │                             ├── Surface creation
    │                             ├── Input wiring
    │                             └── Render loop:
    │                                  ├── Subscribe to scene stream
    │                                  ├── Clear canvas
    │                                  ├── SceneRenderer.render
    │                                  ├── GPU flush / texture upload
    │                                  ├── Draw quad
    │                                  └── SwapBuffers
    │
    ├── sceneEvent.Trigger(scene)  (thread-safe scene push)
    │
    ├── viewer.Screenshot(...)     (thread-safe capture)
    │
    └── viewer.Dispose()          ─▶ shutdownRequested flag
                                     ManualResetEventSlim wait
                                     (5-second timeout)
```

**Key thread-safety mechanisms:**

- `surfaceLock` — a `lock` object protecting SKSurface access during screenshots
  and rendering
- `sceneLock` — protects the mutable scene reference updated by the observable subscription
- `shutdownRequested` — a mutable boolean flag checked each frame for cross-thread shutdown
- `ManualResetEventSlim` — used by `Dispose()` to wait for the window thread to complete

## Rendering Pipeline

### Backend Selection

On startup, the viewer attempts to initialize a Vulkan GPU backend via `VulkanBackend.tryInit()`.
If Vulkan is available, SkiaSharp creates a GPU-backed `SKSurface` through a `GRContext`.
If Vulkan initialization fails (no driver, unsupported hardware), it falls back to CPU
raster rendering.

The backend selection is logged to stderr:

```
Backend selected: Vulkan (NVIDIA GeForce RTX 4090), MSAA: 4x
```

or

```
Backend selected: GL (CPU raster)
```

### Frame Rendering

Each frame follows this sequence:

1. **Scene acquisition** — read the latest `Scene` from the mutable reference (under lock)
2. **Canvas clear** — `canvas.Clear(scene.BackgroundColor)`
3. **Scene rendering** — `SceneRenderer.render` walks the element tree depth-first:
   - Converts DSL `Paint` to `SKPaint` (fill and/or stroke)
   - Applies transforms via `canvas.Save()`/`canvas.Restore()`
   - Applies clips to groups
   - Draws each element type with the appropriate `canvas.Draw*` method
4. **GPU flush** — `GRContext.Flush()` for Vulkan, or `canvas.Flush()` for GL raster
5. **Texture upload** — pixel data is uploaded to an OpenGL RGBA8 texture
6. **Quad render** — a fullscreen quad is drawn with the texture via vertex/fragment shaders
7. **Buffer swap** — `window.SwapBuffers()`

### OpenGL Pipeline

The GL pipeline uses a minimal vertex/fragment shader pair to render a fullscreen quad:

- **Vertex shader** — passes through quad vertices and flips UV coordinates vertically
- **Fragment shader** — samples the RGBA8 texture
- **VAO/VBO** — a single quad covering the viewport
- **Texture** — one RGBA8 texture, re-uploaded each frame with the current surface pixels

## Exception Recovery

The render loop wraps frame rendering in a `try/with` block that catches and
recovers from:

| Exception | Recovery |
|---|---|
| `ObjectDisposedException` | Skip frame (resource was cleaned up during shutdown) |
| `NullReferenceException` | Skip frame (surface not yet created or already disposed) |
| `ArgumentNullException` | Skip frame (transient null during initialization) |

All other exceptions propagate and terminate the window thread.

## VulkanBackend Internals

The `VulkanBackend` module handles Vulkan initialization:

1. **Instance creation** — creates a Vulkan instance
2. **Physical device enumeration** — selects the first available GPU
3. **Queue family selection** — finds a graphics-capable queue family
4. **Logical device creation** — creates a device with the selected queue
5. **GRContext creation** — wraps the Vulkan device in a SkiaSharp `GRContext`
6. **MSAA** — attempts 4x MSAA, falls back to no MSAA if unsupported

The `ActiveBackend` discriminated union tracks whether Vulkan or GL raster is active:

```
VulkanActive of State    — Vulkan device, queue, GRContext, MSAA config
GlRasterActive           — CPU raster fallback
```

## Design Decisions

### Declarative Scene Model

Scenes are immutable data structures rather than imperative draw commands. This enables:

- Thread-safe scene updates (push a new `Scene` value, no shared mutable canvas)
- Scene serialization and diffing (future optimization potential)
- Testability — `SceneRenderer` can render to any `SKCanvas`, including offscreen bitmaps

### IObservable Streams

Using `IObservable<Scene>` and `IObservable<InputEvent>` rather than callbacks:

- Decouples the viewer from application logic
- Composes naturally with Rx operators
- Allows multiple subscribers
- Error handling via `OnError` (viewer preserves last valid scene)

### Background Thread

Running the window on a background thread rather than the main thread:

- `Viewer.run` returns immediately — caller decides when to block
- Multiple viewers could theoretically run concurrently (GLFW limits this to one)
- Integrates naturally into async/reactive applications

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| `SkiaSharp` | 2.88.6 | 2D rendering engine |
| `SkiaSharp.NativeAssets.Linux.NoDependencies` | 2.88.6 | Linux native binaries |
| `Silk.NET.Windowing` | 2.22.0 | Cross-platform windowing (GLFW) |
| `Silk.NET.OpenGL` | 2.22.0 | OpenGL bindings for texture pipeline |
| `Silk.NET.Input` | 2.22.0 | Keyboard and mouse input |
| `Silk.NET.Vulkan` | 2.22.0 | Vulkan bindings for GPU backend |

## Next Steps

- [API Reference](reference/index.html) — auto-generated API documentation
- [Known Issues](known-issues.html) — current limitations
- [Test Suite Documentation](tests.html) — comprehensive test coverage
*)

(**
---
title: Known Issues & Limitations
category: Reference
categoryindex: 5
index: 7
description: Current bugs, limitations, and not-yet-implemented features.
---
*)

(**
# Known Issues & Limitations

This document tracks known limitations, compiler warnings, and design constraints
in SkiaViewer.

## Compiler Warnings

| Warning | Location | Description |
|---|---|---|
| FS0009 | `Viewer.fs:150-152` | Uses of `NativeInterop.NativePtr` for OpenGL vertex attribute pointers generate "unverifiable .NET IL" warnings. These are expected and safe — they are required for setting up the OpenGL VAO with correct byte offsets. |

## Design Limitations

### Single Window Only

`Viewer.run` creates a single window per call. GLFW requires window operations
on the thread that created the window, so each call to `Viewer.run` creates a new
background thread. Running multiple viewers simultaneously is untested and may
conflict due to shared GLFW global state.

### CPU Raster Rendering

SkiaViewer uses CPU-side SkiaSharp rendering (`SKSurface.Create` with `SKImageInfo`)
rather than GPU-accelerated SkiaSharp (via `GRContext`). This means:

- Every frame's pixel data is uploaded from CPU to GPU via `glTexImage2D`
- Performance is bounded by the CPU rendering speed and texture upload bandwidth
- For simple 2D visualizations this is adequate; for complex scenes, GPU-accelerated
  SkiaSharp would be faster

### No High-DPI Scaling

The viewer uses raw framebuffer size for the SkiaSharp surface. On high-DPI displays,
the logical window size and framebuffer size differ. The `OnRender` callback receives
the framebuffer size, but no DPI scaling factor is provided.

### No Window Configuration After Creation

The `ViewerConfig` is consumed once at creation. There is no API to change the window
title, size, or other properties after the viewer is running.

### Shutdown Timeout

The `Dispose` method waits up to 5 seconds for the window thread to exit. If GLFW
is unresponsive (e.g., blocked on a system dialog), the dispose will return after
the timeout with a warning, but the thread may still be running.

## No TODOs or FIXMEs

A scan of the source code found no `TODO`, `FIXME`, `HACK`, `BUG`, `XXX`, or
`WORKAROUND` comments. No `NotImplementedException` or `failwith "not implemented"`
calls were found.

## GitHub Issues

Check the [GitHub Issues](https://github.com/EHotwagner/SkiaViewer/issues) page for
the latest reported bugs and feature requests.
*)

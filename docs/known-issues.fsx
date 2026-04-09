(**
---
title: Known Issues & Limitations
category: Reference
categoryindex: 5
index: 5
description: Current bugs, limitations, and design constraints.
---
*)

(**
# Known Issues & Limitations

This document lists known limitations and design constraints in SkiaViewer 1.1.0.

## Compiler Warnings

The `VulkanBackend.fs` module uses `NativePtr` operations for Vulkan interop, which
produce FS0009 warnings. These are expected and safe — the native pointer usage is
limited to Vulkan device initialization where it is required by the API.

## Design Limitations

| Limitation | Impact | Rationale |
|---|---|---|
| **Single window only** | Cannot open multiple viewer windows simultaneously | GLFW requires single-threaded window lifecycle; the viewer registers the platform once globally |
| **No high-DPI scaling** | Window dimensions are in logical pixels without DPI awareness | Silk.NET's DPI support varies by platform; not yet implemented |
| **No post-creation config changes** | Cannot resize, retitle, or change FPS after `Viewer.run` | Config is consumed at window creation; dynamic changes would require message passing to the window thread |
| **5-second shutdown timeout** | `Dispose()` blocks up to 5 seconds waiting for the window thread | Hard timeout prevents deadlocks if the window thread is stuck; in practice, shutdown completes in milliseconds |
| **JPEG quality fixed at 80** | No way to specify JPEG quality for screenshots | Simplified API; quality 80 is a reasonable default |
| **MSAA capped at 4x** | Vulkan MSAA is limited to 4 samples maximum | Higher sample counts showed diminishing returns for 2D rendering |

## Not Yet Implemented

| Feature | Notes |
|---|---|
| Raster backend (headless) | `Backend.Raster` case exists in the type but is reserved for future use |
| TextBlob rendering | `Element.TextBlob` type exists but renderer support is not exercised in tests |
| High-DPI awareness | Window dimensions don't account for display scaling |
| Multiple windows | Blocked by GLFW single-thread constraint |

## Code Quality

No `TODO`, `FIXME`, `HACK`, `BUG`, `XXX`, `WORKAROUND`, `NotImplementedException`,
or `failwith "not implemented"` markers were found in the codebase.
*)

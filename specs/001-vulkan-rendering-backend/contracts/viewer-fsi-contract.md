# Public API Contract: Viewer.fsi

**Branch**: `001-vulkan-rendering-backend` | **Date**: 2026-04-08

This document defines the planned changes to the public API surface exposed by `Viewer.fsi`.

## New Type: Backend

```fsharp
/// <summary>
/// Rendering backend selection for the viewer.
/// </summary>
[<RequireQualifiedAccess>]
type Backend =
    /// <summary>GPU-accelerated rendering via Vulkan and SkiaSharp's Vulkan backend.</summary>
    | Vulkan
    /// <summary>CPU raster rendering uploaded to an OpenGL texture each frame.</summary>
    | GL
    /// <summary>CPU raster rendering without windowed display (headless).</summary>
    | Raster
```

## Modified Type: ViewerConfig

New field appended to the existing record:

```fsharp
type ViewerConfig =
    { Title: string
      Width: int
      Height: int
      TargetFps: int
      ClearColor: SKColor
      OnRender: SKCanvas -> Vector2D<int> -> unit
      OnResize: int -> int -> unit
      OnKeyDown: Silk.NET.Input.Key -> unit
      OnMouseScroll: float32 -> float32 -> float32 -> unit
      OnMouseDrag: float32 -> float32 -> unit
      PreferredBackend: Backend option }
```

**Migration**: Existing consumers add `PreferredBackend = None` to their config record expression.

## Unchanged: Viewer.run

```fsharp
module Viewer =
    val run: config: ViewerConfig -> IDisposable
```

Signature unchanged. Behavior changes: the returned viewer may use either Vulkan or GL raster backend depending on environment and `PreferredBackend`.

## Surface-Area Delta Summary

| Symbol | Change | Kind |
|--------|--------|------|
| `Backend` | Added | Discriminated union (3 cases) |
| `ViewerConfig.PreferredBackend` | Added | Record field (`Backend option`) |
| `Viewer.run` | Unchanged | Function signature identical |

## XML Documentation Updates Required

- `ViewerConfig` remarks: update to mention backend selection behavior
- `Viewer` module summary: update to mention Vulkan as primary backend with GL fallback
- `Viewer.run` remarks: document backend selection order and logging

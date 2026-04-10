# SkiaViewer Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-04-09

## Active Technologies
- F# on .NET 10.0 + SkiaSharp 2.88.6, Silk.NET.Windowing 2.22.0, Silk.NET.OpenGL 2.22.0, Silk.NET.Vulkan 2.22.0 (002-screenshot-function)
- File system (screenshot images written to user-specified directory) (002-screenshot-function)
- F# on .NET 10.0 + SkiaSharp 2.88.6, Silk.NET.Windowing 2.22.0, Silk.NET.OpenGL 2.22.0, Silk.NET.Input 2.22.0, Silk.NET.Vulkan 2.22.0 (003-declarative-scene-dsl)
- F# on .NET 10.0 + SkiaViewer (project reference), SkiaSharp 2.88.6, Silk.NET 2.22.0 (Windowing, OpenGL, Input, Vulkan) (004-perf-test-suite)
- Stdout for results; optional JSON file output (004-perf-test-suite)
- F# on .NET 10.0 + SkiaSharp 2.88.6 (SKPictureRecorder, SKPicture), Silk.NET 2.22.0 (006-scene-diff-caching)
- In-memory dictionaries (render-thread only) (006-scene-diff-caching)
- In-memory position-indexed array (render-thread only) (007-cache-overhead-opt)
- F# on .NET 10.0 + SkiaSharp 2.88.6 (transitive via SkiaViewer project reference). No new NuGet dependencies. (008-charting-datagrid-library)
- N/A — all data is immutable per render frame (008-charting-datagrid-library)
- F# on .NET 10.0 + SkiaSharp 3.x (upgrade from 2.88.6), Silk.NET 2.22.0, Microsoft.Msagl 1.1.6, Microsoft.Msagl.Drawing 1.1.6 (009-layout-graph-viz)

- F# on .NET 10.0 + Silk.NET.Windowing 2.22.0, Silk.NET.OpenGL 2.22.0, Silk.NET.Input 2.22.0, SkiaSharp 2.88.6, SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.6, **Silk.NET.Vulkan 2.22.0 (new)** (001-vulkan-rendering-backend)

## Project Structure

```text
src/
tests/
```

## Commands

# Add commands for F# on .NET 10.0

## Code Style

F# on .NET 10.0: Follow standard conventions

## Recent Changes
- 009-layout-graph-viz: Added F# on .NET 10.0 + SkiaSharp 3.x (upgrade from 2.88.6), Silk.NET 2.22.0, Microsoft.Msagl 1.1.6, Microsoft.Msagl.Drawing 1.1.6
- 008-charting-datagrid-library: Added F# on .NET 10.0 + SkiaSharp 2.88.6 (transitive via SkiaViewer project reference). No new NuGet dependencies.
- 007-cache-overhead-opt: Added F# on .NET 10.0 + SkiaSharp 2.88.6 (SKPictureRecorder, SKPicture)


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->

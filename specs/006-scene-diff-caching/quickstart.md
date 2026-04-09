# Quickstart: Scene Diff Caching

**Branch**: `006-scene-diff-caching` | **Date**: 2026-04-09

## What This Feature Does

Adds an internal caching layer to the SkiaViewer rendering pipeline that detects unchanged `Group` elements between consecutive frames and replays pre-recorded draw commands instead of re-rendering them. Also memoizes converted `Paint` objects to reduce per-element allocation overhead.

## For Users: Nothing Changes

This feature is fully transparent. The existing Scene DSL, `Viewer.run`, and all public APIs remain unchanged. Scenes render pixel-identically with caching enabled.

## Key Implementation Points

### New Files

| File | Purpose |
|------|---------|
| `src/SkiaViewer/CachedRenderer.fsi` | Internal signature: `RenderCache` class, `CacheStats` record |
| `src/SkiaViewer/CachedRenderer.fs` | Cache logic: Group diffing, SKPicture recording/replay, paint memoization, generation eviction |
| `tests/SkiaViewer.Tests/CachedRendererTests.fs` | Unit tests: cache hits/misses, eviction, pixel-identity, toggle, stats |

### Modified Files

| File | Change |
|------|--------|
| `src/SkiaViewer/Viewer.fs` | Instantiate `RenderCache`, call `cache.Render` instead of `SceneRenderer.render`, invalidate on resize, dispose on shutdown |
| `src/SkiaViewer/SkiaViewer.fsproj` | Add `CachedRenderer.fsi` and `CachedRenderer.fs` to compilation order (before `Viewer.fs`, after `SceneRenderer.fs`) |
| `tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj` | Add `CachedRendererTests.fs` |

### Architecture

```
IObservable<Scene>
    │
    └─→ Viewer.run()
         │
         ├─→ [caching enabled]  → RenderCache.Render(scene, canvas)
         │                          ├─→ Compare each Element with previous scene
         │                          ├─→ Group unchanged? → canvas.DrawPicture(cached)
         │                          ├─→ Group changed? → Record new SKPicture, cache it
         │                          ├─→ Leaf element? → SceneRenderer.renderElement(canvas, element)
         │                          └─→ Sweep expired entries
         │
         └─→ [caching disabled] → SceneRenderer.render(scene, canvas)
```

### Build & Test

```bash
# Build
dotnet build src/SkiaViewer/SkiaViewer.fsproj

# Run tests
dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj

# Run perf benchmarks (compare cached vs uncached)
dotnet run --project tests/SkiaViewer.PerfTests/SkiaViewer.PerfTests.fsproj
```

### Verification Strategy

1. **Pixel identity**: Render test scenes with caching on and off, compare pixel buffers.
2. **Performance**: Use perf test suite to measure frame times for mostly-static vs fully-animated scenes.
3. **Memory bounds**: Stream 10,000+ unique scenes, assert memory remains bounded.
4. **Statistics**: Assert hit/miss/eviction counts match expected values for known scene sequences.

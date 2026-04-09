# Quickstart: Cache Overhead Optimization

**Branch**: `007-cache-overhead-opt` | **Date**: 2026-04-09

## What This Feature Does

Optimizes the CachedRenderer's internal data structures and comparison strategy to reduce overhead for fully-animated scenes from ~18.5% to under 5%, while maintaining or improving performance for mostly-static and identical-scene cases.

## For Users: Nothing Changes

The public API (`RenderCache`, `CacheStats`) is unchanged. The optimization is purely internal. Existing code that uses `RenderCache` continues to work identically.

## Key Changes

### Modified Files

| File | Change |
|------|--------|
| `src/SkiaViewer/CachedRenderer.fs` | Replace Dictionary with position-indexed slots, add reference-equality fast paths, use bounded recording regions |

### No New Files

This is a refactor of existing internals. No new `.fsi`, `.fs`, or test files are needed. The existing `CachedRendererTests.fs` and benchmark test validate the changes.

### Architecture (Revised Render Path)

```
RenderCache.Render(scene, canvas)
    │
    ├── Scene reference == previous? ──→ Replay all slots (O(1) per group)
    │
    └── For each element[i]:
        ├── Not Group? → SceneRenderer.renderElements
        └── Group?
            ├── Slot[i] exists AND ReferenceEquals(children)? → Hit (O(1))
            ├── Slot[i] exists AND structural equals? → Hit (O(n))
            └── Neither? → Miss (record with bounded region)
```

### Build & Test

```bash
# Build
dotnet build src/SkiaViewer/SkiaViewer.fsproj

# Run all CachedRenderer tests (correctness + benchmark)
dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "CachedRenderer"

# Run benchmark specifically
dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj --filter "Benchmark" --logger "console;verbosity=detailed"
```

### Verification Strategy

1. **Correctness**: All existing CachedRenderer tests pass (cache hits/misses, eviction, toggle, pixel-identity).
2. **Performance**: Benchmark test verifies fully-animated overhead <=5% and mostly-static speedup >=4x.
3. **Regression**: Full test suite (112+ tests) passes with no regressions.

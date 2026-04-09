# Public API Contract: Cache Overhead Optimization

**Branch**: `007-cache-overhead-opt` | **Date**: 2026-04-09

## Modified Module: CachedRenderer.fsi

The public signature is **unchanged**. `RenderCache`, `CacheStats`, and all members retain their existing signatures. This is a purely internal optimization.

```fsharp
// NO CHANGES to CachedRenderer.fsi
// RenderCache constructor, Render, Enabled, Stats, Invalidate, IDisposable — all identical
```

## Internal Changes: CachedRenderer.fs

| Aspect | Before (006) | After (007) |
|--------|-------------|-------------|
| Cache storage | `Dictionary<Element list, CacheEntry>` | `CacheSlot option[]` (position-indexed) |
| Lookup strategy | Hash + structural equality | Reference equality → structural equality fallback |
| Scene-level fast path | None | `Object.ReferenceEquals` on whole scene |
| Recording bounds | `SKRect(-1e6, -1e6, 1e6, 1e6)` | Surface-sized bounds (default 4096x4096) |
| Eviction | Dictionary iteration + `ResizeArray` of keys to remove | Array sweep with slot clearing |

## Surface Area Impact

- **No changes** to any public types or signatures.
- **No changes** to `CachedRenderer.fsi`.
- **No changes** to `SurfaceAreaBaseline.txt`.
- **No changes** to `Viewer.fsi`, `SceneRenderer.fsi`, or `Scene.fsi`.

## Behavioral Changes Observable to Users

| Aspect | Before | After |
|--------|--------|-------|
| Fully-animated overhead | ~18.5% | <=5% |
| Mostly-static speedup | ~4.8x | >=4.8x (no regression) |
| Identical-scene speedup | ~2.4x | Improved (scene-level fast path) |
| Cache stats accuracy | Correct | Correct (same semantics) |
| Visual output | Pixel-identical | Pixel-identical (unchanged) |

# Public API Contract: Scene Diff Caching

**Branch**: `006-scene-diff-caching` | **Date**: 2026-04-09

## New Module: CachedRenderer.fsi

This is an **internal** module (not public API). It is used by `Viewer.fs` and does not appear in the public surface area. Documented here for contract clarity between internal modules.

```fsharp
namespace SkiaViewer

open SkiaSharp

/// Per-frame cache performance counters.
type CacheStats =
    { Hits: int
      Misses: int
      Evictions: int }

/// Manages rendering cache state for a viewer instance.
/// All methods are intended for single-threaded access from the render thread.
[<Sealed>]
type RenderCache =
    /// Create a new cache with the specified maximum generation age.
    new: maxAge: int -> RenderCache

    /// Render a scene using cached Group subtrees where possible.
    /// Falls back to SceneRenderer.render when caching is disabled.
    member Render: scene: Scene -> canvas: SKCanvas -> unit

    /// Enable or disable caching at runtime.
    member Enabled: bool with get, set

    /// Get the most recent frame's cache statistics.
    member Stats: CacheStats

    /// Invalidate all cached entries (e.g., on surface resize).
    /// Disposes all held SKPicture and SKPaint resources.
    member Invalidate: unit -> unit

    interface System.IDisposable
```

## Modified Module: Viewer.fsi

No changes to the public `Viewer.fsi` signature. The `ViewerConfig` type and `Viewer.run` function remain unchanged.

**Internal change**: `Viewer.fs` will instantiate a `RenderCache` and call `cache.Render(scene, canvas)` instead of `SceneRenderer.render scene canvas`. The `RenderCache` will be invalidated in `recreateSurface()` and disposed on viewer shutdown.

## Modified Module: SceneRenderer.fsi

No changes. `SceneRenderer.render` remains the canonical uncached renderer. `CachedRenderer` delegates to it for leaf elements and when caching is disabled.

## Surface Area Impact

- **No new public types**: `CacheStats` and `RenderCache` are internal.
- **No changes to existing public types**: `Scene`, `Element`, `Paint`, `ViewerConfig`, `ViewerHandle`, `Viewer` are unchanged.
- **SurfaceAreaBaseline.txt**: No updates required.

## Behavioral Changes Observable to Users

| Aspect | Before | After |
|--------|--------|-------|
| Frame rendering time (mostly-static scenes) | Linear in total element count | Linear in changed element count |
| Frame rendering time (fully-animated scenes) | Linear in total element count | Linear in total element count + small comparison overhead |
| Memory usage | Constant per frame | Bounded by cached Group count (generation-evicted) |
| Visual output | Baseline | Pixel-identical to baseline |

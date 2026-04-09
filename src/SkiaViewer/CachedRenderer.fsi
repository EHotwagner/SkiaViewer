namespace SkiaViewer

open SkiaSharp

/// Per-frame cache performance counters.
type internal CacheStats =
    { Hits: int
      Misses: int
      Evictions: int }

/// Manages rendering cache state for a viewer instance.
/// Caches Group element subtrees as pre-recorded SKPicture objects
/// and replays them when the Group's children are structurally unchanged.
/// All methods are intended for single-threaded access from the render thread.
[<Sealed>]
type internal RenderCache =
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
    /// Disposes all held SKPicture resources.
    member Invalidate: unit -> unit

    interface System.IDisposable

# Research: Scene Diff Caching

**Branch**: `006-scene-diff-caching` | **Date**: 2026-04-09

## R1: Structural Equality for Cache Key Detection

**Decision**: Use F# structural equality on `Element` discriminated unions to detect unchanged Group elements.

**Rationale**: F# records and DUs implement `IEquatable<T>`, `IStructuralEquatable`, and `GetHashCode` by default, providing deep structural comparison. The Scene DSL types (`Element`, `Paint`, `Transform`, `Shader`, etc.) are all immutable value types — structural equality is already correct and free. This avoids needing explicit identity keys or user-supplied annotations.

**Caveat**: Types containing `SKBitmap`, `SKPicture`, `SKRegion`, `SKMatrix`, or `SKColor[]` use reference equality for those fields. This means two `Element.Image` nodes with different `SKBitmap` references (even if pixel-identical) will be treated as different. This is acceptable because the spec declares external mutable resources as the caller's responsibility.

**Alternatives considered**:
- User-supplied keys (React-style): Rejected — violates FR-008 (no DSL API changes).
- Reference equality only: Rejected — too coarse; would miss structurally identical but newly-allocated scene trees.
- Custom hash-based fingerprinting: Rejected — duplicates what F# structural equality already provides with extra maintenance cost.

## R2: SKPicture as Cache Storage Format

**Decision**: Use `SKPictureRecorder` to record Group rendering into `SKPicture` objects, then replay via `canvas.DrawPicture()`.

**Rationale**: `SKPicture` is Skia's native mechanism for recording and replaying draw commands. It is immutable once recorded, lightweight to replay, and can be transformed via canvas state (Save/Concat/DrawPicture/Restore). The DSL already has `Scene.recordPicture` and `Element.Picture` demonstrating this pattern. Using `SKPicture` means cached groups replay as a single draw call regardless of child count.

**Alternatives considered**:
- Bitmap/texture caching: Rejected — resolution-dependent, expensive for large groups, breaks with transform changes.
- Custom draw command list: Rejected — reinvents what `SKPictureRecorder` already does natively.

## R3: Generation-Based Eviction Strategy

**Decision**: Maintain a generation counter incremented each frame. Each cache entry is tagged with the generation it was last hit. After rendering, evict entries whose last-hit generation is older than N frames (default N=2).

**Rationale**: Simple, predictable, and bounded. With N=2, a group that disappears for 2 frames has its cached `SKPicture` disposed. A group that alternates between frames (e.g., toggling visibility) stays cached. This aligns with the "latest-value semantics" already used in the viewer's scene stream.

**Alternatives considered**:
- LRU with fixed capacity: Rejected — adds priority queue complexity; generation sweep is simpler and sufficient for the "mostly static scene" use case.
- Weak references: Rejected — GC timing is unpredictable; could evict hot entries under memory pressure while retaining cold ones.

## R4: Paint Memoization Approach

**Decision**: Use a `Dictionary<Paint, SKPaint>` keyed by structural equality of the `Paint` record. Reset the dictionary each frame (or use generation tagging for cross-frame reuse).

**Rationale**: F# record equality on `Paint` covers all fields including nested `Shader`, `ColorFilter`, etc. Since `makeSKPaint` is called for every element, deduplicating identical paints within a frame alone provides significant savings. Cross-frame reuse adds the generation-tag overhead but captures the common case of stable styles.

**Note**: `SKPaint` implements `IDisposable`. Paint cache entries must dispose their `SKPaint` on eviction. The cache owns the `SKPaint` lifecycle.

**Alternatives considered**:
- Per-element paint caching tied to element identity: Rejected — elements don't have stable identity; structural equality on `Paint` is more natural.
- Global persistent paint cache: Rejected — risk of `SKPaint` state drift; generation-based cleanup is safer.

## R5: Integration Point in Rendering Pipeline

**Decision**: Introduce a new `CachedRenderer` module (internal) that wraps `SceneRenderer`. The `Viewer.fs` render callback calls `CachedRenderer.render` instead of `SceneRenderer.render` when caching is enabled.

**Rationale**: Keeping `SceneRenderer` unchanged preserves it as the uncached baseline (needed for FR-005 pixel-identity testing and the runtime toggle FR-010). The `CachedRenderer` compares the current scene's Group elements against the previous scene, records `SKPicture`s for new/changed groups, replays cached pictures for unchanged groups, and delegates leaf elements to `SceneRenderer`'s existing `renderElement`.

**Alternatives considered**:
- Modifying `SceneRenderer` in-place with conditional caching: Rejected — complicates the module, harder to test pixel-identity guarantee.
- Caching at the `Viewer.fs` level (whole-scene picture): Rejected — too coarse; doesn't benefit partially-changing scenes.

## R6: Cache Statistics Exposure

**Decision**: Define a `CacheStats` record with `Hits`, `Misses`, `Evictions` (all `int`). Updated per frame by `CachedRenderer`. Exposed via a query function or property accessible from the `ViewerHandle` or `CachedRenderer` module directly.

**Rationale**: Lightweight value type, no allocation pressure. Updated as simple `+= 1` increments during rendering. Can be read after each frame without locking (atomically swapped snapshot).

## R7: Runtime Toggle Design

**Decision**: A mutable `enabled` flag on the cache state, defaulting to `true`. When disabled, `CachedRenderer.render` delegates directly to `SceneRenderer.render` with zero overhead. Toggle is exposed as a function on the cache module or via `ViewerConfig`.

**Rationale**: Single branch per frame when disabled. No need to clear the cache on disable — entries naturally expire via generation eviction. Re-enabling immediately starts rebuilding the cache from the next frame.

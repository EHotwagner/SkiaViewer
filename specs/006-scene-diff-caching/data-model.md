# Data Model: Scene Diff Caching

**Branch**: `006-scene-diff-caching` | **Date**: 2026-04-09

## Entities

### CacheStats

Per-frame statistics snapshot for cache observability.

| Field      | Type  | Description                                      |
|------------|-------|--------------------------------------------------|
| Hits       | int   | Number of Group elements served from cache        |
| Misses     | int   | Number of Group elements recorded fresh           |
| Evictions  | int   | Number of cache entries evicted this frame         |

**Lifecycle**: Created fresh each frame. Immutable after the frame completes. Read-only to consumers.

### CacheEntry

Internal representation of a cached Group rendering result.

| Field       | Type        | Description                                              |
|-------------|-------------|----------------------------------------------------------|
| Picture     | SKPicture   | Pre-recorded draw commands for the Group's children      |
| Generation  | int         | Frame generation when this entry was last referenced     |
| Bounds      | SKRect      | Bounding rectangle used when recording the picture       |

**Lifecycle**: Created when a Group is first encountered or changes. Updated (generation bump) on cache hit. Disposed when evicted (generation too old). `SKPicture` is disposed with the entry.

### RenderCache

Top-level cache state managed per viewer instance.

| Field           | Type                              | Description                                                   |
|-----------------|-----------------------------------|---------------------------------------------------------------|
| Entries         | Dictionary<Element, CacheEntry>   | Map from Group Element (by structural equality) to its cached rendering |
| PaintCache      | Dictionary<Paint, SKPaint>        | Memoized converted paint objects                              |
| Generation      | int                               | Current frame generation counter                              |
| MaxAge          | int                               | Maximum generations before eviction (default: 2)              |
| Enabled         | bool                              | Runtime toggle for caching                                    |
| LastStats       | CacheStats                        | Most recent frame's cache statistics                          |
| PreviousScene   | Scene option                      | Previous frame's scene for reference-equality fast path       |

**Lifecycle**: Created once when the viewer starts. Mutated each frame during rendering. All entries disposed on viewer shutdown or surface resize. `PaintCache` entries disposed on eviction.

## Relationships

```
RenderCache 1──* CacheEntry     (Entries dictionary)
RenderCache 1──* SKPaint        (PaintCache dictionary)
RenderCache 1──1 CacheStats     (LastStats, replaced each frame)
CacheEntry  1──1 SKPicture      (owns, disposes on eviction)
```

## State Transitions

### CacheEntry Lifecycle

```
[New Group encountered] ──→ Record (SKPictureRecorder) ──→ Cached
                                                              │
                              ┌────────────────────────────────┤
                              │                                │
                    [Group unchanged]                   [Group changed]
                              │                                │
                         Hit (bump generation)          Dispose old → Record new → Cached
                              │
                    [Not referenced for MaxAge frames]
                              │
                         Evict (Dispose SKPicture)
```

### RenderCache per-frame flow

```
Frame Start
  │
  ├── Increment Generation
  ├── Reset frame stats (hits=0, misses=0, evictions=0)
  │
  ├── For each top-level element:
  │   ├── If Group AND cache has matching entry → Hit (bump generation, replay SKPicture)
  │   ├── If Group AND no match → Miss (record via SKPictureRecorder, store entry)
  │   └── If leaf element → Render directly (no caching)
  │
  ├── Sweep: evict entries where (Generation - entry.Generation) > MaxAge
  │   └── Dispose evicted SKPicture and SKPaint objects
  │
  └── Snapshot stats → LastStats
```

## Key Constraints

- `Dictionary<Element, CacheEntry>` relies on F# structural equality and hash code for `Element`. Only `Element.Group` variants are inserted.
- `Dictionary<Paint, SKPaint>` relies on F# structural equality for `Paint` records.
- Both `SKPicture` and `SKPaint` are `IDisposable` — the cache owns their lifecycle and must dispose on eviction/shutdown.
- Cache is single-threaded (accessed only from the render thread). No locking required on cache internals.
- Surface resize triggers `Entries.Clear()` with disposal of all `SKPicture` objects, since recorded pictures may reference the old surface dimensions.

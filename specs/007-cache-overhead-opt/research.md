# Research: Cache Overhead Optimization

**Branch**: `007-cache-overhead-opt` | **Date**: 2026-04-09

## R1: Sources of Overhead in Fully-Animated Path

**Analysis**: Profiling the benchmark (20 groups x 10 children, all changing every frame) identifies three overhead sources in CachedRenderer vs SceneRenderer:

1. **Dictionary lookup with structural equality (dominant cost)**: `Dictionary<Element list, CacheEntry>.TryGetValue` computes `GetHashCode` then `Equals` on `Element list`. For a list of 10 elements, this traverses the entire list structure. With 20 groups, that's 20 hash+equality checks per frame — all resulting in misses.

2. **SKPicture recording overhead**: Each miss records children via `SKPictureRecorder.BeginRecording(SKRect(-1e6, -1e6, 1e6, 1e6))`. The enormous bounds region may cause Skia to allocate unnecessarily large internal structures.

3. **Eviction sweep**: After rendering, the sweep iterates all dictionary entries. With 20 new entries per frame and maxAge=2, the dictionary accumulates ~60 entries before steady state, causing ~20 evictions + disposals per frame.

**Decision**: Address all three sources in priority order: (1) comparison cost, (2) recording bounds, (3) eviction sweep.

## R2: Reference-Equality Fast Path Strategy

**Decision**: Implement a two-level comparison strategy:

1. **Scene-level**: Before iterating elements, check `Object.ReferenceEquals(previousScene, scene)`. If true, all groups are guaranteed cached — skip per-element comparison, just replay all cached pictures and bump generations.

2. **Element-level**: For each Group's children list, check `Object.ReferenceEquals(children, cachedKey)` before `Equals`. This requires storing the children reference alongside the cache entry rather than using it as a dictionary key.

**Rationale**: Reference equality is a single pointer comparison (O(1)). The current dictionary-based approach forces `GetHashCode` (O(n)) + `Equals` (O(n)) on every lookup. By maintaining a parallel lookup by reference identity, the common case becomes O(1).

**Implementation approach**: Replace `Dictionary<Element list, CacheEntry>` with an indexed structure that first checks reference equality on the children list, then falls back to structural equality. Concretely: maintain a `previousElements: Element list array` alongside the cache, indexed by position. On each frame, compare `Object.ReferenceEquals(currentChildren, previousChildren[i])` first.

**Alternatives considered**:
- Custom `IEqualityComparer` with reference-equality short-circuit: Rejected — `GetHashCode` still runs before `Equals` in Dictionary, adding overhead even for reference-equal cases.
- `ConditionalWeakTable`: Rejected — adds GC interaction and doesn't help when references change.

## R3: Position-Based Cache Indexing

**Decision**: Replace the `Dictionary<Element list, CacheEntry>` with a position-indexed array: `CacheEntry option array` where index corresponds to the element's position in `scene.Elements`. Each entry stores the children reference + structural key for fallback.

**Rationale**: The dictionary approach has two costs: hash computation and bucket traversal. Since scene elements are ordered and their positions are stable frame-to-frame, a position-based index eliminates both costs. For element at position `i`:
1. Check if `previousElements[i]` is reference-equal to current element's children → O(1) hit
2. If not, check structural equality → O(n) but only when actually changed
3. If structural match found at same position, update reference

This means the common case (element at same position, same reference) is a single pointer comparison per group.

**Alternatives considered**:
- Keep Dictionary but add reference-equality pre-check: Still pays hash cost on miss; position-based avoids it entirely.
- Hybrid: Dictionary for structural lookup + array for reference: Adds complexity without clear benefit over position-only.

## R4: Recording Bounds Optimization

**Decision**: Use canvas clip bounds or a reasonable finite bound (e.g., the surface dimensions) instead of `SKRect(-1e6, -1e6, 1e6, 1e6)`.

**Rationale**: The 2-million-unit bounding region may cause SkiaSharp's picture recorder to allocate large internal tile structures. Using the actual surface dimensions (e.g., 800x600) is sufficient since content outside the surface is clipped anyway. The CachedRenderer can accept surface dimensions or use a fixed reasonable default.

**Alternatives considered**:
- Compute tight bounds per group: Rejected — bounding box computation itself adds overhead; the whole point is to reduce miss cost.
- Use `SKRect.Empty` and let Skia auto-size: Rejected — `BeginRecording` requires explicit bounds.

## R5: Eviction Sweep Optimization

**Decision**: With position-based indexing, eviction becomes simpler: after rendering, any position that wasn't touched this frame has its entry disposed. No dictionary iteration needed — just compare generation per slot.

**Rationale**: The position-based array makes eviction O(previousSlotCount) with simple index iteration, no dictionary key hashing or allocation for `toRemove` lists.

**Alternatives considered**:
- Amortized sweep (every N frames): Rejected — adds complexity and delays disposal.
- No sweep, just overwrite: Rejected — leaks SKPicture objects for groups that disappear.

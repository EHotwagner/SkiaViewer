# Data Model: Cache Overhead Optimization

**Branch**: `007-cache-overhead-opt` | **Date**: 2026-04-09

## Changes to Existing Entities

### CacheSlot (replaces CacheEntry in Dictionary)

Position-indexed cache slot for a single Group element.

| Field            | Type            | Description                                               |
|------------------|-----------------|-----------------------------------------------------------|
| ChildrenRef      | Element list    | Object reference to the children list (for reference-equality check) |
| Picture          | SKPicture       | Pre-recorded draw commands                                |
| Generation       | int             | Frame generation when last hit                            |

**Lifecycle**: Allocated when a Group at this position first appears. Updated in-place on reference or structural match. Disposed when evicted (generation stale) or when position is vacated.

### RenderCache (modified)

| Field           | Type                | Change from 006                                        |
|-----------------|---------------------|--------------------------------------------------------|
| Slots           | CacheSlot option[]  | **Replaces** `Dictionary<Element list, CacheEntry>`    |
| SlotCapacity    | int                 | Maximum number of slots (resized if scene grows)       |
| PreviousScene   | Scene voption       | **New** — reference to previous scene for whole-scene fast path |
| RecordBounds    | SKRect              | **New** — bounded recording region (surface dimensions) |
| Generation      | int                 | Unchanged                                              |
| MaxAge          | int                 | Unchanged                                              |
| Enabled         | bool                | Unchanged                                              |
| LastStats       | CacheStats          | Unchanged                                              |

### CacheStats (unchanged)

No changes to the stats record.

## Per-Frame Flow (Revised)

```
Frame Start
  │
  ├── Increment Generation
  ├── Reset frame stats
  │
  ├── Reference-equality check: Object.ReferenceEquals(previousScene, scene)?
  │   └── YES: Replay all cached slots, bump generations → Done
  │
  ├── For each element at position i:
  │   ├── If not Group → render directly via SceneRenderer
  │   ├── If Group:
  │   │   ├── Check Slots[i] exists AND ReferenceEquals(children, slot.ChildrenRef)?
  │   │   │   └── YES → Hit (bump generation, replay picture)
  │   │   ├── Check Slots[i] exists AND children = slot.ChildrenRef (structural)?
  │   │   │   └── YES → Hit (bump generation, update ChildrenRef, replay)
  │   │   └── NO → Miss (record, create/replace slot)
  │   └── Continue
  │
  ├── Evict: for each slot where (gen - slot.Generation) > maxAge
  │   └── Dispose picture, clear slot
  │
  ├── Store previousScene reference
  └── Snapshot stats
```

## Key Constraints

- Slot array is indexed by element position in `scene.Elements`. If the scene grows, the array must be resized.
- Reference equality on `Element list` uses `Object.ReferenceEquals`, which is O(1).
- Structural equality fallback uses F#'s built-in structural comparison, same as before.
- `RecordBounds` should be set to surface dimensions. If not available, use a reasonable default (e.g., 4096x4096).

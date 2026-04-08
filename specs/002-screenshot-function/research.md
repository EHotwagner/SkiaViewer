# Research: 002-screenshot-function

**Date**: 2026-04-08

## R1: Screenshot Capture Mechanism for SkiaSharp Surfaces

**Decision**: Capture the current frame by taking a snapshot of the SKSurface under the existing `surfaceLock`, then encode and save to disk.

**Rationale**: The viewer already uses `SKSurface.Snapshot()` and `ReadPixels()` in the Vulkan render path. The screenshot function can reuse this pattern. For GL raster surfaces, `PeekPixels()` provides direct CPU-side pixel access. Both paths produce an `SKImage` that can be encoded via `SKImage.Encode()` or `SKData` encoding.

**Alternatives considered**:
- GL framebuffer readback via `glReadPixels` — requires GL context which lives on the window thread; would bypass SkiaSharp and lose backend abstraction.
- Capturing from the OnRender callback — would couple screenshot timing to the render loop; callers couldn't trigger on demand.

## R2: Thread-Safe Cross-Thread Capture

**Decision**: Use the existing `surfaceLock` to safely snapshot the surface from any thread. The snapshot (SKImage) is an immutable copy that can be encoded off the render thread.

**Rationale**: The viewer already protects `surface` with `surfaceLock`. Taking a snapshot under the lock is fast (no pixel copy for raster, GPU snapshot for Vulkan). The slow part (encoding + file I/O) happens after releasing the lock, so the render loop is not blocked.

**Alternatives considered**:
- Scheduling capture onto the render thread via a callback queue — adds complexity without benefit since `surfaceLock` already provides safe access.
- Double-buffering with a dedicated screenshot surface — unnecessary; SKImage.Snapshot() already provides an immutable copy.

## R3: Vulkan-Specific Considerations

**Decision**: For Vulkan-backed surfaces, flush the GRContext and submit GPU work before taking the snapshot, then use `ReadPixels()` to transfer pixel data to CPU for encoding.

**Rationale**: Vulkan surfaces are GPU-backed. Without flushing, the snapshot may contain stale or incomplete data. The existing render path already demonstrates this pattern (lines 299-311 of Viewer.fs). The encoding must happen from CPU-accessible pixel data.

**Alternatives considered**:
- Encoding directly from GPU memory — SkiaSharp's `Encode()` on GPU-backed images may fail or be slow on some drivers; explicit readback is safer.

## R4: File Naming Strategy

**Decision**: Use `screenshot-YYYYMMDD-HHmmss-fff.{ext}` format where `fff` is milliseconds.

**Rationale**: Millisecond precision ensures uniqueness for rapid successive calls (SC-005 requires 10 calls/sec). Timestamp ordering makes files naturally sortable. The prefix "screenshot-" makes files easily identifiable in a folder.

**Alternatives considered**:
- Sequential numbering — requires tracking state or scanning the directory; more complex.
- GUID-based names — unique but not human-readable or sortable.

## R5: Return Type for Screenshot Function

**Decision**: Return `Result<string, string>` — `Ok` with the full file path on success, `Error` with a descriptive message on failure.

**Rationale**: F# idiomatic error handling without exceptions. Aligns with FR-010 (graceful error indication). The caller can pattern-match to handle success/failure without try/catch. The string error type is sufficient since the consumer is another F# developer who can log or display the message.

**Alternatives considered**:
- `string option` — loses error context; caller can't distinguish between different failure modes.
- Throwing exceptions — non-idiomatic for expected error cases in F#; spec explicitly says "error indication rather than throwing."

## R6: Image Format Encoding

**Decision**: Support PNG (default) and JPEG via `SKEncodedImageFormat`. PNG uses default compression. JPEG uses quality 80.

**Rationale**: SkiaSharp's `SKImage.Encode(SKEncodedImageFormat, quality)` handles both formats natively. PNG is lossless and the natural default. JPEG at quality 80 is a widely-accepted balance of quality and size.

**Alternatives considered**:
- WebP — not widely expected for screenshots; can be added later if needed.
- Configurable quality parameter — spec assumes sensible defaults; overengineering for this scope.

## R7: Public API Surface Design

**Decision**: Expose a discriminated union `ImageFormat` (Png | Jpeg) and a `screenshot` function in the `Viewer` module. The function requires a viewer handle (or captures needed state at construction time) plus folder path and optional format.

**Rationale**: The current `Viewer.run` returns an `IDisposable`. To support screenshot, the return type needs to expose the screenshot function. Options:
1. Return a richer type (e.g., `ViewerHandle`) that implements `IDisposable` and exposes `Screenshot`.
2. Return an interface `IViewer` with `Dispose` and `Screenshot` members.
3. Capture screenshot closure in the returned disposable.

Option 1 is cleanest — change the return type from `IDisposable` to a concrete `ViewerHandle` type with `IDisposable` and a `Screenshot` member. This is a breaking change to the public API but necessary to expose new functionality.

**Alternatives considered**:
- Adding a global/static screenshot function — requires global mutable state; anti-pattern.
- Passing screenshot callback through config — inverts responsibility; caller shouldn't need to wire plumbing.

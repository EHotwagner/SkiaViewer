# Quickstart: 002-screenshot-function

**Date**: 2026-04-08

## Overview

Add a public `Screenshot` method to the viewer handle returned by `Viewer.run`. The method captures the current rendered frame and saves it as an image file to a user-specified folder, blocking until the file is written.

## Key Implementation Steps

1. **Define `ImageFormat` DU** in `Viewer.fs` and expose in `Viewer.fsi`
2. **Promote `ViewerHandle`** from private to public, add `Screenshot` member
3. **Implement screenshot capture logic**:
   - Lock `surfaceLock`, snapshot the surface (with Vulkan flush if needed)
   - Encode to PNG/JPEG via `SKImage.Encode`
   - Create directory if needed, write file with timestamp-based name
   - Return `Result<string, string>`
4. **Update `Viewer.fsi`** signature file with new public types and changed `run` return type
5. **Update surface-area baseline** for public API change
6. **Add tests** covering: basic capture, folder creation, format selection, error cases, rapid successive calls, both backends
7. **Add FSI prelude/example script** for screenshot usage

## Usage Example

```fsharp
use viewer = Viewer.run config
Thread.Sleep(1000) // let some frames render

match viewer.Screenshot("/tmp/screenshots") with
| Ok path -> printfn "Saved to %s" path
| Error msg -> eprintfn "Screenshot failed: %s" msg

// With explicit format:
match viewer.Screenshot("/tmp/screenshots", ImageFormat.Jpeg) with
| Ok path -> printfn "JPEG saved to %s" path
| Error msg -> eprintfn "Screenshot failed: %s" msg
```

## Files to Change

| File | Change |
|------|--------|
| `src/SkiaViewer/Viewer.fsi` | Add `ImageFormat`, `ViewerHandle`, update `run` signature |
| `src/SkiaViewer/Viewer.fs` | Add `ImageFormat`, promote `ViewerHandle`, implement `Screenshot` |
| `tests/SkiaViewer.Tests/ViewerTests.fs` | Add screenshot test cases |
| Surface-area baseline (if exists) | Update for new public API |
| `scripts/prelude.fsx` (new) | FSI prelude with screenshot helpers |
| `scripts/examples/01-screenshot.fsx` (new) | Screenshot example script |

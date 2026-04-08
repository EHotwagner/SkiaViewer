# Public API Contract: 002-screenshot-function

**Date**: 2026-04-08

## New Public Types

### ImageFormat

```fsharp
[<RequireQualifiedAccess>]
type ImageFormat =
    | Png
    | Jpeg
```

### ViewerHandle

```fsharp
type ViewerHandle =
    interface IDisposable
    /// Captures the current rendered frame and saves it to the specified folder.
    /// Returns Ok(filePath) on success, Error(message) on failure.
    /// Blocks the calling thread until the file is written to disk.
    /// Thread-safe: callable from any thread.
    member Screenshot: folder: string * ?format: ImageFormat -> Result<string, string>
```

## Changed Signatures

### Viewer.run

**Before**:
```fsharp
val run: config: ViewerConfig -> IDisposable
```

**After**:
```fsharp
val run: config: ViewerConfig -> ViewerHandle
```

**Breaking change**: Return type changes from `IDisposable` to `ViewerHandle`. Since `ViewerHandle` implements `IDisposable`, existing `use viewer = Viewer.run config` patterns continue to work for disposal. However, code that explicitly types the return as `IDisposable` will need updating.

## Unchanged Types

- `Backend` — no changes
- `ViewerConfig` — no changes

## Surface Area Impact

| Symbol | Change |
|--------|--------|
| `ImageFormat` | Added (new DU) |
| `ImageFormat.Png` | Added (new case) |
| `ImageFormat.Jpeg` | Added (new case) |
| `ViewerHandle` | Added (new class, was internal) |
| `ViewerHandle.Screenshot` | Added (new member) |
| `Viewer.run` | Changed return type: `IDisposable` → `ViewerHandle` |

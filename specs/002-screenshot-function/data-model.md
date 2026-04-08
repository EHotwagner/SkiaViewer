# Data Model: 002-screenshot-function

**Date**: 2026-04-08

## Entities

### ImageFormat (Discriminated Union)

Represents the supported output image formats for screenshots.

| Variant | Description |
|---------|-------------|
| Png     | Lossless PNG encoding (default) |
| Jpeg    | Lossy JPEG encoding at quality 80 |

**Rules**:
- PNG is the default when no format is specified.
- The set of supported formats is closed (no extensibility needed at this time).

### ViewerHandle (Class)

Extends the current `IDisposable` return type from `Viewer.run` to expose screenshot functionality.

| Member | Type | Description |
|--------|------|-------------|
| Screenshot | `string -> ImageFormat option -> Result<string, string>` | Captures current frame, saves to folder, returns file path or error |
| Dispose | `unit -> unit` | Graceful shutdown (existing behavior) |

**State captured at construction**:
- Reference to the mutable `surface` and `surfaceLock` (for thread-safe snapshot)
- Reference to `activeBackend` (for Vulkan flush before snapshot)
- Reference to `surfaceWidth` / `surfaceHeight` (for zero-size detection)

**Lifecycle**:
- Created by `Viewer.run` (replaces current anonymous `ViewerHandle`)
- Screenshot callable any time between creation and disposal
- After disposal, Screenshot returns `Error "Viewer has been disposed"`

### Screenshot File (Output Artifact)

| Attribute | Type | Description |
|-----------|------|-------------|
| Folder path | string | User-specified destination directory |
| Filename | string | Auto-generated: `screenshot-YYYYMMDD-HHmmss-fff.{ext}` |
| Format | ImageFormat | PNG or JPEG |
| Full path | string | Folder + filename, returned to caller on success |

**Rules**:
- Folder is created recursively if absent (Directory.CreateDirectory)
- Filename uses UTC timestamp with millisecond precision
- Extension matches format: `.png` or `.jpg`

## Relationships

```
ViewerHandle --uses--> surfaceLock, surface, activeBackend (internal mutable state)
ViewerHandle --produces--> Screenshot File (via Screenshot member)
ImageFormat --determines--> file extension and encoding parameters
```

## State Transitions

### Screenshot Call Flow

```
Caller invokes Screenshot(folder, format)
  │
  ├─ surface is null? → Error "No active surface"
  ├─ surfaceWidth = 0? → Error "Framebuffer is zero-size"
  │
  ├─ lock surfaceLock
  │   ├─ Vulkan backend? → flush GRContext, submit(true)
  │   ├─ Snapshot() → SKImage
  │   └─ unlock
  │
  ├─ Directory.CreateDirectory(folder)
  │   └─ IOException? → Error with message
  │
  ├─ Encode SKImage to format
  ├─ Write to file
  │   └─ IOException? → Error with message
  │
  └─ Ok(fullPath)
```

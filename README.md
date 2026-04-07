# SkiaViewer

A hardened Silk.NET + SkiaSharp OpenGL viewer for .NET. Renders SkiaSharp raster surfaces to an OpenGL-backed window via texture upload, with thread-safe lifecycle management, cross-thread shutdown, and frame-level exception recovery.

## Installation

```
dotnet add package SkiaViewer
```

## Quick Start

```fsharp
open SkiaViewer
open SkiaSharp

let config =
    { Title = "Hello SkiaViewer"
      Width = 800
      Height = 600
      TargetFps = 60
      ClearColor = SKColors.CornflowerBlue
      OnRender = fun canvas fbSize ->
          use paint = new SKPaint(Color = SKColors.White, TextSize = 32.0f, IsAntialias = true)
          canvas.DrawText("Hello, SkiaViewer!", 50.0f, 80.0f, paint)
      OnResize = fun _ _ -> ()
      OnKeyDown = fun _ -> ()
      OnMouseScroll = fun _ _ _ -> ()
      OnMouseDrag = fun _ _ -> () }

use viewer = Viewer.run config
System.Threading.Thread.Sleep(5000)
```

## Documentation

Full documentation is available at **https://EHotwagner.github.io/SkiaViewer/**

To build and preview locally:

```
dotnet tool restore
dotnet fsdocs watch
```

Then open http://localhost:8901.

## Features

- **Background-thread window** — the viewer runs the GLFW window loop on a dedicated thread, keeping your main thread free
- **SkiaSharp canvas** — draw with the full SkiaSharp 2D API (shapes, text, gradients, paths, images)
- **Frame-level exception recovery** — render callback exceptions are caught and logged; the viewer keeps running
- **Cross-thread shutdown** — dispose from any thread for graceful window shutdown with a 5-second timeout
- **Input callbacks** — keyboard, mouse scroll, and mouse drag events via Silk.NET input

## Known Issues

See [Known Issues](https://EHotwagner.github.io/SkiaViewer/known-issues.html) for current limitations.

## License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.

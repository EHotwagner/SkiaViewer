(**
---
title: Drawing Primitives
category: Tutorials
categoryindex: 2
index: 1
description: Draw rectangles, circles, lines, text, and gradients on the SkiaViewer canvas.
---
*)

(**
# Drawing Primitives

SkiaViewer gives you a standard SkiaSharp `SKCanvas` each frame. This tutorial demonstrates
the common drawing primitives you can use in your `OnRender` callback.

All drawing happens inside the `OnRender` callback. The canvas is pre-cleared with your
configured `ClearColor` before each invocation.
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

open SkiaViewer
open SkiaSharp

(**
## Filled Rectangles

Use `SKPaint` with a solid color and `DrawRect` to fill a rectangle:
*)

(*** do-not-eval ***)
let renderRectangle (canvas: SKCanvas) =
    use paint = new SKPaint(Color = SKColors.DodgerBlue, IsAntialias = true)
    canvas.DrawRect(10.0f, 10.0f, 120.0f, 80.0f, paint)

(**
## Stroked Rounded Rectangles

Set `IsStroke = true` on the paint to draw outlines. `SKRoundRect` adds corner radii:
*)

(*** do-not-eval ***)
let renderRoundedRect (canvas: SKCanvas) =
    use paint = new SKPaint(Color = SKColors.LimeGreen, IsStroke = true, StrokeWidth = 3.0f, IsAntialias = true)
    let rrect = new SKRoundRect(SKRect(150.0f, 10.0f, 300.0f, 90.0f), 10.0f, 10.0f)
    canvas.DrawRoundRect(rrect, paint)

(**
## Circles

`DrawCircle` takes a center point and radius:
*)

(*** do-not-eval ***)
let renderCircle (canvas: SKCanvas) =
    use paint = new SKPaint(Color = SKColors.Tomato, IsAntialias = true)
    canvas.DrawCircle(60.0f, 160.0f, 35.0f, paint)

(**
## Lines

Draw lines with a stroked paint and a specified `StrokeWidth`:
*)

(*** do-not-eval ***)
let renderLine (canvas: SKCanvas) =
    use paint = new SKPaint(Color = SKColors.Gold, StrokeWidth = 2.0f, IsStroke = true, IsAntialias = true)
    canvas.DrawLine(120.0f, 120.0f, 350.0f, 200.0f, paint)

(**
## Text

`DrawText` renders text at a baseline position. Control size with `TextSize`:
*)

(*** do-not-eval ***)
let renderText (canvas: SKCanvas) (frameCount: int) =
    use paint = new SKPaint(Color = SKColors.White, TextSize = 24.0f, IsAntialias = true)
    canvas.DrawText($"Frame {frameCount}", 150.0f, 170.0f, paint)

(**
## Linear Gradients

Create a gradient shader and assign it to a paint's `Shader` property:
*)

(*** do-not-eval ***)
let renderGradient (canvas: SKCanvas) =
    use shader = SKShader.CreateLinearGradient(
        SKPoint(10.0f, 220.0f), SKPoint(350.0f, 270.0f),
        [| SKColors.DeepPink; SKColors.Cyan |], [| 0.0f; 1.0f |],
        SKShaderTileMode.Clamp)
    use paint = new SKPaint(Shader = shader, IsAntialias = true)
    canvas.DrawRect(10.0f, 220.0f, 340.0f, 50.0f, paint)

(**
## Putting It All Together

Combine all primitives in a single `OnRender` callback:
*)

(*** do-not-eval ***)
let config =
    { Title = "Drawing Primitives"
      Width = 400
      Height = 300
      TargetFps = 60
      ClearColor = SKColors.Black
      OnRender = fun canvas _ ->
          renderRectangle canvas
          renderRoundedRect canvas
          renderCircle canvas
          renderLine canvas
          renderText canvas 1
          renderGradient canvas
      OnResize = fun _ _ -> ()
      OnKeyDown = fun _ -> ()
      OnMouseScroll = fun _ _ _ -> ()
      OnMouseDrag = fun _ _ -> () }

let viewer = Viewer.run config
System.Threading.Thread.Sleep(3000)
viewer.Dispose()

(**
<div class="alert alert-info">
<strong>Tip:</strong> Always use <code>use</code> bindings for <code>SKPaint</code> and <code>SKShader</code>
objects to ensure they are disposed each frame. Creating paints per-frame is inexpensive and avoids
lifetime management issues.
</div>

## Next Steps

- [Input Handling](input-handling.html) — respond to keyboard and mouse events
- [Architecture Overview](architecture.html) — how the rendering pipeline works
- [API Reference](reference/index.html)
*)

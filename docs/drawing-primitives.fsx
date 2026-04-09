(**
---
title: Drawing Primitives & Paint
category: Tutorials
categoryindex: 2
index: 1
description: All element types and paint styling options with rendered examples.
---
*)

(**
# Drawing Primitives & Paint

This guide covers every element type and paint configuration available in SkiaViewer,
rendered to offscreen surfaces so you can see the results.

## Setup
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaSharp.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

open SkiaViewer
open SkiaSharp

(*** hide ***)
// Helper to render a scene to a bitmap for display
let renderScene width height scene =
    let info = SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)
    use surface = SKSurface.Create(info)
    SceneRenderer.render scene surface.Canvas
    surface.Canvas.Flush()
    use img = surface.Snapshot()
    SKBitmap.FromImage(img)

(**
## Rectangles

Rectangles are the most basic element, defined by position (x, y) and size (width, height):
*)

let rectScene =
    Scene.create SKColors.White [
        // Solid fill
        Scene.rect 10f 10f 80f 50f (Scene.fill SKColors.CornflowerBlue)
        // Stroke only
        Scene.rect 110f 10f 80f 50f (Scene.stroke SKColors.Red 3f)
        // Fill + stroke
        Scene.rect 10f 80f 80f 50f (Scene.fillStroke SKColors.LightGreen SKColors.DarkGreen 2f)
    ]

use rectBitmap = renderScene 200 150 rectScene

(*** include-value: rectBitmap ***)

(**
## Circles and Ellipses

`circle` creates an ellipse with equal radii. `ellipse` allows independent x and y radii:
*)

let ellipseScene =
    Scene.create SKColors.White [
        Scene.circle 60f 60f 40f (Scene.fill SKColors.Coral)
        Scene.ellipse 170f 60f 50f 30f (Scene.fill SKColors.MediumPurple)
    ]

use ellipseBitmap = renderScene 240 120 ellipseScene

(*** include-value: ellipseBitmap ***)

(**
## Lines

Lines connect two points with a stroked paint:
*)

let lineScene =
    Scene.create SKColors.White [
        Scene.line 10f 20f 190f 20f (Scene.stroke SKColors.Black 2f)
        Scene.line 10f 50f 190f 50f (Scene.stroke SKColors.Red 4f |> Scene.withStrokeCap StrokeCap.Round)
        Scene.line 10f 80f 190f 80f (Scene.stroke SKColors.Blue 6f |> Scene.withStrokeCap StrokeCap.Square)
    ]

use lineBitmap = renderScene 200 100 lineScene

(*** include-value: lineBitmap ***)

(**
## Text

Text elements are positioned by their baseline (x, y) with a specified font size:
*)

let textScene =
    Scene.create SKColors.White [
        Scene.text "Default font" 10f 30f 20f (Scene.fill SKColors.Black)
        Scene.text "Bold" 10f 60f 24f
            (Scene.fill SKColors.DarkBlue
             |> Scene.withFont { Family = "sans-serif"; Weight = 700; Slant = FontSlant.Upright; Width = 5 })
        Scene.text "Italic" 10f 90f 24f
            (Scene.fill SKColors.DarkRed
             |> Scene.withFont { Family = ""; Weight = 400; Slant = FontSlant.Italic; Width = 5 })
    ]

use textBitmap = renderScene 250 110 textScene

(*** include-value: textBitmap ***)

(**
## Paths

Paths use a list of `PathCommand` values to define arbitrary shapes:
*)

let pathScene =
    Scene.create SKColors.White [
        // Triangle
        Scene.path [
            PathCommand.MoveTo(50f, 10f)
            PathCommand.LineTo(90f, 80f)
            PathCommand.LineTo(10f, 80f)
            PathCommand.Close
        ] (Scene.fill SKColors.Teal)

        // Curved path
        Scene.path [
            PathCommand.MoveTo(110f, 80f)
            PathCommand.CubicTo(130f, 10f, 170f, 10f, 190f, 80f)
        ] (Scene.stroke SKColors.DarkOrange 3f)

        // Rounded rectangle via path commands
        Scene.path [
            PathCommand.AddRoundRect(SKRect(210f, 20f, 290f, 70f), 10f, 10f, PathDirection.Clockwise)
        ] (Scene.fillStroke SKColors.LightBlue SKColors.Navy 2f)
    ]

use pathBitmap = renderScene 300 100 pathScene

(*** include-value: pathBitmap ***)

(**
<details>
<summary>Full list of PathCommand cases</summary>

| Command | Description |
|---|---|
| `MoveTo(x, y)` | Move the pen without drawing |
| `LineTo(x, y)` | Draw a straight line |
| `QuadTo(cx, cy, x, y)` | Quadratic Bezier curve |
| `CubicTo(c1x, c1y, c2x, c2y, x, y)` | Cubic Bezier curve |
| `ArcTo(rect, startAngle, sweepAngle)` | Elliptical arc |
| `Close` | Close the current contour |
| `AddRect(rect, direction)` | Add a rectangle contour |
| `AddCircle(cx, cy, radius, direction)` | Add a circle contour |
| `AddOval(rect, direction)` | Add an oval contour |
| `AddRoundRect(rect, rx, ry, direction)` | Add a rounded rectangle contour |

</details>

## Images

Render an `SKBitmap` at a given position and size:
*)

let imageBmp = new SKBitmap(SKImageInfo(20, 20, SKColorType.Rgba8888, SKAlphaType.Premul))
use canvas = new SKCanvas(imageBmp)
canvas.Clear(SKColors.Red)
use paint = new SKPaint(Color = SKColors.White)
canvas.DrawCircle(10f, 10f, 8f, paint)

let imageScene =
    Scene.create SKColors.Black [
        Scene.image imageBmp 10f 10f 80f 80f (Scene.fill SKColors.White)
    ]

use imageBitmap = renderScene 100 100 imageScene

(*** include-value: imageBitmap ***)

(**
## Points, Vertices, and Arcs

Additional drawing primitives for specialized rendering:
*)

let advancedScene =
    Scene.create SKColors.White [
        // Points
        Scene.points
            [| SKPoint(20f, 30f); SKPoint(50f, 30f); SKPoint(80f, 30f) |]
            PointMode.Points
            (Scene.stroke SKColors.Red 8f |> Scene.withStrokeCap StrokeCap.Round)

        // Arc (pie slice)
        Scene.arc (SKRect(110f, 10f, 190f, 80f)) 0f 270f true (Scene.fill SKColors.MediumPurple)

        // Colored triangle via vertices
        Scene.vertices
            [| SKPoint(210f, 80f); SKPoint(250f, 10f); SKPoint(290f, 80f) |]
            [| SKColors.Red; SKColors.Green; SKColors.Blue |]
            VertexMode.Triangles
            (Scene.fill SKColors.White)
    ]

use advancedBitmap = renderScene 310 100 advancedScene

(*** include-value: advancedBitmap ***)

(**
## Paint Reference

All `Paint` record fields:

| Field | Type | Default | Description |
|---|---|---|---|
| `Fill` | `SKColor option` | `None` | Fill color |
| `Stroke` | `SKColor option` | `None` | Stroke color |
| `StrokeWidth` | `float32` | `1.0` | Stroke width in pixels |
| `Opacity` | `float32` | `1.0` | Overall opacity (0.0 - 1.0) |
| `IsAntialias` | `bool` | `true` | Anti-aliasing enabled |
| `StrokeCap` | `StrokeCap` | `Butt` | Line endpoint style |
| `StrokeJoin` | `StrokeJoin` | `Miter` | Line join style |
| `StrokeMiter` | `float32` | `4.0` | Miter limit for sharp joins |
| `BlendMode` | `BlendMode` | `SrcOver` | Compositing mode |
| `Shader` | `Shader option` | `None` | Fill shader (gradients, noise) |
| `ColorFilter` | `ColorFilter option` | `None` | Color transformation |
| `MaskFilter` | `MaskFilter option` | `None` | Alpha channel effect (blur) |
| `ImageFilter` | `ImageFilter option` | `None` | Pixel-level effect |
| `PathEffect` | `PathEffect option` | `None` | Stroke geometry effect |
| `Font` | `FontSpec option` | `None` | Custom font specification |

## Next Steps

- [Shaders & Effects](shaders-and-effects.html) — gradients, filters, and path effects
- [Input Handling](input-handling.html) — keyboard, mouse, and window events
*)

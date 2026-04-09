(**
---
title: Declarative Scene DSL
category: Overview
categoryindex: 1
index: 5
description: Build scenes with a composable F# DSL using immutable types and pipeline operators.
---
*)

(**
# Declarative Scene DSL

SkiaViewer uses a declarative scene graph model. Scenes are built from immutable
F# types — discriminated unions for elements and records for paint styles. You
compose scenes with helper functions from the `Scene` module and push them to the
viewer via `IObservable<Scene>`.

## Scene Structure

A `Scene` contains a background color and a list of elements:
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaSharp.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"

open SkiaViewer
open SkiaSharp

let basicScene =
    Scene.create SKColors.White [
        Scene.rect 10f 10f 100f 60f (Scene.fill SKColors.CornflowerBlue)
        Scene.circle 200f 50f 40f (Scene.fill SKColors.Coral)
    ]

(*** include-value: basicScene ***)

(**
## Paint System

Every element requires a `Paint` value that describes how it is drawn.
The `Scene` module provides builders and modifiers that compose via `|>`:
*)

// Fill only
let fillPaint = Scene.fill SKColors.Red

// Stroke only
let strokePaint = Scene.stroke SKColors.Blue 3f

// Fill + stroke
let bothPaint = Scene.fillStroke SKColors.Red SKColors.Blue 2f

// Modify with pipeline
let styledPaint =
    Scene.fill SKColors.Green
    |> Scene.withOpacity 0.8f
    |> Scene.withStrokeCap StrokeCap.Round
    |> Scene.withBlendMode BlendMode.SrcOver

(*** include-value: styledPaint ***)

(**
## Element Types

The `Element` discriminated union covers all drawable primitives:
*)

let elements = [
    // Basic shapes
    Scene.rect 10f 10f 80f 50f (Scene.fill SKColors.Red)
    Scene.ellipse 150f 35f 40f 25f (Scene.fill SKColors.Green)
    Scene.circle 250f 35f 25f (Scene.fill SKColors.Blue)
    Scene.line 10f 80f 290f 80f (Scene.stroke SKColors.White 2f)

    // Text
    Scene.text "Hello" 10f 120f 24f (Scene.fill SKColors.Yellow)

    // Path with commands
    Scene.path [
        PathCommand.MoveTo(10f, 150f)
        PathCommand.LineTo(50f, 130f)
        PathCommand.CubicTo(80f, 170f, 120f, 130f, 150f, 150f)
        PathCommand.Close
    ] (Scene.fill SKColors.Magenta)
]

(**
## Transforms

Group elements with optional transforms. Transforms compose when nested:
*)

let transformedScene =
    Scene.create SKColors.Black [
        // Translate
        Scene.translate 100f 50f [
            Scene.rect 0f 0f 40f 40f (Scene.fill SKColors.Red)
        ]

        // Rotate around center
        Scene.rotate 45f 200f 100f [
            Scene.rect 180f 80f 40f 40f (Scene.fill SKColors.Green)
        ]

        // Scale
        Scene.scale 2f 2f [
            Scene.circle 50f 150f 10f (Scene.fill SKColors.Blue)
        ]

        // Nested transforms compose
        Scene.translate 300f 0f [
            Scene.rotate 30f 0f 0f [
                Scene.rect 0f 0f 40f 40f (Scene.fill SKColors.Yellow)
            ]
        ]
    ]

(**
## Groups with Clipping

Apply clip regions to groups to restrict rendering:
*)

let clippedScene =
    Scene.create SKColors.Black [
        // Clip to a rectangle
        Scene.groupWithClip None None
            (Clip.Rect(SKRect(20f, 20f, 80f, 80f), ClipOperation.Intersect, true))
            [ Scene.rect 0f 0f 100f 100f (Scene.fill SKColors.Red) ]

        // Clip to a circular path
        Scene.groupWithClip None None
            (Clip.Path(
                [ PathCommand.AddCircle(150f, 50f, 30f, PathDirection.Clockwise) ],
                ClipOperation.Intersect, true))
            [ Scene.rect 120f 20f 60f 60f (Scene.fill SKColors.Green) ]
    ]

(**
## Group Paint and Opacity

Apply paint to a group to affect all children. Group opacity composes with
child opacity:
*)

let groupPaint = Scene.fill SKColors.White |> Scene.withOpacity 0.5f

let semitransparentGroup =
    Scene.create SKColors.Black [
        Scene.group None (Some groupPaint) [
            Scene.rect 10f 10f 80f 80f (Scene.fill SKColors.White)
            Scene.circle 50f 50f 30f (Scene.fill SKColors.Red)
        ]
    ]

(**
## Path Operations

Combine paths with boolean operations, measure path length,
and extract segments:
*)

let circle1 = [ PathCommand.AddCircle(40f, 50f, 30f, PathDirection.Clockwise) ]
let circle2 = [ PathCommand.AddCircle(60f, 50f, 30f, PathDirection.Clockwise) ]

// Boolean union of two circles
let union = Scene.combinePaths PathOp.Union circle1 circle2

// Measure path length
let lineCmds = [ PathCommand.MoveTo(0f, 0f); PathCommand.LineTo(100f, 0f) ]
let pathLength = Scene.measurePath lineCmds

(*** include-value: pathLength ***)

(**
Extract a segment from a path:
*)

let segment = Scene.extractPathSegment lineCmds 20f 60f

(**
## Picture Recording

Record elements into a reusable `SKPicture` for efficient repeated rendering:
*)

let pic = Scene.recordPicture (SKRect(0f, 0f, 100f, 100f)) [
    Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)
    Scene.circle 75f 75f 20f (Scene.fill SKColors.Blue)
]

let pictureScene =
    Scene.create SKColors.White [
        Scene.picture pic None
        Scene.picture pic (Some (Transform.Translate(120f, 0f)))
    ]

(**
## Region Operations

Create and combine regions for hit testing:
*)

let region1 = Scene.createRegionFromRect (SKRectI(0, 0, 50, 50))
let region2 = Scene.createRegionFromRect (SKRectI(40, 40, 100, 100))
let combined = Scene.combineRegions RegionOp.Union region1 region2

let hitTest = Scene.regionContains combined 25 25

(*** include-value: hitTest ***)

(**
## 3D Perspective Transforms

Apply 3D rotations with perspective projection:
*)

let perspectiveScene =
    Scene.create SKColors.Black [
        Scene.group
            (Some (Transform.Perspective(
                Transform3D.Compose [
                    Transform3D.RotateY(30f)
                    Transform3D.Camera(0f, 0f, -200f)
                ])))
            None
            [ Scene.rect 20f 20f 60f 60f (Scene.fill SKColors.White) ]
    ]

(**
## Text Measurement

Measure text bounds before rendering for layout purposes:
*)

let bounds = Scene.measureText "Hello World" 24f None

(*** include-value: bounds ***)

(**
## Next Steps

- [Drawing Primitives & Paint](drawing-primitives.html) — detailed element and paint reference
- [Shaders & Effects](shaders-and-effects.html) — gradients, filters, and advanced effects
- [Architecture Overview](architecture.html) — how the scene graph is rendered
*)

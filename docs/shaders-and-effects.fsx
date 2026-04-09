(**
---
title: Shaders & Effects
category: Tutorials
categoryindex: 2
index: 3
description: Gradients, noise, color filters, mask filters, image filters, blend modes, and path effects.
---
*)

(**
# Shaders & Effects

SkiaViewer exposes the full SkiaSharp effects pipeline through declarative types.
This guide covers shaders, color filters, mask filters, image filters, blend modes,
and path effects.

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
let renderScene width height scene =
    let info = SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)
    use surface = SKSurface.Create(info)
    SceneRenderer.render scene surface.Canvas
    surface.Canvas.Flush()
    use img = surface.Snapshot()
    SKBitmap.FromImage(img)

(**
## Shaders

Shaders define how pixels are colored. Apply them with `Scene.withShader`.

### Linear Gradient
*)

let linearGradient =
    Shader.LinearGradient(
        SKPoint(0f, 0f), SKPoint(200f, 0f),
        [| SKColors.Red; SKColors.Yellow; SKColors.Blue |],
        [| 0f; 0.5f; 1f |],
        TileMode.Clamp)

let gradientScene =
    Scene.create SKColors.Black [
        Scene.rect 10f 10f 200f 60f (Scene.fill SKColors.White |> Scene.withShader linearGradient)
    ]

use gradientBitmap = renderScene 220 80 gradientScene

(*** include-value: gradientBitmap ***)

(**
### Radial Gradient
*)

let radialGradient =
    Shader.RadialGradient(
        SKPoint(60f, 60f), 50f,
        [| SKColors.White; SKColors.Blue |],
        [| 0f; 1f |],
        TileMode.Clamp)

let radialScene =
    Scene.create SKColors.Black [
        Scene.rect 10f 10f 100f 100f (Scene.fill SKColors.White |> Scene.withShader radialGradient)
    ]

use radialBitmap = renderScene 120 120 radialScene

(*** include-value: radialBitmap ***)

(**
### Sweep Gradient
*)

let sweepScene =
    Scene.create SKColors.Black [
        Scene.rect 0f 0f 100f 100f
            (Scene.fill SKColors.White
             |> Scene.withShader (Shader.SweepGradient(
                SKPoint(50f, 50f),
                [| SKColors.Red; SKColors.Green; SKColors.Blue; SKColors.Red |],
                [| 0f; 0.33f; 0.66f; 1f |], 0f, 360f)))
    ]

use sweepBitmap = renderScene 100 100 sweepScene

(*** include-value: sweepBitmap ***)

(**
### Perlin Noise
*)

let noiseScene =
    Scene.create SKColors.Black [
        Scene.rect 0f 0f 100f 100f
            (Scene.fill SKColors.White
             |> Scene.withShader (Shader.PerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0f)))
    ]

use noiseBitmap = renderScene 100 100 noiseScene

(*** include-value: noiseBitmap ***)

(**
### Shader Composition

Compose two shaders with a blend mode:
*)

let composedShader =
    Shader.Compose(
        Shader.SolidColor(SKColors.Red),
        Shader.LinearGradient(
            SKPoint(0f, 0f), SKPoint(100f, 0f),
            [| SKColors.Transparent; SKColors.White |],
            [| 0f; 1f |], TileMode.Clamp),
        BlendMode.SrcATop)

let composedScene =
    Scene.create SKColors.Black [
        Scene.rect 0f 0f 100f 60f (Scene.fill SKColors.White |> Scene.withShader composedShader)
    ]

use composedBitmap = renderScene 110 70 composedScene

(*** include-value: composedBitmap ***)

(**
<details>
<summary>All shader types</summary>

| Shader | Description |
|---|---|
| `LinearGradient` | Gradient along a line |
| `RadialGradient` | Gradient radiating from a center point |
| `SweepGradient` | Angular gradient around a center point |
| `TwoPointConicalGradient` | Gradient between two circles |
| `PerlinNoiseFractalNoise` | Fractal noise pattern |
| `PerlinNoiseTurbulence` | Turbulence noise pattern |
| `SolidColor` | Flat color fill |
| `Image` | Tiled bitmap fill |
| `Compose` | Blend two shaders together |
| `RuntimeEffect` | Custom SkSL shader program |

</details>

## Blend Modes

Blend modes control how elements composite over each other:
*)

let blendScene =
    Scene.create SKColors.White [
        Scene.rect 10f 10f 60f 60f (Scene.fill SKColors.Red)
        Scene.rect 40f 40f 60f 60f
            (Scene.fill SKColors.Green |> Scene.withBlendMode BlendMode.Multiply)
    ]

use blendBitmap = renderScene 110 110 blendScene

(*** include-value: blendBitmap ***)

(**
## Color Filters

Color filters transform the output colors of an element.

### Blend Mode Tinting
*)

let tintScene =
    Scene.create SKColors.Black [
        Scene.rect 10f 10f 80f 80f
            (Scene.fill SKColors.Red
             |> Scene.withColorFilter (ColorFilter.BlendMode(SKColors.Blue, BlendMode.SrcATop)))
    ]

use tintBitmap = renderScene 100 100 tintScene

(*** include-value: tintBitmap ***)

(**
### Grayscale via Color Matrix
*)

let grayscaleMatrix = [|
    0.21f; 0.72f; 0.07f; 0f; 0f
    0.21f; 0.72f; 0.07f; 0f; 0f
    0.21f; 0.72f; 0.07f; 0f; 0f
    0f;    0f;    0f;    1f; 0f
|]

let grayscaleScene =
    Scene.create SKColors.Black [
        Scene.rect 10f 10f 80f 80f
            (Scene.fill SKColors.Red
             |> Scene.withColorFilter (ColorFilter.ColorMatrix(grayscaleMatrix)))
    ]

use grayscaleBitmap = renderScene 100 100 grayscaleScene

(*** include-value: grayscaleBitmap ***)

(**
### Lighting Filter
*)

let lightingScene =
    Scene.create SKColors.Black [
        Scene.rect 10f 10f 80f 80f
            (Scene.fill SKColors.Red
             |> Scene.withColorFilter (ColorFilter.Lighting(SKColors.White, SKColor(50uy, 0uy, 0uy))))
    ]

use lightingBitmap = renderScene 100 100 lightingScene

(*** include-value: lightingBitmap ***)

(**
## Mask Filters

Mask filters operate on the alpha channel. The only mask filter is `Blur`:
*)

let blurScene =
    Scene.create SKColors.Black [
        Scene.rect 30f 30f 40f 40f
            (Scene.fill SKColors.White
             |> Scene.withMaskFilter (MaskFilter.Blur(BlurStyle.Normal, 5f)))
    ]

use blurBitmap = renderScene 100 100 blurScene

(*** include-value: blurBitmap ***)

(**
| Blur Style | Effect |
|---|---|
| `Normal` | Blur inside and outside the shape |
| `Solid` | Solid fill with blurred edges |
| `Outer` | Blur outside only (shadow effect) |
| `Inner` | Blur inside only |

## Image Filters

Image filters operate on the rendered pixels of an element.

### Drop Shadow
*)

let shadowScene =
    Scene.create SKColors.White [
        Scene.rect 20f 20f 60f 40f
            (Scene.fill SKColors.CornflowerBlue
             |> Scene.withImageFilter (ImageFilter.DropShadow(4f, 4f, 3f, 3f, SKColor(0uy, 0uy, 0uy, 128uy))))
    ]

use shadowBitmap = renderScene 110 80 shadowScene

(*** include-value: shadowBitmap ***)

(**
### Blur, Dilate, Erode
*)

let filterCompare =
    Scene.create SKColors.Black [
        // Original
        Scene.rect 10f 10f 30f 30f (Scene.fill SKColors.White)
        // Image blur
        Scene.rect 60f 10f 30f 30f
            (Scene.fill SKColors.White |> Scene.withImageFilter (ImageFilter.Blur(3f, 3f)))
        // Dilate
        Scene.rect 110f 10f 30f 30f
            (Scene.fill SKColors.White |> Scene.withImageFilter (ImageFilter.Dilate(3, 3)))
    ]

use filterBitmap = renderScene 160 60 filterCompare

(*** include-value: filterBitmap ***)

(**
### Composed and Merged Filters
*)

let composedFilterScene =
    Scene.create SKColors.Black [
        Scene.rect 20f 20f 60f 40f
            (Scene.fill SKColors.White
             |> Scene.withImageFilter (
                ImageFilter.Compose(
                    ImageFilter.Blur(2f, 2f),
                    ImageFilter.Offset(5f, 5f))))
    ]

use composedFilterBitmap = renderScene 110 80 composedFilterScene

(*** include-value: composedFilterBitmap ***)

(**
## Path Effects

Path effects modify stroke geometry.

### Dash Effect
*)

let dashScene =
    Scene.create SKColors.Black [
        Scene.line 10f 30f 290f 30f
            (Scene.stroke SKColors.White 3f
             |> Scene.withPathEffect (PathEffect.Dash([| 15f; 8f |], 0f)))
    ]

use dashBitmap = renderScene 300 60 dashScene

(*** include-value: dashBitmap ***)

(**
### Corner and Trim Effects
*)

let cornerTrimScene =
    Scene.create SKColors.Black [
        // Corner rounding
        Scene.path [
            PathCommand.MoveTo(10f, 50f)
            PathCommand.LineTo(50f, 10f)
            PathCommand.LineTo(90f, 50f)
        ] (Scene.stroke SKColors.Yellow 3f |> Scene.withPathEffect (PathEffect.Corner(15f)))

        // Trim — draw only the first half
        Scene.line 110f 30f 290f 30f
            (Scene.stroke SKColors.Cyan 3f
             |> Scene.withPathEffect (PathEffect.Trim(0f, 0.5f, TrimMode.Normal)))
    ]

use cornerTrimBitmap = renderScene 300 60 cornerTrimScene

(*** include-value: cornerTrimBitmap ***)

(**
### 1D Path Effect (Stamping)
*)

let stampScene =
    Scene.create SKColors.Black [
        Scene.line 10f 30f 290f 30f
            (Scene.fill SKColors.White
             |> Scene.withPathEffect (
                PathEffect.Path1D(
                    [ PathCommand.AddCircle(0f, 0f, 4f, PathDirection.Clockwise) ],
                    20f, 0f, Path1DStyle.Translate)))
    ]

use stampBitmap = renderScene 300 60 stampScene

(*** include-value: stampBitmap ***)

(**
## Runtime Effects (Custom SkSL Shaders)

Write custom shaders using SkSL. The shader source must define a `main` function
that returns a `half4` color:
*)

(*** do-not-eval ***)
let skslShader =
    Shader.RuntimeEffect(
        """
        uniform float2 iResolution;
        half4 main(float2 fragCoord) {
            float2 uv = fragCoord / iResolution;
            return half4(uv.x, uv.y, 0.5, 1.0);
        }
        """,
        [ "iResolution", 100f ])

(**
<div class="alert alert-warning">
<strong>Warning:</strong> Invalid SkSL source will throw an
<code>InvalidOperationException</code> with the compilation error message.
</div>

## Next Steps

- [Input Handling](input-handling.html) — keyboard, mouse, and window events
- [Screenshots](screenshots.html) — capture rendered frames
- [Architecture Overview](architecture.html) — how the rendering pipeline works
*)

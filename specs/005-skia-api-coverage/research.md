# Research: Comprehensive SkiaSharp API Coverage

**Branch**: `005-skia-api-coverage` | **Date**: 2026-04-09

## R1: SkiaSharp 2.88.6 API Availability

**Decision**: All target APIs are confirmed available in SkiaSharp 2.88.6.
**Rationale**: Verified against Microsoft Learn documentation with `skiasharp-2.88` moniker.
**Alternatives considered**: SkiaSharp 3.x has additional APIs (SKBlender class, SKRuntimeShaderBuilder, etc.) but upgrading would break existing stable dependency.

### Confirmed APIs

| Category | APIs | Status |
|----------|------|--------|
| SKPathEffect | CreateDash, CreateCorner, CreateTrim, Create1DPath, CreateCompose, CreateSum | All available |
| SKShader | CreateRadialGradient, CreateSweepGradient, CreateTwoPointConicalGradient, CreatePerlinNoiseFractalNoise, CreatePerlinNoiseTurbulence, CreateColor, CreateBitmap, CreateImage, CreateCompose | All available |
| SKBlendMode | All 29 modes (Clear through Luminosity) | All available |
| SKColorFilter | CreateBlendMode, CreateColorMatrix, CreateCompose, CreateHighContrast, CreateLighting, CreateLumaColor | All available |
| SKMaskFilter | CreateBlur (with SKBlurStyle: Normal, Solid, Outer, Inner) | Available |
| SKImageFilter | CreateBlur, CreateDropShadow, CreateDilate, CreateErode, CreateOffset, CreateColorFilter, CreateCompose, CreateMerge, CreateDisplacementMapEffect, CreateMatrixConvolution | All available |
| SKCanvas | ClipRect, ClipPath (with SKClipOperation), DrawPoints, DrawVertices, DrawArc | All available |
| SKFont/SKTypeface | FromFamilyName, SKFont constructor, MeasureText | All available |
| SKPath | Op (with SKPathOp), FillType, SKPathMeasure | All available |
| SKPicture/Recorder | BeginRecording, EndRecording, DrawPicture | All available |
| SKRegion | SetRect, SetPath, Op (with SKRegionOperation), Contains | All available |
| SKRuntimeEffect | CreateShader, CreateColorFilter, ToShader, ToColorFilter | Available (added in 2.88, not in 2.80) |
| SK3dView | Translate, RotateX/Y/Z, GetMatrix | Available |
| SKColorSpace | CreateSrgb, CreateSrgbLinear | Available |
| SKShaderTileMode | Clamp, Repeat, Mirror, Decal | All 4 modes available |
| SKTextBlob/Builder | Create, AddRun, AllocateRun, Build | All available |

### API Name Corrections

- SKRegion.Op uses `SKRegionOperation` enum (not `SKPathOp`)
- SKRuntimeEffect: static factories are `CreateShader(string, out string)` / `CreateColorFilter(string, out string)`; instance methods are `ToShader(...)` / `ToColorFilter(...)`
- SKFont constructor: `SKFont(SKTypeface, Single, Single, Single)` — (typeface, size, scaleX, skewX)
- SKImageFilter.CreateDisplacementMapEffect: `SKDisplacementMapEffectChannelSelectorType` overload is obsolete in 2.88; use `SKColorChannel` overload
- SKImageFilter.CreateMatrixConvolution: `SKMatrixConvolutionTileMode` overload is obsolete in 2.88; use `SKShaderTileMode` overload

## R2: Paint Record Extension Strategy

**Decision**: Add all new fields as required fields to the Paint record. No backward compatibility.
**Rationale**: User clarification during /speckit.clarify — no backward compatibility needed. All existing usages will be updated.
**Alternatives considered**: Optional fields (`option` type with defaults), separate ExtendedPaint record. Both rejected per user directive.

### New Paint Fields

The current Paint has 5 fields. Adding 9 new fields:
- `StrokeCap: StrokeCap` (default-like: Butt)
- `StrokeJoin: StrokeJoin` (default-like: Miter)
- `StrokeMiter: float32` (default-like: 4.0)
- `BlendMode: BlendMode` (default-like: SrcOver)
- `Shader: Shader option` (no fill shader by default)
- `ColorFilter: ColorFilter option` (no filter by default)
- `MaskFilter: MaskFilter option` (no filter by default)
- `ImageFilter: ImageFilter option` (no filter by default)
- `PathEffect: PathEffect option` (no effect by default)
- `Font: FontSpec option` (no custom font by default)

Note: StrokeCap, StrokeJoin, StrokeMiter, and BlendMode are value types with sensible defaults. Shader, ColorFilter, MaskFilter, ImageFilter, PathEffect, and Font are optional since most elements don't use them.

## R3: DSL Type Design Approach

**Decision**: Define new types as F# discriminated unions and records in Scene.fs, following the existing pattern. Each SkiaSharp concept maps to a DU with one case per factory method.
**Rationale**: Consistent with existing DSL design (Transform, PathCommand, Element are all DUs). F# DUs provide exhaustive pattern matching in the renderer.
**Alternatives considered**: Wrapping SkiaSharp types directly (rejected — breaks declarative/immutable contract), class hierarchy (rejected — not idiomatic F#).

## R4: Renderer Extension Strategy

**Decision**: Extend SceneRenderer.fs with new private helper functions for each new type category. The main `renderElement` function delegates to category-specific helpers.
**Rationale**: SceneRenderer already follows this pattern (e.g., `toMatrix`, `makeSKPaint`, `drawWithPaint`). Adding helpers for `toSKShader`, `toSKPathEffect`, `toSKColorFilter`, etc. keeps the module organized.
**Alternatives considered**: Separate renderer modules per category (rejected — unnecessary complexity for a single internal module).

## R5: Runtime Effects CPU Backend Error

**Decision**: Raise `System.NotSupportedException` when runtime effects are used on CPU raster backend.
**Rationale**: User clarification — must error, not degrade gracefully. SkSL requires GPU context; attempting to use it on CPU would produce incorrect results or crash in SkiaSharp internals.
**Alternatives considered**: Graceful degradation with warning (rejected per user directive), silent skip (rejected).

## R6: New Element Types

**Decision**: Add new Element DU cases for Points, Vertices, Arc, and Picture. Clipping is added to the Group case as an optional Clip parameter.
**Rationale**: Points, Vertices, Arc, and Picture are distinct drawing primitives that don't fit existing cases. Clipping is a group-level concern (restricts children), matching SkiaSharp's canvas save/clip/restore pattern.
**Alternatives considered**: Clipping as a separate wrapper element (rejected — clipping semantically belongs to groups, not individual elements).

## R7: Region and Path Utilities

**Decision**: Expose Region and PathMeasure as utility modules alongside the scene DSL, not as Element types. They are used for computation (hit testing, measurement), not direct rendering.
**Rationale**: Regions produce boolean results (containment tests). PathMeasure produces numeric results (lengths, positions). Neither is a visual element.
**Alternatives considered**: Embedding region ops in Element DU (rejected — regions are computational, not visual).

## R8: Text/Font Integration

**Decision**: Add an optional `Font: FontSpec option` field to Paint. The FontSpec record holds family name, weight, slant, and width. The renderer uses SKTypeface.FromFamilyName and SKFont to render text with the specified font. Text measurement is exposed as a utility function in the Scene module.
**Rationale**: Font is a paint-level concern in SkiaSharp (via SKPaint or SKFont). Making it part of Paint keeps the DSL consistent.
**Alternatives considered**: Separate FontSpec parameter on the Text element case (would change Element DU signature unnecessarily).

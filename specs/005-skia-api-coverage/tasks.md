# Tasks: Comprehensive SkiaSharp API Coverage

**Input**: Design documents from `/specs/005-skia-api-coverage/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/public-api.md

**Tests**: Included — constitution requires test evidence for behavior-changing code (Principle III).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No new project setup needed — extending existing project structure.

- [x] T001 Verify build passes before changes: `dotnet build src/SkiaViewer/SkiaViewer.fsproj`
- [x] T002 Verify tests pass before changes: `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define all new types, extend Paint record, and update all existing call sites. MUST complete before ANY user story rendering work.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Type Definitions

- [x] T003 Add all new DU types to `src/SkiaViewer/Scene.fsi` per contracts/public-api.md: StrokeCap, StrokeJoin, BlendMode, TileMode, Shader, TrimMode, Path1DStyle, PathEffect, HighContrastInvertStyle, ColorFilter, BlurStyle, MaskFilter, ColorChannel, ImageFilter, ClipOperation, Clip, FontSlant, FontSpec, PointMode, VertexMode, PathOp, PathFillType, PathDirection, RegionOp, Transform3D
- [x] T004 Add all new DU types to `src/SkiaViewer/Scene.fs` matching the .fsi declarations from T003
- [x] T005 Extend Paint record in `src/SkiaViewer/Scene.fsi` with 10 new fields (StrokeCap, StrokeJoin, StrokeMiter, BlendMode, Shader, ColorFilter, MaskFilter, ImageFilter, PathEffect, Font) per contracts/public-api.md
- [x] T006 Extend Paint record in `src/SkiaViewer/Scene.fs` matching the .fsi from T005
- [x] T007 Extend PathCommand DU in `src/SkiaViewer/Scene.fsi` with AddRect, AddCircle, AddOval, AddRoundRect cases
- [x] T008 Extend PathCommand DU in `src/SkiaViewer/Scene.fs` matching the .fsi from T007
- [x] T009 Extend Transform DU in `src/SkiaViewer/Scene.fsi` with Perspective case
- [x] T010 Extend Transform DU in `src/SkiaViewer/Scene.fs` matching the .fsi from T009
- [x] T011 Extend Element DU in `src/SkiaViewer/Scene.fsi` with Group clip parameter and new cases (Points, Vertices, Arc, Picture)
- [x] T012 Extend Element DU in `src/SkiaViewer/Scene.fs` matching the .fsi from T011

### Scene Module Helpers

- [x] T013 Update `emptyPaint` in `src/SkiaViewer/Scene.fs` with defaults for all new fields (StrokeCap.Butt, StrokeJoin.Miter, StrokeMiter=4f, BlendMode.SrcOver, all optional = None)
- [x] T014 Add `with*` paint modifier functions to `src/SkiaViewer/Scene.fsi`: withStrokeCap, withStrokeJoin, withBlendMode, withShader, withColorFilter, withMaskFilter, withImageFilter, withPathEffect, withFont
- [x] T015 Implement `with*` paint modifier functions in `src/SkiaViewer/Scene.fs`
- [x] T016 Update existing Scene module functions (fill, stroke, fillStroke) in `src/SkiaViewer/Scene.fs` to construct Paint with all new fields
- [x] T017 Add new element constructors to `src/SkiaViewer/Scene.fsi`: points, vertices, arc, picture, groupWithClip
- [x] T018 Implement new element constructors in `src/SkiaViewer/Scene.fs`
- [x] T019 Update existing `group`, `translate`, `rotate`, `scale` functions in `src/SkiaViewer/Scene.fs` to pass `clip = None` to the extended Group case

### Update Existing Call Sites

- [x] T020 Update all Paint constructions in `src/SkiaViewer/Viewer.fs` to include new fields
- [x] T021 Update all Paint constructions and Group patterns in `src/SkiaViewer/SceneRenderer.fs` to include new fields and clip parameter
- [x] T022 Update all Paint constructions in `tests/SkiaViewer.Tests/SceneTests.fs` to include new fields
- [x] T023 Update all Paint constructions and Element patterns in `tests/SkiaViewer.Tests/SceneRendererTests.fs` to include new fields
- [x] T024 Update all Paint constructions in `tests/SkiaViewer.Tests/ViewerTests.fs` to include new fields
- [x] T025 Update Paint constructions in `tests/SkiaViewer.PerfTests/SceneGenerators.fs` to include new fields
- [x] T026 Update Paint constructions in `tests/SkiaViewer.PerfTests/Benchmarks.fs` to include new fields
- [x] T027 Update `scripts/prelude.fsx` paint helpers for new fields
- [x] T028 Update `scripts/examples/01-screenshot.fsx` for new Paint fields
- [x] T029 Update `scripts/examples/02-declarative-scene.fsx` for new Paint fields
- [x] T030 Update `docs/drawing-primitives.fsx` for new Paint fields
- [x] T031 Update `docs/getting-started.fsx` for new Paint fields
- [x] T032 Update `docs/input-handling.fsx` for new Paint fields
- [x] T033 Update `docs/tests.fsx` for new Paint fields
- [x] T034 Handle new PathCommand cases (AddRect, AddCircle, AddOval, AddRoundRect) in SceneRenderer.fs path building logic
- [x] T035 Handle new Element cases (Points, Vertices, Arc, Picture) as stubs (raise NotImplementedException) in `src/SkiaViewer/SceneRenderer.fs` renderElement function
- [x] T036 Handle Transform.Perspective case as stub in `src/SkiaViewer/SceneRenderer.fs` toMatrix function
- [x] T037 Verify full project builds: `dotnet build src/SkiaViewer/SkiaViewer.fsproj`
- [x] T038 Verify all existing tests pass: `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj`

**Checkpoint**: All types defined, project compiles, existing tests pass. User story implementation can now begin.

---

## Phase 3: User Story 1 — Stroke Styling and Path Effects (Priority: P1) 🎯 MVP

**Goal**: Developers can control line appearance (cap, join, miter) and apply path effects (dash, corner, trim, 1D, compose, sum) through the DSL.

**Independent Test**: Render shapes with various stroke caps/joins and dash patterns, verify pixel-level differences.

### Tests for User Story 1

- [x] T039 [P] [US1] Test stroke cap rendering (Butt, Round, Square produce different output) in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T040 [P] [US1] Test stroke join rendering (Miter, Round, Bevel produce different output) in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T041 [P] [US1] Test stroke miter limit in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T042 [P] [US1] Test dash path effect rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T043 [P] [US1] Test corner path effect rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T044 [P] [US1] Test trim path effect rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T044b [P] [US1] Test compose path effect rendering (two effects combined) in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T044c [P] [US1] Test 1D path effect rendering (stamp path along another) in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T044d [P] [US1] Test sum path effect rendering (both effects applied independently) in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 1

- [x] T045 [US1] Implement stroke cap/join/miter application in `makeSKPaint` function in `src/SkiaViewer/SceneRenderer.fs` — map StrokeCap/StrokeJoin/StrokeMiter to SKPaint.StrokeCap/StrokeJoin/StrokeMiter
- [x] T046 [US1] Implement `toSKPathEffect` helper in `src/SkiaViewer/SceneRenderer.fs` — convert PathEffect DU to SKPathEffect via CreateDash/CreateCorner/CreateTrim/Create1DPath/CreateCompose/CreateSum
- [x] T047 [US1] Apply PathEffect in `makeSKPaint` function in `src/SkiaViewer/SceneRenderer.fs` — set SKPaint.PathEffect from Paint.PathEffect

**Checkpoint**: Stroke styling and path effects fully functional and tested.

---

## Phase 4: User Story 2 — Shader System (Priority: P1)

**Goal**: Developers can fill shapes with radial, sweep, conical, Perlin noise, solid color, image, and composed shaders.

**Independent Test**: Render shapes with each shader type and verify fill patterns differ from solid fill.

### Tests for User Story 2

- [x] T048 [P] [US2] Test radial gradient shader rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T049 [P] [US2] Test sweep gradient shader rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T050 [P] [US2] Test two-point conical gradient rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T051 [P] [US2] Test Perlin noise shader rendering (fractal + turbulence) in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T052 [P] [US2] Test solid color and image shader rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T053 [P] [US2] Test composed shader rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 2

- [x] T054 [US2] Implement `toSKShader` helper in `src/SkiaViewer/SceneRenderer.fs` — convert Shader DU to SKShader via CreateLinearGradient/CreateRadialGradient/CreateSweepGradient/CreateTwoPointConicalGradient/CreatePerlinNoiseFractalNoise/CreatePerlinNoiseTurbulence/CreateColor/CreateBitmap/CreateCompose
- [x] T055 [US2] Implement `toSKShaderTileMode` helper in `src/SkiaViewer/SceneRenderer.fs` — map TileMode DU to SKShaderTileMode
- [x] T056 [US2] Apply Shader in `makeSKPaint` function in `src/SkiaViewer/SceneRenderer.fs` — set SKPaint.Shader from Paint.Shader

**Checkpoint**: All shader types render correctly.

---

## Phase 5: User Story 3 — Blend Modes (Priority: P1)

**Goal**: Developers can control how colors combine where shapes overlap using any of 29 standard blend modes.

**Independent Test**: Render overlapping shapes with different blend modes and verify pixel differences.

### Tests for User Story 3

- [x] T057 [P] [US3] Test blend mode rendering (Multiply produces different output than SrcOver) in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T058 [P] [US3] Test Screen blend mode produces lighter result in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 3

- [x] T059 [US3] Implement `toSKBlendMode` helper in `src/SkiaViewer/SceneRenderer.fs` — map BlendMode DU to SKBlendMode enum
- [x] T060 [US3] Apply BlendMode in `makeSKPaint` function in `src/SkiaViewer/SceneRenderer.fs` — set SKPaint.BlendMode from Paint.BlendMode

**Checkpoint**: All 29 blend modes work correctly.

---

## Phase 6: User Story 4 — Color Filters (Priority: P2)

**Goal**: Developers can apply color transformations (tinting, color matrix, high contrast, lighting, luma) to rendered elements.

**Independent Test**: Render a colored shape with a grayscale color matrix and verify desaturation.

### Tests for User Story 4

- [x] T061 [P] [US4] Test blend-mode color filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T062 [P] [US4] Test color matrix filter (grayscale) rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T063 [P] [US4] Test composed color filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T064 [P] [US4] Test high contrast and lighting color filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 4

- [x] T065 [US4] Implement `toSKColorFilter` helper in `src/SkiaViewer/SceneRenderer.fs` — convert ColorFilter DU to SKColorFilter via CreateBlendMode/CreateColorMatrix/CreateCompose/CreateHighContrast/CreateLighting/CreateLumaColor
- [x] T066 [US4] Apply ColorFilter in `makeSKPaint` function in `src/SkiaViewer/SceneRenderer.fs` — set SKPaint.ColorFilter from Paint.ColorFilter

**Checkpoint**: All color filter types produce correct transformations.

---

## Phase 7: User Story 5 — Mask Filters (Priority: P2)

**Goal**: Developers can apply blur effects (normal, solid, outer, inner) to shape edges.

**Independent Test**: Render a sharp rectangle with blur mask filter and verify edges are softened.

### Tests for User Story 5

- [x] T067 [P] [US5] Test blur mask filter (Normal style) rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T068 [P] [US5] Test blur mask filter styles (Inner, Outer, Solid) produce different output in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 5

- [x] T069 [US5] Implement `toSKMaskFilter` helper in `src/SkiaViewer/SceneRenderer.fs` — convert MaskFilter DU to SKMaskFilter via CreateBlur with SKBlurStyle mapping
- [x] T070 [US5] Apply MaskFilter in `makeSKPaint` function in `src/SkiaViewer/SceneRenderer.fs` — set SKPaint.MaskFilter from Paint.MaskFilter

**Checkpoint**: Blur mask filter works with all 4 styles.

---

## Phase 8: User Story 6 — Image Filters (Priority: P2)

**Goal**: Developers can apply image-level effects (drop shadow, blur, dilate, erode, offset, color filter, compose, merge, displacement map, matrix convolution).

**Independent Test**: Render a shape with drop shadow and verify shadow appears offset.

### Tests for User Story 6

- [x] T071 [P] [US6] Test drop shadow image filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T072 [P] [US6] Test blur image filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T073 [P] [US6] Test dilate/erode image filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T074 [P] [US6] Test composed image filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T075 [P] [US6] Test offset and color filter image filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T075b [P] [US6] Test merge image filter rendering (multiple filters combined) in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T075c [P] [US6] Test displacement map image filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T075d [P] [US6] Test matrix convolution image filter rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 6

- [x] T076 [US6] Implement `toSKImageFilter` helper in `src/SkiaViewer/SceneRenderer.fs` — convert ImageFilter DU to SKImageFilter via CreateBlur/CreateDropShadow/CreateDilate/CreateErode/CreateOffset/CreateColorFilter/CreateCompose/CreateMerge/CreateDisplacementMapEffect/CreateMatrixConvolution
- [x] T077 [US6] Apply ImageFilter in `makeSKPaint` function in `src/SkiaViewer/SceneRenderer.fs` — set SKPaint.ImageFilter from Paint.ImageFilter

**Checkpoint**: All 10 image filter types render correctly.

---

## Phase 9: User Story 7 — Canvas Clipping (Priority: P2)

**Goal**: Developers can restrict drawing to rectangular or path-based clip regions on groups.

**Independent Test**: Render a large shape with a small clip rect and verify only the clipped portion is visible.

### Tests for User Story 7

- [x] T078 [P] [US7] Test rectangular clip (Intersect) rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T079 [P] [US7] Test path-based clip rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T080 [P] [US7] Test clip with Difference operation in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 7

- [x] T081 [US7] Implement clip application in Group rendering in `src/SkiaViewer/SceneRenderer.fs` — after canvas.Save, apply ClipRect or ClipPath with ClipOperation and antialias before rendering children
- [x] T082 [US7] Implement `toSKClipOperation` helper in `src/SkiaViewer/SceneRenderer.fs` — map ClipOperation DU to SKClipOperation

**Checkpoint**: Rect and path clipping with Intersect/Difference both work.

---

## Phase 10: User Story 8 — Text and Font System (Priority: P2)

**Goal**: Developers can render text with specific typefaces, weights, slants, and measure text bounds.

**Independent Test**: Render text with a named typeface and verify output differs from default font.

### Tests for User Story 8

- [x] T083 [P] [US8] Test text rendering with custom typeface in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T084 [P] [US8] Test text rendering with italic slant in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T085 [P] [US8] Test measureText returns non-zero bounds in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T086 [P] [US8] Test typeface fallback for unavailable family in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T086b [P] [US8] Test text blob with multiple positioned runs rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 8

- [x] T087 [US8] Implement font application in text rendering in `src/SkiaViewer/SceneRenderer.fs` — when Paint.Font is Some, create SKTypeface.FromFamilyName and SKFont, apply to SKPaint
- [x] T088 [US8] Implement `defaultFont` value in `src/SkiaViewer/Scene.fs`
- [x] T089 [US8] Add `measureText` and `defaultFont` to `src/SkiaViewer/Scene.fsi`
- [x] T090 [US8] Implement `measureText` utility function in `src/SkiaViewer/Scene.fs` — create SKFont/SKPaint, call MeasureText, return SKRect bounds
- [x] T090b [US8] Add TextBlob element case to Element DU in `src/SkiaViewer/Scene.fsi` and `src/SkiaViewer/Scene.fs` — TextBlob of runs: (string * SKPoint * float32 * FontSpec option) list * paint: Paint
- [x] T090c [US8] Implement TextBlob element rendering in `src/SkiaViewer/SceneRenderer.fs` — create SKTextBlobBuilder, call AllocateRun/AddRun for each run, draw via canvas.DrawText with blob

**Checkpoint**: Custom fonts render correctly, text measurement works, text blobs with positioned runs supported.

---

## Phase 11: User Story 9 — Path Operations (Priority: P3)

**Goal**: Developers can combine paths with boolean operations, measure paths, extract segments, control fill types, and use convenience path commands.

**Independent Test**: Union two overlapping circular paths and verify the merged shape covers both areas.

### Tests for User Story 9

- [x] T091 [P] [US9] Test combinePaths Union produces merged path in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T092 [P] [US9] Test combinePaths Intersect produces overlap-only path in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T093 [P] [US9] Test measurePath returns non-zero length in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T094 [P] [US9] Test extractPathSegment returns partial path in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T095 [P] [US9] Test withFillType EvenOdd rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 9

- [x] T096 [US9] Add path operation functions to `src/SkiaViewer/Scene.fsi`: combinePaths, measurePath, extractPathSegment, withFillType
- [x] T097 [US9] Implement `combinePaths` in `src/SkiaViewer/Scene.fs` — build two SKPaths from PathCommand lists, call SKPath.Op with mapped SKPathOp, convert result back to PathCommand list
- [x] T098 [US9] Implement `measurePath` in `src/SkiaViewer/Scene.fs` — build SKPath, create SKPathMeasure, return Length
- [x] T099 [US9] Implement `extractPathSegment` in `src/SkiaViewer/Scene.fs` — build SKPath, create SKPathMeasure, call GetSegment, convert result back to PathCommand list
- [x] T100 [US9] Implement `withFillType` in `src/SkiaViewer/Scene.fs` — create Path element with fill type stored for renderer
- [x] T101 [US9] Apply PathFillType in path rendering in `src/SkiaViewer/SceneRenderer.fs` — set SKPath.FillType before drawing

**Checkpoint**: Boolean path ops, measurement, segment extraction, and fill types all work.

---

## Phase 12: User Story 10 — Picture Recording and Playback (Priority: P3)

**Goal**: Developers can record drawing operations into a reusable picture and replay it at different positions/transforms.

**Independent Test**: Record elements to a picture, draw it twice at different positions, verify both appear.

### Tests for User Story 10

- [x] T102 [P] [US10] Test recordPicture returns non-null SKPicture in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T103 [P] [US10] Test Picture element rendering draws recorded content in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 10

- [x] T104 [US10] Add recordPicture function to `src/SkiaViewer/Scene.fsi`
- [x] T105 [US10] Implement `recordPicture` in `src/SkiaViewer/Scene.fs` — create SKPictureRecorder, call BeginRecording, render elements via SceneRenderer, call EndRecording
- [x] T106 [US10] Implement Picture element rendering in `src/SkiaViewer/SceneRenderer.fs` — call canvas.DrawPicture with optional transform

**Checkpoint**: Picture recording and playback work correctly.

---

## Phase 13: User Story 11 — Regions (Priority: P3)

**Goal**: Developers can create regions from rects/paths, combine with boolean ops, and test point containment.

**Independent Test**: Create region from rect, verify point containment returns true inside and false outside.

### Tests for User Story 11

- [x] T107 [P] [US11] Test createRegionFromRect and regionContains in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T108 [P] [US11] Test combineRegions Union in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T109 [P] [US11] Test createRegionFromPath in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T109b [P] [US11] Test region used as canvas clip restricts rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 11

- [x] T110 [US11] Add region functions to `src/SkiaViewer/Scene.fsi`: createRegionFromRect, createRegionFromPath, combineRegions, regionContains
- [x] T111 [US11] Implement region utility functions in `src/SkiaViewer/Scene.fs` — wrap SKRegion creation, SetRect/SetPath, Op with SKRegionOperation, Contains
- [x] T111b [US11] Add Clip.Region case to Clip DU in `src/SkiaViewer/Scene.fsi` and `src/SkiaViewer/Scene.fs` — Region of region: SKRegion * operation: ClipOperation
- [x] T111c [US11] Implement Clip.Region rendering in Group clip handling in `src/SkiaViewer/SceneRenderer.fs` — call canvas.ClipRegion with SKRegion and operation

**Checkpoint**: Region creation, boolean ops, containment testing, and region-as-clip all work.

---

## Phase 14: User Story 12 — Canvas Drawing Extensions (Priority: P3)

**Goal**: Developers can draw points (dots/lines/polygon), vertices (triangles), and arcs through the DSL.

**Independent Test**: Render points in Points mode and verify dots appear at specified coordinates.

### Tests for User Story 12

- [x] T112 [P] [US12] Test Points element rendering in each mode in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T113 [P] [US12] Test Vertices element rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T114 [P] [US12] Test Arc element rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 12

- [x] T115 [US12] Implement Points element rendering in `src/SkiaViewer/SceneRenderer.fs` — map PointMode DU to SKPointMode, call canvas.DrawPoints
- [x] T116 [US12] Implement Vertices element rendering in `src/SkiaViewer/SceneRenderer.fs` — map VertexMode DU to SKVertexMode, create SKVertices, call canvas.DrawVertices
- [x] T117 [US12] Implement Arc element rendering in `src/SkiaViewer/SceneRenderer.fs` — call canvas.DrawArc with rect, angles, useCenter

**Checkpoint**: Points, Vertices, and Arc elements render correctly.

---

## Phase 15: User Story 13 — Runtime Effects / SkSL (Priority: P3)

**Goal**: Developers can write custom SkSL shaders and apply them as shaders or color filters. CPU backend raises error.

**Independent Test**: Compile a SkSL shader that outputs solid red, apply to a shape, verify red output.

### Tests for User Story 13

- [x] T118 [P] [US13] Test SkSL shader compilation and rendering in `tests/SkiaViewer.Tests/SceneRendererTests.fs`
- [x] T119 [P] [US13] Test invalid SkSL reports compilation error in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T120 [P] [US13] Test runtime effect on CPU backend raises NotSupportedException in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 13

- [x] T121 [US13] Add RuntimeEffect Shader case to Shader DU in `src/SkiaViewer/Scene.fsi` and `src/SkiaViewer/Scene.fs` — RuntimeEffect of source: string * uniforms: (string * float32)[]
- [x] T122 [US13] Implement SkSL shader compilation in `toSKShader` helper in `src/SkiaViewer/SceneRenderer.fs` — call SKRuntimeEffect.CreateShader, set uniforms via ToShader
- [x] T123 [US13] Implement CPU backend detection and NotSupportedException in `src/SkiaViewer/SceneRenderer.fs` for runtime effect shaders

**Checkpoint**: SkSL shaders compile, render on GPU, and error on CPU.

---

## Phase 16: User Story 14 — Color Space Management (Priority: P3)

**Goal**: Developers can create surfaces with specific color spaces and benefit from automatic color conversion.

**Independent Test**: Create surface with sRGB color space and verify the color space property.

### Tests for User Story 14

- [x] T124 [P] [US14] Test SKColorSpace.CreateSrgb and CreateSrgbLinear availability in `tests/SkiaViewer.Tests/SceneTests.fs`

### Implementation for User Story 14

- [x] T125 [US14] Add color space parameter to surface creation utilities (if any exist) or document as available via direct SkiaSharp API in `src/SkiaViewer/Scene.fs`

**Checkpoint**: Color space management available.

---

## Phase 17: User Story 15 — 3D View Utility (Priority: P3)

**Goal**: Developers can apply 3D perspective transformations (rotation around X/Y/Z axes) to 2D elements.

**Independent Test**: Apply Y-axis rotation to a rectangle and verify trapezoidal output (perspective distortion).

### Tests for User Story 15

- [x] T126 [P] [US15] Test Transform.Perspective with RotateY produces non-identity matrix in `tests/SkiaViewer.Tests/SceneTests.fs`
- [x] T127 [P] [US15] Test 3D perspective rendering produces foreshortened output in `tests/SkiaViewer.Tests/SceneRendererTests.fs`

### Implementation for User Story 15

- [x] T128 [US15] Implement `toMatrix` for Transform.Perspective case in `src/SkiaViewer/SceneRenderer.fs` — create SK3dView, apply Transform3D operations (RotateX/Y/Z, Translate, Camera), call GetMatrix
- [x] T129 [US15] Implement `toSK3dView` helper in `src/SkiaViewer/SceneRenderer.fs` — walk Transform3D DU, apply each operation to SK3dView instance

**Checkpoint**: 3D perspective transforms render correctly.

---

## Phase 18: Polish & Cross-Cutting Concerns

**Purpose**: Final integration, documentation, baseline update, scripting.

- [x] T130 [P] Update surface-area baseline in `tests/SkiaViewer.Tests/SurfaceAreaBaseline.txt` to reflect all new types and Scene module functions
- [x] T131 [P] Create `scripts/examples/04-effects-showcase.fsx` — demonstrate shaders, filters, blend modes, path effects with new DSL
- [x] T132 [P] Create `scripts/examples/05-advanced-features.fsx` — demonstrate path ops, regions, runtime effects, 3D, text/font
- [x] T133 Update `scripts/prelude.fsx` with new paint helpers and convenience functions
- [x] T134 Update `docs/drawing-primitives.fsx` with new drawing examples (shaders, effects, blend modes)
- [x] T135 [P] Update `docs/tests.fsx` with new test documentation
- [x] T136 Run full test suite: `dotnet test tests/SkiaViewer.Tests/SkiaViewer.Tests.fsproj`
- [x] T137 Run `dotnet pack src/SkiaViewer/SkiaViewer.fsproj -o ~/.local/share/nuget-local/` to verify packaging
- [x] T138 Verify quickstart.md examples compile and run correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Stories (Phase 3–17)**: All depend on Foundational phase completion
  - P1 stories (US1-3) should complete before P2 stories (US4-8)
  - P2 stories should complete before P3 stories (US9-15)
  - Within same priority: can proceed in parallel
- **Polish (Phase 18)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (Stroke + Path Effects)**: After Foundational — no other story dependencies
- **US2 (Shaders)**: After Foundational — no other story dependencies
- **US3 (Blend Modes)**: After Foundational — no other story dependencies
- **US4 (Color Filters)**: After Foundational — no other story dependencies
- **US5 (Mask Filters)**: After Foundational — no other story dependencies
- **US6 (Image Filters)**: After Foundational — depends on US4 (ColorFilter used by ImageFilter.ColorFilter case)
- **US7 (Clipping)**: After Foundational — no other story dependencies
- **US8 (Text/Font)**: After Foundational — no other story dependencies
- **US9 (Path Ops)**: After Foundational — no other story dependencies
- **US10 (Pictures)**: After Foundational — no other story dependencies
- **US11 (Regions)**: After Foundational — no other story dependencies
- **US12 (Drawing Extensions)**: After Foundational — no other story dependencies
- **US13 (Runtime Effects)**: After US2 (extends Shader DU rendering)
- **US14 (Color Space)**: After Foundational — no other story dependencies
- **US15 (3D View)**: After Foundational — no other story dependencies

### Within Each User Story

- Tests written FIRST, must FAIL before implementation
- Implementation follows test writing
- Story complete when all tests pass

### Parallel Opportunities

- T020–T036 (call site updates) can be partially parallelized across different files
- All test tasks marked [P] within a story can run in parallel
- P1 stories (US1, US2, US3) can run in parallel after Foundational
- P2 stories (US4, US5, US7, US8) can run in parallel; US6 after US4
- P3 stories (US9–US12, US14, US15) can run in parallel; US13 after US2

---

## Parallel Example: User Story 1

```text
# Launch tests in parallel:
T039: Test stroke cap rendering in SceneRendererTests.fs
T040: Test stroke join rendering in SceneRendererTests.fs
T041: Test stroke miter rendering in SceneRendererTests.fs
T042: Test dash path effect rendering in SceneRendererTests.fs
T043: Test corner path effect rendering in SceneRendererTests.fs
T044: Test trim path effect rendering in SceneRendererTests.fs

# Then implement sequentially:
T045: Implement stroke cap/join/miter in makeSKPaint
T046: Implement toSKPathEffect helper
T047: Apply PathEffect in makeSKPaint
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T002)
2. Complete Phase 2: Foundational (T003–T038)
3. Complete Phase 3: User Story 1 — Stroke Styling + Path Effects (T039–T047)
4. **STOP and VALIDATE**: All stroke/path effect tests pass
5. Pack and deploy if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 (Stroke + Effects) → MVP
3. US2 (Shaders) + US3 (Blend Modes) → Core effects complete
4. US4–US8 (Filters, Clipping, Text) → Production-ready effects pipeline
5. US9–US15 (Path ops, Pictures, Regions, Runtime, 3D) → Full API coverage
6. Polish → Documentation, baseline, scripts

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable after Foundational phase
- Constitution requires test evidence (Principle III) — tests are included
- Commit after each story completion
- Total: 149 tasks across 18 phases (15 user stories + setup + foundational + polish)

# Feature Specification: Comprehensive SkiaSharp API Coverage

**Feature Branch**: `005-skia-api-coverage`  
**Created**: 2026-04-09  
**Status**: Draft  
**Input**: User description: "Add remaining SkiaSharp API coverage: stroke styles, path effects, shaders, blend modes, color filters, mask filters, image filters, clipping, text/font system, path operations, regions, pictures, runtime effects, color spaces, 3D view, vertices, and drawables. Excludes SVG and PDF."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Stroke Styling and Path Effects (Priority: P1)

A developer building a visualization wants to control how lines and shape outlines appear — dashed borders, rounded line caps, specific join styles — without dropping down to raw SkiaSharp. They specify stroke caps, joins, miter limits, and path effects (dash patterns, corner rounding, path trimming) through the declarative scene DSL.

**Why this priority**: Stroke styling is the most commonly needed missing feature. Nearly every non-trivial drawing requires control over line appearance, and dashed/dotted lines are ubiquitous in charts, diagrams, and UI.

**Independent Test**: Can be fully tested by rendering shapes with various stroke caps (butt, round, square), join styles (miter, round, bevel), and dash patterns, then verifying the rendered output differs from the default stroke behavior.

**Acceptance Scenarios**:

1. **Given** a scene with a rectangle element, **When** the paint specifies `StrokeCap = Round` and `StrokeJoin = Bevel`, **Then** the rendered rectangle shows rounded line ends and beveled corners.
2. **Given** a scene with a line element, **When** the paint specifies a dash path effect with intervals `[|10f; 5f|]`, **Then** the rendered line appears as a dashed pattern with 10-unit dashes and 5-unit gaps.
3. **Given** a scene with a path element, **When** the paint specifies a corner path effect with radius 8, **Then** sharp corners in the path are rounded with radius 8.
4. **Given** a scene with a path, **When** a trim path effect is applied with start 0.0 and end 0.5, **Then** only the first half of the path is rendered.

---

### User Story 2 - Shader System (Priority: P1)

A developer wants to fill shapes with gradients (radial, sweep, two-point conical), solid color shaders, image-based shaders, Perlin noise patterns, and composed shader combinations. They express these through the DSL without manually creating SkiaSharp shader objects.

**Why this priority**: Gradients and pattern fills are fundamental to any graphics application. The project currently supports only linear gradients — adding the remaining shader types completes a core rendering capability.

**Independent Test**: Can be tested by rendering shapes with each shader type and verifying the fill pattern matches expectations (e.g., radial gradient radiates from center, sweep gradient rotates around a point).

**Acceptance Scenarios**:

1. **Given** a circle element, **When** the paint specifies a radial gradient shader from center with two colors, **Then** the fill smoothly transitions from the center color to the edge color.
2. **Given** a rectangle element, **When** the paint specifies a sweep gradient shader, **Then** colors sweep around the specified center point.
3. **Given** a rectangle element, **When** the paint specifies a two-point conical gradient, **Then** colors transition between two circles of specified centers and radii.
4. **Given** a rectangle element, **When** the paint specifies a Perlin noise shader (fractal noise or turbulence), **Then** the fill shows a procedurally generated noise pattern.
5. **Given** a shape, **When** the paint specifies a composed shader combining two shaders with a blend mode, **Then** the resulting fill blends both shader outputs.
6. **Given** a shape, **When** the paint specifies an image shader with a tile mode, **Then** the image tiles or clamps according to the specified mode.

---

### User Story 3 - Blend Modes (Priority: P1)

A developer compositing multiple layers or shapes needs to control how colors combine where shapes overlap. They specify blend modes (e.g., Multiply, Screen, Overlay, ColorDodge) on elements through the DSL.

**Why this priority**: Blend modes are essential for compositing and are one of the most commonly requested 2D graphics features. Without them, layered graphics cannot achieve standard visual effects.

**Independent Test**: Can be tested by rendering two overlapping shapes with different blend modes and verifying the overlap region pixels differ per blend mode.

**Acceptance Scenarios**:

1. **Given** two overlapping rectangles, **When** the top rectangle uses blend mode `Multiply`, **Then** the overlap region shows the multiplied color values.
2. **Given** a scene with an element using `BlendMode = Screen`, **When** rendered over a colored background, **Then** the result is lighter than either input.
3. **Given** the DSL, **When** a developer specifies any of the standard blend modes (Clear, Src, Dst, SrcOver, DstOver, SrcIn, DstIn, SrcOut, DstOut, SrcATop, DstATop, Xor, Plus, Modulate, Screen, Overlay, Darken, Lighten, ColorDodge, ColorBurn, HardLight, SoftLight, Difference, Exclusion, Multiply, Hue, Saturation, Color, Luminosity), **Then** the system applies that blend mode during rendering.

---

### User Story 4 - Color Filters (Priority: P2)

A developer wants to apply color transformations to rendered elements — tinting, color matrix transformations, high contrast adjustments, or lighting effects. They add color filters to the paint in the DSL.

**Why this priority**: Color filters enable post-processing effects on individual elements without modifying the source colors, which is important for accessibility (high contrast) and visual effects.

**Independent Test**: Can be tested by rendering a multicolored shape with a color matrix filter (e.g., grayscale) and verifying all output pixels are desaturated.

**Acceptance Scenarios**:

1. **Given** a colored shape, **When** a blend-mode color filter is applied with a tint color, **Then** the output color is the blend of the shape color and the tint.
2. **Given** a colored shape, **When** a 5x4 color matrix filter is applied for grayscale, **Then** the rendered pixels are desaturated.
3. **Given** two color filters, **When** they are composed, **Then** the output reflects both filters applied in sequence.
4. **Given** a shape, **When** a high contrast color filter is applied, **Then** the output shows increased contrast per the configuration.
5. **Given** a shape, **When** a lighting color filter is applied with multiply and add colors, **Then** the output pixels are adjusted accordingly.

---

### User Story 5 - Mask Filters (Priority: P2)

A developer wants to apply blur effects to shapes — inner blur, outer blur, normal blur, or solid blur styles. They specify mask filters through the DSL paint.

**Why this priority**: Blur is the most common mask filter and is widely used for shadows, focus effects, and UI depth cues.

**Independent Test**: Can be tested by rendering a sharp-edged shape with a blur mask filter and verifying the edges are softened in the output.

**Acceptance Scenarios**:

1. **Given** a rectangle, **When** a blur mask filter with `Normal` style and sigma 5 is applied, **Then** the shape edges are blurred symmetrically.
2. **Given** a rectangle, **When** a blur mask filter with `Inner` style is applied, **Then** only the interior edges are blurred.
3. **Given** a rectangle, **When** a blur mask filter with `Outer` style is applied, **Then** only the exterior edges are blurred.
4. **Given** a rectangle, **When** a blur mask filter with `Solid` style is applied, **Then** the shape remains solid with a blurred halo.

---

### User Story 6 - Image Filters (Priority: P2)

A developer wants to apply image-level effects — drop shadows, blur, dilation, erosion, color filter effects, offset, displacement maps, matrix convolution, and composed filters. They express these as image filters in the DSL.

**Why this priority**: Image filters enable the most visually impactful effects (drop shadows alone are near-universal in UI rendering) and compose well for complex post-processing.

**Independent Test**: Can be tested by rendering a shape with a drop shadow image filter and verifying a shadow appears offset from the shape.

**Acceptance Scenarios**:

1. **Given** a shape, **When** a drop shadow image filter is applied with offset (5, 5) and blur sigma 3, **Then** a shadow appears 5 units below and right of the shape.
2. **Given** a shape, **When** a blur image filter with sigma (3, 3) is applied, **Then** the entire shape and fill are blurred.
3. **Given** a shape, **When** a dilate image filter with radius (2, 2) is applied, **Then** the shape expands outward.
4. **Given** a shape, **When** an erode image filter with radius (2, 2) is applied, **Then** the shape contracts inward.
5. **Given** two image filters, **When** they are composed, **Then** the output reflects both filters applied in order.
6. **Given** a shape, **When** a color filter image filter wrapping a grayscale color filter is applied, **Then** the shape renders in grayscale.
7. **Given** a shape, **When** an offset image filter with dx=10, dy=10 is applied, **Then** the rendered output is shifted.

---

### User Story 7 - Canvas Clipping (Priority: P2)

A developer wants to restrict drawing to specific rectangular or path-based regions. They specify clip regions in the DSL, supporting both intersection and difference clip operations.

**Why this priority**: Clipping is fundamental for creating masked views, viewport restrictions, and complex shape compositions.

**Independent Test**: Can be tested by rendering a large shape with a smaller clip rect and verifying only the clipped portion is visible.

**Acceptance Scenarios**:

1. **Given** a group element, **When** a rectangular clip is applied, **Then** only the portion of children within the clip rectangle is rendered.
2. **Given** a group element, **When** a path-based clip is applied (e.g., circular path), **Then** children are only visible within the path outline.
3. **Given** a clip, **When** the clip operation is `Difference`, **Then** rendering is excluded from the clip region rather than restricted to it.
4. **Given** nested groups with clips, **When** both clips are active, **Then** the effective clip is the intersection of all clip regions.

---

### User Story 8 - Text and Font System (Priority: P2)

A developer wants richer text rendering — selecting specific typefaces, controlling font weight/slant/width, measuring text, and rendering text blobs with positioned glyphs. They express font choices and text layout through the DSL.

**Why this priority**: The current text support is minimal (fontSize + DrawText only). Real applications need typeface selection, font metrics, and text measurement for proper layout.

**Independent Test**: Can be tested by rendering text with a specific typeface and verifying the output uses that typeface (via visual inspection or pixel comparison with a known rendering).

**Acceptance Scenarios**:

1. **Given** a text element, **When** a typeface name (e.g., "Arial") and weight (Bold) are specified, **Then** the text renders in that typeface and weight.
2. **Given** a text element, **When** font slant is set to Italic, **Then** the text renders in italic style.
3. **Given** the DSL, **When** a developer requests text measurement for a string with a given font configuration, **Then** the system returns the width and height bounds of the text.
4. **Given** a text blob with multiple runs at different positions, **When** rendered, **Then** each run appears at its specified position.

---

### User Story 9 - Path Operations (Priority: P3)

A developer wants to combine paths using boolean operations (union, intersect, difference, XOR), measure path length, extract segments, and control fill types and directions. They use path operations through the DSL.

**Why this priority**: Path operations are powerful but used less frequently than basic drawing. They enable complex shape construction from simpler primitives.

**Independent Test**: Can be tested by creating two overlapping circular paths, performing a Union operation, and verifying the result is a single merged shape.

**Acceptance Scenarios**:

1. **Given** two overlapping paths, **When** a Union path operation is applied, **Then** the result is a single path covering both areas.
2. **Given** two overlapping paths, **When** an Intersect operation is applied, **Then** the result covers only the overlap area.
3. **Given** two overlapping paths, **When** a Difference operation is applied, **Then** the first path minus the second is rendered.
4. **Given** a path, **When** the fill type is set to EvenOdd, **Then** self-intersecting regions alternate between filled and empty.
5. **Given** a path, **When** path measurement is requested, **Then** the total length of the path contour is returned.
6. **Given** a path and path measurement, **When** a segment from 20% to 60% is extracted, **Then** only that portion of the path is returned.
7. **Given** a path, **When** AddRect, AddCircle, AddOval, or AddRoundRect convenience commands are used, **Then** those shapes are added to the path without manual MoveTo/LineTo sequences.

---

### User Story 10 - Picture Recording and Playback (Priority: P3)

A developer wants to record a sequence of drawing operations and replay them later, potentially multiple times or at different transforms. They use the picture recording API through the DSL.

**Why this priority**: Picture recording is an optimization and reuse mechanism — useful for caching complex scenes, but not essential for basic rendering.

**Independent Test**: Can be tested by recording a scene to a picture, then drawing the picture twice at different positions, verifying both appear correctly.

**Acceptance Scenarios**:

1. **Given** a set of drawing elements, **When** they are recorded as a picture, **Then** a reusable picture object is created.
2. **Given** a recorded picture, **When** it is drawn on a canvas at a specific position, **Then** the recorded operations replay at that position.
3. **Given** a recorded picture, **When** it is drawn multiple times with different transforms, **Then** each instance renders correctly with its own transform.

---

### User Story 11 - Regions (Priority: P3)

A developer wants to define geometric regions for hit testing, clipping, or complex area operations. They create regions from rectangles or paths and combine them with boolean operations.

**Why this priority**: Regions are primarily useful for hit testing and complex clipping — important for interactive applications but not for basic rendering.

**Independent Test**: Can be tested by creating a region from a rectangle, testing point containment, and verifying correct results.

**Acceptance Scenarios**:

1. **Given** a rectangular region, **When** a point inside the rectangle is tested, **Then** the containment check returns true.
2. **Given** a rectangular region, **When** a point outside the rectangle is tested, **Then** the containment check returns false.
3. **Given** two regions, **When** combined with Union, Intersect, or Difference operations, **Then** the resulting region reflects the operation.
4. **Given** a region, **When** used as a clip on a canvas, **Then** drawing is restricted to the region bounds.

---

### User Story 12 - Canvas Drawing Extensions (Priority: P3)

A developer wants to use additional canvas drawing methods not currently exposed — DrawPoints, DrawVertices, DrawArc, DrawImage (direct image, not bitmap), DrawPicture, and DrawDrawable.

**Why this priority**: These are supplementary drawing primitives. Most use cases are covered by the existing set, but completeness enables edge-case scenarios.

**Independent Test**: Can be tested by rendering points in different modes (points, lines, polygon) and verifying output matches the specified mode.

**Acceptance Scenarios**:

1. **Given** an array of points, **When** DrawPoints is called with mode `Points`, **Then** each point renders as a dot.
2. **Given** an array of points, **When** DrawPoints is called with mode `Lines`, **Then** pairs of points are connected with lines.
3. **Given** an array of points, **When** DrawPoints is called with mode `Polygon`, **Then** all points are connected sequentially.
4. **Given** vertex data with positions and colors, **When** DrawVertices is called with mode `Triangles`, **Then** the triangles render with interpolated vertex colors.
5. **Given** center, radius, start angle, and sweep angle, **When** DrawArc is called, **Then** an arc segment renders at the specified position.

---

### User Story 13 - Runtime Effects (SkSL Shaders) (Priority: P3)

A developer wants to write custom shaders using SkSL (Skia's shading language) and apply them as shaders, color filters, or blenders through the DSL.

**Why this priority**: Runtime effects are a power-user feature for custom GPU-accelerated effects. Most use cases are served by built-in shaders and filters, but SkSL enables unlimited customization.

**Independent Test**: Can be tested by compiling a simple SkSL shader that outputs a solid color, applying it to a shape, and verifying the output color matches.

**Acceptance Scenarios**:

1. **Given** a valid SkSL shader source string, **When** compiled and applied as a shader to a shape, **Then** the shape renders using the custom shader logic.
2. **Given** a SkSL shader with uniform inputs, **When** uniform values are set and the shader is applied, **Then** the shader uses the provided uniform values.
3. **Given** an invalid SkSL source string, **When** compilation is attempted, **Then** the system reports a clear compilation error.
4. **Given** a SkSL color filter effect, **When** applied to a shape, **Then** the pixel colors are transformed by the custom filter logic.

---

### User Story 14 - Color Space Management (Priority: P3)

A developer working with color-accurate rendering wants to specify color spaces for surfaces and images, and convert between sRGB and other color spaces.

**Why this priority**: Color space management is important for professional/print graphics but not needed for typical screen rendering. Most applications work fine with the default sRGB space.

**Independent Test**: Can be tested by creating a surface with a specific color space and verifying the color space property matches.

**Acceptance Scenarios**:

1. **Given** a surface, **When** created with a specified color space (e.g., sRGB), **Then** the surface reports that color space.
2. **Given** two images in different color spaces, **When** drawn to the same canvas, **Then** colors are correctly converted to the canvas color space.
3. **Given** the DSL, **When** a developer queries available color space profiles, **Then** at least sRGB and linear sRGB are available.

---

### User Story 15 - 3D View Utility (Priority: P3)

A developer wants to apply 3D perspective transformations to 2D elements — rotations around X/Y/Z axes with perspective projection. They use the 3D view utility through the DSL.

**Why this priority**: 3D transformations are niche for a 2D engine. Useful for card-flip animations and perspective effects but not a core 2D feature.

**Independent Test**: Can be tested by applying a Y-axis rotation to a rectangle and verifying the output appears foreshortened (trapezoidal).

**Acceptance Scenarios**:

1. **Given** a rectangle, **When** a 3D rotation of 45 degrees around the Y axis is applied, **Then** the rectangle appears in perspective (narrower on one side).
2. **Given** a shape, **When** 3D camera position is set and rotation applied, **Then** the perspective distortion reflects the camera distance.

---

### Edge Cases

- What happens when a SkSL shader fails to compile? The system must report the error clearly and not crash.
- What happens when a runtime effect is used on the CPU raster backend? The system must raise an error/exception.
- What happens when a composed filter chain is deeply nested (e.g., 10+ filters)? The system should handle it without stack overflow.
- What happens when a clip region results in zero visible area? Drawing should be a no-op, not an error.
- What happens when a path operation (e.g., Intersect) results in an empty path? The system should produce an empty path, not an error.
- What happens when an unsupported typeface name is specified? The system should fall back to the default typeface.
- What happens when blend mode is set but no overlapping elements exist? Rendering should proceed normally with no visible difference.
- What happens when a radial gradient has zero radius? The system should handle it gracefully (solid color or no-op).
- What happens when image shader references a disposed or null bitmap? The system should log a warning and skip the fill, similar to current Image element behavior.

## Requirements *(mandatory)*

### Functional Requirements

**Stroke Styling**:
- **FR-001**: System MUST support setting stroke cap style (Butt, Round, Square) on any paint.
- **FR-002**: System MUST support setting stroke join style (Miter, Round, Bevel) on any paint.
- **FR-003**: System MUST support setting stroke miter limit on any paint.

**Path Effects**:
- **FR-004**: System MUST support dash path effects with configurable dash/gap interval arrays and phase offset.
- **FR-005**: System MUST support corner path effects with configurable radius.
- **FR-006**: System MUST support trim path effects with configurable start, stop, and mode (Normal/Inverted).
- **FR-007**: System MUST support composing two path effects together.
- **FR-008**: System MUST support 1D path effects (stamping a path along another path).
- **FR-009**: System MUST support sum of two path effects (both applied independently).

**Shaders**:
- **FR-010**: System MUST support radial gradient shaders with center, radius, colors, positions, and tile mode.
- **FR-011**: System MUST support sweep gradient shaders with center, start angle, end angle, colors, positions, and tile mode.
- **FR-012**: System MUST support two-point conical gradient shaders.
- **FR-013**: System MUST support Perlin noise shaders (fractal noise and turbulence variants).
- **FR-014**: System MUST support solid color shaders.
- **FR-015**: System MUST support image/bitmap shaders with tile modes (Clamp, Repeat, Mirror, Decal).
- **FR-016**: System MUST support composing two shaders with a blend mode.
- **FR-017**: System MUST continue to support existing linear gradient shaders.

**Blend Modes**:
- **FR-018**: System MUST support all standard SkiaSharp blend modes (29 modes: Clear through Luminosity) on any paint.

**Color Filters**:
- **FR-019**: System MUST support blend-mode color filters (tint color + blend mode).
- **FR-020**: System MUST support 5x4 color matrix filters.
- **FR-021**: System MUST support composing two color filters.
- **FR-022**: System MUST support high contrast color filters.
- **FR-023**: System MUST support lighting color filters (multiply + add colors).
- **FR-024**: System MUST support luma-to-alpha color filters.
**Mask Filters**:
- **FR-025**: System MUST support blur mask filters with style (Normal, Solid, Outer, Inner) and sigma.

**Image Filters**:
- **FR-026**: System MUST support blur image filters with X and Y sigma.
- **FR-027**: System MUST support drop shadow image filters with offset, sigma, and color.
- **FR-028**: System MUST support dilate and erode image filters with X and Y radii.
- **FR-029**: System MUST support offset image filters.
- **FR-030**: System MUST support color filter image filters (wrapping a color filter).
- **FR-031**: System MUST support composing image filters.
- **FR-032**: System MUST support merge image filters (combining multiple filters).
- **FR-033**: System MUST support displacement map image filters.
- **FR-034**: System MUST support matrix convolution image filters.

**Clipping**:
- **FR-035**: System MUST support rectangular clipping on groups with Intersect and Difference operations.
- **FR-036**: System MUST support path-based clipping on groups with Intersect and Difference operations.
- **FR-037**: System MUST support antialiased clipping.

**Text and Font System**:
- **FR-038**: System MUST support specifying typeface by family name.
- **FR-039**: System MUST support specifying font weight, slant, and width.
- **FR-040**: System MUST support text measurement (returning bounding rectangle for a given string and font configuration).
- **FR-041**: System MUST support rendering text blobs with multiple positioned runs.
- **FR-042**: System MUST fall back to the default typeface when a requested typeface is unavailable.

**Path Operations**:
- **FR-043**: System MUST support boolean path operations (Union, Intersect, Difference, XOR, ReverseDifference).
- **FR-044**: System MUST support path fill types (Winding, EvenOdd, InverseWinding, InverseEvenOdd).
- **FR-045**: System MUST support path measurement (total length, position/tangent at distance).
- **FR-046**: System MUST support extracting path segments by start/stop distance.
- **FR-047**: System MUST support convenience path commands: AddRect, AddCircle, AddOval, AddRoundRect.
- **FR-048**: System MUST support path direction (Clockwise, CounterClockwise).

**Canvas Drawing Extensions**:
- **FR-049**: System MUST support DrawPoints with point modes (Points, Lines, Polygon).
- **FR-050**: System MUST support DrawVertices with vertex modes (Triangles, TriangleStrip, TriangleFan) and per-vertex colors.
- **FR-051**: System MUST support DrawArc with center, radius, start angle, sweep angle, and useCenter flag.

**Picture Recording**:
- **FR-052**: System MUST support recording drawing operations into a reusable picture object.
- **FR-053**: System MUST support drawing a picture onto a canvas at a specified position/transform.

**Regions**:
- **FR-054**: System MUST support creating regions from rectangles.
- **FR-055**: System MUST support creating regions from paths.
- **FR-056**: System MUST support region boolean operations (Union, Intersect, Difference, XOR).
- **FR-057**: System MUST support point-in-region containment testing.
- **FR-058**: System MUST support using regions as canvas clips.

**Runtime Effects**:
- **FR-059**: System MUST support compiling SkSL shader source into runtime effects.
- **FR-060**: System MUST support setting uniform values on runtime effects.
- **FR-061**: System MUST support using runtime effects as shaders, color filters, or blenders.
- **FR-062**: System MUST report clear compilation errors for invalid SkSL source.
- **FR-063**: System MUST raise an error/exception when runtime effects are used on the CPU raster backend.

**Color Space Management**:
- **FR-064**: System MUST support creating surfaces with a specified color space.
- **FR-065**: System MUST support sRGB and linear sRGB color spaces at minimum.
- **FR-066**: System MUST support color space conversion when drawing across color spaces.

**3D View**:
- **FR-067**: System MUST support 3D perspective transformations (rotation around X, Y, Z axes).
- **FR-068**: System MUST support configurable camera distance for perspective projection.

### Key Entities

- **Paint**: Extended with stroke cap, stroke join, stroke miter, blend mode, shader, color filter, mask filter, image filter, and path effect.
- **Shader**: Discriminated union covering linear gradient, radial gradient, sweep gradient, two-point conical gradient, Perlin noise, solid color, image, and compose variants.
- **PathEffect**: Discriminated union covering dash, corner, trim, 1D path, compose, and sum variants.
- **ColorFilter**: Discriminated union covering blend mode, color matrix, compose, high contrast, lighting, and luma variants.
- **MaskFilter**: Discriminated union covering blur (with style).
- **ImageFilter**: Discriminated union covering blur, drop shadow, dilate, erode, offset, color filter, compose, merge, displacement map, and matrix convolution variants.
- **BlendMode**: Enumeration of all 29 standard blend modes.
- **Clip**: Discriminated union for rectangular and path-based clips with operation type.
- **FontSpec**: Record for typeface family, weight, slant, and width.
- **PathOp**: Enumeration of boolean path operations.
- **PathFillType**: Enumeration of fill types.
- **Region**: Abstraction for geometric region with boolean operations and containment testing.
- **Picture**: Recorded set of drawing operations that can be replayed.
- **RuntimeEffect**: Compiled SkSL program with uniforms and child effects.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 29 standard blend modes are expressible through the DSL and render correctly.
- **SC-002**: All shader types (7 total: linear, radial, sweep, conical, Perlin noise, color, image) render correctly when applied to any shape element.
- **SC-003**: Stroke styling (cap, join, miter) produces visually distinct output for each option when applied to line and path elements.
- **SC-004**: All path effect types (dash, corner, trim, 1D, compose, sum) modify path rendering as expected.
- **SC-005**: All color filter types produce correct color transformations verified against known input/output color pairs.
- **SC-006**: Blur mask filter produces visually softened edges at the specified sigma.
- **SC-007**: All image filter types produce the expected visual transformations (shadows offset correctly, blur softens, dilate/erode changes shape size).
- **SC-008**: Clipping restricts rendering to the specified region for both rect and path clips.
- **SC-009**: Text renders in the specified typeface and style when the typeface is available on the system.
- **SC-010**: Boolean path operations produce correct geometric results for overlapping paths.
- **SC-011**: SkSL runtime effects compile, accept uniforms, and produce the expected shader output.
- **SC-012**: Picture recording and playback reproduce the original drawing operations faithfully.
- **SC-013**: All new DSL types are expressible as composable, immutable F# discriminated unions and records consistent with the existing DSL style.
- **SC-014**: No regressions in existing rendering functionality (all prior tests continue to pass).

## Clarifications

### Session 2026-04-09

- Q: How should backward compatibility be handled for the extended Paint record? → A: No backward compatibility. New fields are added directly; all existing Paint usages will be updated to include new fields.
- Q: What should happen when runtime effects are used on the CPU backend? → A: Raise an error/exception at render time.
- Q: Should features with uncertain API availability in SkiaSharp 2.88.6 be included? → A: No. Only specify features confirmed available in 2.88.6. Exclude table mask filters (FR-027) and table-based color filters (FR-025).

## Assumptions

- The existing declarative scene DSL pattern (immutable F# DUs/records interpreted by a renderer) will be extended rather than replaced.
- No backward compatibility is maintained for the Paint record. All new fields (StrokeCap, StrokeJoin, StrokeMiter, BlendMode, Shader, ColorFilter, MaskFilter, ImageFilter, PathEffect) are added as required fields. Existing code using the Paint record will be updated to include the new fields.
- SkiaSharp 2.88.6 is the target version; APIs must be available in this version (some SkiaSharp 3.x-only APIs like `SKBlender` custom blenders may not be available — built-in blend modes via `SKBlendMode` enum are used instead).
- SVG canvas (`SKSvgCanvas`) and PDF document (`SKDocument`) generation are explicitly excluded from scope.
- Runtime effects (SkSL) require GPU-backed rendering. If a runtime effect is used on the CPU raster backend, the system MUST raise an error/exception at render time.
- Font/typeface availability depends on the host system; the system will use fallback fonts when requested typefaces are unavailable.
- The `SKColorTable` type is deprecated in modern SkiaSharp and is excluded from scope.
- Table-based mask filters and table-based color filters are excluded due to uncertain API availability in SkiaSharp 2.88.6.
- `SKDrawable` as a base class for custom drawables is excluded — the DSL already provides a functional alternative via the scene graph.

# Research: 008-charting-datagrid-library

**Date**: 2026-04-09

## R1: Integration Pattern — Composition vs. Element Extension

**Decision**: Composition over core Element DU extension.

**Rationale**: The charting library will produce `Element` trees (Groups of Rect, Line, Text, Arc, Path primitives) via pure functions. This avoids modifying the core `SkiaViewer.Element` discriminated union, keeps the core project unchanged (SC-006), and leverages the existing `SceneRenderer` and `CachedRenderer` pipeline without modification.

**Alternatives considered**:
- Extending `Element` DU with chart-specific cases: Rejected — breaks core project boundary, forces SceneRenderer changes, couples chart rendering to core.
- Custom rendering pipeline: Rejected — duplicates existing infrastructure, breaks caching.

## R2: Axis Auto-Scaling Algorithm

**Decision**: Linear nice-number algorithm for axis tick generation.

**Rationale**: The "nice numbers" algorithm (after Heckbert 1990) produces human-readable axis ticks (e.g., 0, 5, 10, 15 instead of 0, 7.3, 14.6). It works by computing a rough step from data range / desired tick count, then rounding to the nearest 1, 2, or 5 multiple. This is the industry standard approach used by D3, matplotlib, and Chart.js.

**Alternatives considered**:
- Fixed tick count with raw divisions: Rejected — produces ugly numbers.
- Logarithmic scaling: Deferred to future iteration; linear covers the common case.

## R3: DataGrid Virtual Scrolling

**Decision**: Row virtualization via offset-based rendering.

**Rationale**: For 10,000+ rows (SC-003), rendering all rows is infeasible at 60fps. Instead, compute visible row range from scroll offset and element height, render only visible rows plus a small buffer. Scroll offset is passed as an immutable parameter per the static data model (clarification Q1).

**Alternatives considered**:
- Full render with clip: Rejected — O(n) rendering for n rows, fails SC-003.
- SKPicture pre-recording of all rows: Rejected — high memory for large datasets.

## R4: Color Palette Defaults

**Decision**: Use a 10-color categorical palette inspired by Tableau 10.

**Rationale**: Tableau 10 is a well-established perceptually distinct palette designed for data visualization. It provides sufficient contrast for the typical number of series (2-8) while remaining accessible to most forms of color vision deficiency.

**Alternatives considered**:
- Material Design colors: Rejected — not designed for data viz; low contrast between adjacent hues.
- User-defined only: Rejected — spec requires sensible defaults.

## R5: Project Structure and Dependencies

**Decision**: New `SkiaViewer.Charts` project referencing `SkiaViewer` as a project dependency, with only `SkiaSharp` as a transitive dependency.

**Rationale**: No new NuGet dependencies needed. SkiaSharp provides all required drawing primitives (SKCanvas, SKPath, SKRect, SKPaint). The charting library composes SkiaViewer's `Element` DU which is built on SkiaSharp types. Silk.NET is NOT needed since the charting library doesn't manage windows — it produces scene elements.

**Alternatives considered**:
- Embedding chart code in core SkiaViewer: Rejected — violates separation of concerns and SC-006.
- Separate solution: Rejected — unnecessary complexity for a project-level dependency.

## R6: DataGrid Cell Rendering Strategy

**Decision**: Type-discriminated rendering per column data type (text/numeric/boolean).

**Rationale**: Per clarification Q2, cells support text (left-aligned), numeric (right-aligned), and boolean (checkbox-style). Each column definition carries a data type tag. Rendering dispatch uses pattern matching on the column type to select alignment and visual treatment. Boolean cells render a filled or empty square indicator rather than text.

**Alternatives considered**:
- All-text with formatting hints: Rejected — loses alignment and visual distinction benefits.
- Custom render functions per cell: Rejected — over-engineered for three fixed types.

## R7: Chart Element API Surface

**Decision**: Each chart type exposes a single creation function that returns `Element` (a Group of composed primitives). Configuration via record types with sensible defaults.

**Rationale**: Aligns with the existing DSL pattern where `Scene.rect`, `Scene.circle`, etc. return `Element`. Chart creation functions follow the same pattern: `Charts.lineChart config data -> Element`. Configuration records use F# `with` syntax for optional overrides, keeping the API declarative and composable.

**Alternatives considered**:
- Builder/fluent API: Rejected — less idiomatic F#; record `with` is more concise.
- OOP class hierarchy: Rejected — violates F# idioms and constitution (F# only, functional style).

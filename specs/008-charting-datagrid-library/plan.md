# Implementation Plan: Charting & DataGrid Library

**Branch**: `008-charting-datagrid-library` | **Date**: 2026-04-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-charting-datagrid-library/spec.md`

## Summary

Add a new `SkiaViewer.Charts` F# library project that provides 8 chart types (line, bar, pie/donut, scatter, area, histogram, candlestick, radar) and a DataGrid element. All elements are pure functions that compose existing `SkiaViewer.Element` primitives into Group trees, integrating seamlessly with the existing declarative scene DSL, rendering pipeline, and caching infrastructure without modifying the core project.

## Technical Context

**Language/Version**: F# on .NET 10.0
**Primary Dependencies**: SkiaSharp 2.88.6 (transitive via SkiaViewer project reference). No new NuGet dependencies.
**Storage**: N/A — all data is immutable per render frame
**Testing**: xUnit (matching existing test project pattern)
**Target Platform**: Desktop (Linux, Windows, macOS) via SkiaViewer
**Project Type**: Library (NuGet-packable)
**Performance Goals**: 100K data points render < 100ms; DataGrid 10K rows with smooth virtual scrolling
**Constraints**: Static/immutable data model; no horizontal scrolling on DataGrid; chart elements return `SkiaViewer.Element`
**Scale/Scope**: 8 chart types + 1 DataGrid element; ~22 F# source files (11 .fsi/.fs pairs)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First Delivery | PASS | Full spec with clarifications, plan, and task chain |
| II. Compiler-Enforced Contracts | PASS | All public modules will have .fsi signature files; surface-area baselines planned |
| III. Test Evidence | PASS | Tests planned for each chart type, DataGrid, axis scaling, edge cases |
| IV. Observability | PASS | Invalid data (NaN, negative pie values) emits structured diagnostics via `System.Diagnostics` |
| V. Scripting Accessibility | PASS | Prelude extension + numbered example scripts planned |
| F# only | PASS | Pure F# project |
| Packable | PASS | `dotnet pack` producing SkiaViewer.Charts.nupkg |
| Minimal deps | PASS | No new NuGet dependencies; only project reference to SkiaViewer |

### Post-Phase 1 Re-check

| Principle | Status | Notes |
|-----------|--------|-------|
| II. .fsi files | PASS | Planned: LineChart.fsi, BarChart.fsi, PieChart.fsi, ScatterPlot.fsi, AreaChart.fsi, Histogram.fsi, Candlestick.fsi, RadarChart.fsi, DataGrid.fsi, Types.fsi, Axis.fsi |
| III. Tests | PASS | Unit tests for rendering output, axis calculation, DataGrid sorting/virtualization, edge cases |
| V. Scripting | PASS | scripts/charts-prelude.fsx + examples/03-charts.fsx, 04-datagrid.fsx |

## Project Structure

### Documentation (this feature)

```text
specs/008-charting-datagrid-library/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── public-api.md    # Public API contract
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/SkiaViewer.Charts/
├── SkiaViewer.Charts.fsproj
├── Types.fsi                    # All public types (configs, data, enums)
├── Types.fs
├── Axis.fsi                     # Axis computation (internal)
├── Axis.fs
├── ChartHelpers.fsi             # Shared internal rendering helpers
├── ChartHelpers.fs
├── LineChart.fsi                # Line chart creation API
├── LineChart.fs
├── BarChart.fsi                 # Bar chart creation API (grouped + stacked)
├── BarChart.fs
├── PieChart.fsi                 # Pie/donut chart creation API
├── PieChart.fs
├── ScatterPlot.fsi              # Scatter plot creation API
├── ScatterPlot.fs
├── AreaChart.fsi                # Area chart creation API
├── AreaChart.fs
├── Histogram.fsi                # Histogram creation API
├── Histogram.fs
├── Candlestick.fsi              # Candlestick chart creation API
├── Candlestick.fs
├── RadarChart.fsi               # Radar/spider chart creation API
├── RadarChart.fs
├── DataGrid.fsi                 # Public DataGrid API
└── DataGrid.fs

tests/SkiaViewer.Charts.Tests/
├── SkiaViewer.Charts.Tests.fsproj
├── AxisTests.fs                 # Axis auto-scaling, nice numbers
├── LineChartTests.fs            # Line chart rendering
├── BarChartTests.fs             # Bar chart rendering
├── PieChartTests.fs             # Pie/donut rendering + zero-value edge case
├── ScatterPlotTests.fs          # Scatter plot rendering
├── AreaChartTests.fs            # Area chart rendering
├── HistogramTests.fs            # Histogram binning
├── CandlestickTests.fs          # Candlestick rendering
├── RadarChartTests.fs           # Radar chart rendering
├── DataGridTests.fs             # DataGrid rendering, sorting, virtualization
├── EdgeCaseTests.fs             # Empty data, NaN, small sizes
└── SurfaceAreaTests.fs          # Surface-area baseline validation

scripts/
├── charts-prelude.fsx           # FSI prelude for Charts library
└── examples/
    ├── 03-charts-gallery.fsx    # All chart types in one scene
    └── 04-datagrid.fsx          # DataGrid with sorting demo
```

**Structure Decision**: Separate `SkiaViewer.Charts` project under `src/` with project reference to `SkiaViewer`. Follows existing solution pattern (src/ and tests/ folders). Each chart type has its own .fsi/.fs file pair enabling parallel development across stories. Internal modules (Axis, ChartHelpers) have .fsi files with `internal` visibility to maintain encapsulation while enabling test access via InternalsVisibleTo.

## Complexity Tracking

No constitution violations to justify. The design stays within constraints:
- Single new library project (not multiple)
- No new NuGet dependencies
- Composition-only integration (no core project changes)
- Pure functions returning existing Element type

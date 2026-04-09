# Public API Contract: SkiaViewer.Charts

**Date**: 2026-04-09

## Chart Modules

Each chart type has its own module. All chart creation functions accept a config record and data, returning a `SkiaViewer.Element` (a Group of composed primitives).

```fsharp
module LineChart =
    val lineChart: config: ChartConfig -> series: DataSeries list -> Element
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

module BarChart =
    val barChart: config: ChartConfig -> data: CategoryValue list -> Element
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

module PieChart =
    val pieChart: config: PieConfig -> slices: SliceData list -> Element
    val defaultConfig: width: float32 -> height: float32 -> PieConfig

module ScatterPlot =
    val scatterPlot: config: ChartConfig -> series: DataSeries list -> Element
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

module AreaChart =
    val areaChart: config: ChartConfig -> series: DataSeries list -> Element
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

module Histogram =
    val histogram: config: HistogramConfig -> values: float list -> Element
    val defaultConfig: width: float32 -> height: float32 -> HistogramConfig

module Candlestick =
    val candlestickChart: config: CandlestickConfig -> data: OhlcData list -> Element
    val defaultConfig: width: float32 -> height: float32 -> CandlestickConfig

module RadarChart =
    val radarChart: config: RadarConfig -> series: RadarSeries list -> Element
    val defaultConfig: width: float32 -> height: float32 -> categories: string list -> RadarConfig
```

### Types Module (shared)

```fsharp
/// Bar chart layout mode.
[<RequireQualifiedAccess>]
type BarLayout = | Grouped | Stacked
```

## Module: DataGrid

```fsharp
module DataGrid =
    /// Create a data grid element.
    val dataGrid: config: DataGridConfig -> data: DataGridData -> Element

    /// Default data grid config for the given width and height.
    val defaultConfig: width: float32 -> height: float32 -> DataGridConfig

    /// Create a text column definition.
    val textColumn: name: string -> ColumnDef

    /// Create a numeric column definition.
    val numericColumn: name: string -> ColumnDef

    /// Create a boolean column definition.
    val boolColumn: name: string -> ColumnDef

    /// Sort rows by the specified column. Returns sorted rows.
    val sortRows: columns: ColumnDef list -> columnIndex: int -> direction: SortDirection -> rows: CellValue list list -> CellValue list list

    /// Compute visible row range for virtual scrolling.
    val visibleRange: config: DataGridConfig -> totalRows: int -> startIndex: int * endIndex: int
```

## Module: ChartHelpers (internal)

Internal utility functions not exposed in .fsi:

- `niceNumber`: axis tick generation
- `computeAxisTicks`: tick positions and labels from data range
- `renderAxis`: axis -> Element list
- `renderLegend`: series names + palette -> Element
- `renderGridLines`: axis config -> bounds -> Element list
- `renderTitle`: title -> bounds -> Element

## Return Type Contract

All public chart/grid creation functions return `SkiaViewer.Element`. Specifically, they return `Element.Group` containing composed primitives. This means:

1. Charts integrate directly into any `Scene.Elements` list
2. Charts benefit from existing `CachedRenderer` Group caching
3. Charts can be transformed using existing `Scene.translate`, `Scene.rotate`, `Scene.scale`
4. Charts compose with other DSL elements in the same scene

## Stability Guarantees

- All types in this contract are records or discriminated unions (immutable by default)
- Config records support F# `with` syntax for partial overrides
- Adding new optional fields to config records is a non-breaking change (defaults provided)
- Adding new DU cases to ColumnType or SortDirection is a breaking change

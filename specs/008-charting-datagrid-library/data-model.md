# Data Model: 008-charting-datagrid-library

**Date**: 2026-04-09

## Core Types

### Color Palette

```
ColorPalette
  Colors: list of color values (default: 10-color categorical palette)
```

Used by all chart types to assign distinct colors to data series.

### Axis Configuration

```
AxisConfig
  Label: string option              — axis title text
  Min: float option                 — manual minimum (None = auto-scale)
  Max: float option                 — manual maximum (None = auto-scale)
  TickCount: int                    — desired number of ticks (default: 5)
  ShowGridLines: bool               — render grid lines (default: true)
  GridLineColor: color option       — grid line color (default: light gray)
```

### Legend Configuration

```
LegendConfig
  Visible: bool                     — show/hide legend (default: true)
  Position: LegendPosition          — Top | Bottom | Left | Right (default: Bottom)
```

### Chart Configuration (shared)

```
ChartConfig
  Title: string option              — chart title
  TitleFontSize: float32            — title font size (default: 16)
  Width: float32                    — element width
  Height: float32                   — element height
  Padding: float32                  — inner padding from edges (default: 40)
  XAxis: AxisConfig                 — x-axis configuration
  YAxis: AxisConfig                 — y-axis configuration
  Legend: LegendConfig              — legend configuration
  Palette: ColorPalette             — color palette for series
  BackgroundColor: color option     — chart background (default: None/transparent)
```

### Bar Layout

```
BarLayout = Grouped | Stacked
```

Controls whether multi-series bar charts display bars side-by-side (Grouped) or stacked vertically (Stacked). Used in `ChartConfig` or passed to `BarChart.barChart`. Default: `Grouped`.

## Chart-Specific Data Types

### DataPoint

```
DataPoint
  X: float                          — x-coordinate value
  Y: float                          — y-coordinate value
```

### DataSeries (for Line, Scatter, Area)

```
DataSeries
  Name: string                      — series name (used in legend)
  Points: DataPoint list            — ordered data points
```

### CategoryValue (for Bar)

```
CategoryValue
  Category: string                  — category label
  Values: (string * float) list     — named values per series within category
```

### SliceData (for Pie/Donut)

```
SliceData
  Label: string                     — slice label
  Value: float                      — slice value (must be >= 0)
```

### PieConfig (extends ChartConfig)

```
PieConfig
  DonutRatio: float32               — 0.0 = full pie, 0.3-0.7 = donut (default: 0.0)
  ShowLabels: bool                  — render labels on slices (default: true)
```

### OhlcData (for Candlestick)

```
OhlcData
  Label: string                     — time period label
  Open: float                       — opening value
  High: float                       — highest value
  Low: float                        — lowest value
  Close: float                      — closing value
```

### CandlestickConfig (extends ChartConfig)

```
CandlestickConfig
  UpColor: color                    — color for close > open (default: green)
  DownColor: color                  — color for close <= open (default: red)
```

### RadarData (for Radar/Spider)

```
RadarSeries
  Name: string                      — series name
  Values: float list                — values per category (length must match category count)

RadarConfig
  Categories: string list           — radial axis labels
  MaxValue: float option            — manual max (None = auto from data)
  ShowGrid: bool                    — render concentric grid (default: true)
  GridLevels: int                   — number of concentric grid rings (default: 5)
```

### HistogramConfig (extends ChartConfig)

```
HistogramConfig
  BinCount: int                     — number of bins (default: 10)
  BarColor: color option            — override palette for single-color bars
```

## DataGrid Types

### ColumnType

```
ColumnType = Text | Numeric | Boolean
```

### ColumnDef

```
ColumnDef
  Name: string                      — column header text
  Type: ColumnType                  — data type for rendering/sorting
  Sortable: bool                    — allow sort on click (default: true)
  MinWidth: float32 option          — minimum column width (default: None)
```

### CellValue

```
CellValue
  TextValue of string
  NumericValue of float
  BoolValue of bool
```

### SortDirection

```
SortDirection = Ascending | Descending | None
```

### SortState

```
SortState
  ColumnIndex: int                  — which column is sorted
  Direction: SortDirection          — current sort direction
```

### DataGridConfig

```
DataGridConfig
  Width: float32                    — total element width
  Height: float32                   — total element height
  RowHeight: float32                — height per row (default: 30)
  HeaderHeight: float32             — header row height (default: 36)
  HeaderColor: color                — header background color
  AlternateRowColor: color option   — alternating row background (default: subtle gray)
  FontSize: float32                 — cell text font size (default: 14)
  HeaderFontSize: float32           — header text font size (default: 14)
  ScrollOffset: float               — vertical scroll position in pixels (default: 0)
  Sort: SortState option            — current sort state (default: None)
```

### DataGridData

```
DataGridData
  Columns: ColumnDef list           — column definitions
  Rows: CellValue list list         — row data (each row is a list of cell values matching column order)
```

## Entity Relationships

```
ChartConfig ──uses──> AxisConfig (x2: XAxis, YAxis)
ChartConfig ──uses──> LegendConfig
ChartConfig ──uses──> ColorPalette

LineChart ──uses──> ChartConfig + DataSeries list
BarChart ──uses──> ChartConfig + CategoryValue list
PieChart ──uses──> PieConfig + SliceData list
ScatterPlot ──uses──> ChartConfig + DataSeries list
AreaChart ──uses──> ChartConfig + DataSeries list
Histogram ──uses──> HistogramConfig + float list (raw values)
Candlestick ──uses──> CandlestickConfig + OhlcData list
RadarChart ──uses──> RadarConfig + RadarSeries list

DataGrid ──uses──> DataGridConfig + DataGridData
```

## Validation Rules

- SliceData.Value must be >= 0 (negative pie slices are meaningless)
- OhlcData: High >= max(Open, Close) and Low <= min(Open, Close)
- RadarSeries.Values length must equal RadarConfig.Categories length
- DataGridData.Rows: each row length must equal Columns length
- CellValue type should match ColumnDef.Type for the corresponding column
- HistogramConfig.BinCount must be >= 1
- ChartConfig.Width and Height must be > 0
- DataGridConfig.ScrollOffset must be >= 0

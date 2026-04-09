namespace SkiaViewer.Charts

open SkiaSharp

/// <namespacedoc>
/// <summary>Provides chart types, configuration records, and default helpers for SkiaViewer.Charts.</summary>
/// </namespacedoc>

/// <summary>Color palette for assigning distinct colors to data series in charts.</summary>
type ColorPalette =
    { Colors: SKColor list }

/// <summary>Specifies where the legend is positioned relative to the chart area.</summary>
[<RequireQualifiedAccess>]
type LegendPosition =
    /// <summary>Place the legend above the chart.</summary>
    | Top
    /// <summary>Place the legend below the chart.</summary>
    | Bottom
    /// <summary>Place the legend to the left of the chart.</summary>
    | Left
    /// <summary>Place the legend to the right of the chart.</summary>
    | Right

/// <summary>Controls legend visibility and placement.</summary>
type LegendConfig =
    { Visible: bool
      Position: LegendPosition }

/// <summary>Configuration for a Cartesian axis, including label, range, ticks, and grid lines.</summary>
type AxisConfig =
    { Label: string option
      Min: float option
      Max: float option
      TickCount: int
      ShowGridLines: bool
      GridLineColor: SKColor option }

/// <summary>Shared configuration for charts that use Cartesian axes (line, bar, scatter, area, etc.).</summary>
type ChartConfig =
    { Title: string option
      TitleFontSize: float32
      Width: float32
      Height: float32
      Padding: float32
      XAxis: AxisConfig
      YAxis: AxisConfig
      Legend: LegendConfig
      Palette: ColorPalette
      BackgroundColor: SKColor option }

/// <summary>Layout mode for bar charts.</summary>
[<RequireQualifiedAccess>]
type BarLayout =
    /// <summary>Bars for each series are placed side by side within each category.</summary>
    | Grouped
    /// <summary>Bars for each series are stacked on top of one another within each category.</summary>
    | Stacked

/// <summary>A single data point with X and Y coordinates, used in line, scatter, and area charts.</summary>
type DataPoint =
    { X: float
      Y: float }

/// <summary>A named collection of data points representing one series in line, scatter, or area charts.</summary>
type DataSeries =
    { Name: string
      Points: DataPoint list }

/// <summary>A category with named values for bar charts, mapping series names to their values.</summary>
type CategoryValue =
    { Category: string
      Values: (string * float) list }

/// <summary>A labeled slice with a numeric value for pie and donut charts.</summary>
type SliceData =
    { Label: string
      Value: float }

/// <summary>Configuration for pie and donut charts, including donut hole ratio and label visibility.</summary>
type PieConfig =
    { Title: string option
      TitleFontSize: float32
      Width: float32
      Height: float32
      Padding: float32
      Legend: LegendConfig
      Palette: ColorPalette
      BackgroundColor: SKColor option
      DonutRatio: float32
      ShowLabels: bool }

/// <summary>Open-High-Low-Close data for a single candlestick in a candlestick chart.</summary>
type OhlcData =
    { Label: string
      Open: float
      High: float
      Low: float
      Close: float }

/// <summary>Configuration for candlestick charts, including colors for up and down candles.</summary>
type CandlestickConfig =
    { Title: string option
      TitleFontSize: float32
      Width: float32
      Height: float32
      Padding: float32
      XAxis: AxisConfig
      YAxis: AxisConfig
      Legend: LegendConfig
      Palette: ColorPalette
      BackgroundColor: SKColor option
      UpColor: SKColor
      DownColor: SKColor }

/// <summary>A named series of values for radar (spider) charts.</summary>
type RadarSeries =
    { Name: string
      Values: float list }

/// <summary>Configuration for radar (spider) charts, including category labels and grid settings.</summary>
type RadarConfig =
    { Title: string option
      TitleFontSize: float32
      Width: float32
      Height: float32
      Padding: float32
      Legend: LegendConfig
      Palette: ColorPalette
      BackgroundColor: SKColor option
      Categories: string list
      MaxValue: float option
      ShowGrid: bool
      GridLevels: int }

/// <summary>Configuration for histogram charts, including bin count and optional bar color.</summary>
type HistogramConfig =
    { Title: string option
      TitleFontSize: float32
      Width: float32
      Height: float32
      Padding: float32
      XAxis: AxisConfig
      YAxis: AxisConfig
      Legend: LegendConfig
      Palette: ColorPalette
      BackgroundColor: SKColor option
      BinCount: int
      BarColor: SKColor option }

/// <summary>Data type of a DataGrid column, used to control formatting and sorting behavior.</summary>
[<RequireQualifiedAccess>]
type ColumnType =
    /// <summary>Free-form text column.</summary>
    | Text
    /// <summary>Numeric column, formatted and sorted as floating-point values.</summary>
    | Numeric
    /// <summary>Boolean column, displayed as a checkbox or true/false indicator.</summary>
    | Boolean

/// <summary>Definition of a single column in a DataGrid, including name, type, and sizing.</summary>
type ColumnDef =
    { Name: string
      Type: ColumnType
      Sortable: bool
      MinWidth: float32 option }

/// <summary>A typed cell value within a DataGrid row.</summary>
[<RequireQualifiedAccess>]
type CellValue =
    /// <summary>A text cell value.</summary>
    | TextValue of string
    /// <summary>A numeric cell value.</summary>
    | NumericValue of float
    /// <summary>A boolean cell value.</summary>
    | BoolValue of bool

/// <summary>Sort direction for DataGrid columns.</summary>
[<RequireQualifiedAccess>]
type SortDirection =
    /// <summary>Sort in ascending order (smallest to largest).</summary>
    | Ascending
    /// <summary>Sort in descending order (largest to smallest).</summary>
    | Descending
    /// <summary>No sorting applied.</summary>
    | None

/// <summary>Represents the current sort state of a DataGrid, identifying which column is sorted and in which direction.</summary>
type SortState =
    { ColumnIndex: int
      Direction: SortDirection }

/// <summary>Visual and layout configuration for a DataGrid element.</summary>
type DataGridConfig =
    { Width: float32
      Height: float32
      RowHeight: float32
      HeaderHeight: float32
      HeaderColor: SKColor
      AlternateRowColor: SKColor option
      FontSize: float32
      HeaderFontSize: float32
      ScrollOffset: float
      Sort: SortState option }

/// <summary>Data payload for a DataGrid element, consisting of column definitions and row data.</summary>
type DataGridData =
    { Columns: ColumnDef list
      Rows: CellValue list list }

/// <summary>Default values and factory functions for chart configuration records.</summary>
module Defaults =
    /// <summary>Tableau-10 categorical color palette with ten distinct colors.</summary>
    val palette: ColorPalette
    /// <summary>Default axis configuration with no label, auto-ranging, and visible grid lines.</summary>
    val axisConfig: AxisConfig
    /// <summary>Default legend configuration (visible, positioned at the bottom).</summary>
    val legendConfig: LegendConfig
    /// <summary>Creates a default ChartConfig for the given width and height.</summary>
    val chartConfig: width: float32 -> height: float32 -> ChartConfig
    /// <summary>Creates a default PieConfig for the given width and height.</summary>
    val pieConfig: width: float32 -> height: float32 -> PieConfig
    /// <summary>Creates a default HistogramConfig for the given width and height.</summary>
    val histogramConfig: width: float32 -> height: float32 -> HistogramConfig
    /// <summary>Creates a default CandlestickConfig for the given width and height.</summary>
    val candlestickConfig: width: float32 -> height: float32 -> CandlestickConfig
    /// <summary>Creates a default RadarConfig for the given width, height, and category labels.</summary>
    val radarConfig: width: float32 -> height: float32 -> categories: string list -> RadarConfig
    /// <summary>Creates a default DataGridConfig for the given width and height.</summary>
    val dataGridConfig: width: float32 -> height: float32 -> DataGridConfig

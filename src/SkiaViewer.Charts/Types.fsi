namespace SkiaViewer.Charts

open SkiaSharp

/// Color palette for assigning distinct colors to data series.
type ColorPalette =
    { Colors: SKColor list }

/// Legend display position.
[<RequireQualifiedAccess>]
type LegendPosition = | Top | Bottom | Left | Right

/// Legend configuration.
type LegendConfig =
    { Visible: bool
      Position: LegendPosition }

/// Axis configuration for charts with Cartesian axes.
type AxisConfig =
    { Label: string option
      Min: float option
      Max: float option
      TickCount: int
      ShowGridLines: bool
      GridLineColor: SKColor option }

/// Shared chart configuration.
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

/// Bar chart layout mode.
[<RequireQualifiedAccess>]
type BarLayout = | Grouped | Stacked

/// A single data point with x and y coordinates.
type DataPoint =
    { X: float
      Y: float }

/// A named collection of data points for line, scatter, and area charts.
type DataSeries =
    { Name: string
      Points: DataPoint list }

/// A category with named values for bar charts.
type CategoryValue =
    { Category: string
      Values: (string * float) list }

/// A labeled slice for pie/donut charts.
type SliceData =
    { Label: string
      Value: float }

/// Configuration for pie/donut charts.
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

/// OHLC data for candlestick charts.
type OhlcData =
    { Label: string
      Open: float
      High: float
      Low: float
      Close: float }

/// Configuration for candlestick charts.
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

/// A named series of values for radar charts.
type RadarSeries =
    { Name: string
      Values: float list }

/// Configuration for radar/spider charts.
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

/// Configuration for histogram charts.
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

/// DataGrid column data type.
[<RequireQualifiedAccess>]
type ColumnType = | Text | Numeric | Boolean

/// DataGrid column definition.
type ColumnDef =
    { Name: string
      Type: ColumnType
      Sortable: bool
      MinWidth: float32 option }

/// A single cell value in a DataGrid row.
[<RequireQualifiedAccess>]
type CellValue =
    | TextValue of string
    | NumericValue of float
    | BoolValue of bool

/// Sort direction for DataGrid columns.
[<RequireQualifiedAccess>]
type SortDirection = | Ascending | Descending | None

/// Current sort state of a DataGrid.
type SortState =
    { ColumnIndex: int
      Direction: SortDirection }

/// Configuration for DataGrid element.
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

/// Data for a DataGrid element.
type DataGridData =
    { Columns: ColumnDef list
      Rows: CellValue list list }

/// Default values and helpers for chart configuration.
module Defaults =
    /// Tableau-10 categorical color palette.
    val palette: ColorPalette
    /// Default axis configuration.
    val axisConfig: AxisConfig
    /// Default legend configuration.
    val legendConfig: LegendConfig
    /// Default chart configuration for the given dimensions.
    val chartConfig: width: float32 -> height: float32 -> ChartConfig
    /// Default pie configuration for the given dimensions.
    val pieConfig: width: float32 -> height: float32 -> PieConfig
    /// Default histogram configuration for the given dimensions.
    val histogramConfig: width: float32 -> height: float32 -> HistogramConfig
    /// Default candlestick configuration for the given dimensions.
    val candlestickConfig: width: float32 -> height: float32 -> CandlestickConfig
    /// Default radar configuration for the given dimensions and categories.
    val radarConfig: width: float32 -> height: float32 -> categories: string list -> RadarConfig
    /// Default DataGrid configuration for the given dimensions.
    val dataGridConfig: width: float32 -> height: float32 -> DataGridConfig

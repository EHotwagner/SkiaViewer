namespace SkiaViewer.Charts

open SkiaSharp

type ColorPalette =
    { Colors: SKColor list }

[<RequireQualifiedAccess>]
type LegendPosition = | Top | Bottom | Left | Right

type LegendConfig =
    { Visible: bool
      Position: LegendPosition }

type AxisConfig =
    { Label: string option
      Min: float option
      Max: float option
      TickCount: int
      ShowGridLines: bool
      GridLineColor: SKColor option }

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

[<RequireQualifiedAccess>]
type BarLayout = | Grouped | Stacked

type DataPoint =
    { X: float
      Y: float }

type DataSeries =
    { Name: string
      Points: DataPoint list }

type CategoryValue =
    { Category: string
      Values: (string * float) list }

type SliceData =
    { Label: string
      Value: float }

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

type OhlcData =
    { Label: string
      Open: float
      High: float
      Low: float
      Close: float }

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

type RadarSeries =
    { Name: string
      Values: float list }

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

[<RequireQualifiedAccess>]
type ColumnType = | Text | Numeric | Boolean

type ColumnDef =
    { Name: string
      Type: ColumnType
      Sortable: bool
      MinWidth: float32 option }

[<RequireQualifiedAccess>]
type CellValue =
    | TextValue of string
    | NumericValue of float
    | BoolValue of bool

[<RequireQualifiedAccess>]
type SortDirection = | Ascending | Descending | None

type SortState =
    { ColumnIndex: int
      Direction: SortDirection }

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

type DataGridData =
    { Columns: ColumnDef list
      Rows: CellValue list list }

module Defaults =

    let palette : ColorPalette =
        { Colors =
            [ SKColor(0x4Euy, 0x79uy, 0xA7uy)   // blue
              SKColor(0xF2uy, 0x8Euy, 0x2Buy)   // orange
              SKColor(0xE1uy, 0x57uy, 0x59uy)   // red
              SKColor(0x76uy, 0xB7uy, 0xB2uy)   // teal
              SKColor(0x59uy, 0xA1uy, 0x4Fuy)   // green
              SKColor(0xEDuy, 0xC9uy, 0x48uy)   // yellow
              SKColor(0xB0uy, 0x7Auy, 0xA1uy)   // purple
              SKColor(0xFFuy, 0x9Duy, 0xA7uy)   // pink
              SKColor(0x9Cuy, 0x75uy, 0x5Fuy)   // brown
              SKColor(0xBAuy, 0xB0uy, 0xACuy) ] // gray
        }

    let axisConfig : AxisConfig =
        { Label = None
          Min = None
          Max = None
          TickCount = 5
          ShowGridLines = true
          GridLineColor = None }

    let legendConfig : LegendConfig =
        { Visible = true
          Position = LegendPosition.Bottom }

    let chartConfig (width: float32) (height: float32) : ChartConfig =
        { Title = None
          TitleFontSize = 16.0f
          Width = width
          Height = height
          Padding = 40.0f
          XAxis = axisConfig
          YAxis = axisConfig
          Legend = legendConfig
          Palette = palette
          BackgroundColor = None }

    let pieConfig (width: float32) (height: float32) : PieConfig =
        { Title = None
          TitleFontSize = 16.0f
          Width = width
          Height = height
          Padding = 40.0f
          Legend = legendConfig
          Palette = palette
          BackgroundColor = None
          DonutRatio = 0.0f
          ShowLabels = true }

    let histogramConfig (width: float32) (height: float32) : HistogramConfig =
        { Title = None
          TitleFontSize = 16.0f
          Width = width
          Height = height
          Padding = 40.0f
          XAxis = axisConfig
          YAxis = axisConfig
          Legend = legendConfig
          Palette = palette
          BackgroundColor = None
          BinCount = 10
          BarColor = None }

    let candlestickConfig (width: float32) (height: float32) : CandlestickConfig =
        { Title = None
          TitleFontSize = 16.0f
          Width = width
          Height = height
          Padding = 40.0f
          XAxis = axisConfig
          YAxis = axisConfig
          Legend = legendConfig
          Palette = palette
          BackgroundColor = None
          UpColor = SKColor(0x59uy, 0xA1uy, 0x4Fuy)
          DownColor = SKColor(0xE1uy, 0x57uy, 0x59uy) }

    let radarConfig (width: float32) (height: float32) (categories: string list) : RadarConfig =
        { Title = None
          TitleFontSize = 16.0f
          Width = width
          Height = height
          Padding = 40.0f
          Legend = legendConfig
          Palette = palette
          BackgroundColor = None
          Categories = categories
          MaxValue = None
          ShowGrid = true
          GridLevels = 5 }

    let dataGridConfig (width: float32) (height: float32) : DataGridConfig =
        { Width = width
          Height = height
          RowHeight = 30.0f
          HeaderHeight = 36.0f
          HeaderColor = SKColor(0xE0uy, 0xE0uy, 0xE0uy)
          AlternateRowColor = Some (SKColor(0xF5uy, 0xF5uy, 0xF5uy))
          FontSize = 14.0f
          HeaderFontSize = 14.0f
          ScrollOffset = 0.0
          Sort = None }

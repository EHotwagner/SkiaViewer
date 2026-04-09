namespace SkiaViewer.Charts

open SkiaSharp
open SkiaViewer

/// Chart area bounds after accounting for padding, title, and legend.
type internal ChartArea =
    { Left: float32
      Top: float32
      Right: float32
      Bottom: float32 }

/// Internal shared rendering helpers for chart elements.
module internal ChartHelpers =
    /// Compute the drawable chart area within the element bounds.
    val computeChartArea: width: float32 -> height: float32 -> padding: float32 -> hasTitle: bool -> titleFontSize: float32 -> hasLegend: bool -> ChartArea

    /// Render a chart title as a Text element.
    val renderTitle: title: string -> fontSize: float32 -> width: float32 -> Element

    /// Render X axis (line + tick marks + labels) as Element list.
    val renderXAxis: area: ChartArea -> ticks: (float * string) list -> axisMin: float -> axisMax: float -> label: string option -> Element list

    /// Render Y axis (line + tick marks + labels) as Element list.
    val renderYAxis: area: ChartArea -> ticks: (float * string) list -> axisMin: float -> axisMax: float -> label: string option -> Element list

    /// Render grid lines as Element list.
    val renderGridLines: area: ChartArea -> xTicks: (float * string) list -> yTicks: (float * string) list -> xMin: float -> xMax: float -> yMin: float -> yMax: float -> gridColor: SKColor -> Element list

    /// Render a legend for the given series names and palette.
    val renderLegend: names: string list -> palette: ColorPalette -> position: LegendPosition -> width: float32 -> height: float32 -> area: ChartArea -> Element

    /// Map a data value to a pixel coordinate within the chart area.
    val mapX: value: float -> dataMin: float -> dataMax: float -> area: ChartArea -> float32

    /// Map a data value to a pixel coordinate within the chart area (Y axis, inverted).
    val mapY: value: float -> dataMin: float -> dataMax: float -> area: ChartArea -> float32

    /// Get a color from the palette by index (wraps around).
    val paletteColor: palette: ColorPalette -> index: int -> SKColor

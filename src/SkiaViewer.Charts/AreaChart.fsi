namespace SkiaViewer.Charts

open SkiaViewer

/// Area chart creation module.
module AreaChart =
    /// Create an area chart element with filled regions under data lines.
    val areaChart: config: ChartConfig -> series: DataSeries list -> Element
    /// Default chart configuration for the given dimensions.
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

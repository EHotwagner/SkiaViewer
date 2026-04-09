namespace SkiaViewer.Charts

open SkiaViewer

/// Line chart creation module.
module LineChart =
    /// Create a line chart element from the given configuration and data series.
    val lineChart: config: ChartConfig -> series: DataSeries list -> Element
    /// Default chart configuration for the given dimensions.
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

namespace SkiaViewer.Charts

open SkiaViewer

/// Scatter plot creation module.
module ScatterPlot =
    /// Create a scatter plot element.
    val scatterPlot: config: ChartConfig -> series: DataSeries list -> Element
    /// Default chart configuration for the given dimensions.
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

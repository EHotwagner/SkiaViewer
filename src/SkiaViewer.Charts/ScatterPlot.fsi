namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>Scatter plot creation module. Renders data points as individual markers on a Cartesian plane with axes and grid lines.</summary>
module ScatterPlot =
    /// <summary>Create a scatter plot element.</summary>
    /// <param name="config">Chart configuration (dimensions, axes, palette, title).</param>
    /// <param name="series">Data series to plot. Each series renders as distinctly colored markers.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete scatter plot.</returns>
    val scatterPlot: config: ChartConfig -> series: DataSeries list -> Element
    /// <summary>Default chart configuration for the given dimensions.</summary>
    /// <param name="width">Width of the chart area in pixels.</param>
    /// <param name="height">Height of the chart area in pixels.</param>
    /// <returns>A ChartConfig with sensible defaults for the specified size.</returns>
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

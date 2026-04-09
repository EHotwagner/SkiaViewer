namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>Line chart creation module. Renders data series as connected line segments with axes, grid lines, and legends.</summary>
module LineChart =
    /// <summary>Create a line chart element from the given configuration and data series.</summary>
    /// <param name="config">Chart configuration (dimensions, axes, palette, title).</param>
    /// <param name="series">Data series to plot. Each series renders as a distinct colored line.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete chart.</returns>
    val lineChart: config: ChartConfig -> series: DataSeries list -> Element
    /// <summary>Default chart configuration for the given dimensions.</summary>
    /// <param name="width">Width of the chart area in pixels.</param>
    /// <param name="height">Height of the chart area in pixels.</param>
    /// <returns>A ChartConfig with sensible defaults for the specified size.</returns>
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

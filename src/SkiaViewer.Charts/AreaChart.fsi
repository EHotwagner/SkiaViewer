namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>Area chart creation module. Renders data series as filled regions under connected line segments with axes and legends.</summary>
module AreaChart =
    /// <summary>Create an area chart element with filled regions under data lines.</summary>
    /// <param name="config">Chart configuration (dimensions, axes, palette, title).</param>
    /// <param name="series">Data series to plot. Each series renders as a filled area with a distinct color.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete area chart.</returns>
    val areaChart: config: ChartConfig -> series: DataSeries list -> Element
    /// <summary>Default chart configuration for the given dimensions.</summary>
    /// <param name="width">Width of the chart area in pixels.</param>
    /// <param name="height">Height of the chart area in pixels.</param>
    /// <returns>A ChartConfig with sensible defaults for the specified size.</returns>
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

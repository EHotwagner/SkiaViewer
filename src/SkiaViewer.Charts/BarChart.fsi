namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>Bar chart creation module. Renders categorical data as vertical or horizontal bars with axes and labels.</summary>
module BarChart =
    /// <summary>Create a bar chart element from the given configuration, layout, and category data.</summary>
    /// <param name="config">Chart configuration (dimensions, axes, palette, title).</param>
    /// <param name="layout">Bar layout specifying orientation, spacing, and grouping.</param>
    /// <param name="data">Category-value pairs to plot as bars.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete bar chart.</returns>
    val barChart: config: ChartConfig -> layout: BarLayout -> data: CategoryValue list -> Element
    /// <summary>Default chart configuration for the given dimensions.</summary>
    /// <param name="width">Width of the chart area in pixels.</param>
    /// <param name="height">Height of the chart area in pixels.</param>
    /// <returns>A ChartConfig with sensible defaults for the specified size.</returns>
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

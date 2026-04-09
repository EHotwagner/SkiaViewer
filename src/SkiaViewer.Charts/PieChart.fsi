namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>Pie and donut chart creation module. Renders proportional data as circular slices with optional labels and legends.</summary>
module PieChart =
    /// <summary>Create a pie or donut chart element.</summary>
    /// <param name="config">Pie configuration (dimensions, inner radius for donut, label options).</param>
    /// <param name="slices">Slice data defining labels, values, and optional colors for each segment.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete pie/donut chart.</returns>
    val pieChart: config: PieConfig -> slices: SliceData list -> Element
    /// <summary>Default pie configuration for the given dimensions.</summary>
    /// <param name="width">Width of the chart area in pixels.</param>
    /// <param name="height">Height of the chart area in pixels.</param>
    /// <returns>A PieConfig with sensible defaults for the specified size.</returns>
    val defaultConfig: width: float32 -> height: float32 -> PieConfig

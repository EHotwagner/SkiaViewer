namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>Histogram creation module. Bins continuous data values and renders them as vertical bars showing frequency distribution.</summary>
module Histogram =
    /// <summary>Create a histogram element from raw data values.</summary>
    /// <param name="config">Histogram configuration (dimensions, bin count, axes, palette).</param>
    /// <param name="values">Raw data values to bin and visualize.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete histogram.</returns>
    val histogram: config: HistogramConfig -> values: float list -> Element
    /// <summary>Default histogram configuration for the given dimensions.</summary>
    /// <param name="width">Width of the chart area in pixels.</param>
    /// <param name="height">Height of the chart area in pixels.</param>
    /// <returns>A HistogramConfig with sensible defaults for the specified size.</returns>
    val defaultConfig: width: float32 -> height: float32 -> HistogramConfig

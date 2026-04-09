namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>Radar/spider chart creation module. Renders multivariate data on radial axes emanating from a center point.</summary>
module RadarChart =
    /// <summary>Create a radar chart element.</summary>
    /// <param name="config">Radar configuration (dimensions, categories, scale, fill options).</param>
    /// <param name="series">Radar series to plot. Each series renders as a colored polygon on the radar axes.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete radar chart.</returns>
    val radarChart: config: RadarConfig -> series: RadarSeries list -> Element
    /// <summary>Default radar configuration for the given dimensions and categories.</summary>
    /// <param name="width">Width of the chart area in pixels.</param>
    /// <param name="height">Height of the chart area in pixels.</param>
    /// <param name="categories">Category labels for each radial axis.</param>
    /// <returns>A RadarConfig with sensible defaults for the specified size and categories.</returns>
    val defaultConfig: width: float32 -> height: float32 -> categories: string list -> RadarConfig

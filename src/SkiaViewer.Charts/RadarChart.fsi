namespace SkiaViewer.Charts

open SkiaViewer

/// Radar/spider chart creation module.
module RadarChart =
    /// Create a radar chart element.
    val radarChart: config: RadarConfig -> series: RadarSeries list -> Element
    /// Default radar configuration for the given dimensions and categories.
    val defaultConfig: width: float32 -> height: float32 -> categories: string list -> RadarConfig

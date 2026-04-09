namespace SkiaViewer.Charts

open SkiaViewer

/// Histogram creation module.
module Histogram =
    /// Create a histogram element from raw data values.
    val histogram: config: HistogramConfig -> values: float list -> Element
    /// Default histogram configuration for the given dimensions.
    val defaultConfig: width: float32 -> height: float32 -> HistogramConfig

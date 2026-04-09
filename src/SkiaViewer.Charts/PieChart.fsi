namespace SkiaViewer.Charts

open SkiaViewer

/// Pie and donut chart creation module.
module PieChart =
    /// Create a pie or donut chart element.
    val pieChart: config: PieConfig -> slices: SliceData list -> Element
    /// Default pie configuration for the given dimensions.
    val defaultConfig: width: float32 -> height: float32 -> PieConfig

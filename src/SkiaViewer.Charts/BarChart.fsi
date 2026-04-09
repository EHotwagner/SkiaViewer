namespace SkiaViewer.Charts

open SkiaViewer

/// Bar chart creation module.
module BarChart =
    /// Create a bar chart element from the given configuration, layout, and category data.
    val barChart: config: ChartConfig -> layout: BarLayout -> data: CategoryValue list -> Element
    /// Default chart configuration for the given dimensions.
    val defaultConfig: width: float32 -> height: float32 -> ChartConfig

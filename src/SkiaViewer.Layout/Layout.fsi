namespace SkiaViewer.Layout

open SkiaViewer

module Layout =
    /// Arrange children in a horizontal stack within the given bounds.
    val hstack: config: StackConfig -> children: LayoutChild list -> width: float32 -> height: float32 -> Element

    /// Arrange children in a vertical stack within the given bounds.
    val vstack: config: StackConfig -> children: LayoutChild list -> width: float32 -> height: float32 -> Element

    /// Arrange children in a dock layout within the given bounds.
    val dock: config: DockConfig -> children: DockChild list -> width: float32 -> height: float32 -> Element

    /// Create a LayoutChild with default sizing and alignment.
    val child: element: Element -> LayoutChild

    /// Create a LayoutChild with specified sizing.
    val childWithSize: width: float32 -> height: float32 -> element: Element -> LayoutChild

    /// Create a DockChild.
    val dockChild: position: DockPosition -> element: Element -> DockChild

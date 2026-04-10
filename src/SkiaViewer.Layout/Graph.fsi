namespace SkiaViewer.Layout

open SkiaViewer

module Graph =
    /// Render a graph definition as a Scene Element within the given bounds.
    val render: graph: GraphDefinition -> width: float32 -> height: float32 -> Result<Element, string>

    /// Create a default graph config for the given kind.
    val defaultConfig: kind: GraphKind -> GraphConfig

    /// Validate a graph definition without rendering.
    val validate: graph: GraphDefinition -> Result<unit, string list>

namespace SkiaViewer.Layout

module GraphValidation =
    /// Validate a graph definition: check unique node IDs, valid edge references, and cycle detection for DAGs.
    val validate: graph: GraphDefinition -> Result<unit, string list>

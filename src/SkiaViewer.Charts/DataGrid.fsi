namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>DataGrid element creation module. Renders tabular data with column headers, sorting, and virtual scrolling support.</summary>
module DataGrid =
    /// <summary>Create a DataGrid element.</summary>
    /// <param name="config">DataGrid configuration (dimensions, row height, scroll offset, column widths).</param>
    /// <param name="data">Grid data containing column definitions and row cell values.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete data grid.</returns>
    val dataGrid: config: DataGridConfig -> data: DataGridData -> Element
    /// <summary>Default DataGrid configuration for the given dimensions.</summary>
    /// <param name="width">Width of the grid area in pixels.</param>
    /// <param name="height">Height of the grid area in pixels.</param>
    /// <returns>A DataGridConfig with sensible defaults for the specified size.</returns>
    val defaultConfig: width: float32 -> height: float32 -> DataGridConfig
    /// <summary>Create a text column definition.</summary>
    /// <param name="name">Display name for the column header.</param>
    /// <returns>A ColumnDef configured for text cell values.</returns>
    val textColumn: name: string -> ColumnDef
    /// <summary>Create a numeric column definition.</summary>
    /// <param name="name">Display name for the column header.</param>
    /// <returns>A ColumnDef configured for numeric cell values.</returns>
    val numericColumn: name: string -> ColumnDef
    /// <summary>Create a boolean column definition.</summary>
    /// <param name="name">Display name for the column header.</param>
    /// <returns>A ColumnDef configured for boolean cell values.</returns>
    val boolColumn: name: string -> ColumnDef
    /// <summary>Sort rows by column index with type-aware comparison.</summary>
    /// <param name="columns">Column definitions used for type-aware comparison.</param>
    /// <param name="columnIndex">Zero-based index of the column to sort by.</param>
    /// <param name="direction">Sort direction (ascending or descending).</param>
    /// <param name="rows">Row data to sort, where each row is a list of cell values.</param>
    /// <returns>A new list of rows sorted by the specified column.</returns>
    val sortRows: columns: ColumnDef list -> columnIndex: int -> direction: SortDirection -> rows: CellValue list list -> CellValue list list
    /// <summary>Compute visible row range for virtual scrolling.</summary>
    /// <param name="config">DataGrid configuration containing row height and scroll offset.</param>
    /// <param name="totalRows">Total number of rows in the data set.</param>
    /// <returns>A tuple (startIndex, endIndex) representing the visible row range.</returns>
    val visibleRange: config: DataGridConfig -> totalRows: int -> int * int

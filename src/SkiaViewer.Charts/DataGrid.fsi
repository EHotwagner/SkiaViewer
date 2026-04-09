namespace SkiaViewer.Charts

open SkiaViewer

/// DataGrid element creation module.
module DataGrid =
    /// Create a DataGrid element.
    val dataGrid: config: DataGridConfig -> data: DataGridData -> Element
    /// Default DataGrid configuration for the given dimensions.
    val defaultConfig: width: float32 -> height: float32 -> DataGridConfig
    /// Create a text column definition.
    val textColumn: name: string -> ColumnDef
    /// Create a numeric column definition.
    val numericColumn: name: string -> ColumnDef
    /// Create a boolean column definition.
    val boolColumn: name: string -> ColumnDef
    /// Sort rows by column index with type-aware comparison.
    val sortRows: columns: ColumnDef list -> columnIndex: int -> direction: SortDirection -> rows: CellValue list list -> CellValue list list
    /// Compute visible row range for virtual scrolling. Returns (startIndex, endIndex).
    val visibleRange: config: DataGridConfig -> totalRows: int -> int * int

/// DataGrid Demo — renders a sortable, scrollable DataGrid.
///
/// Usage:
///   dotnet fsi scripts/examples/04-datagrid.fsx

#load "../charts-prelude.fsx"

open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

// --- Define columns ---
let columns =
    [ DataGrid.textColumn "Name"
      DataGrid.numericColumn "Score"
      DataGrid.boolColumn "Passed"
      DataGrid.textColumn "Grade" ]

// --- Sample data ---
let rows =
    [ [ CellValue.TextValue "Alice";   CellValue.NumericValue 95.0;  CellValue.BoolValue true;  CellValue.TextValue "A" ]
      [ CellValue.TextValue "Bob";     CellValue.NumericValue 72.0;  CellValue.BoolValue true;  CellValue.TextValue "C" ]
      [ CellValue.TextValue "Carol";   CellValue.NumericValue 58.0;  CellValue.BoolValue false; CellValue.TextValue "F" ]
      [ CellValue.TextValue "Dave";    CellValue.NumericValue 88.0;  CellValue.BoolValue true;  CellValue.TextValue "B" ]
      [ CellValue.TextValue "Eve";     CellValue.NumericValue 91.0;  CellValue.BoolValue true;  CellValue.TextValue "A" ]
      [ CellValue.TextValue "Frank";   CellValue.NumericValue 45.0;  CellValue.BoolValue false; CellValue.TextValue "F" ]
      [ CellValue.TextValue "Grace";   CellValue.NumericValue 79.0;  CellValue.BoolValue true;  CellValue.TextValue "C" ]
      [ CellValue.TextValue "Hank";    CellValue.NumericValue 63.0;  CellValue.BoolValue true;  CellValue.TextValue "D" ] ]

// --- Unsorted grid ---
let gridConfig = DataGrid.defaultConfig 600f 300f
let data = { Columns = columns; Rows = rows }
let unsortedGrid = DataGrid.dataGrid gridConfig data

// --- Sorted by Score descending ---
let sortedRows = DataGrid.sortRows columns 1 SortDirection.Descending rows
let sortedGrid =
    DataGrid.dataGrid
        { gridConfig with Sort = Some { ColumnIndex = 1; Direction = SortDirection.Descending } }
        { data with Rows = sortedRows }

// --- Compose scene ---
let scene =
    Scene.create SKColors.White
        [ Scene.translate 20f 20f [ unsortedGrid ]
          Scene.translate 20f 340f [ sortedGrid ] ]

printfn "DataGrid demo scene created."
printfn "Top: unsorted grid. Bottom: sorted by Score descending."
printfn "To display: pass 'scene' to Viewer.run with your config."

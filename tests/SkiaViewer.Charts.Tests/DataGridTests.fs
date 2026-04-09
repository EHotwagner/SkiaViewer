module SkiaViewer.Charts.Tests.DataGridTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

let rec countElements (el: Element) =
    match el with
    | Element.Group(_, _, _, children) -> children |> List.sumBy countElements
    | _ -> 1

[<Fact>]
let ``DataGrid_BasicData_ReturnsGroup`` () =
    let config = DataGrid.defaultConfig 600f 400f
    let data =
        { Columns = [ DataGrid.textColumn "Name"; DataGrid.numericColumn "Score" ]
          Rows =
            [ [ CellValue.TextValue "Alice"; CellValue.NumericValue 95.0 ]
              [ CellValue.TextValue "Bob"; CellValue.NumericValue 87.0 ] ] }
    let result = DataGrid.dataGrid config data
    match result with
    | Element.Group(_, _, _, children) -> Assert.True(children.Length > 0)
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``DataGrid_EmptyRows_ShowsHeader`` () =
    let config = DataGrid.defaultConfig 600f 400f
    let data =
        { Columns = [ DataGrid.textColumn "Name" ]
          Rows = [] }
    let result = DataGrid.dataGrid config data
    let count = countElements result
    Assert.True(count > 0, "Should have header elements even with no rows")

[<Fact>]
let ``DataGrid_SortRows_Ascending`` () =
    let cols = [ DataGrid.numericColumn "Score" ]
    let rows =
        [ [ CellValue.NumericValue 50.0 ]
          [ CellValue.NumericValue 10.0 ]
          [ CellValue.NumericValue 30.0 ] ]
    let sorted = DataGrid.sortRows cols 0 SortDirection.Ascending rows
    let values = sorted |> List.map (fun r ->
        match r.[0] with CellValue.NumericValue v -> v | _ -> 0.0)
    Assert.Equal<float list>([ 10.0; 30.0; 50.0 ], values)

[<Fact>]
let ``DataGrid_SortRows_Descending`` () =
    let cols = [ DataGrid.numericColumn "Score" ]
    let rows =
        [ [ CellValue.NumericValue 50.0 ]
          [ CellValue.NumericValue 10.0 ]
          [ CellValue.NumericValue 30.0 ] ]
    let sorted = DataGrid.sortRows cols 0 SortDirection.Descending rows
    let values = sorted |> List.map (fun r ->
        match r.[0] with CellValue.NumericValue v -> v | _ -> 0.0)
    Assert.Equal<float list>([ 50.0; 30.0; 10.0 ], values)

[<Fact>]
let ``DataGrid_VisibleRange_FirstPage`` () =
    let config = DataGrid.defaultConfig 600f 400f
    let (startIdx, _) = DataGrid.visibleRange config 100
    Assert.Equal(0, startIdx)

[<Fact>]
let ``DataGrid_VisibleRange_Scrolled`` () =
    let config = { DataGrid.defaultConfig 600f 400f with ScrollOffset = 300.0; RowHeight = 30.0f }
    let (startIdx, _) = DataGrid.visibleRange config 100
    Assert.Equal(10, startIdx)

[<Fact>]
let ``DataGrid_TextColumn_HasCorrectType`` () =
    let col = DataGrid.textColumn "Name"
    Assert.Equal(ColumnType.Text, col.Type)

[<Fact>]
let ``DataGrid_BoolColumn_HasCorrectType`` () =
    let col = DataGrid.boolColumn "Active"
    Assert.Equal(ColumnType.Boolean, col.Type)

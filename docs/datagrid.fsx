(**
---
title: DataGrid
category: Tutorials
categoryindex: 2
index: 7
description: Render sortable, scrollable tabular data with text, numeric, and boolean columns.
---
*)

(**
# DataGrid

The DataGrid element renders tabular data with column headers, type-aware
cell formatting, virtual scrolling for large datasets, and column sorting.
Like all chart elements, it returns a standard `Element` and composes freely
with the rest of the scene DSL.

## Setup
*)

(*** condition: prepare ***)
#r "../src/SkiaViewer/bin/Release/net10.0/SkiaViewer.dll"
#r "../src/SkiaViewer.Charts/bin/Release/net10.0/SkiaViewer.Charts.dll"
(*** condition: fsx ***)
#r "nuget: SkiaViewer"
#r "nuget: SkiaViewer.Charts"

open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

(**
## Defining Columns

Use the helper constructors to define typed columns. Each column has a name,
a data type (Text, Numeric, or Boolean), and is sortable by default.
*)

let columns =
    [ DataGrid.textColumn "Name"
      DataGrid.numericColumn "Score"
      DataGrid.boolColumn "Passed"
      DataGrid.textColumn "Grade" ]

(**
## Providing Row Data

Rows are lists of `CellValue`, matching column order:
*)

let rows =
    [ [ CellValue.TextValue "Alice";   CellValue.NumericValue 95.0
        CellValue.BoolValue true;      CellValue.TextValue "A" ]
      [ CellValue.TextValue "Bob";     CellValue.NumericValue 72.0
        CellValue.BoolValue true;      CellValue.TextValue "C" ]
      [ CellValue.TextValue "Carol";   CellValue.NumericValue 58.0
        CellValue.BoolValue false;     CellValue.TextValue "F" ]
      [ CellValue.TextValue "Dave";    CellValue.NumericValue 88.0
        CellValue.BoolValue true;      CellValue.TextValue "B" ]
      [ CellValue.TextValue "Eve";     CellValue.NumericValue 91.0
        CellValue.BoolValue true;      CellValue.TextValue "A" ] ]

let data = { Columns = columns; Rows = rows }

(**
## Rendering a Basic Grid

Create a grid element with default configuration:
*)

let grid =
    DataGrid.dataGrid (DataGrid.defaultConfig 500f 250f) data

(**
The grid renders a gray header row with column titles, followed by data rows.
Text cells are left-aligned, numeric cells are right-aligned, and boolean
cells display as filled (true) or empty (false) square indicators.

## Sorting

Sort rows by any column using `DataGrid.sortRows`. The function is type-aware:
text sorts lexicographically, numerics by value, booleans by false-before-true.
*)

let sortedByScore =
    DataGrid.sortRows columns 1 SortDirection.Descending rows

let sortedGrid =
    DataGrid.dataGrid
        { DataGrid.defaultConfig 500f 250f with
            Sort = Some { ColumnIndex = 1; Direction = SortDirection.Descending } }
        { data with Rows = sortedByScore }

(**
Sort by name ascending:
*)

let sortedByName =
    DataGrid.sortRows columns 0 SortDirection.Ascending rows

(**
Sort by boolean column (passed students first):
*)

let sortedByPassed =
    DataGrid.sortRows columns 2 SortDirection.Descending rows

(**
## Virtual Scrolling

For large datasets, the DataGrid only renders visible rows. Control the
scroll position with `ScrollOffset` (in pixels):
*)

let largeRows =
    [ for i in 1 .. 1000 ->
        [ CellValue.TextValue $"Student {i}"
          CellValue.NumericValue (float (50 + i % 50))
          CellValue.BoolValue (i % 3 <> 0)
          CellValue.TextValue (if i % 3 = 0 then "F" else "P") ] ]

let scrolledGrid =
    DataGrid.dataGrid
        { DataGrid.defaultConfig 500f 300f with ScrollOffset = 600.0 }
        { Columns = columns; Rows = largeRows }

(**
The `visibleRange` function computes which rows are visible for a given
scroll position:
*)

let startIdx, endIdx =
    DataGrid.visibleRange
        { DataGrid.defaultConfig 500f 300f with ScrollOffset = 600.0 }
        1000

(**
## Column Auto-Fit

Columns automatically divide the available width equally. There is no
horizontal scrolling — all columns fit within the element width.

## Alternating Row Colors

By default, every other row has a subtle gray background
(`AlternateRowColor = Some (SKColor(0xF5, 0xF5, 0xF5))`). Set it to `None`
to disable:
*)

let noAlternating =
    DataGrid.dataGrid
        { DataGrid.defaultConfig 500f 250f with AlternateRowColor = None }
        data

(**
## Composing with Charts

DataGrids compose with chart elements in the same scene:
*)

let lineSeries =
    [ { Name = "Score Trend"
        Points = rows |> List.mapi (fun i row ->
            match row[1] with
            | CellValue.NumericValue v -> { X = float i; Y = v }
            | _ -> { X = float i; Y = 0.0 }) } ]

let dashboardScene =
    Scene.create SKColors.White
        [ Scene.translate 20f 20f
            [ LineChart.lineChart
                  { LineChart.defaultConfig 450f 200f with Title = Some "Score Trend" }
                  lineSeries ]
          Scene.translate 20f 240f
            [ DataGrid.dataGrid (DataGrid.defaultConfig 450f 200f) data ] ]

(**

<div class="alert alert-info">
<strong>Tip:</strong> The DataGrid uses a static/immutable data model. To update
data or change sort order, rebuild the element with new data and push a new scene.
</div>

## Configuration Reference

| Field | Default | Description |
|---|---|---|
| `Width` / `Height` | (required) | Total element dimensions |
| `RowHeight` | 30 | Height of each data row in pixels |
| `HeaderHeight` | 36 | Height of the header row |
| `HeaderColor` | Light gray | Background color of the header |
| `AlternateRowColor` | Subtle gray | Background for even rows (`None` to disable) |
| `FontSize` | 14 | Cell text size |
| `HeaderFontSize` | 14 | Header text size |
| `ScrollOffset` | 0 | Vertical scroll position in pixels |
| `Sort` | None | Current sort state (`ColumnIndex` + `Direction`) |

## Next Steps

- [Charting](charting.html) — line, bar, pie, scatter, area, histogram, candlestick, radar
- [API Reference](reference/index.html) — full type and function documentation
*)

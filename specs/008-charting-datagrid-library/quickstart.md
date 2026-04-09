# Quickstart: SkiaViewer.Charts

## Prerequisites

- .NET 10.0 SDK
- SkiaViewer project built (`dotnet build src/SkiaViewer`)

## Project Setup

Add a reference to `SkiaViewer.Charts` in your project:

```xml
<ProjectReference Include="../SkiaViewer.Charts/SkiaViewer.Charts.fsproj" />
```

## Line Chart Example

```fsharp
open SkiaViewer
open SkiaViewer.Charts

let config = Charts.defaultConfig 600f 400f

let series = [
    { Name = "Revenue"; Points = [ { X = 1.0; Y = 10.0 }; { X = 2.0; Y = 25.0 }; { X = 3.0; Y = 18.0 } ] }
    { Name = "Costs"; Points = [ { X = 1.0; Y = 8.0 }; { X = 2.0; Y = 12.0 }; { X = 3.0; Y = 15.0 } ] }
]

let chart = Charts.lineChart { config with Title = Some "Revenue vs Costs" } series

let scene = Scene.create SKColors.White [ Scene.translate 50f 50f [ chart ] ]
```

## Bar Chart Example

```fsharp
let data = [
    { Category = "Q1"; Values = [ ("2025", 100.0); ("2026", 120.0) ] }
    { Category = "Q2"; Values = [ ("2025", 90.0); ("2026", 140.0) ] }
    { Category = "Q3"; Values = [ ("2025", 110.0); ("2026", 130.0) ] }
]

let chart = Charts.barChart (Charts.defaultConfig 500f 300f) data
```

## Pie Chart Example

```fsharp
let slices = [
    { Label = "Desktop"; Value = 55.0 }
    { Label = "Mobile"; Value = 35.0 }
    { Label = "Tablet"; Value = 10.0 }
]

let chart = Charts.pieChart (Charts.defaultPieConfig 400f 400f) slices

// Donut variant:
let donut = Charts.pieChart { Charts.defaultPieConfig 400f 400f with DonutRatio = 0.5f } slices
```

## DataGrid Example

```fsharp
open SkiaViewer.Charts

let columns = [
    DataGrid.textColumn "Name"
    DataGrid.numericColumn "Score"
    DataGrid.boolColumn "Passed"
]

let rows = [
    [ TextValue "Alice"; NumericValue 95.0; BoolValue true ]
    [ TextValue "Bob"; NumericValue 72.0; BoolValue true ]
    [ TextValue "Carol"; NumericValue 58.0; BoolValue false ]
]

let config = { DataGrid.defaultConfig 500f 300f with Sort = Some { ColumnIndex = 1; Direction = Descending } }
let grid = DataGrid.dataGrid config { Columns = columns; Rows = rows }

let scene = Scene.create SKColors.White [ grid ]
```

## Composing Charts in a Scene

Charts are standard `Element` values. Compose them freely:

```fsharp
let dashboard = Scene.create SKColors.White [
    Scene.translate 20f 20f [ Charts.lineChart config series ]
    Scene.translate 640f 20f [ Charts.barChart config barData ]
    Scene.translate 20f 440f [ DataGrid.dataGrid gridConfig gridData ]
]
```

## FSI Usage

```fsharp
#load "scripts/prelude.fsx"
#r "../src/SkiaViewer.Charts/bin/Debug/net10.0/SkiaViewer.Charts.dll"
open SkiaViewer.Charts

// Create and display a chart
let chart = Charts.lineChart (Charts.defaultConfig 600f 400f) series
let scene = Scene.create SKColors.White [ chart ]
```

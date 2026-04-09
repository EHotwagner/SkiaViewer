module SkiaViewer.Charts.Tests.SurfaceAreaTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

[<Fact>]
let ``LineChart_lineChart_Exists`` () =
    let config = LineChart.defaultConfig 600f 400f
    let result = LineChart.lineChart config [ { Name = "S"; Points = [ { X = 0.0; Y = 1.0 } ] } ]
    Assert.NotNull(box result)

[<Fact>]
let ``LineChart_defaultConfig_Exists`` () =
    let result = LineChart.defaultConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``BarChart_barChart_Exists`` () =
    let config = BarChart.defaultConfig 600f 400f
    let result = BarChart.barChart config BarLayout.Grouped [ { Category = "A"; Values = [ ("S", 1.0) ] } ]
    Assert.NotNull(box result)

[<Fact>]
let ``BarChart_defaultConfig_Exists`` () =
    let result = BarChart.defaultConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``PieChart_pieChart_Exists`` () =
    let config = PieChart.defaultConfig 400f 400f
    let result = PieChart.pieChart config [ { Label = "A"; Value = 1.0 } ]
    Assert.NotNull(box result)

[<Fact>]
let ``PieChart_defaultConfig_Exists`` () =
    let result = PieChart.defaultConfig 400f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``ScatterPlot_scatterPlot_Exists`` () =
    let config = ScatterPlot.defaultConfig 600f 400f
    let result = ScatterPlot.scatterPlot config [ { Name = "S"; Points = [ { X = 1.0; Y = 2.0 } ] } ]
    Assert.NotNull(box result)

[<Fact>]
let ``ScatterPlot_defaultConfig_Exists`` () =
    let result = ScatterPlot.defaultConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``AreaChart_areaChart_Exists`` () =
    let config = AreaChart.defaultConfig 600f 400f
    let result = AreaChart.areaChart config [ { Name = "S"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 2.0 } ] } ]
    Assert.NotNull(box result)

[<Fact>]
let ``AreaChart_defaultConfig_Exists`` () =
    let result = AreaChart.defaultConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``Histogram_histogram_Exists`` () =
    let config = Histogram.defaultConfig 600f 400f
    let result = Histogram.histogram config [ 1.0; 2.0; 3.0 ]
    Assert.NotNull(box result)

[<Fact>]
let ``Histogram_defaultConfig_Exists`` () =
    let result = Histogram.defaultConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``Candlestick_candlestickChart_Exists`` () =
    let config = Candlestick.defaultConfig 600f 400f
    let result = Candlestick.candlestickChart config [ { Label = "D1"; Open = 10.0; High = 15.0; Low = 8.0; Close = 12.0 } ]
    Assert.NotNull(box result)

[<Fact>]
let ``Candlestick_defaultConfig_Exists`` () =
    let result = Candlestick.defaultConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``RadarChart_radarChart_Exists`` () =
    let config = RadarChart.defaultConfig 400f 400f [ "A"; "B"; "C" ]
    let result = RadarChart.radarChart config [ { Name = "P"; Values = [ 1.0; 2.0; 3.0 ] } ]
    Assert.NotNull(box result)

[<Fact>]
let ``RadarChart_defaultConfig_Exists`` () =
    let result = RadarChart.defaultConfig 400f 400f [ "A"; "B" ]
    Assert.NotNull(box result)

[<Fact>]
let ``DataGrid_dataGrid_Exists`` () =
    let config = DataGrid.defaultConfig 600f 400f
    let data = { Columns = [ DataGrid.textColumn "X" ]; Rows = [ [ CellValue.TextValue "a" ] ] }
    let result = DataGrid.dataGrid config data
    Assert.NotNull(box result)

[<Fact>]
let ``DataGrid_defaultConfig_Exists`` () =
    let result = DataGrid.defaultConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``DataGrid_textColumn_Exists`` () =
    let result = DataGrid.textColumn "Name"
    Assert.NotNull(box result)

[<Fact>]
let ``DataGrid_numericColumn_Exists`` () =
    let result = DataGrid.numericColumn "Score"
    Assert.NotNull(box result)

[<Fact>]
let ``DataGrid_boolColumn_Exists`` () =
    let result = DataGrid.boolColumn "Active"
    Assert.NotNull(box result)

[<Fact>]
let ``DataGrid_sortRows_Exists`` () =
    let cols = [ DataGrid.numericColumn "V" ]
    let rows = [ [ CellValue.NumericValue 1.0 ] ]
    let result = DataGrid.sortRows cols 0 SortDirection.Ascending rows
    Assert.NotNull(box result)

[<Fact>]
let ``DataGrid_visibleRange_Exists`` () =
    let config = DataGrid.defaultConfig 600f 400f
    let result = DataGrid.visibleRange config 10
    Assert.NotNull(box result)

[<Fact>]
let ``Defaults_palette_Exists`` () =
    Assert.NotNull(box Defaults.palette)

[<Fact>]
let ``Defaults_chartConfig_Exists`` () =
    let result = Defaults.chartConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``Defaults_pieConfig_Exists`` () =
    let result = Defaults.pieConfig 400f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``Defaults_histogramConfig_Exists`` () =
    let result = Defaults.histogramConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``Defaults_candlestickConfig_Exists`` () =
    let result = Defaults.candlestickConfig 600f 400f
    Assert.NotNull(box result)

[<Fact>]
let ``Defaults_radarConfig_Exists`` () =
    let result = Defaults.radarConfig 400f 400f [ "A" ]
    Assert.NotNull(box result)

[<Fact>]
let ``Defaults_dataGridConfig_Exists`` () =
    let result = Defaults.dataGridConfig 600f 400f
    Assert.NotNull(box result)

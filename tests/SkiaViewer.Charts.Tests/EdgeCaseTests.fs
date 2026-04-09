module SkiaViewer.Charts.Tests.EdgeCaseTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

[<Fact>]
let ``AllCharts_EmptyData_NoException`` () =
    let chartConfig = Defaults.chartConfig 600f 400f
    let pieConfig = Defaults.pieConfig 400f 400f
    let histConfig = Defaults.histogramConfig 600f 400f
    let candleConfig = Defaults.candlestickConfig 600f 400f
    let radarConfig = Defaults.radarConfig 400f 400f [ "A"; "B"; "C" ]
    let gridConfig = Defaults.dataGridConfig 600f 400f

    // Line chart with empty series
    let _ = LineChart.lineChart chartConfig []
    // Bar chart with empty categories
    let _ = BarChart.barChart chartConfig BarLayout.Grouped []
    // Pie chart with empty slices
    let _ = PieChart.pieChart pieConfig []
    // Scatter plot with empty series
    let _ = ScatterPlot.scatterPlot chartConfig []
    // Area chart with empty series
    let _ = AreaChart.areaChart chartConfig []
    // Histogram with empty values
    let _ = Histogram.histogram histConfig []
    // Candlestick with empty data
    let _ = Candlestick.candlestickChart candleConfig []
    // Radar chart with empty series
    let _ = RadarChart.radarChart radarConfig []
    // DataGrid with empty data
    let _ = DataGrid.dataGrid gridConfig { Columns = [ DataGrid.textColumn "X" ]; Rows = [] }
    ()

[<Fact>]
let ``LineChart_VerySmallSize_NoException`` () =
    let config = LineChart.defaultConfig 10f 10f
    let series = [ { Name = "S"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 2.0 } ] } ]
    let result = LineChart.lineChart config series
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``DataGrid_ZeroColumns_NoException`` () =
    let config = DataGrid.defaultConfig 600f 400f
    let data = { Columns = []; Rows = [] }
    let result = DataGrid.dataGrid config data
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

/// Charts Gallery — renders all chart types in a single scene.
///
/// Usage:
///   dotnet fsi scripts/examples/03-charts-gallery.fsx

#load "../charts-prelude.fsx"

open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

// --- Line Chart ---
let lineSeries =
    [ { Name = "Revenue"; Points = [ { X = 1.0; Y = 10.0 }; { X = 2.0; Y = 25.0 }; { X = 3.0; Y = 18.0 }; { X = 4.0; Y = 30.0 } ] }
      { Name = "Costs"; Points = [ { X = 1.0; Y = 8.0 }; { X = 2.0; Y = 12.0 }; { X = 3.0; Y = 15.0 }; { X = 4.0; Y = 20.0 } ] } ]
let lineChart = LineChart.lineChart { LineChart.defaultConfig 350f 250f with Title = Some "Line Chart" } lineSeries

// --- Bar Chart ---
let barData =
    [ { Category = "Q1"; Values = [ ("2025", 100.0); ("2026", 120.0) ] }
      { Category = "Q2"; Values = [ ("2025", 90.0); ("2026", 140.0) ] }
      { Category = "Q3"; Values = [ ("2025", 110.0); ("2026", 130.0) ] } ]
let barChart = BarChart.barChart { BarChart.defaultConfig 350f 250f with Title = Some "Bar Chart" } BarLayout.Grouped barData

// --- Pie Chart ---
let slices =
    [ { Label = "Desktop"; Value = 55.0 }
      { Label = "Mobile"; Value = 35.0 }
      { Label = "Tablet"; Value = 10.0 } ]
let pieChart = PieChart.pieChart { PieChart.defaultConfig 350f 250f with Title = Some "Pie Chart" } slices

// --- Scatter Plot ---
let scatterSeries =
    [ { Name = "Group A"; Points = [ { X = 1.0; Y = 2.0 }; { X = 3.0; Y = 5.0 }; { X = 5.0; Y = 4.0 }; { X = 7.0; Y = 8.0 } ] }
      { Name = "Group B"; Points = [ { X = 2.0; Y = 1.0 }; { X = 4.0; Y = 3.0 }; { X = 6.0; Y = 7.0 }; { X = 8.0; Y = 6.0 } ] } ]
let scatterPlot = ScatterPlot.scatterPlot { ScatterPlot.defaultConfig 350f 250f with Title = Some "Scatter Plot" } scatterSeries

// --- Area Chart ---
let areaSeries =
    [ { Name = "Downloads"; Points = [ { X = 1.0; Y = 5.0 }; { X = 2.0; Y = 12.0 }; { X = 3.0; Y = 8.0 }; { X = 4.0; Y = 15.0 } ] }
      { Name = "Signups"; Points = [ { X = 1.0; Y = 3.0 }; { X = 2.0; Y = 7.0 }; { X = 3.0; Y = 4.0 }; { X = 4.0; Y = 9.0 } ] } ]
let areaChart = AreaChart.areaChart { AreaChart.defaultConfig 350f 250f with Title = Some "Area Chart" } areaSeries

// --- Histogram ---
let histValues = [ 1.0; 2.3; 2.7; 3.1; 3.5; 4.0; 4.2; 5.5; 6.1; 7.0; 7.8; 8.0; 9.0 ]
let histChart = Histogram.histogram { Histogram.defaultConfig 350f 250f with Title = Some "Histogram"; BinCount = 5 } histValues

// --- Candlestick ---
let ohlcData =
    [ { Label = "Mon"; Open = 10.0; High = 15.0; Low = 8.0; Close = 13.0 }
      { Label = "Tue"; Open = 13.0; High = 16.0; Low = 11.0; Close = 12.0 }
      { Label = "Wed"; Open = 12.0; High = 18.0; Low = 10.0; Close = 17.0 }
      { Label = "Thu"; Open = 17.0; High = 20.0; Low = 14.0; Close = 15.0 } ]
let candleChart = Candlestick.candlestickChart { Candlestick.defaultConfig 350f 250f with Title = Some "Candlestick" } ohlcData

// --- Radar Chart ---
let radarSeries =
    [ { Name = "Team A"; Values = [ 80.0; 90.0; 70.0; 60.0; 85.0 ] }
      { Name = "Team B"; Values = [ 65.0; 75.0; 85.0; 80.0; 70.0 ] } ]
let radarChart = RadarChart.radarChart { RadarChart.defaultConfig 350f 250f [ "Speed"; "Power"; "Agility"; "Stamina"; "Technique" ] with Title = Some "Radar Chart" } radarSeries

// --- Compose into a gallery scene ---
let scene =
    Scene.create SKColors.White
        [ Scene.translate 10f 10f [ lineChart ]
          Scene.translate 380f 10f [ barChart ]
          Scene.translate 750f 10f [ pieChart ]
          Scene.translate 10f 280f [ scatterPlot ]
          Scene.translate 380f 280f [ areaChart ]
          Scene.translate 750f 280f [ histChart ]
          Scene.translate 10f 550f [ candleChart ]
          Scene.translate 380f 550f [ radarChart ] ]

printfn "Charts gallery scene created with 8 chart types."
printfn "To display: pass 'scene' to Viewer.run with your config."

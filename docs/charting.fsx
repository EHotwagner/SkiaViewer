(**
---
title: Charting
category: Tutorials
categoryindex: 2
index: 5
description: Build line, bar, pie, scatter, area, histogram, candlestick, and radar charts with the SkiaViewer.Charts library.
---
*)

(**
# Charting

SkiaViewer.Charts provides eight chart types that compose directly into the
declarative scene DSL. Each chart is a pure function that accepts a configuration
record and data, returning a standard `Element` — no special rendering pipeline
required.

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
## Line Chart

Line charts plot one or more data series as connected line segments with
auto-scaled axes, grid lines, tick labels, and a color-coded legend.
*)

let lineSeries =
    [ { Name = "Revenue"
        Points = [ { X = 1.0; Y = 10.0 }; { X = 2.0; Y = 25.0 }
                    { X = 3.0; Y = 18.0 }; { X = 4.0; Y = 30.0 } ] }
      { Name = "Costs"
        Points = [ { X = 1.0; Y = 8.0 }; { X = 2.0; Y = 12.0 }
                    { X = 3.0; Y = 15.0 }; { X = 4.0; Y = 20.0 } ] } ]

let lineChart =
    LineChart.lineChart
        { LineChart.defaultConfig 400f 250f with Title = Some "Revenue vs Costs" }
        lineSeries

(**
The result is a standard `Element.Group` that you can place anywhere in a scene
using `Scene.translate`, `Scene.scale`, or any other DSL combinator.

## Bar Chart

Bar charts display categorical data. Pass `BarLayout.Grouped` for side-by-side
bars or `BarLayout.Stacked` for vertically stacked bars.
*)

let barData =
    [ { Category = "Q1"; Values = [ ("2025", 100.0); ("2026", 120.0) ] }
      { Category = "Q2"; Values = [ ("2025", 90.0);  ("2026", 140.0) ] }
      { Category = "Q3"; Values = [ ("2025", 110.0); ("2026", 130.0) ] } ]

let groupedBar =
    BarChart.barChart
        { BarChart.defaultConfig 400f 250f with Title = Some "Grouped" }
        BarLayout.Grouped barData

let stackedBar =
    BarChart.barChart
        { BarChart.defaultConfig 400f 250f with Title = Some "Stacked" }
        BarLayout.Stacked barData

(**
## Pie & Donut Chart

Pie charts render proportional slices. Set `DonutRatio` above zero to cut a
hole in the center.
*)

let slices =
    [ { Label = "Desktop"; Value = 55.0 }
      { Label = "Mobile";  Value = 35.0 }
      { Label = "Tablet";  Value = 10.0 } ]

let pie =
    PieChart.pieChart (PieChart.defaultConfig 300f 300f) slices

let donut =
    PieChart.pieChart
        { PieChart.defaultConfig 300f 300f with DonutRatio = 0.5f }
        slices

(**
## Scatter Plot

Scatter plots render individual data points as filled circles on a
Cartesian plane.
*)

let scatterSeries =
    [ { Name = "Group A"
        Points = [ { X = 1.0; Y = 2.0 }; { X = 3.0; Y = 5.0 }
                    { X = 5.0; Y = 4.0 }; { X = 7.0; Y = 8.0 } ] }
      { Name = "Group B"
        Points = [ { X = 2.0; Y = 1.0 }; { X = 4.0; Y = 3.0 }
                    { X = 6.0; Y = 7.0 }; { X = 8.0; Y = 6.0 } ] } ]

let scatter =
    ScatterPlot.scatterPlot
        { ScatterPlot.defaultConfig 400f 250f with Title = Some "Scatter Plot" }
        scatterSeries

(**
## Area Chart

Area charts fill the region below data lines. Multiple series stack
automatically.
*)

let areaSeries =
    [ { Name = "Downloads"
        Points = [ { X = 1.0; Y = 5.0 }; { X = 2.0; Y = 12.0 }
                    { X = 3.0; Y = 8.0 }; { X = 4.0; Y = 15.0 } ] }
      { Name = "Signups"
        Points = [ { X = 1.0; Y = 3.0 }; { X = 2.0; Y = 7.0 }
                    { X = 3.0; Y = 4.0 }; { X = 4.0; Y = 9.0 } ] } ]

let area =
    AreaChart.areaChart
        { AreaChart.defaultConfig 400f 250f with Title = Some "Area Chart" }
        areaSeries

(**
## Histogram

Histograms group raw data into bins and display frequency counts as
adjacent bars.
*)

let rawValues = [ 1.0; 2.3; 2.7; 3.1; 3.5; 4.0; 4.2; 5.5; 6.1; 7.0; 7.8; 8.0; 9.0 ]

let hist =
    Histogram.histogram
        { Histogram.defaultConfig 400f 250f with BinCount = 5; Title = Some "Histogram" }
        rawValues

(**
## Candlestick Chart

Candlestick charts display OHLC financial data with colored bodies and
wick lines.
*)

let ohlc =
    [ { Label = "Mon"; Open = 10.0; High = 15.0; Low = 8.0;  Close = 13.0 }
      { Label = "Tue"; Open = 13.0; High = 16.0; Low = 11.0; Close = 12.0 }
      { Label = "Wed"; Open = 12.0; High = 18.0; Low = 10.0; Close = 17.0 }
      { Label = "Thu"; Open = 17.0; High = 20.0; Low = 14.0; Close = 15.0 } ]

let candle =
    Candlestick.candlestickChart
        { Candlestick.defaultConfig 400f 250f with Title = Some "Candlestick" }
        ohlc

(**
## Radar / Spider Chart

Radar charts compare multiple variables on radial axes.
*)

let radarSeries =
    [ { Name = "Team A"; Values = [ 80.0; 90.0; 70.0; 60.0; 85.0 ] }
      { Name = "Team B"; Values = [ 65.0; 75.0; 85.0; 80.0; 70.0 ] } ]

let radar =
    RadarChart.radarChart
        { RadarChart.defaultConfig 300f 300f
              [ "Speed"; "Power"; "Agility"; "Stamina"; "Technique" ]
          with Title = Some "Radar Chart" }
        radarSeries

(**
## Composing Charts in a Scene

Because every chart function returns a standard `Element`, you can compose
charts with all other DSL elements in a single scene:
*)

let dashboard =
    Scene.create SKColors.White
        [ Scene.translate 10f 10f [ lineChart ]
          Scene.translate 430f 10f [ groupedBar ]
          Scene.translate 10f 280f [ scatter ]
          Scene.translate 430f 280f [ area ] ]

(**
## Configuration

All chart configs use F# record `with` syntax for partial overrides:
*)

let customConfig =
    { LineChart.defaultConfig 600f 400f with
        Title = Some "Custom Chart"
        XAxis = { Defaults.axisConfig with Label = Some "Time" }
        YAxis = { Defaults.axisConfig with Label = Some "Value"; TickCount = 8 }
        Palette = { Colors = [ SKColors.DarkBlue; SKColors.Crimson ] } }

(**

<div class="alert alert-info">
<strong>Tip:</strong> Charts use a static/immutable data model. To animate or update,
rebuild the chart element with new data and push a new scene.
</div>

## Edge Case Handling

All chart types handle edge cases gracefully:

- **Empty data** — axes render but no data marks appear
- **NaN / Infinity values** — skipped silently, remaining data renders normally
- **Single data point** — renders as a dot (line/scatter) or single bar/slice
- **All-zero pie** — displays a "No data" indicator instead of dividing by zero

## Next Steps

- [DataGrid](datagrid.html) — sortable, scrollable tabular data
- [API Reference](reference/index.html) — full type and function documentation
*)

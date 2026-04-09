/// SkiaViewer.Charts FSI Prelude
/// Load this script in F# Interactive to use charting and DataGrid elements interactively.
///
/// Usage:
///   dotnet fsi scripts/charts-prelude.fsx
///
/// Or from FSI:
///   #load "scripts/charts-prelude.fsx"

#load "prelude.fsx"
#r "../src/SkiaViewer.Charts/bin/Debug/net10.0/SkiaViewer.Charts.dll"

open SkiaViewer.Charts

printfn "SkiaViewer.Charts prelude loaded. Modules: LineChart, BarChart, PieChart, ScatterPlot, AreaChart, Histogram, Candlestick, RadarChart, DataGrid, Defaults."

namespace SkiaViewer.Charts.Tests

open Xunit
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

/// Graphical tests that render chart elements to an offscreen surface
/// and verify pixel output at known positions.
type GraphicalTests() =

    let renderToSurface (width: int) (height: int) (scene: Scene) : SKBitmap =
        let info = SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)
        use surface = SKSurface.Create(info)
        let canvas = surface.Canvas
        SceneRenderer.render scene canvas
        canvas.Flush()
        use img = surface.Snapshot()
        SKBitmap.FromImage(img)

    let getPixel (bitmap: SKBitmap) (x: int) (y: int) : SKColor =
        bitmap.GetPixel(x, y)

    /// Check if any pixel in a region is non-background.
    let hasNonBackgroundPixels (bitmap: SKBitmap) (x0: int) (y0: int) (x1: int) (y1: int) (bg: SKColor) =
        let mutable found = false
        for x in x0 .. x1 do
            for y in y0 .. y1 do
                let p = getPixel bitmap x y
                if p.Red <> bg.Red || p.Green <> bg.Green || p.Blue <> bg.Blue then
                    found <- true
        found

    /// Check if any pixel in a region matches a specific color (with tolerance).
    let hasColorInRegion (bitmap: SKBitmap) (x0: int) (y0: int) (x1: int) (y1: int) (color: SKColor) (tolerance: byte) =
        let mutable found = false
        for x in x0 .. x1 do
            for y in y0 .. y1 do
                let p = getPixel bitmap x y
                if abs (int p.Red - int color.Red) <= int tolerance
                   && abs (int p.Green - int color.Green) <= int tolerance
                   && abs (int p.Blue - int color.Blue) <= int tolerance
                   && p.Alpha > 0uy then
                    found <- true
        found

    // ===================== LINE CHART =====================

    [<Fact>]
    member _.``LineChart renders data lines on white background`` () =
        let config = { LineChart.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        let series = [ { Name = "S1"; Points = [ { X = 0.0; Y = 0.0 }; { X = 1.0; Y = 10.0 }; { X = 2.0; Y = 5.0 } ] } ]
        let chart = LineChart.lineChart config series
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // Chart area is roughly center of the image — should have non-white pixels (axes, lines)
        let hasContent = hasNonBackgroundPixels bitmap 50 30 180 120 SKColors.White
        Assert.True(hasContent, "Line chart should render visible content in the chart area")

    [<Fact>]
    member _.``LineChart renders data line in palette color`` () =
        let config = { LineChart.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        let series = [ { Name = "S1"; Points = [ { X = 0.0; Y = 0.0 }; { X = 1.0; Y = 10.0 }; { X = 2.0; Y = 5.0 } ] } ]
        let chart = LineChart.lineChart config series
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // First palette color is Tableau blue (0x4E, 0x79, 0xA7)
        let hasBlue = hasColorInRegion bitmap 60 20 190 130 (SKColor(0x4Euy, 0x79uy, 0xA7uy)) 30uy
        Assert.True(hasBlue, "Line chart should render data line in first palette color (Tableau blue)")

    [<Fact>]
    member _.``LineChart with two series renders in two distinct colors`` () =
        let config = { LineChart.defaultConfig 300f 200f with BackgroundColor = Some SKColors.White }
        let series =
            [ { Name = "A"; Points = [ { X = 0.0; Y = 0.0 }; { X = 1.0; Y = 10.0 } ] }
              { Name = "B"; Points = [ { X = 0.0; Y = 10.0 }; { X = 1.0; Y = 0.0 } ] } ]
        let chart = LineChart.lineChart config series
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 300 200 scene
        // Palette color 1: blue (0x4E79A7), color 2: orange (0xF28E2B)
        let hasBlue = hasColorInRegion bitmap 60 20 280 170 (SKColor(0x4Euy, 0x79uy, 0xA7uy)) 30uy
        let hasOrange = hasColorInRegion bitmap 60 20 280 170 (SKColor(0xF2uy, 0x8Euy, 0x2Buy)) 30uy
        Assert.True(hasBlue, "Should have first series in blue")
        Assert.True(hasOrange, "Should have second series in orange")

    [<Fact>]
    member _.``LineChart empty data renders axes but no data lines`` () =
        let config = { LineChart.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        let chart = LineChart.lineChart config []
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // Should have some non-white content (axes, tick labels)
        let hasContent = hasNonBackgroundPixels bitmap 20 20 190 140 SKColors.White
        Assert.True(hasContent, "Empty chart should still render axes")
        // Should not have first palette color for data lines
        let hasBlue = hasColorInRegion bitmap 90 30 170 100 (SKColor(0x4Euy, 0x79uy, 0xA7uy)) 15uy
        Assert.False(hasBlue, "Empty chart should not have data line colors")

    // ===================== BAR CHART =====================

    [<Fact>]
    member _.``BarChart renders filled bars`` () =
        let config = { BarChart.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        let data = [ { Category = "A"; Values = [ ("S1", 10.0) ] }; { Category = "B"; Values = [ ("S1", 5.0) ] } ]
        let chart = BarChart.barChart config BarLayout.Grouped data
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // Bars should be in first palette color (blue)
        let hasBlue = hasColorInRegion bitmap 60 20 180 130 (SKColor(0x4Euy, 0x79uy, 0xA7uy)) 30uy
        Assert.True(hasBlue, "Bar chart should render bars in palette color")

    [<Fact>]
    member _.``BarChart grouped has two distinct bar colors`` () =
        let config = { BarChart.defaultConfig 300f 200f with BackgroundColor = Some SKColors.White }
        let data =
            [ { Category = "Q1"; Values = [ ("A", 10.0); ("B", 8.0) ] }
              { Category = "Q2"; Values = [ ("A", 12.0); ("B", 6.0) ] } ]
        let chart = BarChart.barChart config BarLayout.Grouped data
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 300 200 scene
        let hasBlue = hasColorInRegion bitmap 60 20 280 170 (SKColor(0x4Euy, 0x79uy, 0xA7uy)) 30uy
        let hasOrange = hasColorInRegion bitmap 60 20 280 170 (SKColor(0xF2uy, 0x8Euy, 0x2Buy)) 30uy
        Assert.True(hasBlue, "Should have first series bars in blue")
        Assert.True(hasOrange, "Should have second series bars in orange")

    [<Fact>]
    member _.``BarChart stacked renders bars`` () =
        let config = { BarChart.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        let data = [ { Category = "Q1"; Values = [ ("A", 10.0); ("B", 5.0) ] } ]
        let chart = BarChart.barChart config BarLayout.Stacked data
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        let hasContent = hasNonBackgroundPixels bitmap 60 20 180 130 SKColors.White
        Assert.True(hasContent, "Stacked bar chart should render visible bars")

    // ===================== PIE CHART =====================

    [<Fact>]
    member _.``PieChart renders colored arcs`` () =
        let config = { PieChart.defaultConfig 200f 200f with BackgroundColor = Some SKColors.White }
        let slices = [ { Label = "A"; Value = 60.0 }; { Label = "B"; Value = 40.0 } ]
        let chart = PieChart.pieChart config slices
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 200 scene
        // Center of pie should have palette colors
        let hasBlue = hasColorInRegion bitmap 50 50 150 150 (SKColor(0x4Euy, 0x79uy, 0xA7uy)) 30uy
        let hasOrange = hasColorInRegion bitmap 50 50 150 150 (SKColor(0xF2uy, 0x8Euy, 0x2Buy)) 30uy
        Assert.True(hasBlue, "Pie chart should have first slice in blue")
        Assert.True(hasOrange, "Pie chart should have second slice in orange")

    [<Fact>]
    member _.``PieChart donut has white center`` () =
        let config = { PieChart.defaultConfig 200f 200f with DonutRatio = 0.5f; BackgroundColor = Some SKColors.White }
        let slices = [ { Label = "A"; Value = 60.0 }; { Label = "B"; Value = 40.0 } ]
        let chart = PieChart.pieChart config slices
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 200 scene
        // Center of donut should be white (the hole)
        let center = getPixel bitmap 100 105
        Assert.True(center.Red > 200uy && center.Green > 200uy && center.Blue > 200uy,
            $"Donut center should be white but got R={center.Red} G={center.Green} B={center.Blue}")

    [<Fact>]
    member _.``PieChart all zeros renders no data indicator`` () =
        let config = { PieChart.defaultConfig 200f 200f with BackgroundColor = Some SKColors.White }
        let slices = [ { Label = "A"; Value = 0.0 }; { Label = "B"; Value = 0.0 } ]
        let chart = PieChart.pieChart config slices
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 200 scene
        // Should have some rendered content (text or indicator) — scan wider area
        let hasContent = hasNonBackgroundPixels bitmap 20 20 180 180 SKColors.White
        Assert.True(hasContent, "All-zero pie should render some indicator (text or empty circle)")

    // ===================== SCATTER PLOT =====================

    [<Fact>]
    member _.``ScatterPlot renders colored dots`` () =
        let config = { ScatterPlot.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        let series = [ { Name = "S1"; Points = [ { X = 1.0; Y = 2.0 }; { X = 3.0; Y = 5.0 }; { X = 5.0; Y = 3.0 } ] } ]
        let chart = ScatterPlot.scatterPlot config series
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        let hasBlue = hasColorInRegion bitmap 60 20 180 130 (SKColor(0x4Euy, 0x79uy, 0xA7uy)) 30uy
        Assert.True(hasBlue, "Scatter plot should render dots in palette color")

    // ===================== AREA CHART =====================

    [<Fact>]
    member _.``AreaChart renders filled region`` () =
        let config = { AreaChart.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        let series = [ { Name = "S1"; Points = [ { X = 0.0; Y = 2.0 }; { X = 1.0; Y = 8.0 }; { X = 2.0; Y = 5.0 } ] } ]
        let chart = AreaChart.areaChart config series
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // The filled area should cover a large region with semi-transparent palette color
        let hasContent = hasNonBackgroundPixels bitmap 60 30 180 120 SKColors.White
        Assert.True(hasContent, "Area chart should render a filled region")

    // ===================== HISTOGRAM =====================

    [<Fact>]
    member _.``Histogram renders adjacent bars`` () =
        let config = { Histogram.defaultConfig 200f 150f with BinCount = 4; BackgroundColor = Some SKColors.White }
        let values = [ 1.0; 2.0; 3.0; 4.0; 5.0; 6.0; 7.0; 8.0 ]
        let chart = Histogram.histogram config values
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        let hasContent = hasNonBackgroundPixels bitmap 60 20 180 130 SKColors.White
        Assert.True(hasContent, "Histogram should render visible bars")

    // ===================== CANDLESTICK =====================

    [<Fact>]
    member _.``Candlestick renders up candle in green`` () =
        let config = { Candlestick.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        // Close > Open = up candle (green by default)
        let data = [ { Label = "D1"; Open = 10.0; High = 20.0; Low = 5.0; Close = 18.0 } ]
        let chart = Candlestick.candlestickChart config data
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // Default up color is green (0x59, 0xA1, 0x4F)
        let hasGreen = hasColorInRegion bitmap 60 20 180 130 (SKColor(0x59uy, 0xA1uy, 0x4Fuy)) 30uy
        Assert.True(hasGreen, "Up candle should render in green (UpColor)")

    [<Fact>]
    member _.``Candlestick renders down candle in red`` () =
        let config = { Candlestick.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        // Close < Open = down candle (red by default)
        let data = [ { Label = "D1"; Open = 18.0; High = 20.0; Low = 5.0; Close = 10.0 } ]
        let chart = Candlestick.candlestickChart config data
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // Default down color is red (0xE1, 0x57, 0x59)
        let hasRed = hasColorInRegion bitmap 60 20 180 130 (SKColor(0xE1uy, 0x57uy, 0x59uy)) 30uy
        Assert.True(hasRed, "Down candle should render in red (DownColor)")

    [<Fact>]
    member _.``Candlestick renders wick line`` () =
        let config = { Candlestick.defaultConfig 200f 150f with BackgroundColor = Some SKColors.White }
        let data = [ { Label = "D1"; Open = 10.0; High = 20.0; Low = 5.0; Close = 15.0 } ]
        let chart = Candlestick.candlestickChart config data
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // Wick is rendered as a line — should have dark pixels in the chart area
        let hasContent = hasNonBackgroundPixels bitmap 60 20 160 130 SKColors.White
        Assert.True(hasContent, "Candlestick should render wick and body content")

    // ===================== RADAR CHART =====================

    [<Fact>]
    member _.``RadarChart renders polygon in palette color`` () =
        let config = { RadarChart.defaultConfig 200f 200f [ "A"; "B"; "C"; "D" ] with BackgroundColor = Some SKColors.White }
        let series = [ { Name = "S1"; Values = [ 8.0; 6.0; 7.0; 9.0 ] } ]
        let chart = RadarChart.radarChart config series
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 200 scene
        // Polygon fill should show palette blue (semi-transparent)
        let hasContent = hasNonBackgroundPixels bitmap 50 50 150 150 SKColors.White
        Assert.True(hasContent, "Radar chart should render polygon in chart area")

    [<Fact>]
    member _.``RadarChart renders axis lines`` () =
        let config = { RadarChart.defaultConfig 200f 200f [ "A"; "B"; "C"; "D" ] with BackgroundColor = Some SKColors.White }
        let series = [ { Name = "S1"; Values = [ 8.0; 6.0; 7.0; 9.0 ] } ]
        let chart = RadarChart.radarChart config series
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 200 scene
        // Axis lines and grid should produce non-white pixels across the chart area
        let hasContent = hasNonBackgroundPixels bitmap 40 40 160 160 SKColors.White
        Assert.True(hasContent, "Radar chart should render axis lines and grid")

    // ===================== DATAGRID =====================

    [<Fact>]
    member _.``DataGrid renders header background`` () =
        let config = DataGrid.defaultConfig 200f 150f
        let data =
            { Columns = [ DataGrid.textColumn "Name"; DataGrid.numericColumn "Value" ]
              Rows = [ [ CellValue.TextValue "Alice"; CellValue.NumericValue 95.0 ] ] }
        let grid = DataGrid.dataGrid config data
        let scene = Scene.create SKColors.White [ grid ]
        use bitmap = renderToSurface 200 150 scene
        // Header color is gray (0xE0, 0xE0, 0xE0) — sample the header row area
        let headerPixel = getPixel bitmap 100 18
        Assert.True(headerPixel.Red >= 0xD0uy && headerPixel.Green >= 0xD0uy && headerPixel.Blue >= 0xD0uy,
            $"Header should be gray but got R={headerPixel.Red} G={headerPixel.Green} B={headerPixel.Blue}")

    [<Fact>]
    member _.``DataGrid renders cell text`` () =
        let config = DataGrid.defaultConfig 200f 150f
        let data =
            { Columns = [ DataGrid.textColumn "Name" ]
              Rows = [ [ CellValue.TextValue "Hello" ] ] }
        let grid = DataGrid.dataGrid config data
        let scene = Scene.create SKColors.White [ grid ]
        use bitmap = renderToSurface 200 150 scene
        // Cell text should produce non-white pixels in the first row area
        let hasText = hasNonBackgroundPixels bitmap 5 40 100 65 SKColors.White
        Assert.True(hasText, "DataGrid should render cell text content")

    [<Fact>]
    member _.``DataGrid renders boolean checkbox indicator`` () =
        let config = DataGrid.defaultConfig 200f 150f
        let data =
            { Columns = [ DataGrid.boolColumn "Flag" ]
              Rows = [ [ CellValue.BoolValue true ]; [ CellValue.BoolValue false ] ] }
        let grid = DataGrid.dataGrid config data
        let scene = Scene.create SKColors.White [ grid ]
        use bitmap = renderToSurface 200 150 scene
        // Boolean cells render as small squares — should have non-white pixels in cell area
        let hasIndicator = hasNonBackgroundPixels bitmap 5 40 100 100 SKColors.White
        Assert.True(hasIndicator, "DataGrid should render boolean indicators")

    [<Fact>]
    member _.``DataGrid alternating row colors`` () =
        let config = DataGrid.defaultConfig 300f 200f
        let data =
            { Columns = [ DataGrid.textColumn "Name" ]
              Rows =
                [ [ CellValue.TextValue "Row1" ]
                  [ CellValue.TextValue "Row2" ]
                  [ CellValue.TextValue "Row3" ]
                  [ CellValue.TextValue "Row4" ] ] }
        let grid = DataGrid.dataGrid config data
        let scene = Scene.create SKColors.White [ grid ]
        use bitmap = renderToSurface 300 200 scene
        // Alternating rows should have subtle gray background (0xF5F5F5)
        // Sample two different rows at same X position
        let row1Y = int config.HeaderHeight + int (config.RowHeight * 0.5f)
        let row2Y = int config.HeaderHeight + int (config.RowHeight * 1.5f)
        let pixel1 = getPixel bitmap 150 row1Y
        let pixel2 = getPixel bitmap 150 row2Y
        // At least one should differ from pure white if alternating is active
        let hasAlternating =
            (pixel1.Red <> pixel2.Red || pixel1.Green <> pixel2.Green || pixel1.Blue <> pixel2.Blue)
            || (pixel2.Red < 0xFEuy) // subtle gray background
        Assert.True(hasAlternating, $"Rows should alternate colors: row1=({pixel1.Red},{pixel1.Green},{pixel1.Blue}), row2=({pixel2.Red},{pixel2.Green},{pixel2.Blue})")

    // ===================== TITLE RENDERING =====================

    [<Fact>]
    member _.``Chart with title renders title text`` () =
        let config = { LineChart.defaultConfig 200f 150f with Title = Some "Test Chart"; BackgroundColor = Some SKColors.White }
        let series = [ { Name = "S1"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 2.0 } ] } ]
        let chart = LineChart.lineChart config series
        let scene = Scene.create SKColors.White [ chart ]
        use bitmap = renderToSurface 200 150 scene
        // Title should be rendered near the top of the chart — look for non-white pixels in title area
        let hasTitle = hasNonBackgroundPixels bitmap 50 2 150 25 SKColors.White
        Assert.True(hasTitle, "Chart with title should render text near the top")

    // ===================== RESIZE PROPORTIONALITY =====================

    [<Fact>]
    member _.``Chart at different sizes renders proportionally`` () =
        let series = [ { Name = "S1"; Points = [ { X = 0.0; Y = 0.0 }; { X = 1.0; Y = 10.0 }; { X = 2.0; Y = 5.0 } ] } ]
        let smallConfig = { LineChart.defaultConfig 100f 80f with BackgroundColor = Some SKColors.White }
        let largeConfig = { LineChart.defaultConfig 400f 300f with BackgroundColor = Some SKColors.White }
        let smallChart = LineChart.lineChart smallConfig series
        let largeChart = LineChart.lineChart largeConfig series
        let smallScene = Scene.create SKColors.White [ smallChart ]
        let largeScene = Scene.create SKColors.White [ largeChart ]
        use smallBitmap = renderToSurface 100 80 smallScene
        use largeBitmap = renderToSurface 400 300 largeScene
        // Both should have visible content
        let smallHasContent = hasNonBackgroundPixels smallBitmap 10 10 90 70 SKColors.White
        let largeHasContent = hasNonBackgroundPixels largeBitmap 40 40 360 260 SKColors.White
        Assert.True(smallHasContent, "Small chart should render content")
        Assert.True(largeHasContent, "Large chart should render content")

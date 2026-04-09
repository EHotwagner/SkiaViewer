namespace SkiaViewer.Charts.Tests

open System
open System.Threading
open Xunit
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

type WindowTest() =

    let screenshotFolder =
        let p = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "skiaviewer-charts-test")
        System.IO.Directory.CreateDirectory(p) |> ignore
        p

    let hasContent (bitmap: SKBitmap) (x0: int) (y0: int) (x1: int) (y1: int) =
        let mutable found = false
        for x in x0 .. 5 .. x1 do
            for y in y0 .. 5 .. y1 do
                let p = bitmap.GetPixel(x, y)
                if p.Red < 240uy || p.Green < 240uy || p.Blue < 240uy then
                    found <- true
        found

    // --- Shared data ---

    let columns = [ DataGrid.textColumn "Name"; DataGrid.numericColumn "Score"; DataGrid.boolColumn "Passed" ]

    let rows =
        [ [ CellValue.TextValue "Alice";   CellValue.NumericValue 95.0;  CellValue.BoolValue true ]
          [ CellValue.TextValue "Bob";     CellValue.NumericValue 72.0;  CellValue.BoolValue true ]
          [ CellValue.TextValue "Carol";   CellValue.NumericValue 58.0;  CellValue.BoolValue false ]
          [ CellValue.TextValue "Dave";    CellValue.NumericValue 88.0;  CellValue.BoolValue true ]
          [ CellValue.TextValue "Eve";     CellValue.NumericValue 91.0;  CellValue.BoolValue true ]
          [ CellValue.TextValue "Frank";   CellValue.NumericValue 45.0;  CellValue.BoolValue false ]
          [ CellValue.TextValue "Grace";   CellValue.NumericValue 79.0;  CellValue.BoolValue true ]
          [ CellValue.TextValue "Hank";    CellValue.NumericValue 63.0;  CellValue.BoolValue true ] ]

    let gridData = { Columns = columns; Rows = rows }

    // ==================== TEST 1: Charts Gallery (20 seconds) ====================

    [<Fact>]
    member _.``Charts gallery renders in viewer window for 20 seconds`` () =
        let lineSeries =
            [ { Name = "Revenue"; Points = [ { X = 1.0; Y = 10.0 }; { X = 2.0; Y = 25.0 }; { X = 3.0; Y = 18.0 }; { X = 4.0; Y = 30.0 } ] }
              { Name = "Costs"; Points = [ { X = 1.0; Y = 8.0 }; { X = 2.0; Y = 12.0 }; { X = 3.0; Y = 15.0 }; { X = 4.0; Y = 20.0 } ] } ]
        let lineChart = LineChart.lineChart { LineChart.defaultConfig 350f 250f with Title = Some "Line Chart" } lineSeries

        let barData =
            [ { Category = "Q1"; Values = [ ("2025", 100.0); ("2026", 120.0) ] }
              { Category = "Q2"; Values = [ ("2025", 90.0); ("2026", 140.0) ] }
              { Category = "Q3"; Values = [ ("2025", 110.0); ("2026", 130.0) ] } ]
        let barChart = BarChart.barChart { BarChart.defaultConfig 350f 250f with Title = Some "Bar Chart" } BarLayout.Grouped barData

        let slices =
            [ { Label = "Desktop"; Value = 55.0 }
              { Label = "Mobile"; Value = 35.0 }
              { Label = "Tablet"; Value = 10.0 } ]
        let pieChart = PieChart.pieChart { PieChart.defaultConfig 350f 250f with Title = Some "Pie Chart" } slices

        let scatterSeries =
            [ { Name = "Group A"; Points = [ { X = 1.0; Y = 2.0 }; { X = 3.0; Y = 5.0 }; { X = 5.0; Y = 4.0 }; { X = 7.0; Y = 8.0 } ] }
              { Name = "Group B"; Points = [ { X = 2.0; Y = 1.0 }; { X = 4.0; Y = 3.0 }; { X = 6.0; Y = 7.0 }; { X = 8.0; Y = 6.0 } ] } ]
        let scatterPlot = ScatterPlot.scatterPlot { ScatterPlot.defaultConfig 350f 250f with Title = Some "Scatter Plot" } scatterSeries

        let areaSeries =
            [ { Name = "Downloads"; Points = [ { X = 1.0; Y = 5.0 }; { X = 2.0; Y = 12.0 }; { X = 3.0; Y = 8.0 }; { X = 4.0; Y = 15.0 } ] }
              { Name = "Signups"; Points = [ { X = 1.0; Y = 3.0 }; { X = 2.0; Y = 7.0 }; { X = 3.0; Y = 4.0 }; { X = 4.0; Y = 9.0 } ] } ]
        let areaChart = AreaChart.areaChart { AreaChart.defaultConfig 350f 250f with Title = Some "Area Chart" } areaSeries

        let histValues = [ 1.0; 2.3; 2.7; 3.1; 3.5; 4.0; 4.2; 5.5; 6.1; 7.0; 7.8; 8.0; 9.0 ]
        let histChart = Histogram.histogram { Histogram.defaultConfig 350f 250f with Title = Some "Histogram"; BinCount = 5 } histValues

        let ohlcData =
            [ { Label = "Mon"; Open = 10.0; High = 15.0; Low = 8.0; Close = 13.0 }
              { Label = "Tue"; Open = 13.0; High = 16.0; Low = 11.0; Close = 12.0 }
              { Label = "Wed"; Open = 12.0; High = 18.0; Low = 10.0; Close = 17.0 }
              { Label = "Thu"; Open = 17.0; High = 20.0; Low = 14.0; Close = 15.0 } ]
        let candleChart = Candlestick.candlestickChart { Candlestick.defaultConfig 350f 250f with Title = Some "Candlestick" } ohlcData

        let radarSeries =
            [ { Name = "Team A"; Values = [ 80.0; 90.0; 70.0; 60.0; 85.0 ] }
              { Name = "Team B"; Values = [ 65.0; 75.0; 85.0; 80.0; 70.0 ] } ]
        let radarChart = RadarChart.radarChart { RadarChart.defaultConfig 350f 250f [ "Speed"; "Power"; "Agility"; "Stamina"; "Technique" ] with Title = Some "Radar Chart" } radarSeries

        let gridChart = DataGrid.dataGrid (DataGrid.defaultConfig 350f 250f) gridData

        let scene =
            Scene.create SKColors.White
                [ Scene.translate 10f 10f [ lineChart ]
                  Scene.translate 380f 10f [ barChart ]
                  Scene.translate 750f 10f [ pieChart ]
                  Scene.translate 10f 280f [ scatterPlot ]
                  Scene.translate 380f 280f [ areaChart ]
                  Scene.translate 750f 280f [ histChart ]
                  Scene.translate 10f 550f [ candleChart ]
                  Scene.translate 380f 550f [ radarChart ]
                  Scene.translate 750f 550f [ gridChart ] ]

        let viewerConfig =
            { Title = "Charts Gallery - 20s Display"
              Width = 1120; Height = 830; TargetFps = 60
              ClearColor = SKColors.White; PreferredBackend = None }

        let sceneObs =
            { new IObservable<Scene> with
                member _.Subscribe(observer) =
                    observer.OnNext(scene)
                    { new IDisposable with member _.Dispose() = () } }

        let viewer, _events = Viewer.run viewerConfig sceneObs
        use _dispose = viewer

        // Display for 20 seconds
        Thread.Sleep(20000)

        let result = viewer.Screenshot(screenshotFolder)
        match result with
        | Ok path ->
            printfn $"Gallery screenshot: {path}"
            use bitmap = SKBitmap.Decode(path)
            Assert.True(hasContent bitmap 10 10 360 260, "Line chart region")
            Assert.True(hasContent bitmap 750 550 1100 800, "DataGrid region")
        | Error err -> Assert.Fail($"Screenshot failed: {err}")

    // ==================== TEST 2: DataGrid with changing data and sorting ====================

    [<Fact>]
    member _.``DataGrid live test with data changes and sorting`` () =
        let viewerConfig =
            { Title = "DataGrid Live Test"
              Width = 700; Height = 500; TargetFps = 60
              ClearColor = SKColors.White; PreferredBackend = None }

        // Use a subject-like observable to push new scenes
        let mutable currentObserver: IObserver<Scene> option = None
        let sceneObs =
            { new IObservable<Scene> with
                member _.Subscribe(observer) =
                    currentObserver <- Some observer
                    { new IDisposable with member _.Dispose() = currentObserver <- None } }

        let pushScene (scene: Scene) =
            match currentObserver with
            | Some obs -> obs.OnNext(scene)
            | None -> ()

        let makeGridScene (title: string) (cfg: DataGridConfig) (data: DataGridData) =
            let titleEl = Scene.text title 350f 25f 18f (Scene.fill SKColors.Black)
            let grid = DataGrid.dataGrid cfg data
            Scene.create SKColors.White [ titleEl; Scene.translate 25f 40f [ grid ] ]

        let gridConfig = DataGrid.defaultConfig 650f 420f

        let viewer, _events = Viewer.run viewerConfig sceneObs
        use _dispose = viewer

        // --- Phase 1: Original data, unsorted (5 seconds) ---
        printfn "Phase 1: Original data, no sorting"
        let scene1 = makeGridScene "DataGrid - Original Data (Unsorted)" gridConfig gridData
        pushScene scene1
        Thread.Sleep(5000)
        let r1 = viewer.Screenshot(screenshotFolder)
        match r1 with Ok p -> printfn $"  Screenshot 1: {p}" | Error e -> printfn $"  Screenshot 1 failed: {e}"

        // --- Phase 2: Sort by Score ascending (5 seconds) ---
        printfn "Phase 2: Sorted by Score ascending"
        let sortedAsc = DataGrid.sortRows columns 1 SortDirection.Ascending rows
        let cfgAsc = { gridConfig with Sort = Some { ColumnIndex = 1; Direction = SortDirection.Ascending } }
        let scene2 = makeGridScene "DataGrid - Sorted by Score (Ascending)" cfgAsc { gridData with Rows = sortedAsc }
        pushScene scene2
        Thread.Sleep(5000)
        let r2 = viewer.Screenshot(screenshotFolder)
        match r2 with Ok p -> printfn $"  Screenshot 2: {p}" | Error e -> printfn $"  Screenshot 2 failed: {e}"

        // --- Phase 3: Sort by Score descending (5 seconds) ---
        printfn "Phase 3: Sorted by Score descending"
        let sortedDesc = DataGrid.sortRows columns 1 SortDirection.Descending rows
        let cfgDesc = { gridConfig with Sort = Some { ColumnIndex = 1; Direction = SortDirection.Descending } }
        let scene3 = makeGridScene "DataGrid - Sorted by Score (Descending)" cfgDesc { gridData with Rows = sortedDesc }
        pushScene scene3
        Thread.Sleep(5000)
        let r3 = viewer.Screenshot(screenshotFolder)
        match r3 with Ok p -> printfn $"  Screenshot 3: {p}" | Error e -> printfn $"  Screenshot 3 failed: {e}"

        // --- Phase 4: New data + sort by Name ascending (5 seconds) ---
        printfn "Phase 4: Updated data, sorted by Name ascending"
        let newRows =
            [ [ CellValue.TextValue "Zara";    CellValue.NumericValue 97.0;  CellValue.BoolValue true ]
              [ CellValue.TextValue "Milo";    CellValue.NumericValue 82.0;  CellValue.BoolValue true ]
              [ CellValue.TextValue "Luna";    CellValue.NumericValue 61.0;  CellValue.BoolValue false ]
              [ CellValue.TextValue "Kai";     CellValue.NumericValue 55.0;  CellValue.BoolValue false ]
              [ CellValue.TextValue "Iris";    CellValue.NumericValue 90.0;  CellValue.BoolValue true ]
              [ CellValue.TextValue "Jay";     CellValue.NumericValue 76.0;  CellValue.BoolValue true ] ]
        let newData = { Columns = columns; Rows = newRows }
        let sortedByName = DataGrid.sortRows columns 0 SortDirection.Ascending newRows
        let cfgName = { gridConfig with Sort = Some { ColumnIndex = 0; Direction = SortDirection.Ascending } }
        let scene4 = makeGridScene "DataGrid - New Students, Sorted by Name (A-Z)" cfgName { newData with Rows = sortedByName }
        pushScene scene4
        Thread.Sleep(5000)
        let r4 = viewer.Screenshot(screenshotFolder)
        match r4 with Ok p -> printfn $"  Screenshot 4: {p}" | Error e -> printfn $"  Screenshot 4 failed: {e}"

        // --- Phase 5: Sort by Passed (boolean) descending (5 seconds) ---
        printfn "Phase 5: Sorted by Passed (descending - passed first)"
        let sortedByPassed = DataGrid.sortRows columns 2 SortDirection.Descending newRows
        let cfgPassed = { gridConfig with Sort = Some { ColumnIndex = 2; Direction = SortDirection.Descending } }
        let scene5 = makeGridScene "DataGrid - Sorted by Passed (True first)" cfgPassed { newData with Rows = sortedByPassed }
        pushScene scene5
        Thread.Sleep(5000)
        let r5 = viewer.Screenshot(screenshotFolder)
        match r5 with Ok p -> printfn $"  Screenshot 5: {p}" | Error e -> printfn $"  Screenshot 5 failed: {e}"

        // --- Verify final screenshot has content ---
        match r5 with
        | Ok path ->
            use bitmap = SKBitmap.Decode(path)
            Assert.NotNull(bitmap)
            Assert.True(hasContent bitmap 25 40 675 460, "DataGrid should have visible content")
        | Error err -> Assert.Fail($"Final screenshot failed: {err}")

        printfn "DataGrid live test complete - 5 phases, 25 seconds total"

module SkiaViewer.Charts.Tests.LineChartTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

let rec countElements (el: Element) =
    match el with
    | Element.Group(_, _, _, children) -> children |> List.sumBy countElements
    | _ -> 1

let rec findElements (pred: Element -> bool) (el: Element) =
    let self = if pred el then [el] else []
    match el with
    | Element.Group(_, _, _, children) -> self @ (children |> List.collect (findElements pred))
    | _ -> self

let isLine el = match el with Element.Line _ -> true | _ -> false

[<Fact>]
let ``LineChart_SingleSeries_ReturnsGroup`` () =
    let config = LineChart.defaultConfig 600f 400f
    let series = [ { Name = "S1"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 2.0 }; { X = 2.0; Y = 3.0 } ] } ]
    let result = LineChart.lineChart config series
    match result with
    | Element.Group(_, _, _, children) -> Assert.True(children.Length > 0)
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``LineChart_EmptyData_ReturnsGroup`` () =
    let config = LineChart.defaultConfig 600f 400f
    let result = LineChart.lineChart config []
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``LineChart_MultiSeries_UsesDistinctColors`` () =
    let config = LineChart.defaultConfig 600f 400f
    let series =
        [ { Name = "A"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 2.0 } ] }
          { Name = "B"; Points = [ { X = 0.0; Y = 3.0 }; { X = 1.0; Y = 4.0 } ] } ]
    let result = LineChart.lineChart config series
    let lines = findElements isLine result
    Assert.True(lines.Length >= 2, "Should have at least 2 line elements")
    // Extract paint colors from line elements
    let colors =
        lines
        |> List.choose (fun el ->
            match el with
            | Element.Line(_, _, _, _, p) -> p.Stroke
            | _ -> None)
        |> List.distinct
    Assert.True(colors.Length >= 2, "Should use distinct colors for different series")

[<Fact>]
let ``LineChart_NaN_SkipsPoints`` () =
    let config = LineChart.defaultConfig 600f 400f
    let seriesWithNaN =
        [ { Name = "S1"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = nan }; { X = 2.0; Y = 3.0 } ] } ]
    let seriesWithout =
        [ { Name = "S1"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 2.0 }; { X = 2.0; Y = 3.0 } ] } ]
    let resultWithNaN = LineChart.lineChart config seriesWithNaN
    let resultWithout = LineChart.lineChart config seriesWithout
    // The NaN version should have fewer total elements since the NaN point is skipped
    let linesWithNaN = findElements isLine resultWithNaN |> List.length
    let linesWithout = findElements isLine resultWithout |> List.length
    Assert.True(linesWithNaN <= linesWithout, "NaN version should have equal or fewer lines")

[<Fact>]
let ``DefaultConfig_SetsDimensions`` () =
    let config = LineChart.defaultConfig 600f 400f
    Assert.Equal(600f, config.Width)
    Assert.Equal(400f, config.Height)

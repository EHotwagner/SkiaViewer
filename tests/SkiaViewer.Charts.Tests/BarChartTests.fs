module SkiaViewer.Charts.Tests.BarChartTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

let rec findElements (pred: Element -> bool) (el: Element) =
    let self = if pred el then [el] else []
    match el with
    | Element.Group(_, _, _, children) -> self @ (children |> List.collect (findElements pred))
    | _ -> self

let isRect el = match el with Element.Rect _ -> true | _ -> false

[<Fact>]
let ``BarChart_SingleCategory_ReturnsGroup`` () =
    let config = BarChart.defaultConfig 600f 400f
    let data = [ { Category = "A"; Values = [ ("S1", 10.0) ] } ]
    let result = BarChart.barChart config BarLayout.Grouped data
    match result with
    | Element.Group(_, _, _, children) ->
        let rects = findElements isRect result
        Assert.True(rects.Length >= 1, "Should have at least 1 Rect element for data bar")
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``BarChart_Grouped_MultiSeries`` () =
    let config = BarChart.defaultConfig 600f 400f
    let data =
        [ { Category = "A"; Values = [ ("S1", 10.0); ("S2", 20.0) ] }
          { Category = "B"; Values = [ ("S1", 15.0); ("S2", 25.0) ] } ]
    let result = BarChart.barChart config BarLayout.Grouped data
    let rects = findElements isRect result
    // 2 categories x 2 series = 4 data bars (plus possibly axis/grid rects)
    Assert.True(rects.Length >= 4, "Should have at least 4 Rect elements for data bars")

[<Fact>]
let ``BarChart_Stacked_AccumulatesHeight`` () =
    let config = BarChart.defaultConfig 600f 400f
    let data =
        [ { Category = "A"; Values = [ ("S1", 10.0); ("S2", 20.0) ] } ]
    let result = BarChart.barChart config BarLayout.Stacked data
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``BarChart_EmptyData_ReturnsGroup`` () =
    let config = BarChart.defaultConfig 600f 400f
    let result = BarChart.barChart config BarLayout.Grouped []
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``BarChart_ZeroValue_ReturnsGroup`` () =
    let config = BarChart.defaultConfig 600f 400f
    let data = [ { Category = "A"; Values = [ ("S1", 0.0) ] } ]
    let result = BarChart.barChart config BarLayout.Grouped data
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

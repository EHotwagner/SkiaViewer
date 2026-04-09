module SkiaViewer.Charts.Tests.AreaChartTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

let rec findElements (pred: Element -> bool) (el: Element) =
    let self = if pred el then [el] else []
    match el with
    | Element.Group(_, _, _, children) -> self @ (children |> List.collect (findElements pred))
    | _ -> self

let isPath el = match el with Element.Path _ -> true | _ -> false

[<Fact>]
let ``AreaChart_SingleSeries_HasPath`` () =
    let config = AreaChart.defaultConfig 600f 400f
    let series =
        [ { Name = "S1"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 3.0 }; { X = 2.0; Y = 2.0 } ] } ]
    let result = AreaChart.areaChart config series
    let paths = findElements isPath result
    Assert.True(paths.Length >= 1, "Should have at least 1 Path element for the filled area")

[<Fact>]
let ``AreaChart_EmptyData_ReturnsGroup`` () =
    let config = AreaChart.defaultConfig 600f 400f
    let result = AreaChart.areaChart config []
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``AreaChart_MultiSeries_Stacks`` () =
    let config = AreaChart.defaultConfig 600f 400f
    let series =
        [ { Name = "A"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 2.0 } ] }
          { Name = "B"; Points = [ { X = 0.0; Y = 1.0 }; { X = 1.0; Y = 1.0 } ] } ]
    let result = AreaChart.areaChart config series
    let paths = findElements isPath result
    Assert.True(paths.Length >= 2, "Should have multiple Path elements for stacked areas")

module SkiaViewer.Charts.Tests.ScatterPlotTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

let rec findElements (pred: Element -> bool) (el: Element) =
    let self = if pred el then [el] else []
    match el with
    | Element.Group(_, _, _, children) -> self @ (children |> List.collect (findElements pred))
    | _ -> self

let isEllipse el = match el with Element.Ellipse _ -> true | _ -> false

[<Fact>]
let ``ScatterPlot_SingleSeries_HasCircles`` () =
    let config = ScatterPlot.defaultConfig 600f 400f
    let series =
        [ { Name = "S1"; Points = [ { X = 1.0; Y = 2.0 }; { X = 3.0; Y = 4.0 }; { X = 5.0; Y = 6.0 } ] } ]
    let result = ScatterPlot.scatterPlot config series
    let ellipses = findElements isEllipse result
    Assert.True(ellipses.Length >= 3, "Should have Ellipse elements for scatter points")

[<Fact>]
let ``ScatterPlot_EmptyData_ReturnsGroup`` () =
    let config = ScatterPlot.defaultConfig 600f 400f
    let result = ScatterPlot.scatterPlot config []
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``ScatterPlot_MultiSeries_ReturnsGroup`` () =
    let config = ScatterPlot.defaultConfig 600f 400f
    let series =
        [ { Name = "A"; Points = [ { X = 1.0; Y = 2.0 } ] }
          { Name = "B"; Points = [ { X = 3.0; Y = 4.0 } ] } ]
    let result = ScatterPlot.scatterPlot config series
    match result with
    | Element.Group(_, _, _, children) -> Assert.True(children.Length > 0)
    | _ -> Assert.Fail("Expected Group element")

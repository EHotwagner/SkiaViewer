module SkiaViewer.Charts.Tests.PieChartTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

let rec findElements (pred: Element -> bool) (el: Element) =
    let self = if pred el then [el] else []
    match el with
    | Element.Group(_, _, _, children) -> self @ (children |> List.collect (findElements pred))
    | _ -> self

let isArc el = match el with Element.Arc _ -> true | _ -> false
let isText el = match el with Element.Text _ -> true | _ -> false
let isEllipse el = match el with Element.Ellipse _ -> true | _ -> false

[<Fact>]
let ``PieChart_ThreeSlices_ReturnsGroup`` () =
    let config = PieChart.defaultConfig 400f 400f
    let slices =
        [ { Label = "A"; Value = 30.0 }
          { Label = "B"; Value = 50.0 }
          { Label = "C"; Value = 20.0 } ]
    let result = PieChart.pieChart config slices
    let arcs = findElements isArc result
    Assert.True(arcs.Length >= 3, "Should have at least 3 Arc elements")

[<Fact>]
let ``PieChart_AllZero_ShowsNoData`` () =
    let config = PieChart.defaultConfig 400f 400f
    let slices =
        [ { Label = "A"; Value = 0.0 }
          { Label = "B"; Value = 0.0 } ]
    let result = PieChart.pieChart config slices
    let texts = findElements isText result
    let hasNoData = texts |> List.exists (fun el ->
        match el with
        | Element.Text(t, _, _, _, _) -> t.Contains("No data")
        | _ -> false)
    Assert.True(hasNoData, "Should show 'No data' text")

[<Fact>]
let ``PieChart_SingleSlice_FullCircle`` () =
    let config = PieChart.defaultConfig 400f 400f
    let slices = [ { Label = "Only"; Value = 100.0 } ]
    let result = PieChart.pieChart config slices
    let arcs = findElements isArc result
    Assert.True(arcs.Length >= 1, "Should have at least 1 Arc element")

[<Fact>]
let ``PieChart_Donut_HasOverlay`` () =
    let config = { PieChart.defaultConfig 400f 400f with DonutRatio = 0.5f }
    let slices = [ { Label = "A"; Value = 50.0 }; { Label = "B"; Value = 50.0 } ]
    let result = PieChart.pieChart config slices
    let ellipses = findElements isEllipse result
    Assert.True(ellipses.Length >= 1, "Should have Ellipse element for donut hole")

[<Fact>]
let ``PieChart_EmptySlices_ReturnsGroup`` () =
    let config = PieChart.defaultConfig 400f 400f
    let result = PieChart.pieChart config []
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

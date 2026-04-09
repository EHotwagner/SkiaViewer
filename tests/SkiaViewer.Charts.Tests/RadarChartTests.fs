module SkiaViewer.Charts.Tests.RadarChartTests

open Xunit
open SkiaViewer
open SkiaViewer.Charts

let rec findElements (pred: Element -> bool) (el: Element) =
    let self = if pred el then [el] else []
    match el with
    | Element.Group(_, _, _, children) -> self @ (children |> List.collect (findElements pred))
    | _ -> self

let isPath el = match el with Element.Path _ -> true | _ -> false
let isText el = match el with Element.Text _ -> true | _ -> false

[<Fact>]
let ``RadarChart_BasicData_HasPolygon`` () =
    let categories = [ "Speed"; "Power"; "Defense" ]
    let config = RadarChart.defaultConfig 400f 400f categories
    let series = [ { Name = "Player1"; Values = [ 8.0; 6.0; 7.0 ] } ]
    let result = RadarChart.radarChart config series
    let paths = findElements isPath result
    Assert.True(paths.Length >= 1, "Should have Path elements for the polygon")

[<Fact>]
let ``RadarChart_EmptyData_ShowsNoData`` () =
    let categories = [ "A"; "B"; "C" ]
    let config = RadarChart.defaultConfig 400f 400f categories
    let result = RadarChart.radarChart config []
    let texts = findElements isText result
    let hasNoData = texts |> List.exists (fun el ->
        match el with
        | Element.Text(t, _, _, _, _) -> t.Contains("No data")
        | _ -> false)
    Assert.True(hasNoData, "Should show 'No data' text")

[<Fact>]
let ``RadarChart_MultiSeries_MultiplePolygons`` () =
    let categories = [ "A"; "B"; "C" ]
    let config = RadarChart.defaultConfig 400f 400f categories
    let series =
        [ { Name = "P1"; Values = [ 5.0; 3.0; 4.0 ] }
          { Name = "P2"; Values = [ 4.0; 6.0; 2.0 ] } ]
    let result = RadarChart.radarChart config series
    let paths = findElements isPath result
    // Each series produces a fill path + a stroke path = 4 minimum, plus grid polygons
    Assert.True(paths.Length >= 4, "Should have multiple Path elements for series polygons")

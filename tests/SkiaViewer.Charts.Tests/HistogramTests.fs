module SkiaViewer.Charts.Tests.HistogramTests

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
let ``Histogram_BasicData_HasBars`` () =
    let config = { Histogram.defaultConfig 600f 400f with BinCount = 5 }
    let values = [ 1.0; 2.0; 3.0; 4.0; 5.0 ]
    let result = Histogram.histogram config values
    let rects = findElements isRect result
    Assert.True(rects.Length >= 1, "Should have Rect elements for histogram bars")

[<Fact>]
let ``Histogram_EmptyData_ReturnsGroup`` () =
    let config = Histogram.defaultConfig 600f 400f
    let result = Histogram.histogram config []
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

[<Fact>]
let ``Histogram_SingleValue_ReturnsGroup`` () =
    let config = Histogram.defaultConfig 600f 400f
    let result = Histogram.histogram config [ 42.0 ]
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

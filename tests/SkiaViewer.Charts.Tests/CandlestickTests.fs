module SkiaViewer.Charts.Tests.CandlestickTests

open Xunit
open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

let rec findElements (pred: Element -> bool) (el: Element) =
    let self = if pred el then [el] else []
    match el with
    | Element.Group(_, _, _, children) -> self @ (children |> List.collect (findElements pred))
    | _ -> self

let isLine el = match el with Element.Line _ -> true | _ -> false
let isRect el = match el with Element.Rect _ -> true | _ -> false

[<Fact>]
let ``Candlestick_BasicData_HasElements`` () =
    let config = Candlestick.defaultConfig 600f 400f
    let data =
        [ { Label = "Day1"; Open = 10.0; High = 15.0; Low = 8.0; Close = 12.0 }
          { Label = "Day2"; Open = 12.0; High = 18.0; Low = 11.0; Close = 16.0 } ]
    let result = Candlestick.candlestickChart config data
    let lines = findElements isLine result
    let rects = findElements isRect result
    Assert.True(lines.Length >= 2, "Should have Line elements for wicks")
    Assert.True(rects.Length >= 2, "Should have Rect elements for candle bodies")

[<Fact>]
let ``Candlestick_UpCandle_UsesUpColor`` () =
    let config = Candlestick.defaultConfig 600f 400f
    // Close > Open => up candle
    let data = [ { Label = "Day1"; Open = 10.0; High = 15.0; Low = 8.0; Close = 14.0 } ]
    let result = Candlestick.candlestickChart config data
    let rects = findElements isRect result
    // The candle body rect should use UpColor
    let hasUpColor = rects |> List.exists (fun el ->
        match el with
        | Element.Rect(_, _, _, _, p) -> p.Fill = Some config.UpColor
        | _ -> false)
    Assert.True(hasUpColor, "Up candle should use UpColor")

[<Fact>]
let ``Candlestick_EmptyData_ReturnsGroup`` () =
    let config = Candlestick.defaultConfig 600f 400f
    let result = Candlestick.candlestickChart config []
    match result with
    | Element.Group _ -> ()
    | _ -> Assert.Fail("Expected Group element")

module SkiaViewer.Charts.Tests.AxisTests

open Xunit
open SkiaViewer.Charts

[<Fact>]
let ``NiceNumber_ReturnsRoundedValue`` () =
    let result = Axis.niceNumber 97.0 true
    Assert.Equal(100.0, result)

[<Fact>]
let ``NiceNumber_ZeroRange_ReturnsOne`` () =
    let result = Axis.niceNumber 0.0 true
    Assert.Equal(1.0, result)

[<Fact>]
let ``ComputeAxisTicks_NormalRange`` () =
    let (minVal, maxVal, ticks) = Axis.computeAxisTicks 0.0 100.0 5
    Assert.True(minVal <= 0.0, "Min tick should be <= 0")
    Assert.True(maxVal >= 100.0, "Max tick should be >= 100")
    Assert.True(ticks.Length >= 2, "Should have at least 2 ticks")
    let tickValues = ticks |> List.map fst
    Assert.Contains(0.0, tickValues)

[<Fact>]
let ``ComputeAxisTicks_NegativeRange`` () =
    let (minVal, maxVal, ticks) = Axis.computeAxisTicks -50.0 50.0 5
    Assert.True(minVal <= -50.0, "Min should cover -50")
    Assert.True(maxVal >= 50.0, "Max should cover 50")
    let tickValues = ticks |> List.map fst
    let hasNeg = tickValues |> List.exists (fun v -> v < 0.0)
    let hasPos = tickValues |> List.exists (fun v -> v > 0.0)
    Assert.True(hasNeg, "Should have negative ticks")
    Assert.True(hasPos, "Should have positive ticks")

[<Fact>]
let ``ComputeAxisTicks_SingleValue`` () =
    let (minVal, maxVal, ticks) = Axis.computeAxisTicks 5.0 5.0 5
    Assert.True(ticks.Length >= 2, "Should produce valid ticks")
    Assert.True(minVal < maxVal, "Should produce a non-zero range")

[<Fact>]
let ``ComputeAutoRange_EmptyList`` () =
    let (lo, hi) = Axis.computeAutoRange []
    Assert.Equal(0.0, lo)
    Assert.Equal(1.0, hi)

[<Fact>]
let ``ComputeAutoRange_SingleValue`` () =
    let (lo, hi) = Axis.computeAutoRange [5.0]
    Assert.True(lo < 5.0, "Low should be below 5")
    Assert.True(hi > 5.0, "High should be above 5")

[<Fact>]
let ``ComputeAutoRange_WithNaN`` () =
    let (lo, hi) = Axis.computeAutoRange [1.0; nan; 3.0]
    Assert.True(lo <= 1.0, "Low should be <= 1")
    Assert.True(hi >= 3.0, "High should be >= 3")

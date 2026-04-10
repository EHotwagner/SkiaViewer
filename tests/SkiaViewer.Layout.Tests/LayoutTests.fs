module SkiaViewer.Layout.Tests.LayoutTests

open Xunit
open SkiaSharp
open SkiaViewer
open SkiaViewer.Layout

let private dummyElement = Scene.rect 0f 0f 50f 50f (Scene.fill SKColors.Red)

let private extractTranslation (element: Element) : (float32 * float32) option =
    match element with
    | Element.Group(Some (Transform.Translate(x, y)), _, _, _) -> Some (x, y)
    | _ -> None

let private extractGroupChildren (element: Element) : Element list =
    match element with
    | Element.Group(_, _, _, children) -> children
    | _ -> []

// === HStack Tests ===

[<Fact>]
let ``hstack positions children left to right`` () =
    let children = [
        Layout.childWithSize 50f 50f dummyElement
        Layout.childWithSize 50f 50f dummyElement
        Layout.childWithSize 50f 50f dummyElement
    ]
    let result = Layout.hstack Defaults.stackConfig children 300f 100f
    let group = extractGroupChildren result
    Assert.Equal(3, group.Length)
    let pos0 = extractTranslation group.[0]
    let pos1 = extractTranslation group.[1]
    let pos2 = extractTranslation group.[2]
    Assert.True(pos0.IsSome, "First child should have translation")
    Assert.True(pos1.IsSome, "Second child should have translation")
    Assert.True(pos2.IsSome, "Third child should have translation")
    let (x0, _) = pos0.Value
    let (x1, _) = pos1.Value
    let (x2, _) = pos2.Value
    Assert.True(x0 < x1, $"First child x ({x0}) should be left of second ({x1})")
    Assert.True(x1 < x2, $"Second child x ({x1}) should be left of third ({x2})")

[<Fact>]
let ``hstack with spacing adds gaps between children`` () =
    let config = { Defaults.stackConfig with Spacing = 10f }
    let children = [
        Layout.childWithSize 50f 50f dummyElement
        Layout.childWithSize 50f 50f dummyElement
    ]
    let result = Layout.hstack config children 300f 100f
    let group = extractGroupChildren result
    let (x0, _) = (extractTranslation group.[0]).Value
    let (x1, _) = (extractTranslation group.[1]).Value
    Assert.Equal(60f, x1 - x0)  // 50 width + 10 spacing

[<Fact>]
let ``hstack with padding offsets from edges`` () =
    let config = { Defaults.stackConfig with Padding = { Left = 20f; Top = 15f; Right = 20f; Bottom = 15f } }
    let children = [ Layout.childWithSize 50f 50f dummyElement ]
    let result = Layout.hstack config children 300f 100f
    let group = extractGroupChildren result
    let (x, y) = (extractTranslation group.[0]).Value
    Assert.Equal(20f, x)
    Assert.Equal(15f, y)

// === VStack Tests ===

[<Fact>]
let ``vstack positions children top to bottom`` () =
    let children = [
        Layout.childWithSize 50f 30f dummyElement
        Layout.childWithSize 50f 30f dummyElement
        Layout.childWithSize 50f 30f dummyElement
    ]
    let result = Layout.vstack Defaults.stackConfig children 200f 300f
    let group = extractGroupChildren result
    Assert.Equal(3, group.Length)
    let (_, y0) = (extractTranslation group.[0]).Value
    let (_, y1) = (extractTranslation group.[1]).Value
    let (_, y2) = (extractTranslation group.[2]).Value
    Assert.True(y0 < y1, $"First child y ({y0}) should be above second ({y1})")
    Assert.True(y1 < y2, $"Second child y ({y1}) should be above third ({y2})")

[<Fact>]
let ``vstack with spacing adds gaps between children`` () =
    let config = { Defaults.stackConfig with Spacing = 15f }
    let children = [
        Layout.childWithSize 50f 40f dummyElement
        Layout.childWithSize 50f 40f dummyElement
    ]
    let result = Layout.vstack config children 200f 300f
    let group = extractGroupChildren result
    let (_, y0) = (extractTranslation group.[0]).Value
    let (_, y1) = (extractTranslation group.[1]).Value
    Assert.Equal(55f, y1 - y0)  // 40 height + 15 spacing

[<Fact>]
let ``vstack with padding offsets from edges`` () =
    let config = { Defaults.stackConfig with Padding = { Left = 10f; Top = 25f; Right = 10f; Bottom = 25f } }
    let children = [ Layout.childWithSize 50f 30f dummyElement ]
    let result = Layout.vstack config children 200f 300f
    let group = extractGroupChildren result
    let (x, y) = (extractTranslation group.[0]).Value
    Assert.Equal(10f, x)
    Assert.Equal(25f, y)

// === Dock Tests ===

[<Fact>]
let ``dock positions top child at top`` () =
    let topChild = { Layout.dockChild DockPosition.Top (Scene.rect 0f 0f 300f 50f (Scene.fill SKColors.Blue))
                     with Sizing = { Defaults.sizing with DesiredHeight = Some 50f } }
    let children = [ topChild; Layout.dockChild DockPosition.Fill dummyElement ]
    let result = Layout.dock Defaults.dockConfig children 300f 200f
    let group = extractGroupChildren result
    Assert.Equal(2, group.Length)
    let (x, y) = (extractTranslation group.[0]).Value
    Assert.Equal(0f, x)
    Assert.Equal(0f, y)

[<Fact>]
let ``dock positions left child at left after top`` () =
    let topChild = { Layout.dockChild DockPosition.Top (Scene.rect 0f 0f 300f 50f (Scene.fill SKColors.Blue))
                     with Sizing = { Defaults.sizing with DesiredHeight = Some 50f } }
    let leftChild = { Layout.dockChild DockPosition.Left (Scene.rect 0f 0f 80f 150f (Scene.fill SKColors.Green))
                      with Sizing = { Defaults.sizing with DesiredWidth = Some 80f } }
    let children = [ topChild; leftChild; Layout.dockChild DockPosition.Fill dummyElement ]
    let result = Layout.dock Defaults.dockConfig children 300f 200f
    let group = extractGroupChildren result
    let (leftX, leftY) = (extractTranslation group.[1]).Value
    Assert.Equal(0f, leftX)
    Assert.Equal(50f, leftY) // after top (50px)

[<Fact>]
let ``dock fill child occupies remaining space`` () =
    let topChild = { Layout.dockChild DockPosition.Top (Scene.rect 0f 0f 300f 50f (Scene.fill SKColors.Blue))
                     with Sizing = { Defaults.sizing with DesiredHeight = Some 50f } }
    let leftChild = { Layout.dockChild DockPosition.Left (Scene.rect 0f 0f 80f 150f (Scene.fill SKColors.Green))
                      with Sizing = { Defaults.sizing with DesiredWidth = Some 80f } }
    let children = [ topChild; leftChild; Layout.dockChild DockPosition.Fill dummyElement ]
    let result = Layout.dock Defaults.dockConfig children 300f 200f
    let group = extractGroupChildren result
    let (fillX, fillY) = (extractTranslation group.[2]).Value
    Assert.Equal(80f, fillX) // after left (80px)
    Assert.Equal(50f, fillY) // after top (50px)

[<Fact>]
let ``dock with padding offsets from edges`` () =
    let config = { Defaults.dockConfig with Padding = { Left = 10f; Top = 10f; Right = 10f; Bottom = 10f } }
    let children = [ Layout.dockChild DockPosition.Fill dummyElement ]
    let result = Layout.dock config children 300f 200f
    let group = extractGroupChildren result
    let (x, y) = (extractTranslation group.[0]).Value
    Assert.Equal(10f, x)
    Assert.Equal(10f, y)

// === Nesting Tests ===

[<Fact>]
let ``nested stacks produce correct structure`` () =
    let innerStack =
        Layout.hstack Defaults.stackConfig [
            Layout.childWithSize 30f 30f dummyElement
            Layout.childWithSize 30f 30f dummyElement
        ] 100f 30f

    let outerStack =
        Layout.vstack Defaults.stackConfig [
            Layout.childWithSize 100f 50f dummyElement
            Layout.child innerStack
        ] 100f 100f

    let outerChildren = extractGroupChildren outerStack
    Assert.Equal(2, outerChildren.Length)

[<Fact>]
let ``dock with inner vstack nests correctly`` () =
    let sidebar =
        Layout.vstack { Defaults.stackConfig with Spacing = 5f } [
            Layout.childWithSize 80f 30f dummyElement
            Layout.childWithSize 80f 30f dummyElement
        ] 80f 200f

    let docked =
        Layout.dock Defaults.dockConfig [
            Layout.dockChild DockPosition.Left sidebar
            Layout.dockChild DockPosition.Fill dummyElement
        ] 300f 200f

    let children = extractGroupChildren docked
    Assert.Equal(2, children.Length)

// === Resize Tests ===

[<Fact>]
let ``same layout with different sizes produces proportioned output`` () =
    let children = [
        Layout.childWithSize 100f 50f dummyElement
        Layout.childWithSize 100f 50f dummyElement
    ]
    let small = Layout.hstack Defaults.stackConfig children 300f 100f
    let large = Layout.hstack Defaults.stackConfig children 600f 200f
    let smallGroup = extractGroupChildren small
    let largeGroup = extractGroupChildren large
    // Both should have 2 children
    Assert.Equal(2, smallGroup.Length)
    Assert.Equal(2, largeGroup.Length)
    // Child positions should be the same (fixed sizing)
    let (sx0, _) = (extractTranslation smallGroup.[0]).Value
    let (lx0, _) = (extractTranslation largeGroup.[0]).Value
    Assert.Equal(sx0, lx0)

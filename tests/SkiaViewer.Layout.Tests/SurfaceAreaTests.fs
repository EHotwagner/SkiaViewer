module SkiaViewer.Layout.Tests.SurfaceAreaTests

open Xunit
open SkiaViewer.Layout

// === Types module surface area ===

[<Fact>]
let ``HorizontalAlignment has expected cases`` () =
    let _ = HorizontalAlignment.Left
    let _ = HorizontalAlignment.Center
    let _ = HorizontalAlignment.Right
    let _ = HorizontalAlignment.Stretch
    Assert.True(true)

[<Fact>]
let ``VerticalAlignment has expected cases`` () =
    let _ = VerticalAlignment.Top
    let _ = VerticalAlignment.Center
    let _ = VerticalAlignment.Bottom
    let _ = VerticalAlignment.Stretch
    Assert.True(true)

[<Fact>]
let ``DockPosition has expected cases`` () =
    let _ = DockPosition.Top
    let _ = DockPosition.Bottom
    let _ = DockPosition.Left
    let _ = DockPosition.Right
    let _ = DockPosition.Fill
    Assert.True(true)

[<Fact>]
let ``NodeShape has expected cases`` () =
    let _ = NodeShape.Rectangle
    let _ = NodeShape.Ellipse
    let _ = NodeShape.RoundedRect 5f
    Assert.True(true)

[<Fact>]
let ``GraphKind has expected cases`` () =
    let _ = GraphKind.Directed
    let _ = GraphKind.Undirected
    Assert.True(true)

[<Fact>]
let ``LayoutDirection has expected cases`` () =
    let _ = LayoutDirection.TopToBottom
    let _ = LayoutDirection.LeftToRight
    let _ = LayoutDirection.BottomToTop
    let _ = LayoutDirection.RightToLeft
    Assert.True(true)

// === Defaults module surface area ===

[<Fact>]
let ``Defaults module exposes expected values`` () =
    let _ = Defaults.padding
    let _ = Defaults.sizing
    let _ = Defaults.stackConfig
    let _ = Defaults.dockConfig
    let _ = Defaults.nodeStyle
    let _ = Defaults.edgeStyle
    let _ = Defaults.graphConfig GraphKind.Directed
    Assert.True(true)

// === Layout module surface area ===

[<Fact>]
let ``Layout module exposes expected functions`` () =
    let _ = Layout.hstack
    let _ = Layout.vstack
    let _ = Layout.dock
    let _ = Layout.child
    let _ = Layout.childWithSize
    let _ = Layout.dockChild
    Assert.True(true)

// === Graph module surface area ===

[<Fact>]
let ``Graph module exposes expected functions`` () =
    let _ = Graph.render
    let _ = Graph.defaultConfig
    let _ = Graph.validate
    Assert.True(true)

// === GraphValidation module surface area ===

[<Fact>]
let ``GraphValidation module exposes validate`` () =
    let _ = GraphValidation.validate
    Assert.True(true)

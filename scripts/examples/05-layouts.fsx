/// Layout Container Demos
/// Demonstrates HStack, VStack, Dock, and nested layouts.
///
/// Usage: dotnet fsi scripts/examples/05-layouts.fsx

#load "../layout-prelude.fsx"
open SkiaSharp
open SkiaViewer
open SkiaViewer.Layout

// --- Demo 1: Simple HStack ---
let buttons =
    Layout.hstack { Defaults.stackConfig with Spacing = 10f } [
        Layout.childWithSize 80f 30f (Scene.rect 0f 0f 80f 30f (Scene.fill SKColors.Green))
        Layout.childWithSize 80f 30f (Scene.rect 0f 0f 80f 30f (Scene.fill SKColors.Red))
        Layout.childWithSize 80f 30f (Scene.rect 0f 0f 80f 30f (Scene.fill SKColors.Blue))
    ] 300f 30f

// --- Demo 2: VStack with spacing ---
let header = Scene.rect 0f 0f 300f 50f (Scene.fill SKColors.DarkBlue)
let content = Scene.rect 0f 0f 300f 200f (Scene.fill SKColors.White)
let page =
    Layout.vstack { Defaults.stackConfig with Spacing = 20f } [
        Layout.childWithSize 300f 50f header
        Layout.child buttons
        Layout.childWithSize 300f 200f content
    ] 300f 400f

// --- Demo 3: Dock layout ---
let sidebar = Scene.rect 0f 0f 100f 300f (Scene.fill SKColors.DarkGray)
let topBar = Scene.rect 0f 0f 400f 50f (Scene.fill SKColors.DarkBlue)
let mainContent = Scene.rect 0f 0f 300f 250f (Scene.fill SKColors.White)
let docked =
    Layout.dock Defaults.dockConfig [
        { Layout.dockChild DockPosition.Top topBar with Sizing = { Defaults.sizing with DesiredHeight = Some 50f } }
        { Layout.dockChild DockPosition.Left sidebar with Sizing = { Defaults.sizing with DesiredWidth = Some 100f } }
        Layout.dockChild DockPosition.Fill mainContent
    ] 400f 350f

// --- Demo 4: Nested layouts ---
let innerStack =
    Layout.hstack { Defaults.stackConfig with Spacing = 5f } [
        Layout.childWithSize 40f 40f (Scene.rect 0f 0f 40f 40f (Scene.fill SKColors.Orange))
        Layout.childWithSize 40f 40f (Scene.rect 0f 0f 40f 40f (Scene.fill SKColors.Purple))
    ] 100f 40f

let outerStack =
    Layout.vstack { Defaults.stackConfig with Spacing = 10f } [
        Layout.childWithSize 200f 30f (Scene.text "Nested Layout Demo" 10f 20f 16f (Scene.fill SKColors.Black))
        Layout.child innerStack
        Layout.childWithSize 200f 100f (Scene.rect 0f 0f 200f 100f (Scene.fill SKColors.LightGray))
    ] 200f 200f

let scene = { BackgroundColor = SKColors.CornflowerBlue; Elements = [ page ] }
printfn "Layout demos created. Scene has %d elements." scene.Elements.Length
printfn "HStack buttons, VStack page, Dock layout, and nested layouts all built successfully."

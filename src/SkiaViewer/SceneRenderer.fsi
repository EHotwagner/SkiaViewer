namespace SkiaViewer

open SkiaSharp

/// Internal module that renders a declarative scene tree to an SKCanvas.
module internal SceneRenderer =
    /// Render a complete scene to the given canvas.
    /// Clears the canvas with the scene's background color, then renders
    /// all elements in tree order (depth-first, later siblings on top).
    val render: scene: Scene -> canvas: SKCanvas -> unit

    /// Render a list of elements to the given canvas without clearing.
    val renderElements: elements: Element list -> canvas: SKCanvas -> unit

    /// Convert a Transform to an SKMatrix.
    val toMatrix: transform: Transform -> SKMatrix

    /// Apply a Clip to the given canvas.
    val applyClip: canvas: SKCanvas -> clip: Clip -> unit

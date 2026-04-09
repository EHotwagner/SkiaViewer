module SkiaViewer.PerfTests.SceneGenerators

open SkiaSharp
open SkiaViewer

let complexityTiers = [| 10; 100; 1_000; 10_000; 100_000 |]

let private colors =
    [| SKColors.Red; SKColors.Blue; SKColors.Green; SKColors.Yellow
       SKColors.Cyan; SKColors.Magenta; SKColors.Orange; SKColors.White
       SKColors.Coral; SKColors.LimeGreen; SKColors.Purple; SKColors.Gold |]

let private nextColor (rng: System.Random) =
    colors.[rng.Next(colors.Length)]

let private generateRect (rng: System.Random) =
    let x = rng.NextSingle() * 760f
    let y = rng.NextSingle() * 560f
    let w = 10f + rng.NextSingle() * 40f
    let h = 10f + rng.NextSingle() * 40f
    Scene.rect x y w h (Scene.fill (nextColor rng))

let private generateEllipse (rng: System.Random) =
    let cx = rng.NextSingle() * 800f
    let cy = rng.NextSingle() * 600f
    let rx = 5f + rng.NextSingle() * 25f
    let ry = 5f + rng.NextSingle() * 25f
    Scene.ellipse cx cy rx ry (Scene.fill (nextColor rng))

let private generateLine (rng: System.Random) =
    let x1 = rng.NextSingle() * 800f
    let y1 = rng.NextSingle() * 600f
    let x2 = rng.NextSingle() * 800f
    let y2 = rng.NextSingle() * 600f
    Scene.line x1 y1 x2 y2 (Scene.stroke (nextColor rng) (1f + rng.NextSingle() * 3f))

let private generateText (rng: System.Random) =
    let x = rng.NextSingle() * 700f
    let y = rng.NextSingle() * 580f
    let size = 10f + rng.NextSingle() * 20f
    Scene.text "Perf" x y size (Scene.fill (nextColor rng))

let private generatePath (rng: System.Random) =
    let x0 = rng.NextSingle() * 800f
    let y0 = rng.NextSingle() * 600f
    let commands = [
        PathCommand.MoveTo(x0, y0)
        PathCommand.LineTo(x0 + rng.NextSingle() * 40f, y0 + rng.NextSingle() * 40f)
        PathCommand.LineTo(x0 + rng.NextSingle() * 40f, y0 - rng.NextSingle() * 40f)
        PathCommand.Close
    ]
    Scene.path commands (Scene.fill (nextColor rng))

let private generators =
    [| generateRect; generateEllipse; generateLine; generateText; generatePath |]

let generateScene (elementType: string) (count: int) (seed: int) : Scene =
    let rng = System.Random(seed)
    let gen =
        match elementType with
        | "Rect" -> generateRect
        | "Ellipse" -> generateEllipse
        | "Line" -> generateLine
        | "Text" -> generateText
        | "Path" -> generatePath
        | _ -> fun r -> generators.[r.Next(generators.Length)] r // Mixed
    let elements = List.init count (fun _ -> gen rng)
    Scene.create SKColors.Black elements

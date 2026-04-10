/// Offline renderer — renders demo reel frames via reflection on SceneRenderer.
/// Run: dotnet fsi scripts/examples/09-render-reel.fsx

#load "../layout-prelude.fsx"
#r "../../src/SkiaViewer.Charts/bin/Debug/net10.0/SkiaViewer.Charts.dll"

open System
open System.IO
open System.Reflection
open SkiaSharp
open SkiaViewer
open SkiaViewer.Layout
open SkiaViewer.Charts

let W = 1280f
let H = 720f
let FPS = 30
let DURATION = 60f
let TOTAL_FRAMES = int (DURATION * float32 FPS)
let OUTPUT_DIR = "/home/developer/projects/SkiaViewer/frames"

// ── Access internal SceneRenderer via reflection ──
let asm = typeof<Scene>.Assembly
let rendererType = asm.GetType("SkiaViewer.SceneRenderer")
let renderMethod = rendererType.GetMethod("render", BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic)
printfn "SceneRenderer.render found: %b" (not (isNull renderMethod))

let renderScene (scene: Scene) (canvas: SKCanvas) =
    renderMethod.Invoke(null, [| box scene; box canvas |]) |> ignore

let renderToBitmap (scene: Scene) : SKBitmap =
    let bmp = new SKBitmap(int W, int H)
    use canvas = new SKCanvas(bmp)
    renderScene scene canvas
    canvas.Flush()
    bmp

// ── All scene functions (copied from 08-demo-reel.fsx) ──
let pi = float32 Math.PI
let pulse t speed = (sin (t * speed) + 1f) / 2f

let progressBar (elapsed: float32) (total: float32) =
    let pct = min 1f (elapsed / total)
    [ Scene.rect 0f (H - 4f) W 4f (Scene.fill (SKColor(40uy, 40uy, 40uy)))
      Scene.rect 0f (H - 4f) (W * pct) 4f (Scene.fill (SKColor(100uy, 180uy, 255uy))) ]

let titleOverlay (text: string) (sub: string) (alpha: float32) =
    let a = byte (255f * (min 1f (max 0f alpha)))
    if a < 5uy then []
    else [ Scene.rect 0f 0f W H (Scene.fill (SKColor(0uy, 0uy, 0uy, byte (180f * alpha))))
           Scene.text text (W / 2f - 250f) (H / 2f - 20f) 52f (Scene.fill (SKColor(255uy, 255uy, 255uy, a)))
           Scene.text sub (W / 2f - 180f) (H / 2f + 30f) 18f (Scene.fill (SKColor(180uy, 180uy, 220uy, a))) ]

let geometryBurst (t: float32) =
    let bg = SKColor(12uy, 12uy, 25uy)
    let elements = [
        for i in 0..11 do
            let angle = float32 i * 30f + t * (80f + float32 i * 15f)
            let rad = angle * pi / 180f
            let orbitR = 120f + float32 i * 18f
            let cx = W / 2f + cos rad * orbitR
            let cy = H / 2f + sin rad * orbitR
            let r = 6f + pulse t (3f + float32 i * 0.5f) * 12f
            let hue = (float32 i * 30f + t * 40f) % 360f
            let color = SKColor.FromHsl(hue, 90f, 65f)
            yield Scene.circle cx cy (r + 4f) (Scene.fill color |> Scene.withMaskFilter (MaskFilter.Blur(BlurStyle.Normal, 8f)))
            yield Scene.circle cx cy r (Scene.fill color)
        let ringR = 60f + pulse t 2f * 30f
        yield Scene.circle (W / 2f) (H / 2f) ringR (Scene.stroke (SKColor(255uy, 255uy, 255uy, 120uy)) (2f + pulse t 3f * 2f))
        yield Scene.circle (W / 2f) (H / 2f) (ringR * 1.5f) (Scene.stroke (SKColor(100uy, 150uy, 255uy, 60uy)) 1f)
        for i in 0..5 do
            let angle = float32 i * 60f + t * 45f
            let sz = 30f + float32 i * 8f
            yield! Scene.rotate angle (W / 2f) (H / 2f) [ Scene.rect (W / 2f - sz / 2f) (H / 2f - sz / 2f) sz sz (Scene.stroke (SKColor.FromHsl(200f + float32 i * 20f, 80f, 70f)) 2f) ] |> List.singleton
        for i in 0..19 do
            let a = (float32 i * 18f + t * 60f) * pi / 180f
            let r1 = 200f + pulse t (1f + float32 i * 0.2f) * 40f
            let r2 = r1 + 40f + float32 i * 3f
            yield Scene.line (W / 2f + cos a * r1) (H / 2f + sin a * r1) (W / 2f + cos a * r2) (H / 2f + sin a * r2) (Scene.stroke (SKColor(200uy, 220uy, 255uy, byte (100 + int (pulse t (2f + float32 i * 0.3f) * 155f)))) 1.5f)
    ]
    Scene.create bg (elements @ titleOverlay "SkiaViewer" "F# Declarative Rendering on Vulkan" (max 0f (1f - t / 2f)))

let shaderShowcase (t: float32) =
    let bg = SKColor(5uy, 5uy, 15uy)
    let elements = [
        let gc = [| SKColor.FromHsl(t * 30f % 360f, 90f, 60f); SKColor.FromHsl((t * 30f + 120f) % 360f, 90f, 50f); SKColor.FromHsl((t * 30f + 240f) % 360f, 90f, 60f); SKColors.Black |]
        yield Scene.circle 300f 360f 220f (Scene.fill SKColors.White |> Scene.withShader (Shader.RadialGradient(SKPoint(300f, 360f), 200f + pulse t 1f * 80f, gc, [|0f;0.3f;0.7f;1f|], TileMode.Clamp)))
        let sc = [| SKColors.Red; SKColors.Yellow; SKColors.Lime; SKColors.Cyan; SKColors.Blue; SKColors.Magenta; SKColors.Red |]
        yield Scene.circle 640f 360f 180f (Scene.fill SKColors.White |> Scene.withShader (Shader.SweepGradient(SKPoint(640f, 360f), sc, [|0f;0.167f;0.333f;0.5f;0.667f;0.833f;1f|], t * 60f % 360f, (t * 60f + 350f) % 360f + 360f)))
        yield Scene.circle 640f 360f 100f (Scene.fill bg)
        let freq = 0.008f + pulse t 0.5f * 0.012f
        yield Scene.rect 900f 160f 320f 400f (Scene.fill SKColors.White |> Scene.withShader (Shader.PerlinNoiseTurbulence(freq, freq * 1.5f, 4, t * 2f)))
        yield Scene.rect 900f 160f 320f 400f (Scene.stroke (SKColor(100uy, 180uy, 255uy, 100uy)) 2f)
        yield Scene.text "Radial Gradient" 200f 580f 16f (Scene.fill (SKColor(180uy, 180uy, 200uy)))
        yield Scene.text "Sweep Gradient" 555f 580f 16f (Scene.fill (SKColor(180uy, 180uy, 200uy)))
        yield Scene.text "Perlin Turbulence" 950f 580f 16f (Scene.fill (SKColor(180uy, 180uy, 200uy)))
        let cc = [| SKColor.FromHsl(t * 50f % 360f, 100f, 50f); SKColors.White; SKColor.FromHsl((t * 50f + 180f) % 360f, 100f, 50f) |]
        yield Scene.rect 60f 90f 1160f 50f (Scene.fill SKColors.White |> Scene.withShader (Shader.TwoPointConicalGradient(SKPoint(100f, 120f), 10f, SKPoint(1180f, 120f), 100f, cc, [|0f;0.5f;1f|], TileMode.Clamp)) |> Scene.withMaskFilter (MaskFilter.Blur(BlurStyle.Normal, 2f)))
    ]
    Scene.create bg (elements @ titleOverlay "Shaders" "Gradients / Noise / Runtime Effects" (max 0f (1f - t / 1.5f)))

let skslShader (t: float32) =
    let src = "uniform float iTime; uniform float iResW; uniform float iResH; half4 main(float2 fc) { float2 uv = float2(fc.x/iResW, fc.y/iResH); float v=0.0; float2 c=uv*6.0-3.0; v+=sin(c.x+iTime); v+=sin((c.y+iTime)/2.0); v+=sin((c.x+c.y+iTime)/2.0); float cx2=c.x+sin(iTime/3.0)*3.0; float cy2=c.y+cos(iTime/2.0)*3.0; v+=sin(sqrt(cx2*cx2+cy2*cy2+1.0)+iTime); v=v/2.0; half3 col=half3(sin(v*3.14159),sin(v*3.14159+2.094),sin(v*3.14159+4.189)); col=col*0.5+0.5; return half4(col,1.0); }"
    let elements = [
        yield Scene.rect 0f 0f W H (Scene.fill SKColors.White |> Scene.withShader (Shader.RuntimeEffect(src, [("iTime", t); ("iResW", W); ("iResH", H)])))
        yield Scene.rect 40f 560f 400f 120f (Scene.fill (SKColor(0uy, 0uy, 0uy, 140uy)))
        yield Scene.text "Runtime SkSL Shader" 60f 600f 22f (Scene.fill SKColors.White)
        yield Scene.text "Animated plasma — GPU-computed per pixel" 60f 628f 14f (Scene.fill (SKColor(180uy, 200uy, 255uy)))
        yield Scene.text (sprintf "iTime = %.1f" t) 60f 656f 14f (Scene.fill (SKColor(120uy, 180uy, 120uy)))
    ]
    Scene.create SKColors.Black elements

let chartDashboard (t: float32) =
    let bg = SKColor(18uy, 18uy, 28uy)
    let rng = Random(42)
    let p = t * 0.8f
    let lineData = [ { Name = "Revenue"; Points = [ for i in 0..20 -> { X = float i; Y = 50.0 + 30.0 * sin(float i * 0.5 + float p) + float (rng.Next(0, 10)) } ] }; { Name = "Costs"; Points = [ for i in 0..20 -> { X = float i; Y = 30.0 + 15.0 * cos(float i * 0.4 + float p * 0.7) + float (rng.Next(0, 8)) } ] } ]
    let slices = [ { Label="A"; Value=35.0+10.0*sin(float p) }; { Label="B"; Value=25.0+5.0*cos(float p*1.3) }; { Label="C"; Value=20.0+8.0*sin(float p*0.7) }; { Label="D"; Value=15.0+3.0*cos(float p*1.1) }; { Label="Other"; Value=5.0+2.0*sin(float p*2.0) } ]
    let barData = [ for q, v1, v2 in [("Q1",42.0,55.0);("Q2",38.0,48.0);("Q3",51.0,62.0);("Q4",45.0,58.0)] -> { Category=q; Values=[("2024",v1+8.0*sin(float p));("2025",v2+6.0*cos(float p))] } ]
    let areaData = [ { Name="Users"; Points=[for i in 0..30->{X=float i; Y=100.0+50.0*sin(float i*0.3+float p)}] }; { Name="Sessions"; Points=[for i in 0..30->{X=float i; Y=80.0+40.0*cos(float i*0.25+float p*0.8)}] } ]
    let radarData = [ { Name="Warrior"; Values=[60.0+20.0*sin(float p);90.0;30.0+10.0*cos(float p);85.0;20.0;50.0+15.0*sin(float p*1.5)] }; { Name="Mage"; Values=[30.0;40.0+15.0*cos(float p);60.0;25.0;95.0+5.0*sin(float p);70.0] } ]
    let scatterData = [ { Name="A"; Points=[for i in 0..15->{X=20.0+15.0*sin(float i*0.7+float p)+float i*2.0; Y=30.0+20.0*cos(float i*0.5+float p)+float i*1.5}] } ]
    let elements = [
        Scene.rect 0f 0f W 50f (Scene.fill (SKColor(15uy, 15uy, 30uy)))
        Scene.text "Analytics Dashboard" 30f 35f 22f (Scene.fill (SKColor(200uy, 210uy, 240uy)))
        Scene.text (sprintf "Live Data — %.0fs" t) (W - 200f) 35f 14f (Scene.fill (SKColor(100uy, 200uy, 100uy)))
        Scene.translate 20f 60f [ LineChart.lineChart { LineChart.defaultConfig 580f 280f with Title=Some "Revenue vs Costs"; BackgroundColor=Some (SKColor(25uy,25uy,40uy)) } lineData ]
        Scene.translate 620f 60f [ PieChart.pieChart { PieChart.defaultConfig 300f 280f with Title=Some "Market Share"; DonutRatio=0.45f; BackgroundColor=Some (SKColor(25uy,25uy,40uy)) } slices ]
        Scene.translate 890f 60f [ BarChart.barChart { BarChart.defaultConfig 370f 280f with Title=Some "Revenue"; BackgroundColor=Some (SKColor(25uy,25uy,40uy)) } BarLayout.Grouped barData ]
        Scene.translate 20f 370f [ AreaChart.areaChart { AreaChart.defaultConfig 600f 310f with Title=Some "User Activity"; BackgroundColor=Some (SKColor(25uy,25uy,40uy)) } areaData ]
        Scene.translate 640f 370f [ RadarChart.radarChart { RadarChart.defaultConfig 320f 310f ["Spd";"Pwr";"Rng";"Arm";"Mag";"Lck"] with Title=Some "Stats"; BackgroundColor=Some (SKColor(25uy,25uy,40uy)); ShowGrid=true; GridLevels=5 } radarData ]
        Scene.translate 975f 370f [ ScatterPlot.scatterPlot { ScatterPlot.defaultConfig 290f 310f with Title=Some "Scatter"; BackgroundColor=Some (SKColor(25uy,25uy,40uy)) } scatterData ]
    ]
    Scene.create bg (elements @ titleOverlay "Live Charts" "9 Chart Types — Real-time Data" (max 0f (1f - t / 1.5f)))

let dataGridScene (t: float32) =
    let bg = SKColor(15uy, 15uy, 25uy)
    let gc = { DataGrid.defaultConfig 620f 500f with ScrollOffset=float(t*30f%200f); HeaderColor=SKColor(40uy,40uy,80uy); AlternateRowColor=Some(SKColor(22uy,22uy,38uy)); RowHeight=28f; FontSize=13f; HeaderFontSize=14f }
    let gd : DataGridData = {
        Columns=[DataGrid.textColumn "Sym"; DataGrid.numericColumn "Price"; DataGrid.numericColumn "Chg%"; DataGrid.numericColumn "Vol"; DataGrid.textColumn "Sector"]
        Rows=[ for s,pr,ch,vo,se in ["AAPL",189.5,1.2,52.3,"Tech";"MSFT",378.9,-0.3,28.1,"Tech";"GOOGL",141.2,0.8,19.8,"Tech";"AMZN",178.3,2.1,45.6,"Consumer";"TSLA",248.7,-1.5,88.2,"Auto";"NVDA",495.2,3.4,61.3,"Semicon";"META",356.1,0.6,17.5,"Tech";"JPM",172.8,-0.2,9.8,"Finance";"V",261.4,0.4,7.2,"Finance";"JNJ",158.9,-0.7,6.1,"Health";"WMT",162.3,1.1,8.4,"Retail";"PG",151.7,0.3,5.6,"Consumer";"UNH",524.1,-1.2,3.9,"Health";"HD",342.6,0.9,4.2,"Retail";"BAC",33.8,1.8,38.1,"Finance";"XOM",104.5,-0.4,15.2,"Energy";"PFE",28.9,-2.1,27.6,"Health";"KO",59.2,0.5,11.3,"Consumer";"DIS",91.4,1.6,9.7,"Media";"NFLX",478.3,2.3,6.8,"Media"] -> [CellValue.TextValue s; CellValue.NumericValue(pr+sin(float t*2.0+float(s.GetHashCode()))*3.0); CellValue.NumericValue(ch+sin(float t*1.5)*0.5); CellValue.NumericValue vo; CellValue.TextValue se] ] }
    let ohlc = [ for i in 0..14 -> let bp=150.0+20.0*sin(float i*0.6+float t*0.5) in let sp=5.0+3.0*abs(cos(float i*0.8)) in let o=bp+float(i%3)*2.0 in let c=bp+sp*(if i%2=0 then 1.0 else -1.0) in { Label=sprintf "D%d" (i+1); Open=o; High=max o c+sp*0.5; Low=min o c-sp*0.5; Close=c } ]
    let elements = [
        Scene.rect 0f 0f W 50f (Scene.fill (SKColor(15uy,15uy,30uy)))
        Scene.text "Financial Data" 30f 35f 22f (Scene.fill (SKColor(200uy,210uy,240uy)))
        Scene.translate 20f 70f [ DataGrid.dataGrid gc gd ]
        Scene.translate 660f 70f [ Candlestick.candlestickChart { Candlestick.defaultConfig 600f 500f with Title=Some "Stock OHLC"; BackgroundColor=Some(SKColor(25uy,25uy,40uy)); UpColor=SKColor(0uy,200uy,100uy); DownColor=SKColor(255uy,70uy,70uy) } ohlc ]
    ]
    Scene.create bg (elements @ titleOverlay "DataGrid + Candlestick" "Scrolling Data / OHLC Charts" (max 0f (1f - t / 1.5f)))

let graphAndEffects (t: float32) =
    let bg = SKColor(10uy, 10uy, 20uy)
    let ns = { SkiaViewer.Layout.Defaults.nodeStyle with FillColor=Some(SKColor(60uy,80uy,160uy)); StrokeColor=Some(SKColor(100uy,140uy,220uy)); Shape=NodeShape.RoundedRect 6f }
    let es = { SkiaViewer.Layout.Defaults.edgeStyle with Color=Some(SKColor(80uy,120uy,200uy)) }
    let dc = { Graph.defaultConfig GraphKind.Directed with NodeSpacing=35f; LayerSpacing=55f; DefaultNodeStyle=ns; DefaultEdgeStyle=es }
    let gn = Some { SkiaViewer.Layout.Defaults.nodeStyle with FillColor=Some(SKColor(40uy,180uy,100uy)); Shape=NodeShape.RoundedRect 6f }
    let bn = Some { SkiaViewer.Layout.Defaults.nodeStyle with FillColor=Some(SKColor(50uy,130uy,240uy)); Shape=NodeShape.RoundedRect 6f }
    let dag : GraphDefinition = { Config=dc; Nodes=[{Id="A";Label="Input";Style=gn};{Id="B";Label="Parse";Style=None};{Id="C";Label="Analyze";Style=None};{Id="D";Label="Transform";Style=None};{Id="E";Label="Optimize";Style=None};{Id="F";Label="Output";Style=bn}]; Edges=[{Source="A";Target="B";Weight=None;Label=None;Style=None};{Source="A";Target="C";Weight=None;Label=None;Style=None};{Source="B";Target="D";Weight=None;Label=None;Style=None};{Source="C";Target="D";Weight=None;Label=None;Style=None};{Source="C";Target="E";Weight=None;Label=None;Style=None};{Source="D";Target="F";Weight=None;Label=None;Style=None};{Source="E";Target="F";Weight=None;Label=None;Style=None}] }
    let elements = [
        match Graph.render dag 580f 450f with
        | Ok g -> yield Scene.translate 20f 60f [ Scene.group None None [ Scene.rect 0f 0f 600f 480f (Scene.fill (SKColor(20uy,20uy,35uy))); Scene.rect 0f 0f 600f 480f (Scene.stroke (SKColor(60uy,80uy,140uy,80uy)) 1f); Scene.text "DAG — Compiler Pipeline" 15f 24f 16f (Scene.fill (SKColor(160uy,180uy,220uy))); Scene.translate 10f 35f [g] ] ]
        | _ -> ()
        for i in 0..2 do
            let angle = t * 40f + float32 i * 120f
            yield Scene.group (Some (Transform.Perspective(Transform3D.Compose [Transform3D.Translate(900f, 160f+float32 i*160f, 0f); Transform3D.RotateY(angle)]))) None [ Scene.rect -80f -50f 160f 100f (Scene.fill (SKColor.FromHsl(float32 i*80f+200f, 60f, 45f))); Scene.rect -80f -50f 160f 100f (Scene.stroke (SKColor(255uy,255uy,255uy,60uy)) 1f); Scene.text (sprintf "Card %d" (i+1)) -30f 5f 16f (Scene.fill SKColors.White) ]
        let fy = 600f + sin(t * 2f) * 15f
        yield Scene.rect 680f fy 250f 60f (Scene.fill (SKColor(80uy,120uy,220uy)) |> Scene.withImageFilter (ImageFilter.DropShadow(6f,6f,8f,8f,SKColor(0uy,0uy,0uy,160uy))))
        yield Scene.text "Drop Shadow + Float" 700f (fy+38f) 16f (Scene.fill SKColors.White)
        yield Scene.rect 0f 0f W 50f (Scene.fill (SKColor(10uy,10uy,25uy)))
        yield Scene.text "Graphs, 3D Transforms & Effects" 30f 35f 22f (Scene.fill (SKColor(200uy,210uy,240uy)))
    ]
    Scene.create bg (elements @ titleOverlay "Graph Viz + Effects" "DAG Layout / 3D Perspective / Shadows" (max 0f (1f - t / 1.5f)))

let finale (t: float32) =
    let bg = SKColor(5uy, 5uy, 12uy)
    let a = byte (255f * min 1f (t / 2f))
    let elements = [
        for i in 0..35 do
            let rad = (float32 i * 10f + t * 20f) * pi / 180f
            let r = 100f + float32 i * 8f + pulse t (1f + float32 i * 0.1f) * 50f
            yield Scene.line (W/2f) (H/2f) (W/2f+cos rad*r) (H/2f+sin rad*r) (Scene.stroke (SKColor.FromHsl((float32 i*10f+t*60f)%360f, 90f, 65f).WithAlpha(byte(100+int(pulse t 2f*155f)))) 1f)
        let gc = [| SKColor(255uy,255uy,255uy,200uy); SKColor(100uy,150uy,255uy,100uy); SKColor(0uy,0uy,0uy,0uy) |]
        yield Scene.circle (W/2f) (H/2f) 200f (Scene.fill SKColors.White |> Scene.withShader (Shader.RadialGradient(SKPoint(W/2f,H/2f), 150f+pulse t 1f*50f, gc, [|0f;0.4f;1f|], TileMode.Clamp)))
        yield Scene.text "SkiaViewer" (W/2f-160f) (H/2f-10f) 48f (Scene.fill (SKColor(255uy,255uy,255uy,a)))
        yield Scene.text "F# / .NET 10 / SkiaSharp 3 / Vulkan / Silk.NET" (W/2f-230f) (H/2f+30f) 16f (Scene.fill (SKColor(150uy,170uy,220uy,a)))
        yield Scene.text "Layouts / Graphs / Charts / Shaders / 3D" (W/2f-200f) (H/2f+55f) 14f (Scene.fill (SKColor(120uy,150uy,200uy,a)))
        let fa = max 0f ((t-4f)/2f)
        if fa > 0f then yield Scene.rect 0f 0f W H (Scene.fill (SKColor(0uy,0uy,0uy,byte(255f*fa))))
    ]
    Scene.create bg elements

let scenes = [| (0f,8f,geometryBurst); (8f,18f,shaderShowcase); (18f,26f,skslShader); (26f,38f,chartDashboard); (38f,46f,dataGridScene); (46f,54f,graphAndEffects); (54f,60f,finale) |]

let buildFrame (elapsed: float32) =
    match scenes |> Array.tryFind (fun (s,e,_) -> elapsed >= s && elapsed < e) with
    | Some (s,_,fn) -> let sc = fn (elapsed - s) in { sc with Elements = sc.Elements @ progressBar elapsed DURATION }
    | None -> Scene.create SKColors.Black []

// ── Render all frames ──
if Directory.Exists(OUTPUT_DIR) then Directory.Delete(OUTPUT_DIR, true)
Directory.CreateDirectory(OUTPUT_DIR) |> ignore

printfn "Rendering %d frames at %dfps (%.0fs)..." TOTAL_FRAMES FPS DURATION
let sw = Diagnostics.Stopwatch.StartNew()

for frame in 0 .. TOTAL_FRAMES - 1 do
    let elapsed = float32 frame / float32 FPS
    let scene = buildFrame elapsed
    use bmp = renderToBitmap scene
    use stream = File.OpenWrite(Path.Combine(OUTPUT_DIR, sprintf "frame_%05d.png" frame))
    bmp.Encode(stream, SKEncodedImageFormat.Png, 100) |> ignore
    if frame % 90 = 0 then
        printf "\r  Frame %d/%d (%.0f%%)" frame TOTAL_FRAMES (float frame / float TOTAL_FRAMES * 100.0)

printfn "\r  Done — %d frames in %.1fs                    " TOTAL_FRAMES sw.Elapsed.TotalSeconds

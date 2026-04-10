/// SkiaViewer Demo Reel — 1 Minute Showcase
/// A timed sequence of animated demos showing every major feature.
/// Run: dotnet fsi scripts/examples/08-demo-reel.fsx

#load "../layout-prelude.fsx"
#r "../../src/SkiaViewer.Charts/bin/Debug/net10.0/SkiaViewer.Charts.dll"
open Prelude

open System
open System.Threading
open SkiaSharp
open SkiaViewer
open SkiaViewer.Layout
open SkiaViewer.Charts

let W = 1280f
let H = 720f

let config : ViewerConfig =
    { Title = "SkiaViewer — Demo Reel"
      Width = int W; Height = int H
      TargetFps = 60
      ClearColor = SKColors.Black
      PreferredBackend = Some Backend.Vulkan }

// ── Utilities ──
let lerp a b t = a + (b - a) * t
let ease t = t * t * (3f - 2f * t) // smoothstep
let pulse t speed = (sin (t * speed) + 1f) / 2f
let pi = float32 Math.PI

let makeGradientPaint (t: float32) (colors: SKColor[]) =
    let angle = t * 360f
    let rad = angle * pi / 180f
    let cx, cy = W / 2f, H / 2f
    let dx = cos rad * 400f
    let dy = sin rad * 400f
    let shader = Shader.LinearGradient(
        SKPoint(cx - dx, cy - dy), SKPoint(cx + dx, cy + dy),
        colors, [| 0f; 0.33f; 0.66f; 1f |], TileMode.Mirror)
    Scene.fill SKColors.White |> Scene.withShader shader

// ── Title card ──
let titleOverlay (text: string) (sub: string) (alpha: float32) =
    let a = byte (255f * (min 1f (max 0f alpha)))
    if a < 5uy then []
    else [
        Scene.rect 0f 0f W H (Scene.fill (SKColor(0uy, 0uy, 0uy, byte (180f * alpha))))
        Scene.text text (W / 2f - 250f) (H / 2f - 20f) 52f (Scene.fill (SKColor(255uy, 255uy, 255uy, a)))
        Scene.text sub (W / 2f - 180f) (H / 2f + 30f) 18f (Scene.fill (SKColor(180uy, 180uy, 220uy, a)))
    ]

let progressBar (elapsed: float32) (total: float32) =
    let pct = min 1f (elapsed / total)
    [
        Scene.rect 0f (H - 4f) W 4f (Scene.fill (SKColor(40uy, 40uy, 40uy)))
        Scene.rect 0f (H - 4f) (W * pct) 4f (Scene.fill (SKColor(100uy, 180uy, 255uy)))
    ]

// ═══════════════════════════════════════════════════════════
// SCENE 1: Animated Geometry Burst (0-8s)
// ═══════════════════════════════════════════════════════════
let geometryBurst (t: float32) =
    let bg = SKColor(12uy, 12uy, 25uy)
    let elements = [
        // Orbiting circles with trails
        for i in 0..11 do
            let angle = float32 i * 30f + t * (80f + float32 i * 15f)
            let rad = angle * pi / 180f
            let orbitR = 120f + float32 i * 18f
            let cx = W / 2f + cos rad * orbitR
            let cy = H / 2f + sin rad * orbitR
            let r = 6f + pulse t (3f + float32 i * 0.5f) * 12f
            let hue = (float32 i * 30f + t * 40f) % 360f
            let color = SKColor.FromHsl(hue, 90f, 65f)
            let glowPaint = Scene.fill color |> Scene.withMaskFilter (MaskFilter.Blur(BlurStyle.Normal, 8f))
            yield Scene.circle cx cy (r + 4f) glowPaint
            yield Scene.circle cx cy r (Scene.fill color)
        // Central pulsing ring
        let ringR = 60f + pulse t 2f * 30f
        let ringPaint = Scene.stroke (SKColor(255uy, 255uy, 255uy, 120uy)) (2f + pulse t 3f * 2f)
        yield Scene.circle (W / 2f) (H / 2f) ringR ringPaint
        yield Scene.circle (W / 2f) (H / 2f) (ringR * 1.5f) (Scene.stroke (SKColor(100uy, 150uy, 255uy, 60uy)) 1f)
        // Spinning rectangles
        for i in 0..5 do
            let angle = float32 i * 60f + t * 45f
            let sz = 30f + float32 i * 8f
            let paint = Scene.stroke (SKColor.FromHsl(200f + float32 i * 20f, 80f, 70f)) 2f
            yield! Scene.rotate angle (W / 2f) (H / 2f) [
                Scene.rect (W / 2f - sz / 2f) (H / 2f - sz / 2f) sz sz paint
            ] |> List.singleton
        // Particle lines
        for i in 0..19 do
            let a = (float32 i * 18f + t * 60f) * pi / 180f
            let r1 = 200f + pulse t (1f + float32 i * 0.2f) * 40f
            let r2 = r1 + 40f + float32 i * 3f
            let x1 = W / 2f + cos a * r1
            let y1 = H / 2f + sin a * r1
            let x2 = W / 2f + cos a * r2
            let y2 = H / 2f + sin a * r2
            let alpha = byte (100 + int (pulse t (2f + float32 i * 0.3f) * 155f))
            yield Scene.line x1 y1 x2 y2 (Scene.stroke (SKColor(200uy, 220uy, 255uy, alpha)) 1.5f)
    ]
    let titleAlpha = max 0f (1f - t / 2f)
    let title = titleOverlay "SkiaViewer" "F# Declarative Rendering on Vulkan" titleAlpha
    Scene.create bg (elements @ title)

// ═══════════════════════════════════════════════════════════
// SCENE 2: Shader Showcase (8-18s)
// ═══════════════════════════════════════════════════════════
let shaderShowcase (t: float32) =
    let bg = SKColor(5uy, 5uy, 15uy)
    let elements = [
        // Animated radial gradient
        let gradColors = [| SKColor.FromHsl(t * 30f % 360f, 90f, 60f); SKColor.FromHsl((t * 30f + 120f) % 360f, 90f, 50f); SKColor.FromHsl((t * 30f + 240f) % 360f, 90f, 60f); SKColors.Black |]
        let gradShader = Shader.RadialGradient(SKPoint(300f, 360f), 200f + pulse t 1f * 80f, gradColors, [|0f; 0.3f; 0.7f; 1f|], TileMode.Clamp)
        yield Scene.circle 300f 360f 220f (Scene.fill SKColors.White |> Scene.withShader gradShader)

        // Sweep gradient clock
        let sweepColors = [| SKColors.Red; SKColors.Yellow; SKColors.Lime; SKColors.Cyan; SKColors.Blue; SKColors.Magenta; SKColors.Red |]
        let sweepPositions = [| 0f; 0.167f; 0.333f; 0.5f; 0.667f; 0.833f; 1f |]
        let sweepShader = Shader.SweepGradient(SKPoint(640f, 360f), sweepColors, sweepPositions, t * 60f % 360f, (t * 60f + 350f) % 360f + 360f)
        yield Scene.circle 640f 360f 180f (Scene.fill SKColors.White |> Scene.withShader sweepShader)
        // Inner cutout
        yield Scene.circle 640f 360f 100f (Scene.fill bg)
        yield Scene.circle 640f 360f 100f (Scene.stroke (SKColor(255uy, 255uy, 255uy, 40uy)) 1f)

        // Perlin noise turbulence animated via offset
        let freq = 0.008f + pulse t 0.5f * 0.012f
        let noiseShader = Shader.PerlinNoiseTurbulence(freq, freq * 1.5f, 4, t * 2f)
        let noisePaint = Scene.fill SKColors.White |> Scene.withShader noiseShader
        yield Scene.rect 900f 160f 320f 400f noisePaint
        yield Scene.rect 900f 160f 320f 400f (Scene.stroke (SKColor(100uy, 180uy, 255uy, 100uy)) 2f)

        // Labels
        yield Scene.text "Radial Gradient" 200f 580f 16f (Scene.fill (SKColor(180uy, 180uy, 200uy)))
        yield Scene.text "Sweep Gradient" 555f 580f 16f (Scene.fill (SKColor(180uy, 180uy, 200uy)))
        yield Scene.text "Perlin Turbulence" 950f 580f 16f (Scene.fill (SKColor(180uy, 180uy, 200uy)))

        // Conical gradient bar
        let conicalColors = [| SKColor.FromHsl(t * 50f % 360f, 100f, 50f); SKColors.White; SKColor.FromHsl((t * 50f + 180f) % 360f, 100f, 50f) |]
        let conicalShader = Shader.TwoPointConicalGradient(SKPoint(100f, 120f), 10f, SKPoint(1180f, 120f), 100f, conicalColors, [|0f; 0.5f; 1f|], TileMode.Clamp)
        yield Scene.rect 60f 90f 1160f 50f (Scene.fill SKColors.White |> Scene.withShader conicalShader |> Scene.withMaskFilter (MaskFilter.Blur(BlurStyle.Normal, 2f)))
    ]
    let titleAlpha = max 0f (1f - (t - 0f) / 1.5f)
    let title = titleOverlay "Shaders" "Gradients / Noise / Runtime Effects" titleAlpha
    Scene.create bg (elements @ title)

// ═══════════════════════════════════════════════════════════
// SCENE 3: Animated SkSL Shader (18-26s)
// ═══════════════════════════════════════════════════════════
let skslShader (t: float32) =
    let bg = SKColor(0uy, 0uy, 0uy)
    // Plasma shader via SkSL
    let plasmaSource = """
        uniform float iTime;
        uniform float iResW;
        uniform float iResH;
        half4 main(float2 fragCoord) {
            float2 uv = float2(fragCoord.x / iResW, fragCoord.y / iResH);
            float v = 0.0;
            float2 c = uv * 6.0 - 3.0;
            v += sin(c.x + iTime);
            v += sin((c.y + iTime) / 2.0);
            v += sin((c.x + c.y + iTime) / 2.0);
            float cx = c.x + sin(iTime / 3.0) * 3.0;
            float cy = c.y + cos(iTime / 2.0) * 3.0;
            v += sin(sqrt(cx*cx + cy*cy + 1.0) + iTime);
            v = v / 2.0;
            half3 col = half3(sin(v * 3.14159), sin(v * 3.14159 + 2.094), sin(v * 3.14159 + 4.189));
            col = col * 0.5 + 0.5;
            return half4(col, 1.0);
        }
    """
    let elements = [
        let shader = Shader.RuntimeEffect(plasmaSource, [("iTime", t); ("iResW", W); ("iResH", H)])
        yield Scene.rect 0f 0f W H (Scene.fill SKColors.White |> Scene.withShader shader)
        // Frosted glass overlay panel
        let panelPaint =
            Scene.fill (SKColor(0uy, 0uy, 0uy, 140uy))
            |> Scene.withMaskFilter (MaskFilter.Blur(BlurStyle.Normal, 1f))
        yield Scene.rect 40f 560f 400f 120f panelPaint
        yield Scene.rect 40f 560f 400f 120f (Scene.stroke (SKColor(255uy, 255uy, 255uy, 40uy)) 1f)
        yield Scene.text "Runtime SkSL Shader" 60f 600f 22f (Scene.fill SKColors.White)
        yield Scene.text "Animated plasma — GPU-computed per pixel" 60f 628f 14f (Scene.fill (SKColor(180uy, 200uy, 255uy)))
        yield Scene.text (sprintf "iTime = %.1f" t) 60f 656f 14f (Scene.fill (SKColor(120uy, 180uy, 120uy)))
    ]
    Scene.create bg elements

// ═══════════════════════════════════════════════════════════
// SCENE 4: Live Charting Dashboard (26-38s)
// ═══════════════════════════════════════════════════════════
let chartDashboard (t: float32) =
    let bg = SKColor(18uy, 18uy, 28uy)
    let rng = Random(42)
    let phase = t * 0.8f

    // Live line chart
    let lineData = [
        { Name = "Revenue"; Points = [ for i in 0..20 -> { X = float i; Y = 50.0 + 30.0 * sin(float i * 0.5 + float phase) + float (rng.Next(0, 10)) } ] }
        { Name = "Costs"; Points = [ for i in 0..20 -> { X = float i; Y = 30.0 + 15.0 * cos(float i * 0.4 + float phase * 0.7) + float (rng.Next(0, 8)) } ] }
    ]
    let lineConfig = { LineChart.defaultConfig 580f 280f with Title = Some "Revenue vs Costs"; BackgroundColor = Some (SKColor(25uy, 25uy, 40uy)) }
    let lineEl = LineChart.lineChart lineConfig lineData

    // Animated pie chart
    let slicePhase = int (t * 2f) % 5
    let slices = [
        { Label = "Product A"; Value = 35.0 + 10.0 * sin(float phase) }
        { Label = "Product B"; Value = 25.0 + 5.0 * cos(float phase * 1.3) }
        { Label = "Product C"; Value = 20.0 + 8.0 * sin(float phase * 0.7) }
        { Label = "Product D"; Value = 15.0 + 3.0 * cos(float phase * 1.1) }
        { Label = "Other"; Value = 5.0 + 2.0 * sin(float phase * 2.0) }
    ]
    let pieConfig = { PieChart.defaultConfig 300f 280f with Title = Some "Market Share"; DonutRatio = 0.45f; BackgroundColor = Some (SKColor(25uy, 25uy, 40uy)) }
    let pieEl = PieChart.pieChart pieConfig slices

    // Bar chart
    let barData = [
        { Category = "Q1"; Values = [ ("2024", 42.0 + 10.0 * sin(float phase)); ("2025", 55.0 + 8.0 * cos(float phase)) ] }
        { Category = "Q2"; Values = [ ("2024", 38.0 + 6.0 * cos(float phase * 0.8)); ("2025", 48.0 + 12.0 * sin(float phase * 1.2)) ] }
        { Category = "Q3"; Values = [ ("2024", 51.0 + 8.0 * sin(float phase * 1.1)); ("2025", 62.0 + 5.0 * cos(float phase)) ] }
        { Category = "Q4"; Values = [ ("2024", 45.0 + 7.0 * cos(float phase * 0.6)); ("2025", 58.0 + 9.0 * sin(float phase * 0.9)) ] }
    ]
    let barConfig = { BarChart.defaultConfig 370f 280f with Title = Some "Quarterly Revenue"; BackgroundColor = Some (SKColor(25uy, 25uy, 40uy)) }
    let barEl = BarChart.barChart barConfig BarLayout.Grouped barData

    let elements = [
        // Title bar
        Scene.rect 0f 0f W 50f (Scene.fill (SKColor(15uy, 15uy, 30uy)))
        Scene.text "Analytics Dashboard" 30f 35f 22f (Scene.fill (SKColor(200uy, 210uy, 240uy)))
        Scene.text (sprintf "Live Data — %.0fs" t) (W - 200f) 35f 14f (Scene.fill (SKColor(100uy, 200uy, 100uy)))
        // Charts laid out
        Scene.translate 20f 60f [ lineEl ]
        Scene.translate 620f 60f [ pieEl ]
        Scene.translate 890f 60f [ barEl ]
        // Bottom row — area chart
        let areaData = [
            { Name = "Users"; Points = [ for i in 0..30 -> { X = float i; Y = 100.0 + 50.0 * sin(float i * 0.3 + float phase) + 20.0 * cos(float i * 0.7 + float phase * 0.5) } ] }
            { Name = "Sessions"; Points = [ for i in 0..30 -> { X = float i; Y = 80.0 + 40.0 * cos(float i * 0.25 + float phase * 0.8) + 15.0 * sin(float i * 0.6 + float phase) } ] }
        ]
        let areaConfig = { AreaChart.defaultConfig 600f 310f with Title = Some "User Activity"; BackgroundColor = Some (SKColor(25uy, 25uy, 40uy)) }
        Scene.translate 20f 370f [ AreaChart.areaChart areaConfig areaData ]
        // Radar chart
        let radarConfig = { RadarChart.defaultConfig 320f 310f ["Speed"; "Power"; "Range"; "Armor"; "Magic"; "Luck"] with Title = Some "Character Stats"; BackgroundColor = Some (SKColor(25uy, 25uy, 40uy)); ShowGrid = true; GridLevels = 5 }
        let radarData = [
            { Name = "Warrior"; Values = [ 60.0 + 20.0 * sin(float phase); 90.0; 30.0 + 10.0 * cos(float phase); 85.0; 20.0; 50.0 + 15.0 * sin(float phase * 1.5) ] }
            { Name = "Mage"; Values = [ 30.0; 40.0 + 15.0 * cos(float phase); 60.0; 25.0; 95.0 + 5.0 * sin(float phase); 70.0 ] }
        ]
        Scene.translate 640f 370f [ RadarChart.radarChart radarConfig radarData ]
        // Scatter plot
        let scatterData = [
            { Name = "Cluster A"; Points = [ for i in 0..15 -> { X = 20.0 + 15.0 * sin(float i * 0.7 + float phase) + float i * 2.0; Y = 30.0 + 20.0 * cos(float i * 0.5 + float phase) + float i * 1.5 } ] }
        ]
        let scatterConfig = { ScatterPlot.defaultConfig 290f 310f with Title = Some "Scatter"; BackgroundColor = Some (SKColor(25uy, 25uy, 40uy)) }
        Scene.translate 975f 370f [ ScatterPlot.scatterPlot scatterConfig scatterData ]
    ]
    let titleAlpha = max 0f (1f - (t - 0f) / 1.5f)
    let title = titleOverlay "Live Charts" "9 Chart Types — Real-time Data" titleAlpha
    Scene.create bg (elements @ title)

// ═══════════════════════════════════════════════════════════
// SCENE 5: DataGrid + Candlestick (38-46s)
// ═══════════════════════════════════════════════════════════
let dataGridScene (t: float32) =
    let bg = SKColor(15uy, 15uy, 25uy)
    let scrollOffset = float (t * 30f % 200f)
    let gridConfig = { DataGrid.defaultConfig 620f 500f with
                          ScrollOffset = scrollOffset
                          HeaderColor = SKColor(40uy, 40uy, 80uy)
                          AlternateRowColor = Some (SKColor(22uy, 22uy, 38uy))
                          RowHeight = 28f
                          FontSize = 13f
                          HeaderFontSize = 14f }
    let gridData : DataGridData = {
        Columns = [
            DataGrid.textColumn "Symbol"
            DataGrid.numericColumn "Price"
            DataGrid.numericColumn "Change %"
            DataGrid.numericColumn "Volume"
            DataGrid.textColumn "Sector"
        ]
        Rows = [
            for sym, price, chg, vol, sector in [
                "AAPL", 189.5, 1.2, 52340000.0, "Tech"
                "MSFT", 378.9, -0.3, 28100000.0, "Tech"
                "GOOGL", 141.2, 0.8, 19800000.0, "Tech"
                "AMZN", 178.3, 2.1, 45600000.0, "Consumer"
                "TSLA", 248.7, -1.5, 88200000.0, "Auto"
                "NVDA", 495.2, 3.4, 61300000.0, "Semicon"
                "META", 356.1, 0.6, 17500000.0, "Tech"
                "JPM", 172.8, -0.2, 9800000.0, "Finance"
                "V", 261.4, 0.4, 7200000.0, "Finance"
                "JNJ", 158.9, -0.7, 6100000.0, "Health"
                "WMT", 162.3, 1.1, 8400000.0, "Retail"
                "PG", 151.7, 0.3, 5600000.0, "Consumer"
                "UNH", 524.1, -1.2, 3900000.0, "Health"
                "HD", 342.6, 0.9, 4200000.0, "Retail"
                "BAC", 33.8, 1.8, 38100000.0, "Finance"
                "XOM", 104.5, -0.4, 15200000.0, "Energy"
                "PFE", 28.9, -2.1, 27600000.0, "Health"
                "KO", 59.2, 0.5, 11300000.0, "Consumer"
                "DIS", 91.4, 1.6, 9700000.0, "Media"
                "NFLX", 478.3, 2.3, 6800000.0, "Media"
            ] -> [
                CellValue.TextValue sym
                CellValue.NumericValue (price + sin(float t * 2.0 + float (sym.GetHashCode())) * 3.0)
                CellValue.NumericValue (chg + sin(float t * 1.5 + float (sym.GetHashCode()) * 0.1) * 0.5)
                CellValue.NumericValue vol
                CellValue.TextValue sector
            ]
        ]
    }
    let gridEl = DataGrid.dataGrid gridConfig gridData

    // Candlestick chart
    let ohlcData = [
        for i in 0..14 ->
            let basePrice = 150.0 + 20.0 * sin(float i * 0.6 + float t * 0.5)
            let spread = 5.0 + 3.0 * abs(cos(float i * 0.8))
            let o = basePrice + float (i % 3) * 2.0
            let c = basePrice + spread * (if i % 2 = 0 then 1.0 else -1.0)
            { Label = sprintf "Day %d" (i + 1); Open = o; High = max o c + spread * 0.5; Low = min o c - spread * 0.5; Close = c }
    ]
    let candleConfig = { Candlestick.defaultConfig 600f 500f with Title = Some "Stock Price — OHLC"; BackgroundColor = Some (SKColor(25uy, 25uy, 40uy)); UpColor = SKColor(0uy, 200uy, 100uy); DownColor = SKColor(255uy, 70uy, 70uy) }
    let candleEl = Candlestick.candlestickChart candleConfig ohlcData

    let elements = [
        Scene.rect 0f 0f W 50f (Scene.fill (SKColor(15uy, 15uy, 30uy)))
        Scene.text "Financial Data" 30f 35f 22f (Scene.fill (SKColor(200uy, 210uy, 240uy)))
        Scene.translate 20f 70f [ gridEl ]
        Scene.translate 660f 70f [ candleEl ]
    ]
    let titleAlpha = max 0f (1f - t / 1.5f)
    let title = titleOverlay "DataGrid + Candlestick" "Scrolling Data / OHLC Charts" titleAlpha
    Scene.create bg (elements @ title)

// ═══════════════════════════════════════════════════════════
// SCENE 6: Graph Visualization + 3D (46-54s)
// ═══════════════════════════════════════════════════════════
let graphAndEffects (t: float32) =
    let bg = SKColor(10uy, 10uy, 20uy)

    // DAG — pre-rendered
    let dagNodeStyle =
        { SkiaViewer.Layout.Defaults.nodeStyle with
            FillColor = Some (SKColor(60uy, 80uy, 160uy))
            StrokeColor = Some (SKColor(100uy, 140uy, 220uy))
            Shape = NodeShape.RoundedRect 6f }
    let dagEdgeStyle =
        { SkiaViewer.Layout.Defaults.edgeStyle with Color = Some (SKColor(80uy, 120uy, 200uy)) }
    let dagConfig =
        { Graph.defaultConfig GraphKind.Directed with
            NodeSpacing = 35f
            LayerSpacing = 55f
            DefaultNodeStyle = dagNodeStyle
            DefaultEdgeStyle = dagEdgeStyle }
    let greenNode =
        Some { SkiaViewer.Layout.Defaults.nodeStyle with FillColor = Some (SKColor(40uy, 180uy, 100uy)); Shape = NodeShape.RoundedRect 6f }
    let blueNode =
        Some { SkiaViewer.Layout.Defaults.nodeStyle with FillColor = Some (SKColor(50uy, 130uy, 240uy)); Shape = NodeShape.RoundedRect 6f }
    let dag : GraphDefinition =
        { Config = dagConfig
          Nodes = [
              { Id = "A"; Label = "Input"; Style = greenNode }
              { Id = "B"; Label = "Parse"; Style = None }
              { Id = "C"; Label = "Analyze"; Style = None }
              { Id = "D"; Label = "Transform"; Style = None }
              { Id = "E"; Label = "Optimize"; Style = None }
              { Id = "F"; Label = "Output"; Style = blueNode }
          ]
          Edges = [
              { Source = "A"; Target = "B"; Weight = None; Label = None; Style = None }
              { Source = "A"; Target = "C"; Weight = None; Label = None; Style = None }
              { Source = "B"; Target = "D"; Weight = None; Label = None; Style = None }
              { Source = "C"; Target = "D"; Weight = None; Label = None; Style = None }
              { Source = "C"; Target = "E"; Weight = None; Label = None; Style = None }
              { Source = "D"; Target = "F"; Weight = None; Label = None; Style = None }
              { Source = "E"; Target = "F"; Weight = None; Label = None; Style = None }
          ] }

    let elements = [
        // DAG
        match Graph.render dag 580f 450f with
        | Ok graphEl ->
            let panel = Scene.group None None [
                Scene.rect 0f 0f 600f 480f (Scene.fill (SKColor(20uy, 20uy, 35uy)))
                Scene.rect 0f 0f 600f 480f (Scene.stroke (SKColor(60uy, 80uy, 140uy, 80uy)) 1f)
                Scene.text "DAG — Compiler Pipeline" 15f 24f 16f (Scene.fill (SKColor(160uy, 180uy, 220uy)))
                Scene.translate 10f 35f [ graphEl ]
            ]
            yield Scene.translate 20f 60f [ panel ]
        | _ -> ()

        // 3D perspective rotating cards
        for i in 0..2 do
            let angle = t * 40f + float32 i * 120f
            let yOffset = 160f + float32 i * 160f
            let transform3d = Transform.Perspective(
                Transform3D.Compose [
                    Transform3D.Translate(900f, yOffset, 0f)
                    Transform3D.RotateY(angle)
                ])
            let hue = float32 i * 80f + 200f
            let cardColor = SKColor.FromHsl(hue, 60f, 45f)
            let card = Scene.group (Some transform3d) None [
                Scene.rect -80f -50f 160f 100f (Scene.fill cardColor)
                Scene.rect -80f -50f 160f 100f (Scene.stroke (SKColor(255uy, 255uy, 255uy, 60uy)) 1f)
                Scene.text (sprintf "Card %d" (i + 1)) -30f 5f 16f (Scene.fill SKColors.White)
            ]
            yield card

        // Drop shadow demo
        let shadowPaint =
            Scene.fill (SKColor(80uy, 120uy, 220uy))
            |> Scene.withImageFilter (ImageFilter.DropShadow(6f, 6f, 8f, 8f, SKColor(0uy, 0uy, 0uy, 160uy)))
        let floatY = 600f + sin(t * 2f) * 15f
        yield Scene.rect 680f floatY 250f 60f shadowPaint
        yield Scene.text "Drop Shadow + Float" 700f (floatY + 38f) 16f (Scene.fill SKColors.White)

        // Title bar
        yield Scene.rect 0f 0f W 50f (Scene.fill (SKColor(10uy, 10uy, 25uy)))
        yield Scene.text "Graphs, 3D Transforms & Effects" 30f 35f 22f (Scene.fill (SKColor(200uy, 210uy, 240uy)))
    ]
    let titleAlpha = max 0f (1f - t / 1.5f)
    let title = titleOverlay "Graph Viz + Effects" "DAG Layout / 3D Perspective / Shadows" titleAlpha
    Scene.create bg (elements @ title)

// ═══════════════════════════════════════════════════════════
// SCENE 7: Grand Finale (54-60s)
// ═══════════════════════════════════════════════════════════
let finale (t: float32) =
    let bg = SKColor(5uy, 5uy, 12uy)
    let alpha = min 1f (t / 2f)
    let a = byte (255f * alpha)
    let elements = [
        // Starburst
        for i in 0..35 do
            let angle = float32 i * 10f + t * 20f
            let rad = angle * pi / 180f
            let r = 100f + float32 i * 8f + pulse t (1f + float32 i * 0.1f) * 50f
            let x = W / 2f + cos rad * r
            let y = H / 2f + sin rad * r
            let hue = (float32 i * 10f + t * 60f) % 360f
            let color = SKColor.FromHsl(hue, 90f, 65f)
            yield Scene.line (W / 2f) (H / 2f) x y (Scene.stroke (color.WithAlpha(byte (100 + int (pulse t 2f * 155f)))) 1f)
        // Central glow
        let glowColors = [| SKColor(255uy, 255uy, 255uy, 200uy); SKColor(100uy, 150uy, 255uy, 100uy); SKColor(0uy, 0uy, 0uy, 0uy) |]
        let glowShader = Shader.RadialGradient(SKPoint(W / 2f, H / 2f), 150f + pulse t 1f * 50f, glowColors, [|0f; 0.4f; 1f|], TileMode.Clamp)
        yield Scene.circle (W / 2f) (H / 2f) 200f (Scene.fill SKColors.White |> Scene.withShader glowShader)
        // Text
        yield Scene.text "SkiaViewer" (W / 2f - 160f) (H / 2f - 10f) 48f (Scene.fill (SKColor(255uy, 255uy, 255uy, a)))
        yield Scene.text "F# / .NET 10 / SkiaSharp 3 / Vulkan / Silk.NET" (W / 2f - 230f) (H / 2f + 30f) 16f (Scene.fill (SKColor(150uy, 170uy, 220uy, a)))
        yield Scene.text "Layouts / Graphs / Charts / Shaders / 3D" (W / 2f - 200f) (H / 2f + 55f) 14f (Scene.fill (SKColor(120uy, 150uy, 200uy, a)))
        // Fade to black at end
        let fadeAlpha = max 0f ((t - 4f) / 2f)
        if fadeAlpha > 0f then
            yield Scene.rect 0f 0f W H (Scene.fill (SKColor(0uy, 0uy, 0uy, byte (255f * fadeAlpha))))
    ]
    Scene.create bg elements

// ═══════════════════════════════════════════════════════════
// SEQUENCER
// ═══════════════════════════════════════════════════════════
let totalDuration = 60f

let scenes : (float32 * float32 * (float32 -> Scene))[] = [|
    (0f,  8f,  geometryBurst)
    (8f,  18f, shaderShowcase)
    (18f, 26f, skslShader)
    (26f, 38f, chartDashboard)
    (38f, 46f, dataGridScene)
    (46f, 54f, graphAndEffects)
    (54f, 60f, finale)
|]

// Pre-render graphs at startup
printfn "Pre-rendering graph scenes..."
let _ = graphAndEffects 0f
printfn "Ready."

let sceneEvent = Event<Scene>()
let mutable elapsed = 0f
let mutable running = true

let buildFrame () =
    let current =
        scenes
        |> Array.tryFind (fun (start, stop, _) -> elapsed >= start && elapsed < stop)
    match current with
    | Some (start, _, sceneFn) ->
        let localT = elapsed - start
        let scene = sceneFn localT
        { scene with Elements = scene.Elements @ progressBar elapsed totalDuration }
    | None -> Scene.create SKColors.Black [ Scene.text "Done." (W / 2f - 30f) (H / 2f) 24f (Scene.fill SKColors.White) ]

let (viewerHandle, inputs) = Viewer.run config sceneEvent.Publish

let _sub = inputs.Subscribe(fun event ->
    match event with
    | InputEvent.FrameTick dt ->
        elapsed <- elapsed + float32 dt
        if elapsed < totalDuration + 2f then
            sceneEvent.Trigger(buildFrame ())
        else
            running <- false
    | InputEvent.KeyDown key ->
        match key with
        | Silk.NET.Input.Key.Escape -> running <- false
        | Silk.NET.Input.Key.Space -> elapsed <- 0f // restart
        | _ -> ()
    | _ -> ())

sceneEvent.Trigger(buildFrame ())

printfn "SkiaViewer Demo Reel — 60 seconds"
printfn "  [Space] Restart   [Esc] Quit"
printfn "  0-8s   Animated Geometry"
printfn "  8-18s  Shader Showcase"
printfn "  18-26s SkSL Plasma Shader"
printfn "  26-38s Live Charts Dashboard"
printfn "  38-46s DataGrid + Candlestick"
printfn "  46-54s Graphs, 3D, Effects"
printfn "  54-60s Grand Finale"

while running do
    Thread.Sleep(16)

printfn "Done."

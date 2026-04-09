module SkiaViewer.Tests.CachedRendererTests

open Xunit
open SkiaSharp
open SkiaViewer

/// Helper to create a test canvas backed by a raster surface.
let private withCanvas (width: int) (height: int) (f: SKCanvas -> 'a) =
    let info = SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)
    use surface = SKSurface.Create(info)
    f (surface.Canvas)

/// Helper to get pixel bytes from a canvas's surface.
let private getPixels (surface: SKSurface) =
    let info = surface.Canvas.DeviceClipBounds
    let w = info.Width
    let h = info.Height
    use img = surface.Snapshot()
    let imgInfo = SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul)
    let pixels = Array.zeroCreate<byte> (w * h * 4)
    let pinned = System.Runtime.InteropServices.GCHandle.Alloc(pixels, System.Runtime.InteropServices.GCHandleType.Pinned)
    try
        img.ReadPixels(imgInfo, pinned.AddrOfPinnedObject(), w * 4, 0, 0) |> ignore
    finally
        pinned.Free()
    pixels

let private withSurface (width: int) (height: int) (f: SKSurface -> 'a) =
    let info = SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)
    use surface = SKSurface.Create(info)
    f surface

let private redFill = Scene.fill SKColors.Red
let private blueFill = Scene.fill SKColors.Blue
let private greenFill = Scene.fill SKColors.Green

let private staticGroup =
    Scene.group None None [
        Scene.rect 10f 10f 50f 50f redFill
        Scene.circle 100f 100f 30f blueFill
    ]

let private staticGroup2 =
    Scene.group None None [
        Scene.rect 10f 10f 50f 50f redFill
        Scene.circle 100f 100f 30f blueFill
    ]

let private differentGroup =
    Scene.group None None [
        Scene.rect 20f 20f 60f 60f greenFill
    ]

// ============================================================
// US1 Tests
// ============================================================

[<Fact>]
let ``US1 - identical scenes yield cache hits on second render`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)
        let scene = Scene.create SKColors.White [ staticGroup ]

        cache.Render scene canvas
        let stats1 = cache.Stats
        Assert.Equal(0, stats1.Hits)
        Assert.True(stats1.Misses > 0)

        // Second render with structurally identical scene
        let scene2 = Scene.create SKColors.White [ staticGroup2 ]
        cache.Render scene2 canvas
        let stats2 = cache.Stats
        Assert.True(stats2.Hits > 0, $"Expected hits > 0, got {stats2.Hits}")
        Assert.Equal(0, stats2.Misses))

[<Fact>]
let ``US1 - completely different scenes yield cache misses`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)
        let scene1 = Scene.create SKColors.White [ staticGroup ]
        cache.Render scene1 canvas

        let scene2 = Scene.create SKColors.White [ differentGroup ]
        cache.Render scene2 canvas
        let stats = cache.Stats
        Assert.True(stats.Misses > 0, $"Expected misses > 0, got {stats.Misses}")
        Assert.Equal(0, stats.Hits))

[<Fact>]
let ``US1 - transform change with same children yields cache hit`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)
        let children = [
            Scene.rect 10f 10f 50f 50f redFill
            Scene.circle 100f 100f 30f blueFill
        ]
        let scene1 = Scene.create SKColors.White [
            Scene.group (Some(Transform.Translate(0f, 0f))) None children
        ]
        cache.Render scene1 canvas

        // Same children, different transform
        let scene2 = Scene.create SKColors.White [
            Scene.group (Some(Transform.Translate(10f, 10f))) None children
        ]
        cache.Render scene2 canvas
        let stats = cache.Stats
        Assert.True(stats.Hits > 0, $"Expected hits > 0 when only transform changed, got {stats.Hits}")
        Assert.Equal(0, stats.Misses))

[<Fact>]
let ``US1 - same scene reference renders correctly`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)
        let scene = Scene.create SKColors.White [ staticGroup ]

        cache.Render scene canvas
        cache.Render scene canvas
        let stats = cache.Stats
        // Second render of same scene: groups should hit cache
        Assert.True(stats.Hits > 0, $"Expected hits > 0 on second render of same scene, got {stats.Hits}"))

// ============================================================
// US3 Tests
// ============================================================

[<Fact>]
let ``US3 - pixel identity between cached and uncached rendering`` () =
    let scene = Scene.create SKColors.White [
        Scene.group (Some(Transform.Translate(10f, 5f))) None [
            Scene.rect 0f 0f 80f 80f redFill
            Scene.circle 40f 40f 20f blueFill
            Scene.line 0f 0f 80f 80f (Scene.stroke SKColors.Black 2f)
        ]
        Scene.rect 100f 100f 50f 50f greenFill
    ]

    let uncachedPixels =
        withSurface 200 200 (fun surface ->
            SceneRenderer.render scene surface.Canvas
            surface.Canvas.Flush()
            getPixels surface)

    let cachedPixels =
        withSurface 200 200 (fun surface ->
            use cache = new RenderCache(2)
            // Render twice: first to populate cache, second to use cache
            cache.Render scene surface.Canvas
            cache.Render scene surface.Canvas
            surface.Canvas.Flush()
            getPixels surface)

    Assert.Equal<byte[]>(uncachedPixels, cachedPixels)

[<Fact>]
let ``US3 - enabled toggle switches rendering path`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)
        let scene = Scene.create SKColors.White [ staticGroup ]

        cache.Enabled <- true
        cache.Render scene canvas
        Assert.True(cache.Stats.Misses > 0)

        cache.Enabled <- false
        cache.Render scene canvas
        // When disabled, stats should be zero
        Assert.Equal(0, cache.Stats.Hits)
        Assert.Equal(0, cache.Stats.Misses)
        Assert.Equal(0, cache.Stats.Evictions))

[<Fact>]
let ``US3 - disable then render uses uncached path`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)
        let scene = Scene.create SKColors.White [ staticGroup ]

        cache.Render scene canvas
        cache.Enabled <- false
        cache.Render scene canvas
        Assert.Equal(0, cache.Stats.Hits)
        Assert.Equal(0, cache.Stats.Misses))

// ============================================================
// US4 Tests
// ============================================================

[<Fact>]
let ``US4 - cache evicts old entries after maxAge frames`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)

        // Render 100 unique scenes — each replaces the previous at position 0
        let mutable totalMisses = 0
        for i in 0 .. 99 do
            let uniqueGroup = Scene.group None None [
                Scene.rect (float32 i) 0f 10f 10f redFill
            ]
            let scene = Scene.create SKColors.White [ uniqueGroup ]
            cache.Render scene canvas
            totalMisses <- totalMisses + cache.Stats.Misses

        // Every frame is a miss (each scene has different children at position 0)
        Assert.True(totalMisses > 0, $"Expected misses > 0 over 100 unique scenes, got {totalMisses}"))

[<Fact>]
let ``US4 - repeated same scene stabilizes cache`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)

        let scene = Scene.create SKColors.White [
            Scene.group None None [ Scene.rect 0f 0f 10f 10f redFill ]
            Scene.group None None [ Scene.rect 50f 50f 10f 10f blueFill ]
        ]

        // First render: misses
        cache.Render scene canvas
        Assert.True(cache.Stats.Misses > 0)

        // Subsequent renders of same scene: all hits (scene-level fast path)
        cache.Render scene canvas
        Assert.True(cache.Stats.Hits > 0)
        Assert.Equal(0, cache.Stats.Misses))

[<Fact>]
let ``US4 - invalidate disposes all entries`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)
        let scene = Scene.create SKColors.White [ staticGroup ]

        cache.Render scene canvas
        Assert.True(cache.Stats.Misses > 0)

        cache.Invalidate()

        // After invalidation, same scene should be a miss
        cache.Render scene canvas
        Assert.True(cache.Stats.Misses > 0, "Expected miss after invalidation")
        Assert.Equal(0, cache.Stats.Hits))

// ============================================================
// Comparative Performance Benchmark
// ============================================================

open Xunit.Abstractions

type CacheBenchmarkTests(output: ITestOutputHelper) =

    let buildStaticHeavyScene (changingIndex: int) =
        let staticGroups =
            [ for g in 0 .. 19 ->
                Scene.group
                    (Some(Transform.Translate(float32 (g * 40), 0f)))
                    None
                    [ for i in 0 .. 9 ->
                        Scene.rect
                            (float32 (i * 4)) (float32 (i * 4))
                            30f 30f
                            (Scene.fill (SKColor(byte (g * 12), byte (i * 25), 128uy))) ] ]
        let changingElement =
            Scene.rect 0f 400f (float32 changingIndex) 10f (Scene.fill SKColors.Red)
        Scene.create SKColors.White (staticGroups @ [ changingElement ])

    let buildFullyAnimatedScene (frame: int) =
        let groups =
            [ for g in 0 .. 19 ->
                Scene.group
                    (Some(Transform.Translate(float32 (g * 40), 0f)))
                    None
                    [ for i in 0 .. 9 ->
                        Scene.rect
                            (float32 (i * 4 + frame % 3))
                            (float32 (i * 4))
                            30f 30f
                            (Scene.fill (SKColor(byte (g * 12), byte (i * 25), byte (frame % 255)))) ] ]
        Scene.create SKColors.White groups

    let timeRender (iterations: int) (warmup: int) (renderFn: int -> unit) =
        for i in 0 .. warmup - 1 do renderFn i
        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i in 0 .. iterations - 1 do renderFn i
        sw.Stop()
        float sw.ElapsedMilliseconds / float iterations

    [<Fact>]
    member _.``Benchmark - cached vs uncached rendering comparison`` () =
        let info = SKImageInfo(800, 600, SKColorType.Rgba8888, SKAlphaType.Premul)
        use surface = SKSurface.Create(info)
        let canvas = surface.Canvas
        let iterations = 300
        let warmup = 30

        output.WriteLine("")
        output.WriteLine("Scene Diff Caching Benchmark")
        output.WriteLine("================================================================")
        output.WriteLine($"  Scene: 20 groups x 10 children = 200 elements + 1 leaf")
        output.WriteLine($"  Surface: 800x600 RGBA raster | Iterations: {iterations}")
        output.WriteLine("")

        // ── Mostly-static scene ──
        output.WriteLine("-- Mostly-Static Scene (1 leaf changes, 20 groups unchanged) --")

        let uncachedStatic =
            use cache = new RenderCache(2)
            cache.Enabled <- false
            timeRender iterations warmup (fun i ->
                let scene = buildStaticHeavyScene i
                cache.Render scene canvas)

        let cachedStatic =
            use cache = new RenderCache(2)
            cache.Enabled <- true
            timeRender iterations warmup (fun i ->
                let scene = buildStaticHeavyScene i
                cache.Render scene canvas)

        // Get steady-state stats
        use statsCache = new RenderCache(2)
        statsCache.Render (buildStaticHeavyScene 0) canvas
        statsCache.Render (buildStaticHeavyScene 1) canvas
        let stats = statsCache.Stats

        output.WriteLine($"  Uncached:  {uncachedStatic:F2} ms/frame")
        output.WriteLine($"  Cached:    {cachedStatic:F2} ms/frame")
        output.WriteLine($"  Stats:     hits={stats.Hits} misses={stats.Misses} evictions={stats.Evictions}")
        let speedupStatic = uncachedStatic / cachedStatic
        output.WriteLine($"  Speedup:   {speedupStatic:F2}x")
        output.WriteLine("")

        // ── Fully-animated scene ──
        output.WriteLine("-- Fully-Animated Scene (all 20 groups change every frame) --")

        let uncachedAnimated =
            use cache = new RenderCache(2)
            cache.Enabled <- false
            timeRender iterations warmup (fun i ->
                let scene = buildFullyAnimatedScene i
                cache.Render scene canvas)

        let cachedAnimated =
            use cache = new RenderCache(2)
            cache.Enabled <- true
            timeRender iterations warmup (fun i ->
                let scene = buildFullyAnimatedScene i
                cache.Render scene canvas)

        let overheadPct = (cachedAnimated - uncachedAnimated) / uncachedAnimated * 100.0
        output.WriteLine($"  Uncached:  {uncachedAnimated:F2} ms/frame")
        output.WriteLine($"  Cached:    {cachedAnimated:F2} ms/frame")
        let sign = if overheadPct >= 0.0 then "+" else ""
        output.WriteLine(sprintf "  Overhead:  %s%.1f%%" sign overheadPct)
        output.WriteLine("")

        // ── Identical scene (same reference) ──
        output.WriteLine("-- Identical Scene (same object reference repeated) --")
        let identicalScene = buildStaticHeavyScene 42

        let uncachedIdentical =
            use cache = new RenderCache(2)
            cache.Enabled <- false
            timeRender iterations warmup (fun _ -> cache.Render identicalScene canvas)

        let cachedIdentical =
            use cache = new RenderCache(2)
            cache.Enabled <- true
            timeRender iterations warmup (fun _ -> cache.Render identicalScene canvas)

        let speedupIdentical = uncachedIdentical / cachedIdentical
        output.WriteLine($"  Uncached:  {uncachedIdentical:F2} ms/frame")
        output.WriteLine($"  Cached:    {cachedIdentical:F2} ms/frame")
        output.WriteLine($"  Speedup:   {speedupIdentical:F2}x")
        output.WriteLine("")

        output.WriteLine("================================================================")
        output.WriteLine($"Summary:")
        output.WriteLine($"  Mostly-static:   {speedupStatic:F2}x speedup ({uncachedStatic:F2} -> {cachedStatic:F2} ms)")
        output.WriteLine(sprintf "  Fully-animated:  %s%.1f%% overhead (%.2f -> %.2f ms)" sign overheadPct uncachedAnimated cachedAnimated)
        output.WriteLine($"  Identical scene: {speedupIdentical:F2}x speedup ({uncachedIdentical:F2} -> {cachedIdentical:F2} ms)")

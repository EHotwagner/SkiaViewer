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

        // Render 100 unique scenes
        let mutable totalEvictions = 0
        for i in 0 .. 99 do
            let uniqueGroup = Scene.group None None [
                Scene.rect (float32 i) 0f 10f 10f redFill
            ]
            let scene = Scene.create SKColors.White [ uniqueGroup ]
            cache.Render scene canvas
            totalEvictions <- totalEvictions + cache.Stats.Evictions

        Assert.True(totalEvictions > 0, $"Expected evictions > 0 over 100 unique scenes, got {totalEvictions}"))

[<Fact>]
let ``US4 - alternating scenes stabilize cache`` () =
    withCanvas 200 200 (fun canvas ->
        use cache = new RenderCache(2)

        let sceneA = Scene.create SKColors.White [
            Scene.group None None [ Scene.rect 0f 0f 10f 10f redFill ]
        ]
        let sceneB = Scene.create SKColors.White [
            Scene.group None None [ Scene.rect 0f 0f 10f 10f blueFill ]
        ]
        let sceneC = Scene.create SKColors.White [
            Scene.group None None [ Scene.rect 0f 0f 10f 10f greenFill ]
        ]

        // Warmup: cycle through all three scenes a few times
        for _ in 0 .. 9 do
            cache.Render sceneA canvas
            cache.Render sceneB canvas
            cache.Render sceneC canvas

        // After warmup, cycling should produce only hits
        cache.Render sceneA canvas
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

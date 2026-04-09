namespace SkiaViewer

open System
open System.Collections.Generic
open SkiaSharp

type internal CacheStats =
    { Hits: int
      Misses: int
      Evictions: int }

[<NoEquality; NoComparison>]
type private CacheEntry(picture: SKPicture, generation: int) =
    member _.Picture = picture
    member val Generation = generation with get, set
    member _.DisposePicture() =
        if not (isNull picture) then picture.Dispose()

[<Sealed>]
type internal RenderCache(maxAge: int) =
    // Cache key: children Element list (structural equality).
    // Transform, clip, and group paint are applied at replay time,
    // so changing only those gives a cache hit.
    let groupCache = Dictionary<Element list, CacheEntry>()
    let mutable generation = 0
    let mutable enabled = true
    let mutable lastStats = { Hits = 0; Misses = 0; Evictions = 0 }

    let recordChildren (children: Element list) : SKPicture =
        use recorder = new SKPictureRecorder()
        let bounds = SKRect(-1e6f, -1e6f, 1e6f, 1e6f)
        let recCanvas = recorder.BeginRecording(bounds)
        SceneRenderer.renderElements children recCanvas
        recorder.EndRecording()

    let sweepExpired () =
        let mutable evictions = 0
        let toRemove = ResizeArray()
        for kvp in groupCache do
            if generation - kvp.Value.Generation > maxAge then
                toRemove.Add(kvp.Key)
        for key in toRemove do
            match groupCache.TryGetValue(key) with
            | true, entry ->
                entry.DisposePicture()
                groupCache.Remove(key) |> ignore
                evictions <- evictions + 1
            | _ -> ()
        evictions

    let renderGroupCached
        (canvas: SKCanvas)
        (transform: Transform option)
        (groupPaint: Paint option)
        (clip: Clip option)
        (children: Element list)
        (hits: byref<int>)
        (misses: byref<int>)
        =
        // Look up children in cache
        let entry =
            match groupCache.TryGetValue(children) with
            | true, existing ->
                existing.Generation <- generation
                hits <- hits + 1
                existing
            | false, _ ->
                let picture = recordChildren children
                let newEntry = CacheEntry(picture, generation)
                groupCache.[children] <- newEntry
                misses <- misses + 1
                newEntry

        // Apply group state and replay cached picture
        let useLayer =
            match groupPaint with
            | Some p when p.Opacity < 1.0f -> true
            | _ -> false

        if useLayer then
            let opacity = Math.Clamp(groupPaint.Value.Opacity, 0.0f, 1.0f)
            use layerPaint = new SKPaint()
            layerPaint.Color <- SKColor(0uy, 0uy, 0uy, byte (255.0f * opacity))
            canvas.SaveLayer(layerPaint) |> ignore
        else
            canvas.Save() |> ignore

        match transform with
        | Some t ->
            let mutable matrix = SceneRenderer.toMatrix t
            canvas.Concat(&matrix)
        | None -> ()

        match clip with
        | Some c -> SceneRenderer.applyClip canvas c
        | None -> ()

        canvas.DrawPicture(entry.Picture)
        canvas.Restore()

    member _.Enabled
        with get () = enabled
        and set value = enabled <- value

    member _.Stats = lastStats

    member _.Invalidate() =
        for kvp in groupCache do
            kvp.Value.DisposePicture()
        groupCache.Clear()

    member this.Render (scene: Scene) (canvas: SKCanvas) =
        if not enabled then
            SceneRenderer.render scene canvas
            lastStats <- { Hits = 0; Misses = 0; Evictions = 0 }
        else
            generation <- generation + 1
            let mutable hits = 0
            let mutable misses = 0

            canvas.Clear(scene.BackgroundColor)

            for element in scene.Elements do
                match element with
                | Element.Group(transform, groupPaint, clip, children) ->
                    renderGroupCached canvas transform groupPaint clip children &hits &misses
                | _ ->
                    SceneRenderer.renderElements [ element ] canvas

            let evictions = sweepExpired ()
            lastStats <- { Hits = hits; Misses = misses; Evictions = evictions }

    interface IDisposable with
        member this.Dispose() = this.Invalidate()

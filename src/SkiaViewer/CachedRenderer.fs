namespace SkiaViewer

open System
open SkiaSharp

type internal CacheStats =
    { Hits: int
      Misses: int
      Evictions: int }

/// Position-indexed cache slot for a single Group element.
[<NoEquality; NoComparison>]
type private CacheSlot =
    { ChildrenRef: Element list
      Picture: SKPicture
      mutable Generation: int }
    member s.DisposePicture() =
        if not (isNull s.Picture) then s.Picture.Dispose()

[<Sealed>]
type internal RenderCache(maxAge: int) =
    let mutable slots: CacheSlot option array = Array.zeroCreate 32
    let mutable generation = 0
    let mutable enabled = true
    let mutable lastStats = { Hits = 0; Misses = 0; Evictions = 0 }
    let mutable previousScene: Scene voption = ValueNone
    let recordBounds = SKRect(0f, 0f, 4096f, 4096f)

    let ensureCapacity (needed: int) =
        if needed > slots.Length then
            let newSlots = Array.zeroCreate (max needed (slots.Length * 2))
            Array.Copy(slots, newSlots, slots.Length)
            slots <- newSlots

    let recordChildren (children: Element list) : SKPicture =
        use recorder = new SKPictureRecorder()
        let recCanvas = recorder.BeginRecording(recordBounds)
        SceneRenderer.renderElements children recCanvas
        recorder.EndRecording()

    let replaySlot (canvas: SKCanvas) (transform: Transform option) (groupPaint: Paint option) (clip: Clip option) (slot: CacheSlot) =
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

        canvas.DrawPicture(slot.Picture)
        canvas.Restore()

    let sweepExpired () =
        let mutable evictions = 0
        for i in 0 .. slots.Length - 1 do
            match slots.[i] with
            | Some slot when generation - slot.Generation > maxAge ->
                slot.DisposePicture()
                slots.[i] <- None
                evictions <- evictions + 1
            | _ -> ()
        evictions

    member _.Enabled
        with get () = enabled
        and set value = enabled <- value

    member _.Stats = lastStats

    member _.Invalidate() =
        for i in 0 .. slots.Length - 1 do
            match slots.[i] with
            | Some slot ->
                slot.DisposePicture()
                slots.[i] <- None
            | None -> ()
        previousScene <- ValueNone

    member this.Render (scene: Scene) (canvas: SKCanvas) =
        if not enabled then
            SceneRenderer.render scene canvas
            lastStats <- { Hits = 0; Misses = 0; Evictions = 0 }
        else
            generation <- generation + 1

            // Scene-level reference-equality fast path
            match previousScene with
            | ValueSome prev when Object.ReferenceEquals(prev, scene) ->
                canvas.Clear(scene.BackgroundColor)
                let mutable hits = 0
                let elements = scene.Elements
                let mutable idx = 0
                for element in elements do
                    match element with
                    | Element.Group(transform, groupPaint, clip, _children) ->
                        match slots.[idx] with
                        | Some slot ->
                            slot.Generation <- generation
                            replaySlot canvas transform groupPaint clip slot
                            hits <- hits + 1
                        | None ->
                            // Slot was evicted — re-render directly
                            SceneRenderer.renderElements [ element ] canvas
                    | _ ->
                        SceneRenderer.renderElements [ element ] canvas
                    idx <- idx + 1
                lastStats <- { Hits = hits; Misses = 0; Evictions = 0 }
            | _ ->
                // Per-element comparison path
                let elements = scene.Elements
                let elementCount = elements.Length
                ensureCapacity elementCount

                canvas.Clear(scene.BackgroundColor)
                let mutable hits = 0
                let mutable misses = 0
                let mutable idx = 0

                for element in elements do
                    match element with
                    | Element.Group(transform, groupPaint, clip, children) ->
                        let cached =
                            match slots.[idx] with
                            | Some slot ->
                                // Fast: reference equality on children list
                                if Object.ReferenceEquals(children, slot.ChildrenRef) then
                                    Some slot
                                // Slow: structural equality fallback
                                elif children = slot.ChildrenRef then
                                    Some { slot with ChildrenRef = children }
                                else
                                    None
                            | None -> None

                        match cached with
                        | Some slot ->
                            slot.Generation <- generation
                            slots.[idx] <- Some slot
                            replaySlot canvas transform groupPaint clip slot
                            hits <- hits + 1
                        | None ->
                            // Dispose old slot if present (counts as eviction)
                            match slots.[idx] with
                            | Some old ->
                                old.DisposePicture()
                            | None -> ()
                            // Record new
                            let picture = recordChildren children
                            let newSlot = { ChildrenRef = children; Picture = picture; Generation = generation }
                            slots.[idx] <- Some newSlot
                            replaySlot canvas transform groupPaint clip newSlot
                            misses <- misses + 1
                    | _ ->
                        SceneRenderer.renderElements [ element ] canvas
                    idx <- idx + 1

                let evictions = sweepExpired ()
                previousScene <- ValueSome scene
                lastStats <- { Hits = hits; Misses = misses; Evictions = evictions }

    interface IDisposable with
        member this.Dispose() = this.Invalidate()

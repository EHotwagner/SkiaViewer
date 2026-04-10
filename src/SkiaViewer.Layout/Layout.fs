namespace SkiaViewer.Layout

open SkiaViewer

module Layout =

    let private clampSize (sizing: LayoutSizing) (value: float32) : float32 =
        let v =
            match sizing.MinWidth with
            | Some min when value < min -> min
            | _ -> value
        match sizing.MaxWidth with
        | Some max when v > max -> max
        | _ -> v

    let private clampHeight (sizing: LayoutSizing) (value: float32) : float32 =
        let v =
            match sizing.MinHeight with
            | Some min when value < min -> min
            | _ -> value
        match sizing.MaxHeight with
        | Some max when v > max -> max
        | _ -> v

    let private resolveChildWidth (sizing: LayoutSizing) (available: float32) : float32 =
        let desired =
            match sizing.DesiredWidth with
            | Some w -> w
            | None -> available
        clampSize sizing desired

    let private resolveChildHeight (sizing: LayoutSizing) (available: float32) : float32 =
        let desired =
            match sizing.DesiredHeight with
            | Some h -> h
            | None -> available
        clampHeight sizing desired

    let private alignH (align: HorizontalAlignment) (childW: float32) (available: float32) : float32 =
        match align with
        | HorizontalAlignment.Left -> 0f
        | HorizontalAlignment.Center -> (available - childW) / 2f
        | HorizontalAlignment.Right -> available - childW
        | HorizontalAlignment.Stretch -> 0f

    let private alignV (align: VerticalAlignment) (childH: float32) (available: float32) : float32 =
        match align with
        | VerticalAlignment.Top -> 0f
        | VerticalAlignment.Center -> (available - childH) / 2f
        | VerticalAlignment.Bottom -> available - childH
        | VerticalAlignment.Stretch -> 0f

    let child (element: Element) : LayoutChild =
        { Element = element
          Sizing = Defaults.sizing
          HAlign = HorizontalAlignment.Left
          VAlign = VerticalAlignment.Top }

    let childWithSize (width: float32) (height: float32) (element: Element) : LayoutChild =
        { Element = element
          Sizing = { Defaults.sizing with DesiredWidth = Some width; DesiredHeight = Some height }
          HAlign = HorizontalAlignment.Left
          VAlign = VerticalAlignment.Top }

    let dockChild (position: DockPosition) (element: Element) : DockChild =
        { Element = element
          Dock = position
          Sizing = Defaults.sizing }

    let hstack (config: StackConfig) (children: LayoutChild list) (width: float32) (height: float32) : Element =
        let pad = config.Padding
        let innerW = width - pad.Left - pad.Right
        let innerH = height - pad.Top - pad.Bottom
        let totalSpacing = if children.Length > 1 then config.Spacing * float32 (children.Length - 1) else 0f
        let availW = innerW - totalSpacing

        // Measure children
        let childWidths =
            children
            |> List.map (fun c -> resolveChildWidth c.Sizing (availW / float32 (max children.Length 1)))

        let mutable x = pad.Left
        let elements =
            children
            |> List.mapi (fun i c ->
                let cw = childWidths.[i]
                let ch =
                    if c.VAlign = VerticalAlignment.Stretch then innerH
                    else resolveChildHeight c.Sizing innerH
                let dy = pad.Top + alignV c.VAlign ch innerH
                let element = Scene.translate x dy [ c.Element ]
                x <- x + cw + config.Spacing
                element)

        Scene.group None None elements

    let vstack (config: StackConfig) (children: LayoutChild list) (width: float32) (height: float32) : Element =
        let pad = config.Padding
        let innerW = width - pad.Left - pad.Right
        let innerH = height - pad.Top - pad.Bottom
        let totalSpacing = if children.Length > 1 then config.Spacing * float32 (children.Length - 1) else 0f
        let availH = innerH - totalSpacing

        let childHeights =
            children
            |> List.map (fun c -> resolveChildHeight c.Sizing (availH / float32 (max children.Length 1)))

        let mutable y = pad.Top
        let elements =
            children
            |> List.mapi (fun i c ->
                let ch = childHeights.[i]
                let cw =
                    if c.HAlign = HorizontalAlignment.Stretch then innerW
                    else resolveChildWidth c.Sizing innerW
                let dx = pad.Left + alignH c.HAlign cw innerW
                let element = Scene.translate dx y [ c.Element ]
                y <- y + ch + config.Spacing
                element)

        Scene.group None None elements

    let dock (config: DockConfig) (children: DockChild list) (width: float32) (height: float32) : Element =
        let pad = config.Padding
        let mutable left = pad.Left
        let mutable top = pad.Top
        let mutable right = width - pad.Right
        let mutable bottom = height - pad.Bottom

        let elements =
            children
            |> List.mapi (fun i c ->
                let isFill =
                    c.Dock = DockPosition.Fill ||
                    (config.LastChildFill && i = children.Length - 1)

                if isFill then
                    let el = Scene.translate left top [ c.Element ]
                    el
                else
                    match c.Dock with
                    | DockPosition.Top ->
                        let ch = resolveChildHeight c.Sizing (bottom - top)
                        let el = Scene.translate left top [ c.Element ]
                        top <- top + ch
                        el
                    | DockPosition.Bottom ->
                        let ch = resolveChildHeight c.Sizing (bottom - top)
                        bottom <- bottom - ch
                        let el = Scene.translate left bottom [ c.Element ]
                        el
                    | DockPosition.Left ->
                        let cw = resolveChildWidth c.Sizing (right - left)
                        let el = Scene.translate left top [ c.Element ]
                        left <- left + cw
                        el
                    | DockPosition.Right ->
                        let cw = resolveChildWidth c.Sizing (right - left)
                        right <- right - cw
                        let el = Scene.translate right top [ c.Element ]
                        el
                    | DockPosition.Fill ->
                        Scene.translate left top [ c.Element ])

        Scene.group None None elements

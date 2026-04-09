namespace SkiaViewer.Charts

open System

module internal Axis =

    let niceNumber (range: float) (round: bool) : float =
        if range <= 0.0 then 1.0
        else
            let exponent = floor (log10 range)
            let fraction = range / (10.0 ** exponent)
            let niceFraction =
                if round then
                    if fraction < 1.5 then 1.0
                    elif fraction < 3.0 then 2.0
                    elif fraction < 7.0 then 5.0
                    else 10.0
                else
                    if fraction <= 1.0 then 1.0
                    elif fraction <= 2.0 then 2.0
                    elif fraction <= 5.0 then 5.0
                    else 10.0
            niceFraction * (10.0 ** exponent)

    let computeAxisTicks (dataMin: float) (dataMax: float) (tickCount: int) : float * float * (float * string) list =
        let tickCount = max 2 tickCount
        if Double.IsNaN dataMin || Double.IsNaN dataMax || Double.IsInfinity dataMin || Double.IsInfinity dataMax then
            (0.0, 1.0, [ (0.0, "0"); (1.0, "1") ])
        elif abs (dataMax - dataMin) < 1e-10 then
            let center = dataMin
            let half = if abs center < 1e-10 then 0.5 else abs center * 0.1
            let lo = center - half
            let hi = center + half
            let step = (hi - lo) / float (tickCount - 1)
            let ticks = [ for i in 0 .. tickCount - 1 -> let v = lo + float i * step in (v, sprintf "%.1f" v) ]
            (lo, hi, ticks)
        else
            let range = niceNumber (dataMax - dataMin) false
            let tickSpacing = niceNumber (range / float (tickCount - 1)) true
            let niceMin = floor (dataMin / tickSpacing) * tickSpacing
            let niceMax = ceil (dataMax / tickSpacing) * tickSpacing
            let mutable ticks = []
            let mutable v = niceMin
            while v <= niceMax + tickSpacing * 0.5 do
                let label =
                    if abs v < 1e-10 then "0"
                    elif abs tickSpacing >= 1.0 then sprintf "%.0f" v
                    elif abs tickSpacing >= 0.1 then sprintf "%.1f" v
                    else sprintf "%.2f" v
                ticks <- ticks @ [ (v, label) ]
                v <- v + tickSpacing
            (niceMin, niceMax, ticks)

    let computeAutoRange (values: float list) : float * float =
        let valid = values |> List.filter (fun v -> not (Double.IsNaN v) && not (Double.IsInfinity v))
        match valid with
        | [] -> (0.0, 1.0)
        | [ single ] ->
            let half = if abs single < 1e-10 then 0.5 else abs single * 0.1
            (single - half, single + half)
        | _ ->
            let lo = List.min valid
            let hi = List.max valid
            if abs (hi - lo) < 1e-10 then
                let half = if abs lo < 1e-10 then 0.5 else abs lo * 0.1
                (lo - half, lo + half)
            else
                (lo, hi)

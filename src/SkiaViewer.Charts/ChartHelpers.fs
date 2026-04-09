namespace SkiaViewer.Charts

open SkiaSharp
open SkiaViewer

type internal ChartArea =
    { Left: float32
      Top: float32
      Right: float32
      Bottom: float32 }

module internal ChartHelpers =

    let computeChartArea (width: float32) (height: float32) (padding: float32) (hasTitle: bool) (titleFontSize: float32) (hasLegend: bool) : ChartArea =
        let top = padding + (if hasTitle then titleFontSize + 10.0f else 0.0f)
        let bottom = height - padding - (if hasLegend then 30.0f else 0.0f)
        { Left = padding + 40.0f  // extra room for Y axis labels
          Top = top
          Right = width - padding
          Bottom = bottom }

    let renderTitle (title: string) (fontSize: float32) (width: float32) : Element =
        let x = width / 2.0f
        let y = fontSize + 5.0f
        Scene.text title x y fontSize (Scene.fill SKColors.Black)

    let mapX (value: float) (dataMin: float) (dataMax: float) (area: ChartArea) : float32 =
        let range = dataMax - dataMin
        if abs range < 1e-10 then (area.Left + area.Right) / 2.0f
        else area.Left + float32 ((value - dataMin) / range) * (area.Right - area.Left)

    let mapY (value: float) (dataMin: float) (dataMax: float) (area: ChartArea) : float32 =
        let range = dataMax - dataMin
        if abs range < 1e-10 then (area.Top + area.Bottom) / 2.0f
        else area.Bottom - float32 ((value - dataMin) / range) * (area.Bottom - area.Top)

    let paletteColor (palette: ColorPalette) (index: int) : SKColor =
        match palette.Colors with
        | [] -> SKColors.Black
        | colors -> colors[index % colors.Length]

    let private axisLinePaint = Scene.stroke SKColors.Black 1.0f
    let private tickPaint = Scene.stroke SKColors.Black 1.0f
    let private labelPaint = Scene.fill SKColors.Black

    let renderXAxis (area: ChartArea) (ticks: (float * string) list) (axisMin: float) (axisMax: float) (label: string option) : Element list =
        let axisLine = Scene.line area.Left area.Bottom area.Right area.Bottom axisLinePaint
        let tickElements =
            ticks |> List.collect (fun (value, text) ->
                let x = mapX value axisMin axisMax area
                [ Scene.line x area.Bottom x (area.Bottom + 5.0f) tickPaint
                  Scene.text text x (area.Bottom + 18.0f) 10.0f labelPaint ])
        let labelElements =
            match label with
            | Some lbl ->
                let x = (area.Left + area.Right) / 2.0f
                [ Scene.text lbl x (area.Bottom + 32.0f) 12.0f labelPaint ]
            | None -> []
        axisLine :: tickElements @ labelElements

    let renderYAxis (area: ChartArea) (ticks: (float * string) list) (axisMin: float) (axisMax: float) (label: string option) : Element list =
        let axisLine = Scene.line area.Left area.Top area.Left area.Bottom axisLinePaint
        let tickElements =
            ticks |> List.collect (fun (value, text) ->
                let y = mapY value axisMin axisMax area
                [ Scene.line (area.Left - 5.0f) y area.Left y tickPaint
                  Scene.text text (area.Left - 8.0f) (y + 4.0f) 10.0f labelPaint ])
        let labelElements =
            match label with
            | Some lbl ->
                let y = (area.Top + area.Bottom) / 2.0f
                [ Scene.text lbl 5.0f y 12.0f labelPaint ]
            | None -> []
        axisLine :: tickElements @ labelElements

    let renderGridLines (area: ChartArea) (xTicks: (float * string) list) (yTicks: (float * string) list) (xMin: float) (xMax: float) (yMin: float) (yMax: float) (gridColor: SKColor) : Element list =
        let gridPaint = { Scene.stroke gridColor 0.5f with Opacity = 0.5f }
        let vLines =
            xTicks |> List.map (fun (value, _) ->
                let x = mapX value xMin xMax area
                Scene.line x area.Top x area.Bottom gridPaint)
        let hLines =
            yTicks |> List.map (fun (value, _) ->
                let y = mapY value yMin yMax area
                Scene.line area.Left y area.Right y gridPaint)
        vLines @ hLines

    let renderLegend (names: string list) (palette: ColorPalette) (position: LegendPosition) (width: float32) (height: float32) (area: ChartArea) : Element =
        let itemWidth = 80.0f
        let totalWidth = float32 names.Length * itemWidth
        let startX = (width - totalWidth) / 2.0f
        let y =
            match position with
            | LegendPosition.Bottom -> area.Bottom + 20.0f
            | LegendPosition.Top -> area.Top - 20.0f
            | _ -> area.Bottom + 20.0f
        let items =
            names |> List.mapi (fun i name ->
                let x = startX + float32 i * itemWidth
                let color = paletteColor palette i
                [ Scene.rect x (y - 5.0f) 12.0f 12.0f (Scene.fill color)
                  Scene.text name (x + 16.0f) (y + 5.0f) 10.0f labelPaint ])
            |> List.concat
        Scene.group None None items

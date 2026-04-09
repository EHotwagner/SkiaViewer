namespace SkiaViewer.Charts

open SkiaSharp
open SkiaViewer
open SkiaViewer.Charts

module DataGrid =

    let defaultConfig (width: float32) (height: float32) : DataGridConfig =
        Defaults.dataGridConfig width height

    let textColumn (name: string) : ColumnDef =
        { Name = name; Type = ColumnType.Text; Sortable = true; MinWidth = None }

    let numericColumn (name: string) : ColumnDef =
        { Name = name; Type = ColumnType.Numeric; Sortable = true; MinWidth = None }

    let boolColumn (name: string) : ColumnDef =
        { Name = name; Type = ColumnType.Boolean; Sortable = true; MinWidth = None }

    let sortRows (columns: ColumnDef list) (columnIndex: int) (direction: SortDirection) (rows: CellValue list list) : CellValue list list =
        match direction with
        | SortDirection.None -> rows
        | _ when columnIndex < 0 || columnIndex >= columns.Length -> rows
        | _ ->
            let compareCell (a: CellValue) (b: CellValue) =
                match a, b with
                | CellValue.TextValue sa, CellValue.TextValue sb -> compare sa sb
                | CellValue.NumericValue na, CellValue.NumericValue nb -> compare na nb
                | CellValue.BoolValue ba, CellValue.BoolValue bb -> compare ba bb
                | _ -> 0

            let sorted =
                rows
                |> List.sortWith (fun rowA rowB ->
                    let cellA = rowA |> List.item columnIndex
                    let cellB = rowB |> List.item columnIndex
                    compareCell cellA cellB)

            match direction with
            | SortDirection.Ascending -> sorted
            | SortDirection.Descending -> List.rev sorted
            | SortDirection.None -> rows

    let visibleRange (config: DataGridConfig) (totalRows: int) : int * int =
        if totalRows <= 0 then
            (0, -1)
        else
            let bodyHeight = config.Height - config.HeaderHeight
            let visibleCount = int (ceil (bodyHeight / config.RowHeight)) + 1
            let startIndex = int (config.ScrollOffset / float config.RowHeight)
            let startIndex = max 0 (min startIndex (totalRows - 1))
            let endIndex = min (startIndex + visibleCount - 1) (totalRows - 1)
            (startIndex, endIndex)

    let dataGrid (config: DataGridConfig) (data: DataGridData) : Element =
        let columns = data.Columns
        let rows = data.Rows

        if columns.IsEmpty then
            Scene.group None None []
        else
            // Sort rows if sort state is set
            let sortedRows =
                match config.Sort with
                | Some sort -> sortRows columns sort.ColumnIndex sort.Direction rows
                | None -> rows

            let colCount = columns.Length
            let colWidth = config.Width / float32 colCount

            // --- Header ---
            let headerElements = ResizeArray<Element>()

            // Header background
            headerElements.Add(
                Scene.rect 0.0f 0.0f config.Width config.HeaderHeight (Scene.fill config.HeaderColor))

            // Header column labels
            columns |> List.iteri (fun i col ->
                let x = float32 i * colWidth + 4.0f
                let y = config.HeaderHeight / 2.0f + config.HeaderFontSize / 2.0f - 2.0f
                headerElements.Add(
                    Scene.text col.Name x y config.HeaderFontSize (Scene.fill SKColors.Black)))

            let headerGroup = Scene.group None None (Seq.toList headerElements)

            // --- Body ---
            let totalRows = sortedRows.Length
            let startIndex, endIndex = visibleRange config totalRows

            let bodyElements = ResizeArray<Element>()

            if totalRows > 0 then
                for rowIdx in startIndex .. endIndex do
                    let row = sortedRows.[rowIdx]
                    let yPos = config.HeaderHeight + float32 (rowIdx - startIndex) * config.RowHeight

                    // Alternate row background
                    if rowIdx % 2 = 1 then
                        match config.AlternateRowColor with
                        | Some altColor ->
                            bodyElements.Add(
                                Scene.rect 0.0f yPos config.Width config.RowHeight (Scene.fill altColor))
                        | None -> ()

                    // Render cells
                    row |> List.iteri (fun colIdx cell ->
                        let colX = float32 colIdx * colWidth
                        let cellY = yPos + config.RowHeight / 2.0f + config.FontSize / 2.0f - 2.0f

                        match cell with
                        | CellValue.TextValue s ->
                            bodyElements.Add(
                                Scene.text s (colX + 4.0f) cellY config.FontSize (Scene.fill SKColors.Black))

                        | CellValue.NumericValue v ->
                            let formatted = sprintf "%.2f" v
                            let roughWidth = float32 formatted.Length * config.FontSize * 0.55f
                            let xPos = colX + colWidth - roughWidth - 4.0f
                            bodyElements.Add(
                                Scene.text formatted xPos cellY config.FontSize (Scene.fill SKColors.Black))

                        | CellValue.BoolValue b ->
                            let boxX = colX + colWidth / 2.0f - 5.0f
                            let boxY = yPos + config.RowHeight / 2.0f - 5.0f
                            if b then
                                bodyElements.Add(
                                    Scene.rect boxX boxY 10.0f 10.0f (Scene.fill (SKColor(0x33uy, 0x33uy, 0x33uy))))
                            else
                                bodyElements.Add(
                                    Scene.rect boxX boxY 10.0f 10.0f (Scene.stroke SKColors.Black 1.0f)))

                    // Row separator line
                    let lineY = yPos + config.RowHeight
                    bodyElements.Add(
                        Scene.line 0.0f lineY config.Width lineY (Scene.stroke (SKColor(0xD0uy, 0xD0uy, 0xD0uy)) 1.0f))

            // Clip body to area below header
            let bodyClipRect =
                SKRect(0.0f, config.HeaderHeight, config.Width, config.Height)

            let clippedBody =
                Scene.groupWithClip None None
                    (Clip.Rect(bodyClipRect, ClipOperation.Intersect, true))
                    (Seq.toList bodyElements)

            Scene.group None None [ headerGroup; clippedBody ]

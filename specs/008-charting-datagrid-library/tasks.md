# Tasks: Charting & DataGrid Library

**Input**: Design documents from `/specs/008-charting-datagrid-library/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Included per constitution requirement III (test evidence mandatory for behavior-changing code).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and solution structure

- [x] T001 Create `src/SkiaViewer.Charts/SkiaViewer.Charts.fsproj` with net10.0, project reference to SkiaViewer, IsPackable=true, PackageId=SkiaViewer.Charts, InternalsVisibleTo for test project. Add all .fsi/.fs file pairs to `<Compile>` item list in dependency order: Types → Axis → ChartHelpers → LineChart → BarChart → PieChart → ScatterPlot → AreaChart → Histogram → Candlestick → RadarChart → DataGrid
- [x] T002 Create `tests/SkiaViewer.Charts.Tests/SkiaViewer.Charts.Tests.fsproj` with xUnit dependencies and project reference to SkiaViewer.Charts
- [x] T003 Update `SkiaViewer.slnx` to include both new projects under `/src/` and `/tests/` folders

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core types, axis computation, and shared rendering helpers that ALL chart stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Create `src/SkiaViewer.Charts/Types.fsi` — define all public types: ColorPalette, LegendPosition, LegendConfig, AxisConfig, ChartConfig, BarLayout (Grouped | Stacked), DataPoint, DataSeries, CategoryValue, SliceData, PieConfig, OhlcData, CandlestickConfig, RadarSeries, RadarConfig, HistogramConfig, ColumnType, ColumnDef, CellValue, SortDirection, SortState, DataGridConfig, DataGridData
- [x] T005 Create `src/SkiaViewer.Charts/Types.fs` — implement all types from Types.fsi with default values, Tableau-10 color palette, and BarLayout DU (Grouped | Stacked, default Grouped)
- [x] T006 Create `src/SkiaViewer.Charts/Axis.fsi` — define internal module: niceNumber, computeAxisTicks, computeAutoRange
- [x] T007 Create `src/SkiaViewer.Charts/Axis.fs` — implement nice-number algorithm for tick generation, auto-range computation from data bounds
- [x] T008 Create `src/SkiaViewer.Charts/ChartHelpers.fsi` — define internal module: renderAxis, renderTitle, renderLegend, renderGridLines, mapToChartArea
- [x] T009 Create `src/SkiaViewer.Charts/ChartHelpers.fs` — implement shared rendering helpers that produce Element lists (axes as Line+Text elements, title as Text element, legend as Group of Rect+Text, grid lines as Line elements)
- [x] T010 [P] Create `tests/SkiaViewer.Charts.Tests/AxisTests.fs` — test nice-number algorithm (e.g., range 0-97 → ticks at 0,20,40,60,80,100), auto-range with negative values, single-value range, empty data range
- [x] T011 Verify foundational build: `dotnet build src/SkiaViewer.Charts` and `dotnet build tests/SkiaViewer.Charts.Tests` compile successfully

**Checkpoint**: Foundation ready — chart and DataGrid implementation can now begin

---

## Phase 3: User Story 1 — Line Chart (Priority: P1) MVP

**Goal**: Render one or more data series as connected line segments with axes, grid lines, labels, and legend

**Independent Test**: Create a line chart with sample data points and verify it renders correctly with axes, labels, grid lines, and data lines visible on screen

- [x] T012 [P] [US1] Create `tests/SkiaViewer.Charts.Tests/LineChartTests.fs` — test single series renders Line elements, multi-series uses distinct palette colors, empty data renders axes only, NaN points are skipped, resize produces proportional output
- [x] T013 [US1] Create `src/SkiaViewer.Charts/LineChart.fsi` — define `LineChart.lineChart` and `LineChart.defaultConfig`
- [x] T014 [US1] Implement `LineChart.lineChart` in `src/SkiaViewer.Charts/LineChart.fs` — compute axis ranges from data, render axes/grid/title/legend via ChartHelpers, map data points to canvas coordinates, connect points with Line elements, wrap all in a Group element
- [x] T015 [US1] Handle edge cases in lineChart: empty DataSeries list (render axes only), NaN/infinity Y values (skip point), single data point (render as dot)

**Checkpoint**: Line chart is fully functional and independently testable

---

## Phase 4: User Story 2 — Bar Chart (Priority: P1)

**Goal**: Render categorical data as vertical bars with support for grouped layout

**Independent Test**: Create a bar chart with category-value pairs and verify bars render at correct heights with proper labels

- [x] T016 [P] [US2] Create `tests/SkiaViewer.Charts.Tests/BarChartTests.fs` — test single series bar heights proportional to values, grouped bars render side-by-side with distinct colors, stacked bars accumulate heights within each category, empty categories handled, legend shows series names
- [x] T017 [US2] Create `src/SkiaViewer.Charts/BarChart.fsi` — define `BarChart.barChart` and `BarChart.defaultConfig`
- [x] T018 [US2] Implement `BarChart.barChart` in `src/SkiaViewer.Charts/BarChart.fs` — compute category positions, bar widths for grouping, auto-scale Y axis, render bars as Rect elements with fill colors from palette, category labels as Text on X axis; support stacked layout (BarLayout.Stacked: series values stack vertically within each category with cumulative Y offset)
- [x] T019 [US2] Handle edge cases in barChart: empty CategoryValue list, single category, zero-value bars (render zero-height)

**Checkpoint**: Bar chart is fully functional and independently testable

---

## Phase 5: User Story 6 — DataGrid (Priority: P1)

**Goal**: Render tabular data with column headers, virtual scrolling, and column sorting

**Independent Test**: Provide column definitions and row data, verify the grid renders headers, rows, and supports scrolling through data

- [x] T020 [P] [US6] Create `tests/SkiaViewer.Charts.Tests/DataGridTests.fs` — test header rendering with column names, row rendering with correct alignment per type (text left, numeric right, boolean checkbox), sorting ascending/descending, virtual scrolling visible range computation, empty rows show header only, fixed header during scroll
- [x] T021 [US6] Create `src/SkiaViewer.Charts/DataGrid.fsi` — define public module: dataGrid, defaultConfig, textColumn, numericColumn, boolColumn, sortRows, visibleRange
- [x] T022 [US6] Implement `DataGrid.sortRows` and `DataGrid.visibleRange` in `src/SkiaViewer.Charts/DataGrid.fs` — sort by column index with type-aware comparison (text: string, numeric: float, boolean: false<true), compute start/end row indices from scrollOffset and rowHeight
- [x] T023 [US6] Implement `DataGrid.dataGrid` in `src/SkiaViewer.Charts/DataGrid.fs` — render header row (Rect background + Text per column), compute auto-fit column widths from available width, render visible rows only (virtual scrolling), dispatch cell rendering by ColumnType (TextValue→left-aligned Text, NumericValue→right-aligned Text, BoolValue→filled/empty square), alternating row backgrounds, fixed header as separate Group above scrollable body clipped to remaining height
- [x] T024 [US6] Implement `DataGrid.textColumn`, `DataGrid.numericColumn`, `DataGrid.boolColumn` helper constructors in `src/SkiaViewer.Charts/DataGrid.fs`

**Checkpoint**: DataGrid is fully functional with sorting and virtual scrolling

---

## Phase 6: User Story 3 — Pie/Donut Chart (Priority: P2)

**Goal**: Render proportional data as arc segments with donut option

**Independent Test**: Create a pie chart with labeled value slices and verify arcs are proportionally sized and labeled

- [x] T025 [P] [US3] Create `tests/SkiaViewer.Charts.Tests/PieChartTests.fs` — test slice angles proportional to values, donut mode renders hollow center, all-zero values render empty circle, single slice renders full circle, labels rendered
- [x] T026 [US3] Create `src/SkiaViewer.Charts/PieChart.fsi` — define `PieChart.pieChart` and `PieChart.defaultConfig`
- [x] T027 [US3] Implement `PieChart.pieChart` in `src/SkiaViewer.Charts/PieChart.fs` — compute slice angles from value proportions, render each slice as Arc element with useCenter=true, apply palette colors, render labels at midpoint of each arc, donut mode: overlay a filled circle in background color at center with radius = outerRadius * donutRatio
- [x] T028 [US3] Handle edge cases in pieChart: all values zero (render "no data" text), single slice (full 360-degree arc), empty slices list

**Checkpoint**: Pie/donut chart is fully functional

---

## Phase 7: User Story 4 — Scatter Plot (Priority: P2)

**Goal**: Render individual data points on a two-dimensional plane with distinct markers per series

**Independent Test**: Provide coordinate data and verify points appear at correct positions with axes

- [x] T029 [P] [US4] Create `tests/SkiaViewer.Charts.Tests/ScatterPlotTests.fs` — test points positioned correctly, multi-series uses distinct colors, empty series renders axes only
- [x] T030 [US4] Create `src/SkiaViewer.Charts/ScatterPlot.fsi` — define `ScatterPlot.scatterPlot` and `ScatterPlot.defaultConfig`
- [x] T031 [US4] Implement `ScatterPlot.scatterPlot` in `src/SkiaViewer.Charts/ScatterPlot.fs` — auto-scale both axes from data, render each point as a small filled Ellipse element at mapped coordinates, use palette colors per series, render axes/grid/legend via ChartHelpers

**Checkpoint**: Scatter plot is fully functional

---

## Phase 8: User Story 5 — Area Chart (Priority: P2)

**Goal**: Render filled regions under data lines with stacking support

**Independent Test**: Provide data series and verify filled region renders below data line

- [x] T032 [P] [US5] Create `tests/SkiaViewer.Charts.Tests/AreaChartTests.fs` — test filled area rendered as Path element, stacked series accumulate Y values, empty series renders axes only
- [x] T033 [US5] Create `src/SkiaViewer.Charts/AreaChart.fsi` — define `AreaChart.areaChart` and `AreaChart.defaultConfig`
- [x] T034 [US5] Implement `AreaChart.areaChart` in `src/SkiaViewer.Charts/AreaChart.fs` — like lineChart but build a closed Path (line across data points, then down to baseline, across baseline, close) with semi-transparent fill, for stacked mode: accumulate Y values across series so each series baseline is the previous series top

**Checkpoint**: Area chart is fully functional

---

## Phase 9: User Story 7 — Histogram (Priority: P3)

**Goal**: Group raw data into bins and display frequency bars

**Independent Test**: Provide dataset and bin count, verify correct number of bars with proportional heights

- [x] T035 [P] [US7] Create `tests/SkiaViewer.Charts.Tests/HistogramTests.fs` — test correct bin count, bin boundaries, frequency heights proportional, empty data handled, single value
- [x] T036 [US7] Create `src/SkiaViewer.Charts/Histogram.fsi` — define `Histogram.histogram` and `Histogram.defaultConfig`
- [x] T037 [US7] Implement `Histogram.histogram` in `src/SkiaViewer.Charts/Histogram.fs` — compute bin edges from data min/max and binCount, count values per bin, render adjacent Rect bars (no gap) with frequency-proportional heights, X axis shows bin boundaries, Y axis shows frequency

**Checkpoint**: Histogram is fully functional

---

## Phase 10: User Story 8 — Candlestick Chart (Priority: P3)

**Goal**: Render OHLC financial data with colored bodies and wicks

**Independent Test**: Provide OHLC data and verify candlestick bodies and wicks render correctly

- [x] T038 [P] [US8] Create `tests/SkiaViewer.Charts.Tests/CandlestickTests.fs` — test up candle uses upColor, down candle uses downColor, wick spans high-low, body spans open-close, empty data handled
- [x] T039 [US8] Create `src/SkiaViewer.Charts/Candlestick.fsi` — define `Candlestick.candlestickChart` and `Candlestick.defaultConfig`
- [x] T040 [US8] Implement `Candlestick.candlestickChart` in `src/SkiaViewer.Charts/Candlestick.fs` — for each OhlcData: render wick as vertical Line from low to high, render body as Rect from open to close (or close to open), fill with upColor (close>open) or downColor (close<=open), auto-scale Y axis from all high/low values

**Checkpoint**: Candlestick chart is fully functional

---

## Phase 11: User Story 9 — Radar/Spider Chart (Priority: P3)

**Goal**: Render multi-variable data on radial axes forming polygon shapes

**Independent Test**: Provide category-value sets and verify polygon renders on radial axes

- [x] T041 [P] [US9] Create `tests/SkiaViewer.Charts.Tests/RadarChartTests.fs` — test polygon vertices at correct radial positions, concentric grid rendered, category labels at axis endpoints, multi-series renders multiple polygons
- [x] T042 [US9] Create `src/SkiaViewer.Charts/RadarChart.fsi` — define `RadarChart.radarChart` and `RadarChart.defaultConfig`
- [x] T043 [US9] Implement `RadarChart.radarChart` in `src/SkiaViewer.Charts/RadarChart.fs` — compute radial axis angles from category count (evenly spaced), render concentric grid polygons at gridLevels intervals, render axis lines from center to edge, render category labels at axis endpoints, for each RadarSeries: compute vertex positions from values/maxValue and render as closed Path polygon with semi-transparent fill

**Checkpoint**: Radar chart is fully functional

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Surface-area baselines, FSI scripting, edge case hardening, and packaging

- [x] T044 [P] Create `tests/SkiaViewer.Charts.Tests/EdgeCaseTests.fs` — test all chart types with empty data, NaN values, very small element sizes (10x10), very large datasets (100K points for line chart performance)
- [x] T045 [P] Create `tests/SkiaViewer.Charts.Tests/SurfaceAreaTests.fs` — surface-area baseline tests for Charts module and DataGrid module public API (verify expected function signatures exist)
- [x] T046 [P] Create `scripts/charts-prelude.fsx` — FSI prelude that loads SkiaViewer.Charts.dll, re-exports chart creation helpers
- [x] T047 [P] Create `scripts/examples/03-charts-gallery.fsx` — example script rendering all chart types in a single scene with sample data
- [x] T048 [P] Create `scripts/examples/04-datagrid.fsx` — example script rendering a DataGrid with sorting and scrolling demo
- [x] T049 Verify `dotnet pack src/SkiaViewer.Charts` produces SkiaViewer.Charts.nupkg and copy to `~/.local/share/nuget-local/`
- [x] T050 Run all tests: `dotnet test tests/SkiaViewer.Charts.Tests` — verify all pass
- [x] T051 Validate quickstart.md code examples compile and render correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 Line Chart (Phase 3)**: Depends on Phase 2 — MVP target
- **US2 Bar Chart (Phase 4)**: Depends on Phase 2 — can run in parallel with US1
- **US6 DataGrid (Phase 5)**: Depends on Phase 2 — can run in parallel with US1/US2
- **US3 Pie/Donut (Phase 6)**: Depends on Phase 2 — can run in parallel with P1 stories
- **US4 Scatter (Phase 7)**: Depends on Phase 2 — can run in parallel
- **US5 Area (Phase 8)**: Depends on Phase 2 — shares line chart infrastructure, benefits from US1 completion
- **US7 Histogram (Phase 9)**: Depends on Phase 2 — can run in parallel
- **US8 Candlestick (Phase 10)**: Depends on Phase 2 — can run in parallel
- **US9 Radar (Phase 11)**: Depends on Phase 2 — can run in parallel
- **Polish (Phase 12)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (Line Chart)**: No dependencies on other stories
- **US2 (Bar Chart)**: No dependencies on other stories
- **US6 (DataGrid)**: No dependencies on other stories (separate DataGrid module)
- **US3 (Pie/Donut)**: No dependencies on other stories
- **US4 (Scatter)**: No dependencies on other stories
- **US5 (Area)**: Shares line chart rendering pattern with US1 but independently implementable
- **US7 (Histogram)**: Shares bar rendering pattern with US2 but independently implementable
- **US8 (Candlestick)**: No dependencies on other stories
- **US9 (Radar)**: No dependencies on other stories

### Within Each User Story

- Tests written first (FAIL before implementation)
- .fsi signature defined before .fs implementation
- Core implementation before edge case handling
- Story complete before moving to next priority

### Parallel Opportunities

- After Phase 2, ALL user stories can start in parallel
- Within Phase 2: T006+T007 (Axis) and T008+T009 (ChartHelpers) can run in parallel after T004+T005 (Types)
- All test file creation tasks marked [P] can run in parallel with each other
- P1 stories (US1, US2, US6) can all run in parallel — each writes to its own .fsi/.fs file pair (LineChart.fsi/fs, BarChart.fsi/fs, DataGrid.fsi/fs)

---

## Parallel Example: P1 Stories (after Phase 2)

```
# All three P1 stories can start simultaneously:
Agent 1: T012→T013→T014→T015 (Line Chart)
Agent 2: T016→T017→T018→T019 (Bar Chart)  
Agent 3: T020→T021→T022→T023→T024 (DataGrid)
```

---

## Implementation Strategy

### MVP First (Line Chart Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T011)
3. Complete Phase 3: User Story 1 — Line Chart (T012-T015)
4. **STOP and VALIDATE**: Test line chart independently, run quickstart example
5. Commit and demo

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add Line Chart (US1) → MVP
3. Add Bar Chart (US2) + DataGrid (US6) → Core P1 complete
4. Add Pie/Donut (US3) + Scatter (US4) + Area (US5) → P2 complete
5. Add Histogram (US7) + Candlestick (US8) + Radar (US9) → P3 complete
6. Polish → Ship

### Parallel Team Strategy

With multiple developers after Phase 2:
- Developer A: US1 (Line) → US5 (Area, reuses line patterns) → US7 (Histogram)
- Developer B: US2 (Bar) → US3 (Pie) → US8 (Candlestick)
- Developer C: US6 (DataGrid) → US4 (Scatter) → US9 (Radar)

---

## Notes

- All chart creation functions return `SkiaViewer.Element` (Group of composed primitives)
- No modifications to core SkiaViewer project required
- Each chart type has its own .fsi/.fs file pair (LineChart, BarChart, etc.) enabling true parallel development
- DataGrid.fsi is a separate module file, independent of chart modules
- Surface-area baselines (T045) should snapshot the final public API after all stories complete

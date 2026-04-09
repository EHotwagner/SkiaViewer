# Feature Specification: Charting & DataGrid Library

**Feature Branch**: `008-charting-datagrid-library`  
**Created**: 2026-04-09  
**Status**: Draft  
**Input**: User description: "create a skiaviewer libary project for charting elements. add all popular elements. also add a datagrid element."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Render a Line Chart (Priority: P1)

A developer wants to display time-series data as a line chart inside a SkiaViewer scene. They create a line chart element, provide it with a series of data points, configure axis labels, and render it within their application window. The chart draws axes, grid lines, data lines, and a legend.

**Why this priority**: Line charts are the most commonly used chart type and form the foundation for validating the charting library's rendering pipeline, axis system, and data binding.

**Independent Test**: Can be fully tested by creating a line chart element with sample data points and verifying it renders correctly with axes, labels, grid lines, and data lines visible on screen.

**Acceptance Scenarios**:

1. **Given** a developer provides a list of (x, y) data points and axis labels, **When** the line chart element is rendered, **Then** the chart displays the data as connected line segments with labeled axes and grid lines.
2. **Given** multiple data series are provided, **When** the line chart is rendered, **Then** each series is drawn in a distinct color and a legend identifies each series.
3. **Given** the chart element is resized, **When** the scene is re-rendered, **Then** the chart scales proportionally to fill its allocated area.

---

### User Story 2 - Render a Bar Chart (Priority: P1)

A developer wants to visualize categorical comparisons using a bar chart. They create a bar chart element, supply category labels and values, and render it. The chart draws vertical bars with category labels on the x-axis and value labels on the y-axis.

**Why this priority**: Bar charts are the second most widely used chart type and share axis/label infrastructure with line charts, validating reusability of core components.

**Independent Test**: Can be fully tested by creating a bar chart with category-value pairs and verifying bars render at correct heights with proper labels.

**Acceptance Scenarios**:

1. **Given** a set of categories and corresponding values, **When** the bar chart element is rendered, **Then** each category is displayed as a vertical bar whose height corresponds to its value.
2. **Given** grouped data with multiple series per category, **When** the bar chart is rendered, **Then** bars for each series are displayed side by side within each category with distinct colors and a legend.

---

### User Story 3 - Render a Pie/Donut Chart (Priority: P2)

A developer wants to show proportional data as a pie chart or donut chart. They provide labeled slices with values and the chart renders proportional arcs with labels or a legend.

**Why this priority**: Pie and donut charts are essential for proportion visualization and test the library's arc-drawing and label-placement capabilities.

**Independent Test**: Can be fully tested by creating a pie chart with labeled value slices and verifying arcs are proportionally sized and labeled.

**Acceptance Scenarios**:

1. **Given** a set of labeled values, **When** the pie chart is rendered, **Then** each value is represented as a proportional arc segment with its label.
2. **Given** the developer configures a donut style, **When** the chart is rendered, **Then** a hollow center is displayed instead of a filled circle.

---

### User Story 4 - Render a Scatter Plot (Priority: P2)

A developer wants to plot individual data points on a two-dimensional plane. They provide (x, y) coordinates and optional point styling, and the chart renders points with axes and grid lines.

**Why this priority**: Scatter plots are widely used for correlation analysis and test point rendering and axis scaling.

**Independent Test**: Can be fully tested by providing coordinate data and verifying points appear at correct positions with axes.

**Acceptance Scenarios**:

1. **Given** a set of (x, y) data points, **When** the scatter plot is rendered, **Then** each point appears at the correct position on the chart with labeled axes.
2. **Given** multiple data series, **When** the scatter plot is rendered, **Then** each series uses a distinct marker style or color.

---

### User Story 5 - Render an Area Chart (Priority: P2)

A developer wants to display cumulative data using an area chart. They provide data series and the chart renders filled regions under the line.

**Why this priority**: Area charts extend line chart functionality with filled regions and are commonly used for showing volume or cumulative totals.

**Independent Test**: Can be fully tested by providing data series and verifying the filled region renders correctly below the data line.

**Acceptance Scenarios**:

1. **Given** a data series, **When** the area chart is rendered, **Then** the area below the data line is filled with a semi-transparent color.
2. **Given** stacked data series, **When** the area chart is rendered, **Then** each series stacks on top of the previous one with distinct colors.

---

### User Story 6 - Render a DataGrid (Priority: P1)

A developer wants to display tabular data in a scrollable, sortable grid. They provide column definitions and row data, and the DataGrid renders a header row with column titles and data rows beneath. The grid supports vertical scrolling for large datasets.

**Why this priority**: DataGrids are a fundamental UI element for data-heavy applications and represent a distinct rendering challenge from charts, validating the library's versatility.

**Independent Test**: Can be fully tested by providing column definitions and row data, then verifying the grid renders headers, rows, and supports scrolling through the data.

**Acceptance Scenarios**:

1. **Given** column definitions and row data, **When** the DataGrid is rendered, **Then** a header row displays column titles and data rows display corresponding values.
2. **Given** more rows than fit in the visible area, **When** the user scrolls, **Then** the grid scrolls vertically to reveal additional rows while the header remains fixed.
3. **Given** a sortable column, **When** the user clicks the column header, **Then** the rows are reordered by that column's values in ascending/descending order.

---

### User Story 7 - Render a Histogram (Priority: P3)

A developer wants to show the distribution of a dataset using a histogram. They provide raw data values and a bin count, and the chart groups data into bins and displays frequency bars.

**Why this priority**: Histograms are important for statistical analysis but build on bar chart infrastructure already established.

**Independent Test**: Can be fully tested by providing a dataset and bin count, then verifying the correct number of bars with proportional heights.

**Acceptance Scenarios**:

1. **Given** a dataset and bin count, **When** the histogram is rendered, **Then** data is grouped into the specified number of bins and displayed as adjacent bars representing frequency.

---

### User Story 8 - Render a Candlestick Chart (Priority: P3)

A developer wants to display financial OHLC (Open, High, Low, Close) data. They provide time-stamped OHLC values and the chart renders candlestick bodies and wicks.

**Why this priority**: Candlestick charts serve a specific but important financial domain and validate the library's ability to render complex composite shapes.

**Independent Test**: Can be fully tested by providing OHLC data and verifying candlestick bodies and wicks render correctly with appropriate colors for up/down periods.

**Acceptance Scenarios**:

1. **Given** OHLC data for a series of time periods, **When** the candlestick chart is rendered, **Then** each period displays a body (open-close range) and wicks (high-low range) with up periods in one color and down periods in another.

---

### User Story 9 - Render a Radar/Spider Chart (Priority: P3)

A developer wants to compare multiple variables on a radial layout. They provide categories and values, and the chart renders a polygon on radial axes.

**Why this priority**: Radar charts serve multi-variable comparison use cases and test radial layout capabilities.

**Independent Test**: Can be fully tested by providing category-value sets and verifying the polygon shape renders on radial axes.

**Acceptance Scenarios**:

1. **Given** a set of categories and values, **When** the radar chart is rendered, **Then** each category is represented as a radial axis and the values form a polygon connecting the data points.

---

### Edge Cases

- What happens when a chart element receives an empty data set? The chart should render axes and labels but no data marks, with an optional "no data" message.
- What happens when a data point has a NaN or infinity value? The chart should skip that point and continue rendering remaining data without crashing.
- What happens when the DataGrid receives zero rows? The grid should display the header row with an empty body area.
- What happens when a chart element is given a very small size (e.g., 10x10 pixels)? The chart should degrade gracefully, potentially hiding labels but still rendering the data area.
- What happens when all pie chart values are zero? The chart should display an empty circle or a "no data" indicator rather than dividing by zero.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST provide a separate project/assembly that developers can reference to use charting and DataGrid elements within SkiaViewer scenes.
- **FR-002**: The library MUST provide a line chart element that renders one or more data series as connected line segments with configurable axes, labels, and grid lines.
- **FR-003**: The library MUST provide a bar chart element that renders categorical data as vertical bars with support for grouped and stacked layouts.
- **FR-004**: The library MUST provide a pie chart element that renders proportional data as arc segments, with an option for donut style (hollow center).
- **FR-005**: The library MUST provide a scatter plot element that renders individual data points on a two-dimensional plane.
- **FR-006**: The library MUST provide an area chart element that renders filled regions under data lines, with support for stacking.
- **FR-007**: The library MUST provide a DataGrid element that renders tabular data with column headers, scrollable rows, and column sorting.
- **FR-008**: The library MUST provide a histogram element that groups raw data into bins and renders frequency bars.
- **FR-009**: The library MUST provide a candlestick chart element that renders OHLC financial data.
- **FR-010**: The library MUST provide a radar/spider chart element that renders multi-variable data on radial axes.
- **FR-011**: All chart elements MUST support automatic axis scaling based on data range.
- **FR-012**: All chart elements MUST support legends that identify each data series by color and label.
- **FR-013**: All chart elements MUST support configurable titles and axis labels.
- **FR-014**: All chart elements MUST handle empty datasets gracefully without errors.
- **FR-015**: All chart elements MUST resize proportionally when their allocated area changes.
- **FR-016**: The DataGrid MUST keep column headers visible (fixed) when scrolling vertically.
- **FR-017**: The DataGrid MUST support sorting rows by clicking column headers (ascending/descending toggle).
- **FR-020**: The DataGrid MUST auto-fit columns to the available element width; horizontal scrolling is not supported.
- **FR-018**: All elements MUST integrate with the existing SkiaViewer declarative scene DSL.
- **FR-019**: All elements MUST use a static/immutable data model; data is provided at element creation and updates are achieved by rebuilding the element with new data.

### Key Entities

- **ChartElement**: Base concept for all chart types; holds data series, title, axis configuration, and legend settings.
- **DataSeries**: A named collection of data points associated with a visual style (color, marker shape).
- **Axis**: Represents a chart axis with label, range (auto or manual), tick marks, and grid lines.
- **DataGridElement**: A tabular display element with column definitions (name, width, sortable flag, data type) and row data. Supported cell data types: text (left-aligned), numeric (right-aligned), and boolean (rendered as checkbox-style indicator).
- **Legend**: A visual key mapping series names to their colors/markers.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can render any of the supported chart types with sample data in under 10 lines of declarative code.
- **SC-002**: All chart types render correctly at sizes ranging from 100x100 to 4000x4000 pixels without visual artifacts.
- **SC-003**: The DataGrid can display 10,000 rows with smooth scrolling (no visible stutter during scroll).
- **SC-004**: Charts with up to 100,000 data points render within 100 milliseconds on the first frame.
- **SC-005**: All chart elements handle edge cases (empty data, NaN values, zero-size slices) without errors or crashes.
- **SC-006**: The library is usable as a standalone project reference without modifying the core SkiaViewer project.

## Clarifications

### Session 2026-04-09

- Q: Should charts and DataGrid support dynamic data updates or static/immutable data per render frame? → A: Static/immutable data per render frame; rebuild element with new data to update.
- Q: What content types should DataGrid cells support? → A: Text, numeric, and boolean (with appropriate formatting, alignment, and checkbox-style rendering for booleans).
- Q: Should the DataGrid support horizontal scrolling when columns exceed visible width? → A: No; columns auto-fit to available width.

## Assumptions

- The charting library will be a separate F# project within the SkiaViewer solution, referenced as a project dependency.
- Charts are rendered using the SkiaSharp canvas drawing primitives already available in SkiaViewer.
- The existing declarative scene DSL (from feature 003) will be extended or composed with to support chart elements.
- Interactive features beyond DataGrid sorting and scrolling (e.g., tooltips, click-to-zoom) are out of scope for the initial version.
- The library targets desktop rendering scenarios; mobile-specific considerations are out of scope.
- Chart theming and color palettes will use sensible defaults; full customization of themes is deferred to a future iteration.

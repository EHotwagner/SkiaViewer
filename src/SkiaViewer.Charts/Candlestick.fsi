namespace SkiaViewer.Charts

open SkiaViewer

/// <summary>Candlestick chart creation module. Renders OHLC (Open-High-Low-Close) financial data as candlestick bars with wicks.</summary>
module Candlestick =
    /// <summary>Create a candlestick chart element from OHLC data.</summary>
    /// <param name="config">Candlestick configuration (dimensions, colors for up/down, axes).</param>
    /// <param name="data">OHLC data points, each containing open, high, low, and close values.</param>
    /// <returns>A SkiaViewer.Element (Group) containing the complete candlestick chart.</returns>
    val candlestickChart: config: CandlestickConfig -> data: OhlcData list -> Element
    /// <summary>Default candlestick configuration for the given dimensions.</summary>
    /// <param name="width">Width of the chart area in pixels.</param>
    /// <param name="height">Height of the chart area in pixels.</param>
    /// <returns>A CandlestickConfig with sensible defaults for the specified size.</returns>
    val defaultConfig: width: float32 -> height: float32 -> CandlestickConfig

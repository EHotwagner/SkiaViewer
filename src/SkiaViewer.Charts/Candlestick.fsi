namespace SkiaViewer.Charts

open SkiaViewer

/// Candlestick chart creation module.
module Candlestick =
    /// Create a candlestick chart element from OHLC data.
    val candlestickChart: config: CandlestickConfig -> data: OhlcData list -> Element
    /// Default candlestick configuration for the given dimensions.
    val defaultConfig: width: float32 -> height: float32 -> CandlestickConfig

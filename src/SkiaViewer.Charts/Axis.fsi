namespace SkiaViewer.Charts

/// Internal axis computation utilities.
module internal Axis =
    /// Compute a "nice" number for axis tick spacing.
    /// If round is true, rounds to nearest nice number; otherwise takes ceiling.
    val niceNumber: range: float -> round: bool -> float

    /// Compute axis tick positions and labels for a given data range.
    /// Returns (min, max, ticks) where ticks is a list of (value, label) pairs.
    val computeAxisTicks: dataMin: float -> dataMax: float -> tickCount: int -> float * float * (float * string) list

    /// Compute auto-scaled range from a list of values.
    /// Returns (min, max) with nice boundaries. Returns (0, 1) for empty input.
    val computeAutoRange: values: float list -> float * float

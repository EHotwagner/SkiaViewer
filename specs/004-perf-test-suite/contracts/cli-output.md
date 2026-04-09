# CLI Output Contract: Performance Test Suite

The perf test suite is a console application. Its contract is the structured output format.

## Exit Codes

- `0` — All benchmarks completed (some backends may have been skipped)
- `1` — Fatal error (no benchmarks could run)

## Stdout Output Structure

```
================================================================================
  SkiaViewer Performance Test Suite
  Machine: {hostname}
  Date: {ISO 8601 timestamp}
  Vulkan: {Available (device name) | Unavailable (reason)}
================================================================================

── Rendering Throughput ────────────────────────────────────────────────────────

Backend     | Avg FPS | Median (ms) | Min (ms) | Max (ms) | P99 (ms) | Frames
------------|---------|-------------|----------|----------|----------|-------
Vulkan      |  {fps}  |   {median}  |  {min}   |  {max}   |  {p99}   | {count}
GL          |  {fps}  |   {median}  |  {min}   |  {max}   |  {p99}   | {count}

── Stress Test: {ElementType} ─────────────────────────────────────────────────

Backend: {Vulkan|GL}
Elements | Avg FPS | Median (ms) | P99 (ms) | Memory (MB)
---------|---------|-------------|----------|------------
      10 |  {fps}  |   {median}  |  {p99}   |   {mem}
     100 |  {fps}  |   {median}  |  {p99}   |   {mem}
   1,000 |  {fps}  |   {median}  |  {p99}   |   {mem}
  10,000 |  {fps}  |   {median}  |  {p99}   |   {mem}
 100,000 |  {fps}  |   {median}  |  {p99}   |   {mem}

── Scene Composition ──────────────────────────────────────────────────────────

Elements | Construction (ms) | Renderer (ms) | Scenes/sec
---------|-------------------|---------------|----------
      10 |     {time}        |    {time}     |  {rate}
     100 |     {time}        |    {time}     |  {rate}
   1,000 |     {time}        |    {time}     |  {rate}
  10,000 |     {time}        |    {time}     |  {rate}
 100,000 |     {time}        |    {time}     |  {rate}

── Screenshot Capture ─────────────────────────────────────────────────────────

Backend     | Elements | Avg (ms) | Captures/sec
------------|----------|----------|-------------
Vulkan      |   100    |  {time}  |   {rate}
GL          |   100    |  {time}  |   {rate}

── Interactive Rate Thresholds ────────────────────────────────────────────────

Backend | 60 FPS Limit | 30 FPS Limit
--------|-------------|-------------
Vulkan  | {count} elements | {count} elements
GL      | {count} elements | {count} elements

================================================================================
  Total duration: {seconds}s
================================================================================
```

## Optional JSON Output

When `--json` flag is passed, write `perf-results.json` in the current directory with the `SuiteReport` structure defined in `data-model.md`.

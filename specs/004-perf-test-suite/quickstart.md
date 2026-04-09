# Quickstart: Performance Test Suite

## Run the suite

```bash
cd tests/SkiaViewer.PerfTests
dotnet run -c Release
```

## Run with JSON output

```bash
dotnet run -c Release -- --json
```

## What to expect

1. The suite opens a window for each backend (Vulkan, GL) and runs benchmarks sequentially
2. Warm-up frames are discarded (2 seconds per benchmark)
3. Measurement runs for at least 5 seconds or 200 frames per benchmark
4. Results print to stdout as each benchmark completes
5. A summary with interactive rate thresholds prints at the end
6. Total suite duration: ~5-10 minutes depending on machine

## Prerequisites

- Vulkan driver installed (for Vulkan benchmarks; GL benchmarks run regardless)
- Silk.NET native dependencies available
- Display server or virtual framebuffer (GLFW requires a display)

## Interpreting results

- **Avg FPS**: Higher is better. >60 = smooth interactive rendering
- **P99 frame time**: Indicates worst-case jank. <16.7ms = consistent 60 FPS
- **Interactive Rate Thresholds**: Shows the max element count that sustains 60/30 FPS
- **Scene Composition**: Shows CPU overhead of building scenes (relevant for dynamic UIs)

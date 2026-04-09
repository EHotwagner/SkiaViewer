// 03-perf-suite.fsx
// SkiaViewer Performance Test Suite — How to run
//
// The performance suite is a standalone console application that benchmarks
// the SkiaViewer rendering pipeline across Vulkan and GL backends.
//
// Prerequisites:
//   - .NET 10.0 SDK
//   - Vulkan driver (optional, GL benchmarks run regardless)
//   - Display server or virtual framebuffer
//
// Run the suite:
//   dotnet run -c Release --project tests/SkiaViewer.PerfTests
//
// Run with JSON output:
//   dotnet run -c Release --project tests/SkiaViewer.PerfTests -- --json
//
// What it measures:
//   1. Rendering Throughput — max FPS for Vulkan vs GL with a standard scene
//   2. Stress Tests — FPS scaling from 10 to 100,000 elements per element type
//   3. Scene Composition — CPU-bound scene DSL construction and renderer overhead
//   4. Screenshot Capture — screenshot pipeline throughput per backend
//
// Output includes interactive rate thresholds showing the max element count
// that sustains 60 FPS and 30 FPS for each backend.
//
// Typical duration: 5-10 minutes depending on machine.

printfn "This script documents how to run the performance suite."
printfn "Run: dotnet run -c Release --project tests/SkiaViewer.PerfTests"

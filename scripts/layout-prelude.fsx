/// SkiaViewer.Layout FSI Prelude
/// Load this script in F# Interactive to use layout containers and graph visualization interactively.
///
/// Usage:
///   dotnet fsi scripts/layout-prelude.fsx
///
/// Or from FSI:
///   #load "scripts/layout-prelude.fsx"

#load "prelude.fsx"
#r "nuget: Microsoft.Msagl, 1.1.6"
#r "nuget: Microsoft.Msagl.Drawing, 1.1.6"
#r "../src/SkiaViewer.Layout/bin/Debug/net10.0/SkiaViewer.Layout.dll"

open SkiaViewer.Layout

printfn "SkiaViewer.Layout prelude loaded. Modules: Layout, Graph, GraphValidation, Defaults, Types."

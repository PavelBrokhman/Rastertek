# RastertekCS docs

Documentation for the C# port of the Rastertek tutorials. The repo contains two parallel ports — `OpenGL/` (Linux + Windows, Silk.NET.OpenGL) and `Windows/` (Windows-only, Silk.NET.Direct3D11). Both build with .NET 8 and are runnable from Visual Studio, Rider, or `dotnet build` + `dotnet run`.

## Layout

| Folder | Contents |
|---|---|
| [`architecture/`](architecture/) | How the C# wrappers around DX11 are organised; conventions for shaders, vertex layouts, cbuffer packing, and per-tutorial decisions made along the way. |
| [`references/`](references/) | Pointers to the official Rastertek source URLs, per-tutorial mapping, and asset provenance. |
| [`project-structure/`](project-structure/) | Repo layout, naming conventions, build/run workflow on **both Windows and Linux**, and the current status of every tutorial. |

## Quick links

- [DX11 wrapper pattern](architecture/dx11-wrapper-pattern.md) — what every `Windows/TutorialNN/` looks like inside.
- [Decisions log](architecture/decisions-log.md) — wrapper-level fixes and minor divergences from Rastertek (asset swaps, DPI handling, Model variants, TextureShader patterns, etc).
- [Rastertek source references](references/rastertek-sources.md) — official tutorial pages, source archive URLs, per-tutorial mapping for the 16-30 set.
- [Project layout](project-structure/layout.md) — directory tree, namespace conventions, csproj asset linking.
- [Build and run workflow](project-structure/build-and-run.md) — `run.bat` (Windows), `run.sh` (Linux), Linux→Windows push/pull loop, shader integrity check (PowerShell + bash variants).
- [Tutorial status](project-structure/tutorial-status.md) — done/in-progress table for both APIs.

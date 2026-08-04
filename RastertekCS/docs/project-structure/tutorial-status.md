# Tutorial Status

Inventory of every Rastertek tutorial across the series being ported, and
what exists for each: the original C++ source, the C# port, and whether the
port has ever been checked against the article's reference image.

Taken **2026-08-03** by reading the local mirror in `RastertekOriginal/`.

## Status vocabulary

| Value | Meaning |
|---|---|
| `-` | does not exist |
| `ported` | C# code exists; **nothing is claimed about whether it is correct** |
| `built` | compiles on Windows (a `build_log.txt` or a build run this session) |
| `verified` | screenshot captured and compared against the **original C++ binary**, built and run from `RastertekOriginal/` |
| `MISMATCH` | captured, compared, and it does **not** match |

### What counts as the reference

The **running original binary**, not the screenshot on the tutorial page.
Established 2026-08-03 on Tut54: the C# port was declared a mismatch against
the article's `pic6093.gif` - a dark blue forest - and then the original C++
build was captured on the same machine and produced the same **light** blue
image the port did. The published GIF was made with different, darker assets
than the ones shipped in the source archive.

Build an original with `RastertekCS/Tools/build-original.cmd <series> <NN>`,
then capture `Engine/client.exe`. The page's image is still worth a glance -
it catches "nothing is drawn at all" - but it cannot settle a colour or
blending question.

`ported` and `verified` are different claims. Everything ported before
2026-08-03 was written without a visual check, so it is `ported` only.

## Series 1 - DirectX 11 on Windows 10 (`tutdx11win10`)

60 articles, C++ source present for all of them.

| NN | Topic | C++ src | C# DX port | Visual |
|----|-------|---------|-----------|--------|
| 01 | Setting up DirectX 11 with Visual Studio 2022 | n/a | n/a | n/a - article only, no code |
| 02-23 | framework, lighting, 2D, fonts, input, mapping | yes | ported | not checked |
| 24 | Loading Maya Models | none | stub (`Program.cs` only) | n/a - no source published |
| 25-50 | RTT through deferred shading | yes | ported | not checked |
| 51 | Screen Space Ambient Occlusion | yes | ported, built | `verified` (2026-08-03) |
| 52 | Physically Based Rendering | yes | ported, built | `verified` (2026-08-03) |
| 53 | Heat | yes | ported, built | `verified` (2026-08-03) |
| 54 | Parallax Scrolling | yes | ported, built | `verified` (2026-08-03) |
| 55 | Direct Sound | yes | - | - |
| 56 | 3D Sound | yes | - | - |
| 57 | XAudio2 | yes | - | - |
| 58 | X3DAudio | yes | - | - |
| 59 | Animated Particles | yes | ported, built | `verified` (2026-08-03) |
| 60 | XInput | yes | - | - |

Ports present: 02-54 and 59 (24 is a stub). **Every visual tutorial in this
series is done.**

Tut55-58 are audio and Tut60 is XInput; both are deferred by
[[rastertek-scope-decisions]] until every visual tutorial in all three series
is done.

## Series 2 - Terrain, legacy (`tutterr`)

19 articles. No C# port of any kind exists yet.

| NN | Topic | C++ src | C# DX port |
|----|-------|---------|-----------|
| 01-10 | grid/camera, height maps, lighting, texturing, quad trees, height-based movement, colour mapping, mini-maps, blending, sky domes | yes | - |
| 11 | Bitmap Clouds | **zip only** (`_archives/tersrc11.zip`, not extracted) | - |
| 12-17 | perturbed clouds, detail mapping, slope texturing, bump mapping, small body water, texture layers | yes | - |
| 18 | Large Terrain Rendering | **none** - no source archive published | - |
| 19 | Foliage | yes | - |

## Series 3 - DirectX 11 Terrain, series 2 (`tutdx11s2ter`)

14 articles, C++ source present for all. No C# port of any kind exists yet.

| NN | Topic | C++ src | C# DX port |
|----|-------|---------|-----------|
| 01-14 | grid/camera, bitmap + RAW height maps, texturing, lighting, colour mapping, normal mapping, sky domes, terrain cells + culling, height-based movement, mini-maps, procedural texturing, distance normal mapping | yes | - |

## OpenGL - out of the current scope

`RastertekCS/OpenGL/` holds ports for 02-55. They are not part of the
DirectX effort described above and carry the same caveat: `ported`, with
visual checks recorded only where a commit message says so.

Known open item: Tut55 OGL (particle system) was committed with a trail
defect and a follow-up blend-mode fix whose effect was never confirmed.

## What this inventory says about remaining work

- **42 tutorials have no DirectX port**: 9 in series 1, 19 in series 2,
  14 in series 3. Two of them (series 2 tut 11 and 18) need their source
  resolved first - one is an unextracted zip, the other was never published.
- **Series 1 tutorials 55-58 are audio** (Direct Sound, 3D Sound, XAudio2,
  X3DAudio) and **60 is XInput**. The current package set - Silk.NET
  Direct3D11 / DXGI / Windowing / Input / Maths - covers neither DirectSound
  nor XAudio2, and none of the five can be checked from a screenshot. They
  need a decision before they can be planned, not just implementation time.
- **The 50 existing ports have never been systematically verified.** That is
  a second body of work sitting behind the first, and it is invisible in the
  git history. Tut54 was the first one ever captured and compared; it failed.

## Per-tutorial Windows artefacts

Each `RastertekCS/Windows/TutorialNN/build_log.txt` holds the latest Windows
build+run output (committed for cross-machine visibility). Present for
34-50. When a build fails on Windows, that log is the first place to look.

## Tut08 / Tut24 - non-graphics tutorials

Rastertek's OpenGL tut08 and tut24 are CLI utilities (Wavefront `.obj` to
Rastertek `.txt` model converter), not graphics lessons. The real tut08 is
"Scaling, Rotation, and Translation", so the Windows port uses that slot for
it. Tut24 is a Maya exporter chapter with no source zip on rastertek.com;
`Windows/Tutorial24` is a stub and `OpenGL/Tutorial24` is still the converter.

The OpenGL Tut08 OBJ converter now lives in `RastertekCS/Tools/ObjToModel/`.

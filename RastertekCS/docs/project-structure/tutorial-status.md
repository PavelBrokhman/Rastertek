# Tutorial Status

Status of every tutorial in `RastertekCS/Windows/` and `RastertekCS/OpenGL/`.

| NN | Topic                          | OpenGL  | Windows |
|----|--------------------------------|---------|---------|
| 02 | Window framework               | done    | done    |
| 03 | DirectX init / OpenGL init     | done    | done    |
| 04 | Vertex buffers                 | done    | done    |
| 05 | Texturing                      | done    | done    |
| 06 | Diffuse lighting (triangle)    | done    | done    |
| 07 | 3D model rendering             | done    | done    |
| 08 | 3D object transforms (2 cubes) | done    | done    |
| 09 | Ambient lighting               | done    | done    |
| 10 | Specular lighting              | done    | done    |
| 11 | Multiple point lights          | done    | done    |
| 12 | 2D bitmap rendering            | done    | done    |
| 13 | Sprite animation               | done    | done    |
| 14 | Font / text rendering          | done    | done    |
| 15 | FPS counter                    | done    | done    |
| 16 | Mouse input                    | done    | done    |
| 17 | Multi-texturing                | done    | done    |
| 18 | Light maps                     | done    | done    |
| 19 | Alpha mapping                  | done    | done    |
| 20 | Normal mapping                 | done    | done    |
| 21 | Specular mapping               | done    | done    |
| 22 | Shader manager                 | done    | done    |
| 23 | Frustum culling                | done    | done    |
| 24 | (Maya exporter — no DX11 src)  | OBJ tool | n/a    |
| 25 | Render to texture              | done    | done    |
| 26 | Fog                            | done    | done    |
| 27 | Clip plane                     | done    | done    |
| 28 | Texture translation            | done    | done    |
| 29 | Transparency                   | done    | done    |
| 30 | Planar reflection              | done    | done    |
| 31 | Water (refraction+reflection)  | done    | done    |
| 32..59 | (later)                    | done    | not started |

"OpenGL done" reflects the pre-existing state of `RastertekCS/OpenGL/` (cloned from a separate port). "Windows done" tracks this port's progress.

## Tut08 / Tut24 — non-graphics tutorials

Both Rastertek tut08 and tut24 in the OpenGL series are CLI utilities (Wavefront `.obj` → Rastertek `.txt` model converter), not graphics lessons. The actual Rastertek tut08 is "Manipulating 3D Objects" (two cubes with SRT transforms), so the Windows port re-uses the slot for that. The tut24 chapter is a Maya exporter tutorial with no source zip on rastertek.com — there is no `Windows/Tutorial24` and `OpenGL/Tutorial24` is currently still the OBJ converter.

The OpenGL Tut08 OBJ converter has been moved to `RastertekCS/Tools/ObjToModel/`.

## Per-tutorial Windows artefacts

Each `RastertekCS/Windows/TutorialNN/build_log.txt` contains the latest Windows build+run output (committed for cross-machine visibility). When a build fails on Windows, that log is the first place to look.

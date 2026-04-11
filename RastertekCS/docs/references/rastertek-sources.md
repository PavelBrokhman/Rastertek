# Rastertek Source References

The shader files (`.vs`/`.ps`) in every Windows tutorial are byte-identical copies of the official Rastertek DX11 source. The OpenGL tutorials track the official Rastertek OpenGL source.

## Official URLs

| Series | Index | Source zips |
|---|---|---|
| DX11 (Windows) | https://www.rastertek.com/tutdx11win10.html | `https://www.rastertek.com/dx11win10tutNN_src.zip` |
| OpenGL (Linux) | https://www.rastertek.com/tutgl4linux.html | `https://www.rastertek.com/gl4linuxtutNN_src.tar.gz` |

`NN` is the two-digit tutorial number (`01`..`60` etc.).

## Per-tutorial mapping (16-30, current scope)

| Tut | DX11 page / source | Notes |
|---|---|---|
| 16 | dx11win10tut16 | Mouse input via DirectInput → Silk.NET mouse events |
| 17 | dx11win10tut17 | Multi-texturing, square + dirt + stone |
| 18 | dx11win10tut18 | Light maps, square + light01 + stone |
| 19 | dx11win10tut19 | Alpha mapping, three textures |
| 20 | dx11win10tut20 | Normal mapping, cube + normal01, adds tangent/binormal |
| 21 | dx11win10tut21 | Specular mapping, cube + normal02 + spec02 + stone02 |
| 22 | dx11win10tut22 | Shader manager: three shaders × one sphere model |
| 23 | dx11win10tut23 | Frustum culling, 25 spheres + on-screen render-count text |
| 24 | (no DX11 source) | Maya exporter chapter — no port. OpenGL/Tut24 stays an OBJ converter |
| 25 | dx11win10tut25 | Render to texture, three display planes |
| 26 | dx11win10tut26 | Per-vertex linear fog |
| 27 | dx11win10tut27 | Clip plane via SV_ClipDistance0 |
| 28 | dx11win10tut28 | UV translation (texture scrolling) |
| 29 | dx11win10tut29 | Transparency via blend state + per-pixel alpha override |
| 30 | dx11win10tut30 | Planar reflection via render-to-texture + reflection view matrix |
| 31 | dx11win10tut31 | Water: refraction (clip plane) + reflection (mirrored view) + normal-map ripple |

## Shader integrity check

See `docs/project-structure/build-and-run.md` for the full Windows (PowerShell) and Linux (bash) verification snippets that compare every official `.vs`/`.ps` against the corresponding file in `RastertekCS/Windows/TutorialNN/Shaders/`.

Fetch any tutorial's Rastertek source archive once:

- **Windows (PowerShell)**: `Invoke-WebRequest https://www.rastertek.com/dx11win10tut17_src.zip -OutFile $env:TEMP\dx11tut17.zip; Expand-Archive $env:TEMP\dx11tut17.zip $env:TEMP\dx11tut17\`
- **Linux (bash)**: `curl -fsSL https://www.rastertek.com/dx11win10tut17_src.zip -o /tmp/dx11tut17.zip && unzip -q /tmp/dx11tut17.zip -d /tmp/dx11tut17/`

## Asset provenance

Shipped under `RastertekCS/Assets/`:

- `Assets/Data/*.tga` — Rastertek textures (stone01/02, normal01/02/03, spec02, dirt01, light01, blue01, alpha01, Sprite01-04, font01, opengl_logo, Sprite01-02, etc.)
- `Assets/Data/font01.txt` — Rastertek font glyph table
- `Assets/Data/Sprite.txt` — local sprite list format `TextureCount: N / CycleTime: ms / BitmapSize: w h / file1 / file2 / ...` (custom; differs from Rastertek's space-delimited format)
- `Assets/Models/*.txt` — Rastertek `.txt` model format: `Vertex Count: N`, blank line, `Data:`, blank line, then `x y z tu tv nx ny nz` per vertex. Triangles are stored sequentially (no indexing in the file).

`cube.txt` and `Cube.txt` both exist — Linux is case-sensitive so the duplicate is intentional. Windows tutorials reference `Cube.txt`; OpenGL tutorials reference whichever the original sibling used.

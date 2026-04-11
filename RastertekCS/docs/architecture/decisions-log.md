# Decisions Log

Minor wrapper-level changes made to keep the C# port working while still mirroring the Rastertek tutorials.

## Asset / model corrections

- **Tut08 cube transforms** match Rastertek exactly: rotation+translate (-2,0,0) and scale 0.5 + rotation + translate (+2,0,0), camera at (0,0,-10), light direction (0,0,1).
- **Tut10 specular** uses `sphere.txt` (Rastertek's choice) — initially cloned with `Cube.txt` from Tut09, fixed before commit.
- **Tut11 multi-light** uses `Plane.txt` — initially cloned csproj kept the previous tutorial's `Cube.txt` reference, fixed.
- **Tut27 clip plane** camera position should be `(0,0,-10)`; initially cloned with `-5`, fixed during verification pass.
- **Tut28 texture translation** uses `square.txt` (flat plane shows scrolling clearly); initially cloned with `Cube.txt`, fixed during verification pass.
- **Tut30 reflection** RenderTexture uses screen dimensions, not a hardcoded 256×256; initially hardcoded 800×600, fixed during verification pass.
- **csproj asset links must match the model loaded by code.** Whenever a tutorial swaps a model, both the `<None Include="...">` line and the `Initialize(..., "Models/...")` call have to change. Building locally on Linux silently masked this — Linux is case-sensitive but the missing-asset failure only surfaced on Windows runtime. Same gotcha twice (Tut10, Tut11) — caught immediately after the first Windows run.

## OpenGL Tut08 / Tut24 are not graphics tutorials

Both are CLI utilities (Wavefront `.obj` → Rastertek `.txt` model converters) inherited from the OpenGL project. They were moved out of the tutorial sequence:

- `Tools/ObjToModel/` — RH (OpenGL) variant, originally OpenGL Tut08.
- (Tut24 is not yet relocated; same OBJ-converter shape, currently still under `OpenGL/Tutorial24`.)

`Windows/Tutorial08` was rebuilt from `Tutorial07` to render two cubes with SRT transforms (the actual Rastertek "Manipulating 3D Objects" lesson). `Windows/Tutorial24` does not exist — Rastertek's tut 24 has no DX11 source zip on rastertek.com (it's a Maya exporter chapter).

## Window titles

Every Tut07-15 originally cloned the same `"TutorialNN - Diffuse Lighting"` title from Tut07. Renamed to `"TutorialNN - <topic> (DirectX 11)"` and `"TutorialNN - <topic> (OpenGL)"` so side-by-side runs are distinguishable.

## DPI awareness (Tut12 detour)

When Tut12's bitmap initially rendered "twice as big" on Windows we chased a DPI scaling theory:

1. First fix: `m_window.FramebufferSize` instead of `m_window.Size` in `InitializeWindows`. (Kept — semantically correct, no harm if Size==FramebufferSize.)
2. Second attempt: `SetProcessDpiAwarenessContext` P/Invoke chain in `Program.cs`. (Reverted — wasn't the actual cause.)
3. Third attempt: `app.manifest` declaring PerMonitorV2. (Reverted — wasn't needed.)

The actual bug: `Stone01.tga` is 512×512, and my `Bitmap` was using the texture's native size (Rastertek-style). The OpenGL sibling tutorial hardcodes `BITMAP_SIZE = 256` and passes it in. Fix was to give `Bitmap.Initialize` explicit `bitmapWidth`/`bitmapHeight` parameters and pass `256` from `GraphicsFramework`. Now `FramebufferSize` is the only DPI-related artifact still in the code.

## Model.cs evolution

`Model.cs` exists in several variants across tutorials. The deltas:

- **1 texture** (Tut07-10, 23, 25, 26, 27, 28, 29, 30): single `Texture m_Texture`, `SetTexture(slot)` binds it.
- **2 textures** (Tut17, 18): `m_Texture1`, `m_Texture2`, `SetTextures()` binds both at slots 0/1.
- **3 textures** (Tut19, 21): adds `m_Texture3` at slot 2.
- **2 textures + tangent/binormal** (Tut20, 22): adds `tx,ty,tz,bx,by,bz` to `VertexType`/`ModelType`, runs `CalculateModelVectors`/`CalculateTangentBinormal` after `LoadModel`. Vertex stride 56.

When cloning a tutorial, pick the variant whose vertex layout matches what the next shader needs. Tut23 uses the 1-texture variant from Tut09 even though it cloned Tut15 — that's the right call because the light shader only reads `pos+tex+normal`.

## TextureShader two patterns

There are two `TextureShader.cs` patterns in the codebase, both faithful to Rastertek but binding the SRV differently:

- **Slot-bound** (Tut12, Tut13, Tut22's TextureShader): `Render` only takes matrices. The caller binds the SRV via `m_Bitmap/m_Sprite/m_Model.SetTexture(slot)` before calling. Used when the SRV always comes from the same bitmap/sprite/model.
- **Param-bound** (Tut25 TextureShader, Tut29 TextureShader/TransparentShader, Tut30 TextureShader/ReflectionShader): `Render` takes a `ComPtr<ID3D11ShaderResourceView>` and binds it with `PSSetShaderResources(0, ...)`. Used when the same shader renders both a model texture and a render-target SRV (e.g. Tut25 displays a render-target across three planes; Tut30 reflects the render texture onto the floor).

Param-bound shaders also create their own `ID3D11SamplerState`. Slot-bound shaders rely on the sampler that `Texture.SetTexture` already binds.

## Frustum culling (Tut23)

`Frustum.ConstructFrustum` extracts six planes from `view * projection` using the standard Gribb/Hartmann formulas (`col4 ± colN`). Verified against `Silk.NET.Maths.Matrix4X4.Mij` row/column indexing.

Rastertek's `frustumclass.cpp` additionally takes a `screenDepth` parameter and rewrites the projection matrix's `_33`/`_43` entries before extraction (a "modified projection" trick to use a tighter cull distance than the actual far plane). This is **not** ported — the un-modified frustum is functionally correct, just a hair more permissive at the far plane.

## Position turn speeds (Tut23)

Rastertek `PositionClass::TurnLeft` uses `m_frameTime * 1.5f` for acceleration and caps at `m_frameTime * 200.0f`. This port copied the OpenGL sibling's values: `* 10.0f` and `* 100.0f`. Both work; the turn feel is slightly different.

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

## OpenGL → LH pipeline conversion

The inherited OpenGL port originally used Silk.NET.Maths' right-handed helpers (`CreateLookAt`, `CreatePerspectiveFieldOfView`) and `FrontFaceDirection.Ccw`, so every tutorial rendered as a mirror image of the Windows sibling. The user wanted both ports to look identical (Tut25 was already consistent because its render-to-texture double-cancelled the mirror; everything else was flipped). Converted the OpenGL series to LH math end-to-end:

- Replaced `Matrix4X4.CreateLookAt` with a local `LookAtLH` in every `Camera.cs` (zaxis = target − eye, xaxis = normalize(cross(up, zaxis)), etc. — matches `XMMatrixLookAtLH`).
- Replaced `Matrix4X4.CreatePerspectiveFieldOfView` with a local `PerspectiveFovLH` in every `GL4.cs` (NDC z in [0,1], matching `XMMatrixPerspectiveFovLH`).
- Flipped `GL4.FrontFace(Ccw)` to `FrontFace(CW)` so CW-wound (Maya / LH) models are front-facing.
- Switched every model reference from the RH-wound `cubeGL.txt` to the LH-wound `Cube.txt` (shared with the Windows port) — tutorials 20, 21, 25, 26, 27, 30, 32, 36, 37, 39, 50.
- Tut25, 30, 31, 32, 36, 37 each have their own `RenderTexture.cs` with a separate projection matrix — those also got `PerspectiveFovLH` (initially missed).
- Tut39 `ViewPoint.cs` (the shadow-map light camera) needs the same LH conversion as the main `Camera.cs`.

### Tut30-39 GL4.cs "black screen" trap

Several `GL4.cs` files (Tut30-35, 38, 39) had the `PerspectiveFovLH` helper defined but still called `Matrix4X4.CreatePerspectiveFieldOfView` on line ~27. Projection stayed RH while cameras/world matrices were LH, so everything ended up outside the clip volume — every tutorial was a black screen with no errors. The helper exists but isn't used until you edit the call site. When creating new GL tutorials, grep the initialization line.

## OpenGL texture V-coord convention

`Texture.cs` (OpenGL) uploads TGAs top-down regardless of the TGA header's origin bit — effectively storing textures with V=0 at top, the same as DX. So `.txt` model files (which use DX-convention UVs) can be sampled directly without flipping V.

Two places had a leftover `1 - v` flip from the original RH port:

- **Tut05, Tut06** hardcoded triangle UVs — flipped to DX convention (V=0 = top).
- **Tut31 `Model.cs`** `LoadModel` — was doing `1.0f - float.Parse(p[4])` on every loaded vertex. Removed, so the marble bath, wall and ground render the same way in OpenGL and DX11.

Any future OpenGL tutorial that loads models from `.txt` should use the raw V coordinate. If a tutorial looks "upside-down" compared to the Windows sibling, look here first.

## Windows Tut31 (Water)

Three shaders copied verbatim from `dx11win10tut31_src`:

- **`light.vs`/`light.ps`** — basic directional-light + single texture (bath, wall, ground in the main pass).
- **`refraction.vs`/`refraction.ps`** — same lighting but with a `cbuffer ClipPlaneBuffer` and `SV_ClipDistance0` in the pixel-input struct so the vertex shader outputs a per-vertex clip distance. No state toggle needed on the DX11 side (unlike OpenGL's `glEnable(GL_CLIP_DISTANCE0)`) — writing `SV_ClipDistance0` is enough.
- **`water.vs`/`water.ps`** — samples three SRVs: `t0` = reflection RTV (top-flipped via `-y/w/2 + 0.5`), `t1` = refraction RTV, `t2` = water model's own normal map. The shader's register slots differ from the OpenGL version (which uses glUniform1i to assign slots by name) — bind `PSSetShaderResources(0..2)` in that exact order.

The water model is loaded through the same shared `Model.cs` as everything else (8-float vertex), but the water input layout only declares `POSITION` + `TEXCOORD` and skips the normal — stride still 32.

### Rastertek Tut31 "pic0099 vs pic0100" confusion

The hero image at the top of <https://www.rastertek.com/dx11win10tut31.html> (`pic0099.gif`) shows the wall on the right; the lower screenshots (`pic0100.gif`, `pic0101.gif`) show it on the left. The actual source code uses camera `SetPosition(-10, 6, -10)` + `SetRotation(0, 45, 0)`, which produces the pic0100/pic0101 view. `pic0099` is a separately-rendered hero shot from a different angle — **do not "fix" the port to match it**.

## OpenGL csproj model asset links

Most OpenGL tutorials link `Assets/Data/**` via `<Content Include="..\..\Assets\Data\**" LinkBase="Data" />` but are missing the matching `<Content Include="..\..\Assets\Models\**" LinkBase="Models" />` line. Models end up absent from `bin/Debug/net8.0/Models/` and the tutorial errors with `Model not found: Models/xxx.txt` on startup. Add both lines when creating a new OpenGL tutorial.

## Yaw rotation is left-handed in Silk.NET.Maths

`Matrix4X4.CreateFromYawPitchRoll(yaw, 0, 0)` rotates `(0,0,1)` to `(sin yaw, 0, cos yaw)` — identical to DirectXMath's `XMMatrixRotationRollPitchYaw(0, yaw, 0)`. Both produce the same result, so the Windows and OpenGL cameras are drop-in compatible and there is no need for a custom `RotationYLH` when composing yaw+pitch+roll via the library helper. (`DXMath.RotationYLH` exists but is currently unused by the port.)

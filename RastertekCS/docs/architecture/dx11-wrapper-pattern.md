# DX11 Wrapper Pattern

The Windows tutorials wrap Silk.NET's DX11 API. Each tutorial directory has the same skeleton; new tutorials are clones of the previous one with shader/class additions.

## Class skeleton

| Class | Role |
|---|---|
| `Program.cs` | Entry point: creates `SystemFramework`, calls `Initialize` then `Run`. |
| `System/SystemFramework.cs` | Window creation (Silk.NET `IWindow`), keyboard/mouse hookup, render loop. |
| `System/SystemConfiguration.cs` | Constants for screen size, vsync, fullscreen, near/far depth. |
| `System/Timer.cs` (when needed) | `Stopwatch`-based per-frame ms delta. |
| `Inputs/Input.cs` | Tracks keyboard (and mouse for Tut16+). |
| `Graphics/DX11.cs` | Device, swap chain, RTV, DSV, rasterizer, viewport, optional blend & disabled-depth states, projection/world/ortho matrices. |
| `Graphics/DXMath.cs` | Left-handed math helpers (`LookAtLH`, `PerspectiveFovLH`, `OrthographicLH`, `RotationYLH`). Silk.NET.Maths only ships RH variants. |
| `Graphics/Camera.cs` | View matrix from position + yaw/pitch/roll. From Tut21 has `GetPosition()` (specular). From Tut30 has `RenderReflection`/`GetReflectionViewMatrix`. |
| `Graphics/Texture.cs` | Loads `.tga` (24/32 bpp, both pixel orderings) into `ID3D11Texture2D` + SRV + sampler. Generates a fallback checkerboard if file missing. |
| `Graphics/Model.cs` | Loads `.txt` model (Vertex Count + interleaved x y z tu tv nx ny nz), creates vertex+index buffers, owns the texture(s). Variants: 1-texture, 2-texture, 3-texture, and tangent/binormal-augmented (Tut20+). |
| `Graphics/<Name>Shader.cs` | One per HLSL shader pair. Compiles `.vs`/`.ps` at runtime with `D3DCompiler`, builds `ID3D11InputLayout`, owns matrix and per-shader cbuffers, optionally a sampler. |
| `Graphics/GraphicsFramework.cs` | Owns the per-tutorial scene: camera, model(s), shader(s), light(s), per-frame `Frame()` and `Render()`. |

## Shader class pattern

Every `*Shader.cs` follows the same shape:

1. Private cbuffer structs (`MatrixBufferType`, plus per-shader extras like `LightBufferType`, `CameraBufferType`, `FogBufferType`, etc). Layout matches the HLSL `cbuffer` exactly, with explicit `padding` floats for HLSL's 16-byte packing rules.
2. `ComPtr` fields for vertex shader, pixel shader, input layout, each cbuffer, and (when the shader binds a texture itself) a sampler.
3. `Initialize(DX11)` → `InitializeShader(...)` which compiles vs/ps blobs, creates the shaders, builds the input layout, creates the cbuffers and sampler.
4. `Render(DX11, indexCount, matrices..., shader-specific args...)` → `SetShaderParameters(...)` (transposes matrices, maps each cbuffer with `Map.WriteDiscard`, binds via `VSSetConstantBuffers`/`PSSetConstantBuffers`, binds SRVs via `PSSetShaderResources` if needed) → `RenderShader(...)` (sets layout, vs, ps, sampler, then `DrawIndexed`).
5. `Shutdown()` releases everything in reverse order.

Two SRV-binding conventions exist in the codebase:
- **Model-bound** (default for the lighting tutorials): `m_Model.SetTexture(DirectX, slot)` binds the SRV+sampler before `Shader.Render(...)`. The shader's `Render` signature has no SRV parameter.
- **Shader-bound** (used in Tut12 `TextureShader`, Tut25 `TextureShader`, Tut29 `TextureShader`/`TransparentShader`, Tut30 `ReflectionShader`): the shader takes a `ComPtr<ID3D11ShaderResourceView>` parameter and binds it itself; the shader also owns its own sampler. This is needed when the same shader is reused with different textures (e.g. render-target SRV vs model texture SRV).

When extending, pick whichever matches the surrounding tutorial's pattern.

## Vertex layouts

| Layout | Used by | Stride |
|---|---|---|
| pos+tex (R32G32B32 + R32G32) | `Bitmap` (Tut12), `Sprite` (Tut13), `Text` (Tut14/15/16/23), `DisplayPlane` (Tut25), `Translate`/`ClipPlane`/`Fog`/`Transparent`/`Reflection`/`Texture` shaders | 20 |
| pos+tex+normal (12+8+12) | `Model` (Tut07-onwards, except below) | 32 |
| pos+tex+normal+tangent+binormal (12+8+12+12+12) | `Model` (Tut20+, normal-mapping path) | 56 |

The HLSL declares `float4 position : POSITION` even though the layout supplies `float3` — the shader sets `input.position.w = 1.0f` explicitly. D3D pads the missing `w` with 1.0 (or 0.0 for float4 attributes).

DisplayPlane uses the pos+tex+normal stride (32) so it shares an input layout with `Model` even though its normal is unused.

## Cbuffer packing rules

HLSL packs cbuffer fields in 16-byte (`float4`) registers. Common pitfalls used as references in this codebase:

- `float3 lightDirection; float padding;` → fits in one register; `padding` is mandatory.
- `float specularPower; float3 lightDirection;` → also fits in one register (note the order).
- `float4 ambientColor; float4 diffuseColor; float3 lightDirection; float specularPower; float4 specularColor;` → 5 × 16 bytes = 80 bytes (Tut10).
- Arrays of `float4` always start on a new register; arrays of `float3` are padded to `float4` per element. That's why `LightPositionBufferType` for Tut11 has explicit `pad1..pad4` floats between `posN{X,Y,Z}` triples.

## DX11 state additions across tutorials

The base `DX11.cs` from Tut02-08 has device, swap chain, depth-stencil, rasterizer. Subsequent tutorials extend it as needed:

- **Tut12** (2D bitmap): `m_orthoMatrix`, `OrthographicLH` helper, `m_depthDisabledStencilState`, `TurnZBufferOff`/`TurnZBufferOn`.
- **Tut14** (font/text): `m_alphaEnableBlendingState`, `m_alphaDisableBlendingState`, `EnableAlphaBlending`/`DisableAlphaBlending`. Source-over blend (`SrcBlend = One`, `DestBlend = InvSrcAlpha`) for premultiplied font glyphs.
- **Tut25** (render to texture): cached `m_viewport`, `SetBackBufferRenderTarget`, `ResetViewport`.
- **Tut29** (transparency): `SrcBlend = SrcAlpha` instead of `One` (different blend equation than Tut14's font path).

Tutorials that need both font alpha-blending and transparency would have to choose one blend state — none of the current tutorials do.

## DPI handling

Silk.NET on Windows reports `window.Size` in logical pixels and `window.FramebufferSize` in physical pixels. From Tut12 onward, `SystemFramework.InitializeWindows` uses `m_window.FramebufferSize` so the swap chain and ortho matrix match the actual backbuffer. Lesson learned during Tut12: high-DPI Windows would otherwise render 2D content at half the expected pixel coverage (or DWM stretches the backbuffer).

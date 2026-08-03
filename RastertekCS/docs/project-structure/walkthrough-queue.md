# Walkthrough verdicts and remaining defects

Pavel judged all 50 DirectX tutorials side by side against the original C++
binaries on **2026-08-03** (`Tools/shot.cmd <NN> live`, original left, port
right). This is what came out of it and what is left.

## Status

Every tutorial in the walkthrough is accepted. One residual, in Tut42, is
open and described at the bottom.

Accepted outright: 02-06, 08, 10, 14-18, 20-22, 26, 28, 29, 32, 33, 35, 36,
47, 48, 49.

Accepted after a fix this session: 03 (clear colour), 07 and 09 (light
direction), 11 (plane model, then the invented camera tilt), 12 (bitmap
sizing), 13 (sprite data), 19 (wrong tutorial's alpha map), 31 and 45 (wrong
tutorial's texture), 37, 38 (blend recipe), 39 (wrong logo), 42 (depth
range), and the group reported as "matte" - 23, 25, 27, 30, 34, 40, 41, 43,
44, 46, 50 - which were all the missing mip chain.

No original exists for **Tut24**: Rastertek never published the Maya chapter's
source, and the port is a stub. Nothing to compare, nothing to do.

## What the walkthrough actually found

Almost none of it was one-off. Each visible defect turned out to be an
instance of a class, and each class was then swept across all 50 tutorials at
once. In order of how much they affected:

| Class | Instances |
|---|---|
| shaders rewritten instead of copied | 76 of 146 |
| texture created without a mip chain | 43 |
| rotation constants invented | 13 |
| blend recipe wrong (three recipes exist, the port used one) | 8 |
| depth range wrong in the shadow tutorials | 4 |
| an asset name carrying different files in different tutorials | 4 |
| camera position | 3 |
| light direction | 2 |
| clear colour, camera tilt, cube geometry, plane model, sprite timing | 1 each |

The lesson worth keeping: a symptom Pavel could see in one tutorial was
usually present, invisibly, in a dozen others. Chasing the report alone would
have fixed the tutorial and left the class.

## Open: Tut42's sphere

The scene, both shadows, the geometry and the layout match. The sphere's
surface does not: the original shows the mottled cloud-like ice texture,
the port shows a smoother surface with faint arcs.

Verified identical or equal, so **do not re-check these**:

- `ice.tga` (looked at directly via `Tools/tga-to-png.cmd` - it is the
  cloudy texture), `sphere.txt`, the four shaders, the sampler description,
  the csproj link, the shadow map bias 0.0022, both light positions and
  colours, the shader parameter set
- the mip recipe: Tut42's own `textureclass.cpp` generates mips exactly as
  the port now does
- mip *selection*: pinning the sampler to the top level (`MaxLOD = 0`)
  changed nothing on the sphere

The cube and the ground are textured correctly in the same frame, so the
input layout, the model loader and the slot bindings are sound. The
difference is confined to the sphere's shading and is not visible by reading
the sources.

Magnified 4x (`Tools/crop.ps1`), the difference is structural rather than a
loss of sharpness: the original's surface reads as cloud-like ice, while the
port lays horizontal ring-shaped bands over it, following lines of latitude.

**UV orientation is ruled out.** The sphere is not generated in code - it is
read from `sphere.txt`, which is byte-identical - and the port's `LoadModel`
takes the same columns as the original (`tokens[3]` to `tu`, `tokens[4]` to
`tv`), copying them into the vertex buffer unchanged. No swap, no flip. So
the usual suspects for a rotated sphere texture (atan2 argument order,
winding, a V-flip on load) do not apply here.

Latitude-aligned banding points at the shading rather than the texture, so
the next thing to compare is the normal handling and the diffuse term - not
the UVs, and not the texture.

Beyond that, the instrument matters more than another pass over the sources:
a RenderDoc frame capture of both processes, or dumping the constant buffer
contents at runtime and diffing the numbers.

## How to re-run one

```
.\RastertekCS\Tools\shot.cmd 23 live    # original left, port right, both topmost
.\RastertekCS\Tools\shot.cmd 00 stop    # close the pair
```
The pair is killed and the port rebuilt automatically on the next `live`.

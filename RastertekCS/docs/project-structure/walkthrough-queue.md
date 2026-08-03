# Walkthrough verdicts and remaining defects

Pavel judged all 50 DirectX tutorials side by side against the original C++
binaries on **2026-08-03** (`Tools/shot.cmd <NN> live`). This is the queue
that came out of it.

## Accepted

02, 04, 05, 06, 08, 10, 14, 15, 16, 17, 18, 20, 21, 22, 26, 28, 29, 32, 33,
35, 36, 47, 48, 49 - and 03 after its clear colour was fixed mid-walkthrough.

## Closed by the mip-chain fix

Reported as "matte" - the C++ surface soft, the C# one hard-edged. One cause,
fixed across all 43 affected `Texture.cs` (see the commit "build the full mip
chain, as textureclass.cpp does"). **Needs re-judging, not re-diagnosing:**

23, 25, 27, 30, 31, 34, 40, 41, 43, 44, 46, 50

Two of these carry a second, separate symptom - see below (11, 37, 45).

## Still open, one by one

| NN | Symptom | Note |
|----|---------|------|
| 07 | the right-hand side goes black almost immediately as it rotates right; Tut08 does the same thing correctly | Tut07 has no ambient term yet, so a dark side is expected - but compare against the original's falloff and the normal transform |
| 09 | ambient does not work | the tutorial's whole subject |
| 11 | colours duller, zone boundaries blurred, plane sits higher | partly the mip chain; the plane height is separate |
| 12 | wrong size | |
| 13 | sprites differ and behave differently | |
| 19 | green appears at the top and left; should only be on the right | alpha map orientation |
| 37 | fade does not work | also reported matte |
| 38 | harness crashed on a null `MainWindowHandle` - fixed in `live.ps1`; the tutorial itself is unjudged | |
| 39 | different pictures | |
| 45 | **reversed**: C++ is the sharper one, C# the matte one | so not the mip chain - a different cause |

## No original to compare against

| NN | |
|----|---|
| 24 | `NO_ORIGINAL` - Rastertek never published a source archive for the Maya chapter; the port is a stub |
| 42 | `BUILD_FAILED` - `RastertekOriginal/DirectX/Tutorial42` has no `applicationclass.cpp` and no shaders of the port's names. Resolve the source before judging |

## How to re-run one

```
.\RastertekCS\Tools\shot.cmd 23 live    # original left, port right, both topmost
.\RastertekCS\Tools\shot.cmd 00 stop    # close the pair
```
The pair is killed and the port rebuilt automatically on the next `live`.

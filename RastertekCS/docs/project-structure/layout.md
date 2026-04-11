# Project Layout

```
RastertekCS/
├── Assets/                # Shared textures + models, linked into each tutorial's bin/
│   ├── Data/              # *.tga, font01.txt, Sprite.txt
│   └── Models/            # *.txt model files (Cube.txt, sphere.txt, square.txt, ...)
├── OpenGL/
│   ├── Tutorial02..Tutorial59  # Linux/OpenGL ports (Silk.NET.OpenGL)
│   ├── run.sh             # Build + run helper: ./run.sh NN
│   ├── run.bat            # Same on Windows for OpenGL builds
│   └── gen_tutorials.sh
├── Windows/
│   ├── Tutorial02..Tutorial30  # Windows/DX11 ports (Silk.NET.Direct3D11)
│   ├── nuget.config
│   └── run.bat            # Build + run helper: run.bat NN
├── Tools/
│   └── ObjToModel/        # Wavefront .obj → Rastertek .txt converter
│                          # (was OpenGL/Tutorial08, relocated)
└── docs/                  # This folder
    ├── architecture/
    ├── references/
    └── project-structure/
```

## Per-tutorial layout

Both `OpenGL/TutorialNN` and `Windows/TutorialNN` follow the same skeleton:

```
TutorialNN/
├── Graphics/
│   ├── DX11.cs (or GL4.cs)
│   ├── DXMath.cs (Windows only)
│   ├── Camera.cs
│   ├── Texture.cs
│   ├── Model.cs
│   ├── <FeatureName>Shader.cs (one or more)
│   ├── (Light.cs, Frustum.cs, ModelList.cs, Position.cs, ...)
│   └── GraphicsFramework.cs
├── Inputs/
│   └── Input.cs
├── Shaders/
│   ├── *.vs
│   └── *.ps
├── System/
│   ├── SystemConfiguration.cs
│   ├── SystemFramework.cs
│   └── (Timer.cs, Fps.cs)
├── Program.cs
└── TutorialNN.csproj
```

## Asset linking

Each `TutorialNN.csproj` `<ItemGroup>` lists the shaders it copies as `<None Update>` (already in `Shaders/`) and links shared assets from `Assets/` via `<None Include>` with a `Link="..."`. Example (Tut21):

```xml
<None Update="Shaders\SpecMap.vs" CopyToOutputDirectory="PreserveNewest" />
<None Update="Shaders\SpecMap.ps" CopyToOutputDirectory="PreserveNewest" />
<None Include="..\..\Assets\Data\stone02.tga" Link="Data\stone02.tga" CopyToOutputDirectory="PreserveNewest" />
<None Include="..\..\Assets\Data\normal02.tga" Link="Data\normal02.tga" CopyToOutputDirectory="PreserveNewest" />
<None Include="..\..\Assets\Data\spec02.tga" Link="Data\spec02.tga" CopyToOutputDirectory="PreserveNewest" />
<None Include="..\..\Assets\Models\Cube.txt" Link="Models\Cube.txt" CopyToOutputDirectory="PreserveNewest" />
```

The `Link=` path is what the running app sees relative to its working directory (`bin/Debug/net8.0/`). Code uses paths like `"Data/stone02.tga"` and `"Models/Cube.txt"`. The csproj line and the code path **must agree** — when swapping a model in code, update the csproj too.

`<None Update>` applies to files already inside the project tree (the per-tutorial `Shaders/`). `<None Include>` adds an external file with a virtual `Link` location. Both produce the same on-disk layout in `bin/`.

## Tutorial naming and namespaces

| OpenGL | Windows |
|---|---|
| `RastertekCS.OpenGL.TutorialNN` | `RastertekCS.Windows.TutorialNN` |
| `RastertekCS.OpenGL.TutorialNN.Graphics` | `RastertekCS.Windows.TutorialNN.Graphics` |
| `RastertekCS.OpenGL.TutorialNN.System` | `RastertekCS.Windows.TutorialNN.System` |
| `RastertekCS.OpenGL.TutorialNN.Inputs` | `RastertekCS.Windows.TutorialNN.Inputs` |

Be careful with `System` — `using System;` is implicit, and a class named `Buffer` inside `RastertekCS.Windows.TutorialNN.System` shadows `global::System.Buffer`. Use `global::System.Buffer.MemoryCopy(...)` (Tut12 `Bitmap.cs`, Tut13 `Sprite.cs`, Tut15 `Text.cs` etc.).

## Window titles

`SystemFramework.InitializeWindows()` sets `options.Title = "TutorialNN - <topic> (DirectX 11)"` (Windows) or `(OpenGL)` (Linux). Topics for the Tut07-30 set:

| NN | Topic |
|---|---|
| 07 | 3D Model Rendering |
| 08 | 3D Object Transforms |
| 09 | Ambient Lighting |
| 10 | Specular Lighting |
| 11 | Multiple Point Lights |
| 12 | 2D Bitmap Rendering |
| 13 | Sprite Animation |
| 14 | Font/Text Rendering |
| 15 | FPS Counter |
| 16 | Mouse Input |
| 17 | Multi Texturing |
| 18 | Light Maps |
| 19 | Alpha Mapping |
| 20 | Normal Mapping |
| 21 | Specular Mapping |
| 22 | Shader Manager |
| 23 | Frustum Culling |
| 25 | Render to Texture |
| 26 | Fog |
| 27 | Clip Plane |
| 28 | Texture Translation |
| 29 | Transparency |
| 30 | Planar Reflection |

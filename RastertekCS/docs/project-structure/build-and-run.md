# Build and Run Workflow

The same `.csproj` files build identically on Windows and Linux via .NET 8. Tutorial selection is via small wrapper scripts.

## Windows

### Run a single tutorial

```cmd
cd RastertekCS\Windows
run.bat 17
```

`run.bat NN` (in both `RastertekCS\OpenGL` and `RastertekCS\Windows`):

1. `cd` into `TutorialNN`
2. `dotnet restore`, `dotnet build --no-restore`
3. `dotnet TutorialNN.dll` from `bin\Debug\net8.0\`
4. Logs everything (build + run output + exit code) to `TutorialNN\build_log.txt`

### Manual build

```cmd
dotnet build RastertekCS\Windows\Tutorial17
dotnet run --project RastertekCS\Windows\Tutorial17
```

### Open in Visual Studio / Rider

There is **no `.sln`** in the repo. Open the individual `TutorialNN.csproj` directly, or create a local solution file:

```cmd
dotnet new sln -n Rastertek
for /d %d in (RastertekCS\Windows\Tutorial*) do dotnet sln Rastertek.sln add %d\*.csproj
```

(`.sln` is intentionally not committed — each tutorial is independent and they don't share project references.)

## Linux

### Run a single tutorial (OpenGL only — Windows tutorials are DX11 and Linux can't run them)

```bash
cd RastertekCS/OpenGL
./run.sh 17
```

`run.sh NN` mirrors `run.bat`: restore, build, run, log to `TutorialNN/build_log.txt`.

### Build a Windows tutorial on Linux for syntax-check only

```bash
dotnet build RastertekCS/Windows/Tutorial17
```

This compiles successfully on Linux even though the resulting binary won't run there — Silk.NET.Direct3D11 is available as a managed package; only the actual D3D11 runtime requires Windows. Use this for fast local iteration before pushing to a Windows machine.

## Push / pull workflow used during this port

Windows is the only place that can actually visualise the DX11 tutorials, so the development loop has been:

1. Edit + `dotnet build` on Linux to confirm it compiles.
2. Commit and push from Linux.
3. Pull on Windows.
4. `run.bat NN` on Windows.
5. `git pull` back on Linux to receive the freshly written `build_log.txt`.

`build_log.txt` files are intentionally tracked in git so each side can see the other's last run. The Windows logs include `dotnet --version`, OS string, and exit code, which helps diagnose runtime errors after a pull.

## Git tips for the loop

- Always run git from the repo root or use `git -C /path/to/Rastertek <command>` — combinations of `cd` + `git` in a single shell command can trigger safety prompts in some terminal sandboxes.
- Avoid `git add .` near `bin/` directories that haven't been built yet — explicitly stage `RastertekCS/Windows/TutorialNN` or `RastertekCS/OpenGL/TutorialNN`.
- `build_log.txt` conflicts on pull are normal when both sides ran the same tutorial — `git checkout HEAD -- RastertekCS/.../build_log.txt` (or accept whichever side is fresher) and re-pull.

## Verifying shaders against Rastertek source

The shader files in each tutorial's `Shaders/` folder are intended to be byte-identical to the official Rastertek `.vs`/`.ps` source. To verify (after fetching the Rastertek zip into `/tmp` or `%TEMP%`):

**Windows (PowerShell):**

```powershell
$tut = '17'
$src = "$env:TEMP\dx11tut$tut\dx11win10tut${tut}_src\source"
$dst = "RastertekCS\Windows\Tutorial$tut\Shaders"
Get-ChildItem $src -Include *.vs,*.ps -Recurse | ForEach-Object {
    $name = $_.Name
    $target = Get-ChildItem $dst -Filter $name -Recurse | Select-Object -First 1
    if (-not $target) { Write-Host "MISSING $name" }
    elseif ((Get-FileHash $_.FullName).Hash -ne (Get-FileHash $target.FullName).Hash) { Write-Host "DIFFERS $name" }
}
```

**Linux (bash):**

```bash
tut=17
src=/tmp/dx11tut$tut/dx11win10tut${tut}_src/source
dst=RastertekCS/Windows/Tutorial$tut/Shaders
for f in "$src"/*.vs "$src"/*.ps; do
  [ -f "$f" ] || continue
  base=$(basename "$f")
  target=$(find "$dst" -iname "$base" -print -quit)
  [ -z "$target" ] && echo "MISSING $base" && continue
  diff -q "$f" "$target" >/dev/null 2>&1 || echo "DIFFERS $base"
done
```

(Fetch the source archive once with `curl -fsSL https://www.rastertek.com/dx11win10tutNN_src.zip -o src.zip` and extract under `%TEMP%\dx11tutNN\` or `/tmp/dx11tutNN/`.)

# Copy original shaders / assets over the port's copies so they are
# byte-identical. RastertekCS stays self-contained - it keeps its own copies -
# but those copies must be exactly the original's bytes.
#
#   powershell -File sync-original.ps1 [-Series DirectX] [-Tutorial 07] [-Assets] [-WhatIf]
#
# Writes only inside RastertekCS. Never touches RastertekOriginal.

param(
    [string]$Series = 'DirectX',
    [string]$Tutorial = '',
    [switch]$Assets,
    [string]$Asset = '',
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$orig = Join-Path $root "RastertekOriginal\$Series"
$port = Join-Path $root 'RastertekCS'

function Get-Hash($path) { (Get-FileHash -Path $path -Algorithm SHA256).Hash }

$copied = 0; $already = 0; $noOriginal = 0

# ---------------------------------------------------------------- shaders ---
$filter = if ($Tutorial -ne '') { "Tutorial$Tutorial" } else { 'Tutorial*' }

foreach ($dir in Get-ChildItem (Join-Path $port 'Windows') -Directory -Filter $filter) {
    $shaderDir = Join-Path $dir.FullName 'Shaders'
    if (-not (Test-Path $shaderDir)) { continue }
    $nn = $dir.Name -replace 'Tutorial', ''
    $origDir = Join-Path $orig "Tutorial$nn\Engine"
    if (-not (Test-Path $origDir)) { continue }

    foreach ($f in Get-ChildItem $shaderDir -File) {
        $o = Join-Path $origDir $f.Name
        if (-not (Test-Path $o)) {
            Write-Output "  no-original  Tut$nn $($f.Name)"
            $noOriginal++
            continue
        }
        if ((Get-Hash $f.FullName) -eq (Get-Hash $o)) { $already++; continue }

        if ($WhatIf) {
            Write-Output "  would copy   Tut$nn $($f.Name)"
        } else {
            Copy-Item -Path $o -Destination $f.FullName -Force
            Write-Output "  copied       Tut$nn $($f.Name)"
        }
        $copied++
    }
}

# ----------------------------------------------------------------- assets ---
if ($Assets) {
    $origAssets = @{}
    foreach ($tut in Get-ChildItem $orig -Directory -Filter 'Tutorial*') {
        $dataDir = Join-Path $tut.FullName 'Engine\data'
        if (-not (Test-Path $dataDir)) { continue }
        foreach ($f in Get-ChildItem $dataDir -File -Recurse) {
            $key = $f.Name.ToLower()
            if (-not $origAssets.ContainsKey($key)) { $origAssets[$key] = @() }
            $origAssets[$key] += $f.FullName
        }
    }

    foreach ($f in Get-ChildItem (Join-Path $port 'Assets') -File -Recurse) {
        # -Asset narrows the sync to one file; without it every asset is checked.
        if ($Asset -ne '' -and $f.Name -ne $Asset) { continue }
        $key = $f.Name.ToLower()
        if (-not $origAssets.ContainsKey($key)) { $noOriginal++; continue }

        $hash = Get-Hash $f.FullName
        $match = $false
        foreach ($cand in $origAssets[$key]) {
            if ((Get-Hash $cand) -eq $hash) { $match = $true; break }
        }
        if ($match) { $already++; continue }

        # More than one original may share a name; if they disagree the choice
        # is not mechanical, so report instead of guessing.
        $distinct = ($origAssets[$key] | ForEach-Object { Get-Hash $_ } | Sort-Object -Unique)
        if ($distinct.Count -gt 1) {
            Write-Output "  AMBIGUOUS    $($f.Name) - $($distinct.Count) different originals share this name, not copying"
            continue
        }

        if ($WhatIf) {
            Write-Output "  would copy   $($f.Name)"
        } else {
            Copy-Item -Path $origAssets[$key][0] -Destination $f.FullName -Force
            Write-Output "  copied       $($f.Name)"
        }
        $copied++
    }
}

Write-Output ''
Write-Output "$copied to copy/copied, $already already identical, $noOriginal without an original"

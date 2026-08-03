# Audit the C# port against the original C++ tutorials.
# The original is the source of truth: shaders and asset data files must be
# byte-identical. This reports every file that is not.
#
#   powershell -File audit-original.ps1 [-Series DirectX] [-ShowOk]

param(
    [string]$Series = 'DirectX',
    [switch]$ShowOk
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)   # repo root
$orig = Join-Path $root "RastertekOriginal\$Series"
$port = Join-Path $root 'RastertekCS'

function Get-Hash($path) { (Get-FileHash -Path $path -Algorithm SHA256).Hash }

# Text compare that ignores line-ending style and a trailing newline, so a
# CRLF/LF difference is reported separately from a real content change.
function Test-SameText($a, $b) {
    $ta = (Get-Content -Raw -Path $a) -replace "`r`n", "`n"
    $tb = (Get-Content -Raw -Path $b) -replace "`r`n", "`n"
    return $ta.TrimEnd("`n") -eq $tb.TrimEnd("`n")
}

# ---------------------------------------------------------------- shaders ---
Write-Output '=== SHADERS ==='
$shaderIdentical = 0; $shaderDiffer = 0; $shaderMissing = 0; $shaderEol = 0

foreach ($dir in Get-ChildItem (Join-Path $port 'Windows') -Directory -Filter 'Tutorial*') {
    $shaderDir = Join-Path $dir.FullName 'Shaders'
    if (-not (Test-Path $shaderDir)) { continue }
    $nn = $dir.Name -replace 'Tutorial', ''
    $origDir = Join-Path $orig "Tutorial$nn\Engine"
    if (-not (Test-Path $origDir)) { continue }

    foreach ($f in Get-ChildItem $shaderDir -File) {
        $o = Join-Path $origDir $f.Name
        if (-not (Test-Path $o)) {
            Write-Output "  MISSING  Tut$nn $($f.Name) - no original of that name"
            $shaderMissing++
            continue
        }
        if ((Get-Hash $f.FullName) -eq (Get-Hash $o)) {
            $shaderIdentical++
            if ($ShowOk) { Write-Output "  ok       Tut$nn $($f.Name)" }
        } elseif (Test-SameText $f.FullName $o) {
            $shaderEol++
            if ($ShowOk) { Write-Output "  eol-only Tut$nn $($f.Name)" }
        } else {
            Write-Output "  DIFFERS  Tut$nn $($f.Name)"
            $shaderDiffer++
        }
    }
}
Write-Output "shaders: $shaderIdentical byte-identical, $shaderEol differ only in line endings, $shaderDiffer differ in content, $shaderMissing without an original"

# ----------------------------------------------------------------- assets ---
Write-Output ''
Write-Output '=== ASSETS ==='

# Originals are per-tutorial; the port keeps one shared pool. An asset is
# considered faithful if it matches the original of the same name in ANY
# tutorial of the series.
$origAssets = @{}
foreach ($tut in Get-ChildItem $orig -Directory -Filter 'Tutorial*') {
    $dataDir = Join-Path $tut.FullName 'Engine\data'
    if (-not (Test-Path $dataDir)) { continue }
    foreach ($f in Get-ChildItem $dataDir -File -Recurse) {
        $key = $f.Name.ToLower()
        if (-not $origAssets.ContainsKey($key)) { $origAssets[$key] = @() }
        $origAssets[$key] += (Get-Hash $f.FullName)
    }
}
Write-Output "(original asset pool: $($origAssets.Count) distinct file names)"

$assetIdentical = 0; $assetDiffer = 0; $assetMissing = 0
foreach ($f in Get-ChildItem (Join-Path $port 'Assets') -File -Recurse) {
    $key = $f.Name.ToLower()
    if (-not $origAssets.ContainsKey($key)) {
        if ($ShowOk) { Write-Output "  no-original  $($f.Name)" }
        $assetMissing++
        continue
    }
    if ($origAssets[$key] -contains (Get-Hash $f.FullName)) {
        $assetIdentical++
        if ($ShowOk) { Write-Output "  ok           $($f.Name)" }
    } else {
        Write-Output "  DIFFERS      $($f.Name)"
        $assetDiffer++
    }
}
Write-Output "assets: $assetIdentical identical, $assetDiffer differ, $assetMissing without an original in $Series"

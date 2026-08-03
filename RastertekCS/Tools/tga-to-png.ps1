# Convert an uncompressed 24/32-bit TGA to PNG so it can be looked at.
# Uses the same orientation rule as the port's loader: bit 5 of the image
# descriptor selects top-left origin, otherwise the file is bottom-up.
#
#   powershell -File tga-to-png.ps1 -In <file.tga> -Out <file.png>

param(
    [Parameter(Mandatory=$true)][string]$In,
    [Parameter(Mandatory=$true)][string]$Out
)

Add-Type -AssemblyName System.Drawing

# .NET's current directory is not PowerShell's, so relative paths must be
# resolved before they reach System.IO.
$In  = (Resolve-Path $In).Path
$Out = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Out))

$d = [System.IO.File]::ReadAllBytes($In)
$idLength = [int]$d[0]
$imageType = [int]$d[2]
$width  = [int]$d[12] -bor ([int]$d[13] -shl 8)
$height = [int]$d[14] -bor ([int]$d[15] -shl 8)
$bpp = $d[16]
$descriptor = $d[17]

if ($imageType -ne 2) { Write-Output "unsupported image type $imageType"; exit 1 }
if ($bpp -ne 24 -and $bpp -ne 32) { Write-Output "unsupported bpp $bpp"; exit 1 }

$channels = $bpp / 8
$offset = 18 + $idLength
$topLeft = ($descriptor -band 0x20) -ne 0

$bmp = New-Object System.Drawing.Bitmap $width, $height
for ($y = 0; $y -lt $height; $y++) {
    $srcRow = if ($topLeft) { $y } else { $height - 1 - $y }
    $rowOff = $offset + $srcRow * $width * $channels
    for ($x = 0; $x -lt $width; $x++) {
        $p = $rowOff + $x * $channels
        $b = $d[$p]; $g = $d[$p + 1]; $r = $d[$p + 2]
        $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $r, $g, $b))
    }
}
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output "OK ${width}x${height} bpp=$bpp topLeft=$topLeft -> $Out"

# Crop a region out of a PNG and scale it up, for comparing details that are
# too small to judge at full frame.
#
#   powershell -File crop.ps1 -In shot.png -Out detail.png -X 270 -Y 60 -W 130 -H 130 [-Scale 4]

param(
    [Parameter(Mandatory=$true)][string]$In,
    [Parameter(Mandatory=$true)][string]$Out,
    [Parameter(Mandatory=$true)][int]$X,
    [Parameter(Mandatory=$true)][int]$Y,
    [Parameter(Mandatory=$true)][int]$W,
    [Parameter(Mandatory=$true)][int]$H,
    [int]$Scale = 4
)

Add-Type -AssemblyName System.Drawing

$In  = (Resolve-Path $In).Path
$Out = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Out))

$src = [System.Drawing.Image]::FromFile($In)
$rect = New-Object System.Drawing.Rectangle $X, $Y, $W, $H
$dst = New-Object System.Drawing.Bitmap ($W * $Scale), ($H * $Scale)
$g = [System.Drawing.Graphics]::FromImage($dst)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
$g.DrawImage($src, (New-Object System.Drawing.Rectangle 0, 0, ($W * $Scale), ($H * $Scale)), $rect, [System.Drawing.GraphicsUnit]::Pixel)
$g.Dispose()
$dst.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$dst.Dispose()
$src.Dispose()
Write-Output "OK ${W}x${H} at $X,$Y scaled ${Scale}x -> $Out"

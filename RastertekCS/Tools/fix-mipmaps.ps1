# One-shot: give every port's Texture.cs the full mip chain the original
# textureclass.cpp builds (MipLevels 0 + GENERATE_MIPS + GenerateMips call).
# Without it every surface samples the top mip and minified geometry aliases
# instead of softening - the "matte vs crisp" difference Pavel spotted.
#
#   powershell -File fix-mipmaps.ps1 [-WhatIf]

param([switch]$WhatIf)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$files = Get-ChildItem (Join-Path $root 'RastertekCS\Windows') -Recurse -Filter 'Texture.cs'

$patched = 0; $already = 0; $skipped = @()

foreach ($f in $files) {
    $t = Get-Content -Raw -Path $f.FullName
    if ($t -match 'GenerateMips') { $already++; continue }

    $orig = $t

    $t = $t -replace '(?m)^(\s*)MipLevels = 1,(\r?\n\s*ArraySize = 1,)', '${1}MipLevels = 0,${2}'
    $t = $t -replace 'BindFlags = \(uint\)BindFlag\.ShaderResource,',
                     'BindFlags = (uint)(BindFlag.ShaderResource | BindFlag.RenderTarget),'
    $t = $t -replace '(?m)^(\s*)MiscFlags = 0,', '${1}MiscFlags = (uint)ResourceMiscFlag.GenerateMips,'

    $createOld = '(?s)\r?\n(\s*)fixed \(byte\* pPixels = pixels\)\r?\n\s*\{\r?\n\s*var initData = new SubresourceData\r?\n.*?SilkMarshal\.ThrowHResult\(device\.CreateTexture2D\(&textureDesc, &initData, ref _texture\)\);\r?\n\s*\}'
    $createNew = @'

$1// Like textureclass.cpp: create empty with a full mip chain, fill mip 0,
$1// then let the GPU build the rest.
$1SilkMarshal.ThrowHResult(device.CreateTexture2D(&textureDesc, null, ref _texture));

$1fixed (byte* pPixels = pixels)
$1{
$1    DirectX.DeviceContext.UpdateSubresource(
$1        _texture, 0, null, pPixels, (uint)(width * 4), 0);
$1}
'@
    $t = $t -replace $createOld, $createNew

    $t = $t -replace 'MostDetailedMip = 0, MipLevels = 1 \}',
                     'MostDetailedMip = 0, MipLevels = unchecked((uint)-1) }'

    $srvOld = '(?s)(\r?\n(\s*)SilkMarshal\.ThrowHResult\(\r?\n\s*device\.CreateShaderResourceView\(_texture, &srvDesc, ref _textureView\)\r?\n\s*\);)'
    $t = $t -replace $srvOld, "`$1`r`n`r`n`$2DirectX.DeviceContext.GenerateMips(_textureView);"

    $rel = $f.FullName.Substring($root.Length + 1)
    if ($t -eq $orig) { $skipped += $rel; continue }
    if ($t -notmatch 'GenerateMips\(_textureView\)' -or $t -notmatch 'MipLevels = 0,') {
        $skipped += "$rel (partial - not written)"
        continue
    }

    if (-not $WhatIf) { Set-Content -Path $f.FullName -Value $t -NoNewline }
    Write-Output "  patched  $rel"
    $patched++
}

Write-Output ''
Write-Output "$patched patched, $already already had mips, $($skipped.Count) skipped"
foreach ($s in $skipped) { Write-Output "  SKIPPED  $s" }

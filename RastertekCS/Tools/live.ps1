# Launch the original C++ tutorial and the C# port side by side and leave them
# running, so Pavel can judge them against each other. Also stops the pair.
#
#   powershell -File live.ps1 -Nn 07 [-Series DirectX] -Action start|stop

param(
    [Parameter(Mandatory=$true)][string]$Nn,
    [string]$Series = 'DirectX',
    [ValidateSet('start','stop')][string]$Action = 'start'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms

$src = @"
using System;
using System.Runtime.InteropServices;
public class LiveWin {
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hAfter, int X, int Y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT r);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
}
"@
if (-not ('LiveWin' -as [type])) { Add-Type -TypeDefinition $src }

$root    = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$pidFile = Join-Path $root 'RastertekCS\.captures\live-pids.txt'

function Stop-Pair {
    if (-not (Test-Path $pidFile)) { return }
    foreach ($line in Get-Content $pidFile) {
        if ($line -match '^\d+$') {
            try { Stop-Process -Id ([int]$line) -Force -ErrorAction Stop } catch {}
        }
    }
    Remove-Item $pidFile -Force -ErrorAction SilentlyContinue
}

Stop-Pair
if ($Action -eq 'stop') { Write-Output 'stopped'; exit 0 }

$origEngine = Join-Path $root "RastertekOriginal\$Series\Tutorial$Nn\Engine"
$exe        = Join-Path $origEngine 'client.exe'
$portDir    = Join-Path $root "RastertekCS\Windows\Tutorial$Nn\bin\Debug\net8.0"

if (-not (Test-Path $origEngine)) { Write-Output "NO_ORIGINAL Tutorial$Nn"; exit 1 }

if (-not (Test-Path $exe)) {
    Write-Output 'building the original (first time for this tutorial, takes a moment)...'
    & (Join-Path $PSScriptRoot 'build-original.cmd') $Series $Nn | Out-Null
    if (-not (Test-Path $exe)) { Write-Output "BUILD_FAILED original Tutorial$Nn"; exit 1 }
}

# Where the original hardcodes FULL_SCREEN = true the port does the same, so the
# two cannot sit beside each other. Say so rather than pretending otherwise -
# the original's code is never altered to make comparison convenient.
$bothFullScreen = (Select-String -Path (Join-Path $origEngine 'applicationclass.h') `
    -Pattern 'const bool FULL_SCREEN = true' -Quiet)
# Always rebuild the port: the pair was just killed, so nothing holds the DLL,
# and this picks up any fix made between one tutorial and the next.
& dotnet build (Join-Path $root "RastertekCS\Windows\Tutorial$Nn") -v q --nologo -clp:ErrorsOnly | Out-Null
if (-not (Test-Path (Join-Path $portDir "Tutorial$Nn.dll"))) { Write-Output "NO_PORT Tutorial$Nn"; exit 1 }

$pCpp = Start-Process -FilePath $exe -WorkingDirectory $origEngine -PassThru
$pCs  = Start-Process -FilePath 'dotnet' -ArgumentList "Tutorial$Nn.dll" -WorkingDirectory $portDir -PassThru

Start-Sleep -Milliseconds 4500

"$($pCpp.Id)`n$($pCs.Id)" | Set-Content -Path $pidFile -Encoding ascii

# Place them side by side on the primary screen: original left, port right.
$screen = [System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea
foreach ($pair in @(@($pCpp, 0), @($pCs, 1))) {
    $proc = $pair[0]; $slot = $pair[1]
    if ($proc.HasExited) { continue }
    $proc.Refresh()
    # A process that died, or has not opened its window yet, yields $null here -
    # passing that to GetWindowRect throws a conversion error.
    $h = $proc.MainWindowHandle
    if ($null -eq $h -or $h -eq [IntPtr]::Zero) { continue }

    $r = New-Object LiveWin+RECT
    [void][LiveWin]::GetWindowRect($h, [ref]$r)
    $w = $r.Right - $r.Left; $hgt = $r.Bottom - $r.Top

    $x = if ($slot -eq 0) { 0 } else { [Math]::Max(0, $screen.Width - $w) }
    $y = [Math]::Max(0, [int](($screen.Height - $hgt) / 2))
    # HWND_TOPMOST rather than minimising the terminal: reliable, and it does
    # not touch a window that is not ours.
    [void][LiveWin]::SetWindowPos($h, [IntPtr](-1), $x, $y, 0, 0, 0x0041)  # NOSIZE|SHOWWINDOW
}

if ($bothFullScreen) {
    Write-Output "Tutorial$Nn is up: both run FULL SCREEN, as the original does."
    Write-Output "  They overlap - alt-tab between them, or use shot-many.cmd $Nn for two captures."
} else {
    Write-Output "Tutorial$Nn is up:  LEFT = C++ original   RIGHT = C# port"
}
if ($pCpp.HasExited) { Write-Output "  WARNING: the original exited immediately (code $($pCpp.ExitCode))" }
if ($pCs.HasExited)  { Write-Output "  WARNING: the port exited immediately (code $($pCs.ExitCode))" }

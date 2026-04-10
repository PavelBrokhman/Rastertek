using System.Runtime.InteropServices;
using RastertekCS.Windows.Tutorial12.System;

namespace RastertekCS.Windows.Tutorial12;

internal static class Program
{
    // Without DPI-awareness DWM stretches our DX11 backbuffer on high-DPI
    // screens and 2D bitmaps end up ~2x on 200% DPI. Try the modern context
    // API first, fall back to older calls for older Windows.
    [DllImport("user32.dll")]
    private static extern int SetProcessDpiAwarenessContext(nint value);
    private static readonly nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

    [DllImport("shcore.dll")]
    private static extern int SetProcessDpiAwareness(int value); // 2 = PROCESS_PER_MONITOR_DPI_AWARE

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    private static void MakeDpiAware()
    {
        try { if (SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2) != 0) return; } catch { }
        try { if (SetProcessDpiAwareness(2) == 0) return; } catch { }
        try { SetProcessDPIAware(); } catch { }
    }

    private static int Main()
    {
        MakeDpiAware();

        var system = new SystemFramework();
        if (system.Initialize())
        {
            system.Run();
        }
        Environment.Exit(0);
        return 0;
    }
}

using System.Runtime.InteropServices;
using RastertekCS.Windows.Tutorial12.System;

namespace RastertekCS.Windows.Tutorial12;

internal static class Program
{
    // Tell Windows this process renders its own DPI — stops DWM from scaling
    // the swap-chain client area and doubling 2D bitmap sizes on high-DPI.
    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(nint value);
    private static readonly nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

    private static int Main()
    {
        try { SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2); }
        catch { /* ignore if unavailable */ }

        var system = new SystemFramework();
        if (system.Initialize())
        {
            system.Run();
        }
        Environment.Exit(0);
        return 0;
    }
}

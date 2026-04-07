using RastertekCS.Windows.Tutorial05.System;

namespace RastertekCS.Windows.Tutorial05;

internal static class Program
{
    private static int Main()
    {
        var system = new SystemFramework();
        if (system.Initialize())
        {
            system.Run();
        }
        system.Shutdown();
        return 0;
    }
}

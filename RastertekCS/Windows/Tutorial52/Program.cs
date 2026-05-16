using RastertekCS.Windows.Tutorial52.System;

namespace RastertekCS.Windows.Tutorial52;

internal static class Program
{
    private static int Main()
    {
        var system = new SystemFramework();
        if (system.Initialize())
        {
            system.Run();
        }
        Environment.Exit(0);
        return 0;
    }
}

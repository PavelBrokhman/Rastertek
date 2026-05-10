using RastertekCS.Windows.Tutorial42.System;

namespace RastertekCS.Windows.Tutorial42;

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

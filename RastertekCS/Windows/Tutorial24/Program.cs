using RastertekCS.Windows.Tutorial24.System;

namespace RastertekCS.Windows.Tutorial24;

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

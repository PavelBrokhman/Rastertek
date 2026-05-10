using RastertekCS.Windows.Tutorial35.System;

namespace RastertekCS.Windows.Tutorial35;

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

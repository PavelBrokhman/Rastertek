using RastertekCS.OpenGL.Tutorial07.System;

namespace RastertekCS.OpenGL.Tutorial07;

internal static class Program
{
    private static int Main()
    {
        var system = new SystemFramework();
        if (system.Initialize())
            system.Run();
        Environment.Exit(0);
        return 0;
    }
}

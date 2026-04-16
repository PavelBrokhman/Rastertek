using RastertekCS.OpenGL.Tutorial08.System;

namespace RastertekCS.OpenGL.Tutorial08;

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

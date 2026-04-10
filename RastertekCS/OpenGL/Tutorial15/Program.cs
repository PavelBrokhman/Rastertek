using RastertekCS.OpenGL.Tutorial15.System;

namespace RastertekCS.OpenGL.Tutorial15;

internal static class Program
{
    private static int Main()
    {
        var system = new SystemFramework();
        if (system.Initialize()) system.Run();
        Environment.Exit(0);
        return 0;
    }
}

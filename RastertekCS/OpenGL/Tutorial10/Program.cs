using RastertekCS.OpenGL.Tutorial10.System;

namespace RastertekCS.OpenGL.Tutorial10;

internal static class Program
{
    private static int Main()
    {
        var system = new SystemFramework();
        if (system.Initialize()) system.Run();
        system.Shutdown();
        return 0;
    }
}

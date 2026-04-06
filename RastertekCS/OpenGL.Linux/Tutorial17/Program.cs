using RastertekCS.OpenGL.Tutorial17.System;

namespace RastertekCS.OpenGL.Tutorial17;

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

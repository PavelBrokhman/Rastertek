using RastertekCS.OpenGL.Tutorial13.System;

namespace RastertekCS.OpenGL.Tutorial13;

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

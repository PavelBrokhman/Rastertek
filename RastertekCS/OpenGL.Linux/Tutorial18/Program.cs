using RastertekCS.OpenGL.Tutorial18.System;

namespace RastertekCS.OpenGL.Tutorial18;

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

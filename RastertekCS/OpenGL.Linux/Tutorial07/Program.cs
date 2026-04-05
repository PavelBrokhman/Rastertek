using RastertekCS.OpenGL.Tutorial07.System;

namespace RastertekCS.OpenGL.Tutorial07;

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

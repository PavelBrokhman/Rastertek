using RastertekCS.OpenGL.Tutorial31.System;

namespace RastertekCS.OpenGL.Tutorial31;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var system = new SystemFramework();
            if (system.Initialize()) system.Run();
            else Console.WriteLine("ERROR: Initialize failed");
            Environment.Exit(0);
        }
        catch (Exception ex) { Console.WriteLine($"EXCEPTION: {ex}"); }
        return 0;
    }
}

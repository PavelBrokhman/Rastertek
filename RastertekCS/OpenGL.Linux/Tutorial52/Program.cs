using RastertekCS.OpenGL.Tutorial52.System;

namespace RastertekCS.OpenGL.Tutorial52;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var system = new SystemFramework();
            if (system.Initialize())
                system.Run();
            else
                Console.WriteLine("ERROR: Initialize failed");
            system.Shutdown();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION: {ex}");
        }
        return 0;
    }
}

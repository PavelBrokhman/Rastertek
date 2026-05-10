using RastertekCS.OpenGL.Tutorial35.System;

namespace RastertekCS.OpenGL.Tutorial35;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var s = new SystemFramework();
            if (s.Initialize())
                s.Run();
            else
                Console.WriteLine("ERROR");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION: {ex}");
        }
        return 0;
    }
}

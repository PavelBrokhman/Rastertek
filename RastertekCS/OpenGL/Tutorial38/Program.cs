using RastertekCS.OpenGL.Tutorial38.System;

namespace RastertekCS.OpenGL.Tutorial38;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var s = new SystemFramework();
            if (s.Initialize()) s.Run();
            else Console.WriteLine("ERROR");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION: {ex}");
        }
        return 0;
    }
}

////////////////////////////////////////////////////////////////////////////////
// Filename: Program.cs
////////////////////////////////////////////////////////////////////////////////
namespace RastertekCS.OpenGL.Tutorial03;

internal static class Program
{
    private static int Main()
    {
        var system = new SystemClass();

        if (system.Initialize())
        {
            system.Run();
        }

        system.Shutdown();

        return 0;
    }
}

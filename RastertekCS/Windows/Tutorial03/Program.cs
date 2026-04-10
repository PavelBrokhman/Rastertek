////////////////////////////////////////////////////////////////////////////////
// Filename: Program.cs
////////////////////////////////////////////////////////////////////////////////
namespace RastertekCS.Windows.Tutorial03;

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
        Environment.Exit(0);
        return 0;
    }
}

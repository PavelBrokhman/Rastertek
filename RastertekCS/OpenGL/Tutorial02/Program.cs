////////////////////////////////////////////////////////////////////////////////
// Filename: Program.cs
////////////////////////////////////////////////////////////////////////////////
namespace RastertekCS.OpenGL.Tutorial02;

internal static class Program
{
    private static int Main()
    {
        // Создаём системный объект.
        var system = new SystemClass();

        // Инициализируем и запускаем системный объект.
        if (system.Initialize())
        {
            system.Run();
        }

        // Завершаем работу и освобождаем системный объект.
        system.Shutdown();

        return 0;
    }
}

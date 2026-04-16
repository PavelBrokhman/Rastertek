////////////////////////////////////////////////////////////////////////////////
// Filename: SystemClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial02;

public class SystemClass
{
    private IWindow _window;
    private IInputContext _inputContext;

    private OpenGLClass _openGL;
    private InputClass _input;
    private GraphicsClass _graphics;

    private bool _done;

    public SystemClass()
    {
        _openGL = null;
        _input = null;
        _graphics = null;
    }

    public bool Initialize()
    {
        int screenWidth = 0;
        int screenHeight = 0;

        // Создаём объект OpenGL.
        _openGL = new OpenGLClass();

        // Создаём окно и инициализируем OpenGL.
        if (!InitializeWindows(_openGL, ref screenWidth, ref screenHeight))
        {
            Console.WriteLine("Не удалось инициализировать окно.");
            return false;
        }

        // Создаём объект ввода.
        _input = new InputClass();
        _input.Initialize();

        // Создаём объект графики.
        _graphics = new GraphicsClass();
        if (!_graphics.Initialize(_openGL, _window))
        {
            return false;
        }

        return true;
    }

    public void Shutdown()
    {
        if (_graphics != null)
        {
            _graphics.Shutdown();
            _graphics = null;
        }

        if (_input != null)
        {
            _input = null;
        }

        if (_openGL != null)
        {
            _openGL = null;
        }

        ShutdownWindows();
    }

    public void Run()
    {
        // Запускаем главный цикл окна (Silk.NET выполняет свой цикл сообщений).
        _done = false;
        _window.Run();
    }

    private bool Frame()
    {
        // Проверяем нажатие клавиши Escape.
        if (_input.IsKeyDown(Key.Escape))
        {
            return false;
        }

        // Выполняем обработку кадра.
        if (!_graphics.Frame())
        {
            return false;
        }

        return true;
    }

    private bool InitializeWindows(OpenGLClass OpenGL, ref int screenWidth, ref int screenHeight)
    {
        // Значения экрана по умолчанию.
        screenWidth = GraphicsClass.FULL_SCREEN ? 0 : 800;
        screenHeight = GraphicsClass.FULL_SCREEN ? 0 : 600;

        // Конфигурируем окно Silk.NET с контекстом OpenGL 4.0 core profile.
        var options = WindowOptions.Default;
        options.Title = "Engine";
        options.Size = GraphicsClass.FULL_SCREEN
            ? new Vector2D<int>(1920, 1080)
            : new Vector2D<int>(screenWidth, screenHeight);
        options.WindowBorder = WindowBorder.Resizable;
        options.WindowState = GraphicsClass.FULL_SCREEN
            ? WindowState.Fullscreen
            : WindowState.Normal;
        options.VSync = GraphicsClass.VSYNC_ENABLED;
        options.API = new GraphicsAPI(
            ContextAPI.OpenGL,
            ContextProfile.Core,
            ContextFlags.ForwardCompatible,
            new APIVersion(4, 0)
        );

        _window = Window.Create(options);

        // Привязываем обработчики событий окна.
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        // Инициализируем окно (загружает контекст OpenGL).
        _window.Initialize();

        // Сохраняем реальные размеры экрана после создания окна.
        screenWidth = _window.Size.X;
        screenHeight = _window.Size.Y;

        return true;
    }

    private void ShutdownWindows()
    {
        if (_inputContext != null)
        {
            _inputContext.Dispose();
            _inputContext = null;
        }

        if (_window != null)
        {
            _window.Dispose();
            _window = null;
        }
    }

    private void OnLoad()
    {
        // Создаём контекст ввода Silk.NET и подписываемся на события клавиатуры.
        _inputContext = _window.CreateInput();
        foreach (var keyboard in _inputContext.Keyboards)
        {
            keyboard.KeyDown += (kb, key, _) => _input?.KeyDown(key);
            keyboard.KeyUp += (kb, key, _) => _input?.KeyUp(key);
        }
    }

    private void OnUpdate(double deltaTime)
    {
        // Выполняем один кадр логики.
        if (_done || !Frame())
        {
            _done = true;
            _window.Close();
        }
    }

    private void OnRender(double deltaTime)
    {
        // Рендеринг будет добавлен в последующих туториалах.
    }

    private void OnClosing()
    {
        _done = true;
    }
}

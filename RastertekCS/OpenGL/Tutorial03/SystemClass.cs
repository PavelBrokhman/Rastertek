////////////////////////////////////////////////////////////////////////////////
// Filename: SystemClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial03;

public class SystemClass
{
    private IWindow _window;
    private IInputContext _inputContext;

    private OpenGLClass _openGL;
    private InputClass _input;
    private GraphicsClass _graphics;

    private bool _done;

    public bool Initialize()
    {
        int screenWidth = 0;
        int screenHeight = 0;

        _openGL = new OpenGLClass();

        if (!InitializeWindows(ref screenWidth, ref screenHeight))
        {
            Console.WriteLine("Не удалось инициализировать окно.");
            return false;
        }

        // Инициализируем OpenGL (делается здесь после создания окна,
        // так как нужны screenWidth/Height и контекст окна).
        if (
            !_openGL.Initialize(
                _window,
                screenWidth,
                screenHeight,
                GraphicsClass.SCREEN_DEPTH,
                GraphicsClass.SCREEN_NEAR,
                GraphicsClass.VSYNC_ENABLED
            )
        )
        {
            Console.WriteLine("Не удалось инициализировать OpenGL.");
            return false;
        }

        _input = new InputClass();
        _input.Initialize();

        _graphics = new GraphicsClass();
        if (!_graphics.Initialize(_openGL))
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

        _input = null;

        if (_openGL != null)
        {
            _openGL.Shutdown();
            _openGL = null;
        }

        ShutdownWindows();
    }

    public void Run()
    {
        _done = false;
        _window.Run();
    }

    private bool Frame()
    {
        if (_input.IsKeyDown(Key.Escape))
        {
            return false;
        }

        return _graphics.Frame();
    }

    private bool InitializeWindows(ref int screenWidth, ref int screenHeight)
    {
        screenWidth = GraphicsClass.FULL_SCREEN ? 1920 : 800;
        screenHeight = GraphicsClass.FULL_SCREEN ? 1080 : 600;

        var options = WindowOptions.Default;
        options.Title = "Engine";
        options.Size = new Vector2D<int>(screenWidth, screenHeight);
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

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClosing;

        _window.Initialize();

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
        _inputContext = _window.CreateInput();
        foreach (var keyboard in _inputContext.Keyboards)
        {
            keyboard.KeyDown += (kb, key, _) => _input?.KeyDown(key);
            keyboard.KeyUp += (kb, key, _) => _input?.KeyUp(key);
        }
    }

    private void OnUpdate(double deltaTime)
    {
        if (_done || !Frame())
        {
            _done = true;
            _window.Close();
        }
    }

    private void OnRender(double deltaTime)
    {
        // Render вызывается из Frame() через Graphics, но сам рендеринг
        // требует актуального GL контекста — он как раз доступен здесь.
        // Поскольку в Tutorial 3 рендеринг только очищает экран, можем
        // вызвать Graphics.Frame() отсюда вместо OnUpdate. Переносим рендеринг.
        _graphics?.Frame();
    }

    private void OnClosing()
    {
        _done = true;
    }
}

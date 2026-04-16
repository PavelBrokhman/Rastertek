////////////////////////////////////////////////////////////////////////////////
// Filename: SystemClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.Windows.Tutorial03;

public class SystemClass
{
    private IWindow _window;
    private IInputContext _inputContext;

    private DirectXClass _directX;
    private InputClass _input;
    private GraphicsClass _graphics;

    private bool _done;

    public bool Initialize()
    {
        int screenWidth = 0;
        int screenHeight = 0;

        _directX = new DirectXClass();

        if (!InitializeWindows(ref screenWidth, ref screenHeight))
        {
            Console.WriteLine("Could not initialize the window.");
            return false;
        }

        if (
            !_directX.Initialize(
                _window,
                screenWidth,
                screenHeight,
                GraphicsClass.SCREEN_DEPTH,
                GraphicsClass.SCREEN_NEAR,
                GraphicsClass.VSYNC_ENABLED
            )
        )
        {
            Console.WriteLine("Could not initialize Direct3D.");
            return false;
        }

        _input = new InputClass();
        _input.Initialize();

        _graphics = new GraphicsClass();
        if (!_graphics.Initialize(_directX))
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

        if (_directX != null)
        {
            _directX.Shutdown();
            _directX = null;
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
        options.Title = "Tutorial03 - DirectX";
        options.Size = new Vector2D<int>(screenWidth, screenHeight);
        options.WindowBorder = WindowBorder.Fixed;
        options.WindowState = GraphicsClass.FULL_SCREEN
            ? WindowState.Fullscreen
            : WindowState.Normal;
        options.VSync = GraphicsClass.VSYNC_ENABLED;
        options.API = GraphicsAPI.None;

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
        if (_done)
            return;
        if (!Frame())
        {
            _done = true;
            _window.Close();
        }
    }

    private void OnRender(double deltaTime)
    {
        if (_done)
            return;
        _graphics?.Frame();
    }

    private void OnClosing()
    {
        _done = true;
    }
}

using RastertekCS.OpenGL.Tutorial05.Graphics;
using RastertekCS.OpenGL.Tutorial05.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial05.System;

public class SystemFramework
{
    private IWindow _window;
    private IInputContext _inputContext;
    private GL4 _openGL;
    private Input _input;
    private GraphicsFramework _graphics;
    private bool _done;
    private bool _graphicsInitialized;

    public bool Initialize()
    {
        int screenWidth = 0,
            screenHeight = 0;
        _openGL = new GL4();

        if (!InitializeWindows(ref screenWidth, ref screenHeight))
        {
            global::System.Console.WriteLine("Не удалось инициализировать окно.");
            return false;
        }

        if (
            !_openGL.Initialize(
                _window,
                screenWidth,
                screenHeight,
                SystemConfiguration.ScreenDepth,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.VerticalSyncEnabled
            )
        )
        {
            global::System.Console.WriteLine("Не удалось инициализировать OpenGL.");
            return false;
        }

        _input = new Input();
        _input.Initialize();

        _graphics = new GraphicsFramework();
        if (!_graphics.Initialize(_openGL))
            return false;
        _graphicsInitialized = true;

        return true;
    }

    public void Shutdown()
    {
        _graphics?.Shutdown();
        _graphics = null;
        _input = null;
        _openGL?.Shutdown();
        _openGL = null;
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
            return false;
        return _graphics.Frame();
    }

    private bool InitializeWindows(ref int screenWidth, ref int screenHeight)
    {
        screenWidth = SystemConfiguration.FullScreen ? 1920 : 1024;
        screenHeight = SystemConfiguration.FullScreen ? 1080 : 768;

        var options = WindowOptions.Default;
        options.Title = "Tutorial05";
        options.Size = new Vector2D<int>(screenWidth, screenHeight);
        options.WindowBorder = WindowBorder.Resizable;
        options.WindowState = SystemConfiguration.FullScreen
            ? WindowState.Fullscreen
            : WindowState.Normal;
        options.VSync = SystemConfiguration.VerticalSyncEnabled;
        options.API = new GraphicsAPI(
            ContextAPI.OpenGL,
            ContextProfile.Core,
            ContextFlags.ForwardCompatible,
            new APIVersion(4, 0)
        );

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Closing += OnClosing;
        _window.Initialize();
        _window.Title = "Tutorial05";

        screenWidth = _window.Size.X;
        screenHeight = _window.Size.Y;
        return true;
    }

    private void ShutdownWindows()
    {
        _inputContext?.Dispose();
        _inputContext = null;
        _window?.Dispose();
        _window = null;
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

    private void OnRender(double deltaTime)
    {
        if (!_graphicsInitialized)
            return;
        if (_done || !Frame())
        {
            _done = true;
            _window.Close();
        }
    }

    private void OnClosing()
    {
        _done = true;
        _graphics?.Shutdown();
        _graphics = null;
        _openGL?.Shutdown();
        _openGL = null;
    }
}

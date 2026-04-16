using RastertekCS.OpenGL.Tutorial23.Graphics;
using RastertekCS.OpenGL.Tutorial23.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial23.System;

public class SystemFramework
{
    private IWindow _window;
    private IInputContext _inputContext;
    private GL4 _openGL;
    private Input _input;
    private GraphicsFramework _graphics;
    private Timer _timer;
    private bool _done,
        _graphicsInitialized;

    public bool Initialize()
    {
        int screenWidth = 0,
            screenHeight = 0;
        _openGL = new GL4();
        if (!InitializeWindows(ref screenWidth, ref screenHeight))
            return false;
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
            return false;
        _input = new Input();
        _input.Initialize();
        _timer = new Timer();
        _timer.Initialize();
        _graphics = new GraphicsFramework();
        if (!_graphics.Initialize(_openGL, _input, screenWidth, screenHeight))
            return false;
        _graphicsInitialized = true;
        return true;
    }

    public void Shutdown()
    {
        _graphics?.Shutdown();
        _graphics = null;
        _input = null;
        _timer = null;
        _openGL?.Shutdown();
        _openGL = null;
        ShutdownWindows();
    }

    public void Run()
    {
        _done = false;
        _window.Run();
    }

    private bool InitializeWindows(ref int screenWidth, ref int screenHeight)
    {
        screenWidth = SystemConfiguration.FullScreen ? 1920 : 800;
        screenHeight = SystemConfiguration.FullScreen ? 1080 : 600;
        var options = WindowOptions.Default;
        options.Title = "Tutorial 23 - Frustum Culling";
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
        _window.Title = "Tutorial23";
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
        foreach (var kb in _inputContext.Keyboards)
        {
            kb.KeyDown += (_, key, _) => _input?.KeyDown(key);
            kb.KeyUp += (_, key, _) => _input?.KeyUp(key);
        }
    }

    private void OnRender(double dt)
    {
        if (!_graphicsInitialized)
            return;
        if (_done || _input.IsKeyDown(Key.Escape))
        {
            _done = true;
            _window.Close();
            return;
        }
        _timer.Frame();
        if (!_graphics.Frame(_timer))
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

using RastertekCS.Windows.Tutorial59.Graphics;
using RastertekCS.Windows.Tutorial59.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.Windows.Tutorial59.System;

public class SystemFramework
{
    private IWindow _window;
    private IInputContext _inputContext;
    private DX11 _directX;
    private Input _input;
    private GraphicsFramework _graphics;
    private bool _done;
    private bool _graphicsInitialized;

    public bool Initialize()
    {
        int sw = 0, sh = 0;
        _directX = new DX11();
        if (!InitializeWindows(ref sw, ref sh)) return false;
        if (!_directX.Initialize(_window, sw, sh, SystemConfiguration.ScreenDepth, SystemConfiguration.ScreenNear, SystemConfiguration.VerticalSyncEnabled)) return false;
        _input = new Input(); _input.Initialize();
        _graphics = new GraphicsFramework();
        if (!_graphics.Initialize(_directX, sw, sh)) return false;
        _graphicsInitialized = true;
        return true;
    }

    public void Run() { _done = false; _window.Run(); }

    private bool Frame()
    {
        if (_input.IsKeyDown(Key.Escape)) return false;
        return _graphics.Frame();
    }

    private bool InitializeWindows(ref int sw, ref int sh)
    {
        // systemclass.cpp: 1280x720 when windowed, the desktop resolution otherwise.
        sw = SystemConfiguration.FullScreen ? 1920 : 1280;
        sh = SystemConfiguration.FullScreen ? 1080 : 720;
        var options = WindowOptions.Default;
        options.Title = "Tutorial59 - Animated Particles (DirectX 11)";
        options.Size = new Vector2D<int>(sw, sh);
        options.WindowBorder = WindowBorder.Fixed;
        options.WindowState = SystemConfiguration.FullScreen ? WindowState.Fullscreen : WindowState.Normal;
        options.VSync = SystemConfiguration.VerticalSyncEnabled;
        options.API = GraphicsAPI.None;
        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Closing += OnClosing;
        _window.Initialize();
        sw = _window.FramebufferSize.X; sh = _window.FramebufferSize.Y;
        return true;
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

    private void OnRender(double dt)
    {
        if (!_graphicsInitialized || _done) return;
        if (!Frame()) { _done = true; _window.Close(); }
    }

    private void OnClosing() { _done = true; }
}

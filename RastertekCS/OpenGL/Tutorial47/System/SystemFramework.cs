using RastertekCS.OpenGL.Tutorial47.Graphics;
using RastertekCS.OpenGL.Tutorial47.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial47.System;

public class SystemFramework
{
    private IWindow _window;
    private IInputContext _inputContext;
    private GL4 _driver;
    private Input _input;
    private GraphicsFramework _graphics;
    private bool _done, _initialized;

    public bool Initialize()
    {
        int sw = 0, sh = 0;
        _driver = new GL4();
        if (!InitWin(ref sw, ref sh)) return false;
        if (!_driver.Initialize(_window, sw, sh, SystemConfiguration.ScreenDepth, SystemConfiguration.ScreenNear, SystemConfiguration.VerticalSyncEnabled)) return false;
        _input = new Input(); _input.Initialize();
        _graphics = new GraphicsFramework();
        if (!_graphics.Initialize(_driver, sw, sh)) return false;
        _initialized = true;
        return true;
    }

    public void Shutdown()
    {
        _graphics?.Shutdown(); _graphics = null;
        _driver?.Shutdown(); _driver = null;
        _inputContext?.Dispose(); _inputContext = null;
        _window?.Dispose(); _window = null;
    }

    public void Run() { _done = false; _window.Run(); }

    bool InitWin(ref int sw, ref int sh)
    {
        sw = 1024; sh = 768;
        var o = WindowOptions.Default;
        o.Title = "Tutorial 47 - Bitmap, Font and Light (OpenGL)";
        o.Size = new Vector2D<int>(sw, sh);
        o.WindowBorder = WindowBorder.Resizable;
        o.VSync = SystemConfiguration.VerticalSyncEnabled;
        o.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(4, 0));
        _window = Window.Create(o);
        _window.Load += () =>
        {
            _inputContext = _window.CreateInput();
            foreach (var kb in _inputContext.Keyboards)
            {
                kb.KeyDown += (_, key, _) => _input?.KeyDown(key);
                kb.KeyUp += (_, key, _) => _input?.KeyUp(key);
            }
            foreach (var mouse in _inputContext.Mice)
            {
                mouse.MouseMove += (_, pos) => _input?.SetMouseLocation((int)pos.X, (int)pos.Y);
            }
        };
        _window.Render += _ =>
        {
            if (!_initialized) return;
            if (_done || _input.IsKeyDown(Key.Escape)) { _done = true; _window.Close(); return; }
            if (!_graphics.Frame(_input)) { _done = true; _window.Close(); }
        };
        _window.Closing += () => { _done = true; _graphics?.Shutdown(); _graphics = null; _driver?.Shutdown(); _driver = null; };
        _window.Initialize();
        sw = _window.Size.X; sh = _window.Size.Y;
        return true;
    }
}

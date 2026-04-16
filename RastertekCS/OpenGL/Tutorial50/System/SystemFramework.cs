using RastertekCS.OpenGL.Tutorial50.Graphics;
using RastertekCS.OpenGL.Tutorial50.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial50.System;

public class SystemFramework
{
    private IWindow m_window;
    private IInputContext m_inputContext;
    private GL4 m_OpenGL;
    private Input m_Input;
    private GraphicsFramework m_Graphics;
    private bool m_done, m_graphicsInitialized;

    public bool Initialize()
    {
        int sw = 0, sh = 0;
        m_OpenGL = new GL4();
        if (!InitializeWindows(ref sw, ref sh)) return false;
        if (!m_OpenGL.Initialize(m_window, sw, sh, SystemConfiguration.ScreenDepth,
                                  SystemConfiguration.ScreenNear, SystemConfiguration.VerticalSyncEnabled)) return false;
        m_Input = new Input(); m_Input.Initialize();
        m_Graphics = new GraphicsFramework();
        if (!m_Graphics.Initialize(m_OpenGL, sw, sh)) return false;
        m_graphicsInitialized = true;
        return true;
    }

    public void Shutdown()
    {
        m_Graphics?.Shutdown(); m_Graphics = null;
        m_Input = null;
        m_OpenGL?.Shutdown(); m_OpenGL = null;
        ShutdownWindows();
    }

    public void Run() { m_done = false; m_window.Run(); }

    private bool InitializeWindows(ref int sw, ref int sh)
    {
        sw = SystemConfiguration.FullScreen ? 1920 : 800;
        sh = SystemConfiguration.FullScreen ? 1080 : 600;
        var options = WindowOptions.Default;
        options.Title = "Tutorial50";
        options.Size = new Vector2D<int>(sw, sh);
        options.WindowBorder = WindowBorder.Resizable;
        options.WindowState = SystemConfiguration.FullScreen ? WindowState.Fullscreen : WindowState.Normal;
        options.VSync = SystemConfiguration.VerticalSyncEnabled;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core,
            ContextFlags.ForwardCompatible, new APIVersion(4, 0));
        m_window = Window.Create(options);
        m_window.Load += OnLoad;
        m_window.Render += OnRender;
        m_window.Closing += OnClosing;
        m_window.Initialize();
        m_window.Title = "Tutorial50";
        sw = m_window.Size.X; sh = m_window.Size.Y;
        return true;
    }

    private void ShutdownWindows() { m_inputContext?.Dispose(); m_inputContext = null; m_window?.Dispose(); m_window = null; }

    private void OnLoad()
    {
        m_inputContext = m_window.CreateInput();
        foreach (var kb in m_inputContext.Keyboards)
        {
            kb.KeyDown += (_, key, _) => m_Input?.KeyDown(key);
            kb.KeyUp += (_, key, _) => m_Input?.KeyUp(key);
        }
    }

    private void OnRender(double dt)
    {
        if (!m_graphicsInitialized) return;
        if (m_done || m_Input.IsKeyDown(Key.Escape)) { m_done = true; m_window.Close(); return; }
        if (!m_Graphics.Frame()) { m_done = true; m_window.Close(); }
    }

    private void OnClosing()
    {
        m_done = true;
        m_Graphics?.Shutdown(); m_Graphics = null;
        m_OpenGL?.Shutdown(); m_OpenGL = null;
    }
}

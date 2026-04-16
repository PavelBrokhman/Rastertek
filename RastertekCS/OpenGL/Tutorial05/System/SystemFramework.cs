using RastertekCS.OpenGL.Tutorial05.Graphics;
using RastertekCS.OpenGL.Tutorial05.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial05.System;

public class SystemFramework
{
    private IWindow m_window;
    private IInputContext m_inputContext;
    private GL4 m_OpenGL;
    private Input m_Input;
    private GraphicsFramework m_Graphics;
    private bool m_done;
    private bool m_graphicsInitialized;

    public bool Initialize()
    {
        int screenWidth = 0,
            screenHeight = 0;
        m_OpenGL = new GL4();

        if (!InitializeWindows(ref screenWidth, ref screenHeight))
        {
            global::System.Console.WriteLine("Не удалось инициализировать окно.");
            return false;
        }

        if (
            !m_OpenGL.Initialize(
                m_window,
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

        m_Input = new Input();
        m_Input.Initialize();

        m_Graphics = new GraphicsFramework();
        if (!m_Graphics.Initialize(m_OpenGL))
            return false;
        m_graphicsInitialized = true;

        return true;
    }

    public void Shutdown()
    {
        m_Graphics?.Shutdown();
        m_Graphics = null;
        m_Input = null;
        m_OpenGL?.Shutdown();
        m_OpenGL = null;
        ShutdownWindows();
    }

    public void Run()
    {
        m_done = false;
        m_window.Run();
    }

    private bool Frame()
    {
        if (m_Input.IsKeyDown(Key.Escape))
            return false;
        return m_Graphics.Frame();
    }

    private bool InitializeWindows(ref int screenWidth, ref int screenHeight)
    {
        screenWidth = SystemConfiguration.FullScreen ? 1920 : 800;
        screenHeight = SystemConfiguration.FullScreen ? 1080 : 600;

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

        m_window = Window.Create(options);
        m_window.Load += OnLoad;
        m_window.Render += OnRender;
        m_window.Closing += OnClosing;
        m_window.Initialize();
        m_window.Title = "Tutorial05";

        screenWidth = m_window.Size.X;
        screenHeight = m_window.Size.Y;
        return true;
    }

    private void ShutdownWindows()
    {
        m_inputContext?.Dispose();
        m_inputContext = null;
        m_window?.Dispose();
        m_window = null;
    }

    private void OnLoad()
    {
        m_inputContext = m_window.CreateInput();
        foreach (var keyboard in m_inputContext.Keyboards)
        {
            keyboard.KeyDown += (kb, key, _) => m_Input?.KeyDown(key);
            keyboard.KeyUp += (kb, key, _) => m_Input?.KeyUp(key);
        }
    }

    private void OnRender(double deltaTime)
    {
        if (!m_graphicsInitialized)
            return;
        if (m_done || !Frame())
        {
            m_done = true;
            m_window.Close();
        }
    }

    private void OnClosing()
    {
        m_done = true;
        m_Graphics?.Shutdown();
        m_Graphics = null;
        m_OpenGL?.Shutdown();
        m_OpenGL = null;
    }
}

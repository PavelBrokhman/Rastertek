using RastertekCS.Windows.Tutorial10.Graphics;
using RastertekCS.Windows.Tutorial10.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.Windows.Tutorial10.System;

public class SystemFramework
{
    private IWindow m_window;
    private IInputContext m_inputContext;
    private DX11 m_DirectX;
    private Input m_Input;
    private GraphicsFramework m_Graphics;
    private bool m_done;
    private bool m_graphicsInitialized;

    public bool Initialize()
    {
        int screenWidth = 0,
            screenHeight = 0;
        m_DirectX = new DX11();

        if (!InitializeWindows(ref screenWidth, ref screenHeight))
        {
            Console.WriteLine("Could not initialize the window.");
            return false;
        }

        if (
            !m_DirectX.Initialize(
                m_window,
                screenWidth,
                screenHeight,
                SystemConfiguration.ScreenDepth,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.VerticalSyncEnabled
            )
        )
        {
            Console.WriteLine("Could not initialize Direct3D.");
            return false;
        }

        m_Input = new Input();
        m_Input.Initialize();

        m_Graphics = new GraphicsFramework();
        if (!m_Graphics.Initialize(m_DirectX))
            return false;
        m_graphicsInitialized = true;

        return true;
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
        options.Title = "Tutorial10 - Specular Lighting (DirectX 11)";
        options.Size = new Vector2D<int>(screenWidth, screenHeight);
        options.WindowBorder = WindowBorder.Fixed;
        options.WindowState = SystemConfiguration.FullScreen
            ? WindowState.Fullscreen
            : WindowState.Normal;
        options.VSync = SystemConfiguration.VerticalSyncEnabled;
        options.API = GraphicsAPI.None;

        m_window = Window.Create(options);
        m_window.Load += OnLoad;
        m_window.Render += OnRender;
        m_window.Closing += OnClosing;
        m_window.Initialize();

        screenWidth = m_window.Size.X;
        screenHeight = m_window.Size.Y;
        return true;
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
        if (!m_graphicsInitialized || m_done)
            return;
        if (!Frame())
        {
            m_done = true;
            m_window.Close();
        }
    }

    private void OnClosing()
    {
        m_done = true;
    }
}

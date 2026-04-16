////////////////////////////////////////////////////////////////////////////////
// Filename: SystemClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial03;

public class SystemClass
{
    private IWindow m_window;
    private IInputContext m_inputContext;

    private OpenGLClass m_OpenGL;
    private InputClass m_Input;
    private GraphicsClass m_Graphics;

    private bool m_done;

    public bool Initialize()
    {
        int screenWidth = 0;
        int screenHeight = 0;

        m_OpenGL = new OpenGLClass();

        if (!InitializeWindows(ref screenWidth, ref screenHeight))
        {
            Console.WriteLine("Не удалось инициализировать окно.");
            return false;
        }

        // Инициализируем OpenGL (делается здесь после создания окна,
        // так как нужны screenWidth/Height и контекст окна).
        if (
            !m_OpenGL.Initialize(
                m_window,
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

        m_Input = new InputClass();
        m_Input.Initialize();

        m_Graphics = new GraphicsClass();
        if (!m_Graphics.Initialize(m_OpenGL))
        {
            return false;
        }

        return true;
    }

    public void Shutdown()
    {
        if (m_Graphics != null)
        {
            m_Graphics.Shutdown();
            m_Graphics = null;
        }

        m_Input = null;

        if (m_OpenGL != null)
        {
            m_OpenGL.Shutdown();
            m_OpenGL = null;
        }

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
        {
            return false;
        }

        return m_Graphics.Frame();
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

        m_window = Window.Create(options);

        m_window.Load += OnLoad;
        m_window.Update += OnUpdate;
        m_window.Render += OnRender;
        m_window.Closing += OnClosing;

        m_window.Initialize();

        screenWidth = m_window.Size.X;
        screenHeight = m_window.Size.Y;

        return true;
    }

    private void ShutdownWindows()
    {
        if (m_inputContext != null)
        {
            m_inputContext.Dispose();
            m_inputContext = null;
        }

        if (m_window != null)
        {
            m_window.Dispose();
            m_window = null;
        }
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

    private void OnUpdate(double deltaTime)
    {
        if (m_done || !Frame())
        {
            m_done = true;
            m_window.Close();
        }
    }

    private void OnRender(double deltaTime)
    {
        // Render вызывается из Frame() через Graphics, но сам рендеринг
        // требует актуального GL контекста — он как раз доступен здесь.
        // Поскольку в Tutorial 3 рендеринг только очищает экран, можем
        // вызвать Graphics.Frame() отсюда вместо OnUpdate. Переносим рендеринг.
        m_Graphics?.Frame();
    }

    private void OnClosing()
    {
        m_done = true;
    }
}

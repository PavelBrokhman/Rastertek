////////////////////////////////////////////////////////////////////////////////
// Filename: SystemClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.Windows.Tutorial03;

public class SystemClass
{
    private IWindow m_window;
    private IInputContext m_inputContext;

    private DirectXClass m_DirectX;
    private InputClass m_Input;
    private GraphicsClass m_Graphics;

    private bool m_done;

    public bool Initialize()
    {
        int screenWidth = 0;
        int screenHeight = 0;

        m_DirectX = new DirectXClass();

        if (!InitializeWindows(ref screenWidth, ref screenHeight))
        {
            Console.WriteLine("Could not initialize the window.");
            return false;
        }

        if (!m_DirectX.Initialize(m_window, screenWidth, screenHeight,
                                   GraphicsClass.SCREEN_DEPTH, GraphicsClass.SCREEN_NEAR,
                                   GraphicsClass.VSYNC_ENABLED))
        {
            Console.WriteLine("Could not initialize Direct3D.");
            return false;
        }

        m_Input = new InputClass();
        m_Input.Initialize();

        m_Graphics = new GraphicsClass();
        if (!m_Graphics.Initialize(m_DirectX))
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

        if (m_DirectX != null)
        {
            m_DirectX.Shutdown();
            m_DirectX = null;
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
        options.Title = "Tutorial03 - DirectX";
        options.Size = new Vector2D<int>(screenWidth, screenHeight);
        options.WindowBorder = WindowBorder.Fixed;
        options.WindowState = GraphicsClass.FULL_SCREEN ? WindowState.Fullscreen : WindowState.Normal;
        options.VSync = GraphicsClass.VSYNC_ENABLED;
        options.API = GraphicsAPI.None;

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
        if (m_done) return;
        if (!Frame())
        {
            m_done = true;
            m_window.Close();
        }
    }

    private void OnRender(double deltaTime)
    {
        if (m_done) return;
        m_Graphics?.Frame();
    }

    private void OnClosing()
    {
        m_done = true;
    }
}

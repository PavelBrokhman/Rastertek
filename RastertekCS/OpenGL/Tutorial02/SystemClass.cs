////////////////////////////////////////////////////////////////////////////////
// Filename: SystemClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial02;

public class SystemClass
{
    private IWindow m_window;
    private IInputContext m_inputContext;

    private OpenGLClass m_OpenGL;
    private InputClass m_Input;
    private GraphicsClass m_Graphics;

    private bool m_done;

    public SystemClass()
    {
        m_OpenGL = null;
        m_Input = null;
        m_Graphics = null;
    }

    public bool Initialize()
    {
        int screenWidth = 0;
        int screenHeight = 0;

        // Создаём объект OpenGL.
        m_OpenGL = new OpenGLClass();

        // Создаём окно и инициализируем OpenGL.
        if (!InitializeWindows(m_OpenGL, ref screenWidth, ref screenHeight))
        {
            Console.WriteLine("Не удалось инициализировать окно.");
            return false;
        }

        // Создаём объект ввода.
        m_Input = new InputClass();
        m_Input.Initialize();

        // Создаём объект графики.
        m_Graphics = new GraphicsClass();
        if (!m_Graphics.Initialize(m_OpenGL, m_window))
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

        if (m_Input != null)
        {
            m_Input = null;
        }

        if (m_OpenGL != null)
        {
            m_OpenGL = null;
        }

        ShutdownWindows();
    }

    public void Run()
    {
        // Запускаем главный цикл окна (Silk.NET выполняет свой цикл сообщений).
        m_done = false;
        m_window.Run();
    }

    private bool Frame()
    {
        // Проверяем нажатие клавиши Escape.
        if (m_Input.IsKeyDown(Key.Escape))
        {
            return false;
        }

        // Выполняем обработку кадра.
        if (!m_Graphics.Frame())
        {
            return false;
        }

        return true;
    }

    private bool InitializeWindows(OpenGLClass OpenGL, ref int screenWidth, ref int screenHeight)
    {
        // Значения экрана по умолчанию.
        screenWidth = GraphicsClass.FULL_SCREEN ? 0 : 800;
        screenHeight = GraphicsClass.FULL_SCREEN ? 0 : 600;

        // Конфигурируем окно Silk.NET с контекстом OpenGL 4.0 core profile.
        var options = WindowOptions.Default;
        options.Title = "Engine";
        options.Size = GraphicsClass.FULL_SCREEN
            ? new Vector2D<int>(1920, 1080)
            : new Vector2D<int>(screenWidth, screenHeight);
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

        // Привязываем обработчики событий окна.
        m_window.Load += OnLoad;
        m_window.Update += OnUpdate;
        m_window.Render += OnRender;
        m_window.Closing += OnClosing;

        // Инициализируем окно (загружает контекст OpenGL).
        m_window.Initialize();

        // Сохраняем реальные размеры экрана после создания окна.
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
        // Создаём контекст ввода Silk.NET и подписываемся на события клавиатуры.
        m_inputContext = m_window.CreateInput();
        foreach (var keyboard in m_inputContext.Keyboards)
        {
            keyboard.KeyDown += (kb, key, _) => m_Input?.KeyDown(key);
            keyboard.KeyUp += (kb, key, _) => m_Input?.KeyUp(key);
        }
    }

    private void OnUpdate(double deltaTime)
    {
        // Выполняем один кадр логики.
        if (m_done || !Frame())
        {
            m_done = true;
            m_window.Close();
        }
    }

    private void OnRender(double deltaTime)
    {
        // Рендеринг будет добавлен в последующих туториалах.
    }

    private void OnClosing()
    {
        m_done = true;
    }
}

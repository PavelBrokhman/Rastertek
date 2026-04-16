////////////////////////////////////////////////////////////////////////////////
// Filename: OpenGLClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial03;

public class OpenGLClass
{
    private GL _driver;
    private IWindow _window;

    private Matrix4X4<float> _worldMatrix;
    private Matrix4X4<float> _projectionMatrix;

    private string _videoCardDescription;

    // В C++ все 31 функция OpenGL загружалась вручную через wglGetProcAddress.
    // Silk.NET делает это автоматически — GL объект получает все функции сразу.
    public GL Driver => _driver;

    public bool Initialize(
        IWindow window,
        int screenWidth,
        int screenHeight,
        float screenDepth,
        float screenNear,
        bool vsync
    )
    {
        _window = window;

        // Получаем GL API из окна Silk.NET (замена wglCreateContextAttribsARB + загрузки функций).
        _driver = GL.GetApi(window);

        // Сохраняем информацию о видеокарте.
        var vendor = _driver.GetStringS(StringName.Vendor) ?? string.Empty;
        var renderer = _driver.GetStringS(StringName.Renderer) ?? string.Empty;
        _videoCardDescription = vendor + " - " + renderer;

        // Устанавливаем depth buffer clear value.
        _driver.ClearDepth(1.0f);

        // Включаем depth testing.
        _driver.Enable(EnableCap.DepthTest);

        // Устанавливаем прямое направление для front faces.
        _driver.FrontFace(FrontFaceDirection.Ccw);

        // Включаем back-face culling.
        _driver.Enable(EnableCap.CullFace);
        _driver.CullFace(TriangleFace.Back);

        // Задаём viewport.
        _driver.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);

        // Инициализируем world matrix в identity.
        _worldMatrix = Matrix4X4<float>.Identity;

        // Создаём проекционную матрицу left-handed с fov π/4.
        float fieldOfView = MathF.PI / 4.0f;
        float screenAspect = (float)screenWidth / (float)screenHeight;
        _projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(
            fieldOfView,
            screenAspect,
            screenNear,
            screenDepth
        );

        // VSync контролируется через свойство окна Silk.NET (см. SystemClass).
        _ = vsync;

        return true;
    }

    public void Shutdown()
    {
        _driver?.Dispose();
        _driver = null;
    }

    // В C++ было BeginScene(r,g,b,a) с glClearColor + glClear.
    public void BeginScene(float red, float green, float blue, float alpha)
    {
        _driver.ClearColor(red, green, blue, alpha);
        _driver.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    // В C++ было SwapBuffers(_deviceContext). Silk.NET.Windowing вызывает
    // SwapBuffers автоматически после коллбэка Render, поэтому здесь пусто.
    public void EndScene() { }

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

    public string GetVideoCardInfo() => _videoCardDescription;
}

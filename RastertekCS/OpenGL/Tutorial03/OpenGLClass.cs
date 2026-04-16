////////////////////////////////////////////////////////////////////////////////
// Filename: OpenGLClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial03;

public class OpenGLClass
{
    private GL m_gl;
    private IWindow m_window;

    private Matrix4X4<float> m_worldMatrix;
    private Matrix4X4<float> m_projectionMatrix;

    private string m_videoCardDescription;

    // В C++ все 31 функция OpenGL загружалась вручную через wglGetProcAddress.
    // Silk.NET делает это автоматически — GL объект получает все функции сразу.
    public GL Gl => m_gl;

    public bool Initialize(
        IWindow window,
        int screenWidth,
        int screenHeight,
        float screenDepth,
        float screenNear,
        bool vsync
    )
    {
        m_window = window;

        // Получаем GL API из окна Silk.NET (замена wglCreateContextAttribsARB + загрузки функций).
        m_gl = GL.GetApi(window);

        // Сохраняем информацию о видеокарте.
        var vendor = m_gl.GetStringS(StringName.Vendor) ?? string.Empty;
        var renderer = m_gl.GetStringS(StringName.Renderer) ?? string.Empty;
        m_videoCardDescription = vendor + " - " + renderer;

        // Устанавливаем depth buffer clear value.
        m_gl.ClearDepth(1.0f);

        // Включаем depth testing.
        m_gl.Enable(EnableCap.DepthTest);

        // Устанавливаем прямое направление для front faces.
        m_gl.FrontFace(FrontFaceDirection.Ccw);

        // Включаем back-face culling.
        m_gl.Enable(EnableCap.CullFace);
        m_gl.CullFace(TriangleFace.Back);

        // Задаём viewport.
        m_gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);

        // Инициализируем world matrix в identity.
        m_worldMatrix = Matrix4X4<float>.Identity;

        // Создаём проекционную матрицу left-handed с fov π/4.
        float fieldOfView = MathF.PI / 4.0f;
        float screenAspect = (float)screenWidth / (float)screenHeight;
        m_projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(
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
        m_gl?.Dispose();
        m_gl = null;
    }

    // В C++ было BeginScene(r,g,b,a) с glClearColor + glClear.
    public void BeginScene(float red, float green, float blue, float alpha)
    {
        m_gl.ClearColor(red, green, blue, alpha);
        m_gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    // В C++ было SwapBuffers(m_deviceContext). Silk.NET.Windowing вызывает
    // SwapBuffers автоматически после коллбэка Render, поэтому здесь пусто.
    public void EndScene() { }

    public Matrix4X4<float> GetWorldMatrix() => m_worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;

    public string GetVideoCardInfo() => m_videoCardDescription;
}

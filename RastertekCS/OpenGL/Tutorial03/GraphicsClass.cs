////////////////////////////////////////////////////////////////////////////////
// Filename: GraphicsClass.cs
////////////////////////////////////////////////////////////////////////////////
namespace RastertekCS.OpenGL.Tutorial03;

public class GraphicsClass
{
    /////////////
    // GLOBALS //
    /////////////
    public const bool FULL_SCREEN = false;
    public const bool VSYNC_ENABLED = true;
    public const float SCREEN_DEPTH = 1000.0f;
    public const float SCREEN_NEAR = 0.1f;

    private OpenGLClass m_OpenGL;

    public bool Initialize(OpenGLClass OpenGL)
    {
        m_OpenGL = OpenGL;
        return true;
    }

    public void Shutdown()
    {
        m_OpenGL = null;
    }

    public bool Frame()
    {
        return Render();
    }

    private bool Render()
    {
        // Очищаем буферы серо-синим цветом (как в оригинале).
        m_OpenGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        // SwapBuffers выполняется Silk.NET автоматически.
        m_OpenGL.EndScene();

        return true;
    }
}

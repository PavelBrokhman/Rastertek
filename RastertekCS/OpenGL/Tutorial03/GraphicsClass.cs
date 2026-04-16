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

    private OpenGLClass _openGL;

    public bool Initialize(OpenGLClass OpenGL)
    {
        _openGL = OpenGL;
        return true;
    }

    public void Shutdown()
    {
        _openGL = null;
    }

    public bool Frame()
    {
        return Render();
    }

    private bool Render()
    {
        // Очищаем буферы серо-синим цветом (как в оригинале).
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        // SwapBuffers выполняется Silk.NET автоматически.
        _openGL.EndScene();

        return true;
    }
}

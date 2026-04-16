namespace RastertekCS.OpenGL.Tutorial12.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;
    private const int BITMAP_SIZE = 256;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private TextureShader m_TextureShader;
    private Bitmap m_Bitmap;
    private int m_screenWidth,
        m_screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -1.0f);

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL))
            return false;

        m_Bitmap = new Bitmap();
        if (
            !m_Bitmap.Initialize(
                OpenGL,
                screenWidth,
                screenHeight,
                "Data/Stone01.tga",
                TEXTURE_UNIT,
                BITMAP_SIZE,
                BITMAP_SIZE
            )
        )
            return false;
        m_Bitmap.SetRenderLocation(100, 100);

        return true;
    }

    public void Shutdown()
    {
        m_Bitmap?.Shutdown(m_OpenGL);
        m_TextureShader?.Shutdown(m_OpenGL);
        m_Bitmap = null;
        m_TextureShader = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        m_OpenGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.Render();
        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        // Отключаем z-buffer для 2D рендеринга.
        m_OpenGL.TurnZBufferOff();

        m_TextureShader.SetShader(m_OpenGL);
        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, ortho, (int)TEXTURE_UNIT))
            return false;
        m_Bitmap.Render(m_OpenGL);

        m_OpenGL.TurnZBufferOn();

        m_OpenGL.EndScene();
        return true;
    }
}

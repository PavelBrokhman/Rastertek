namespace RastertekCS.OpenGL.Tutorial13.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private TextureShader m_TextureShader;
    private Sprite m_Sprite;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -1.0f);

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL)) return false;

        m_Sprite = new Sprite();
        if (!m_Sprite.Initialize(OpenGL, screenWidth, screenHeight, "Data/Sprite.txt", TEXTURE_UNIT)) return false;
        m_Sprite.SetRenderLocation(100, 100);
        return true;
    }

    public void Shutdown()
    {
        m_Sprite?.Shutdown(m_OpenGL);
        m_TextureShader?.Shutdown(m_OpenGL);
        m_Sprite = null; m_TextureShader = null; m_Camera = null; m_OpenGL = null;
    }

    public bool Frame(float frameTimeMs)
    {
        m_Sprite.Update(frameTimeMs);
        return Render();
    }

    private bool Render()
    {
        m_OpenGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);
        m_Camera.Render();
        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        m_OpenGL.TurnZBufferOff();

        m_TextureShader.SetShader(m_OpenGL);
        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, ortho, (int)TEXTURE_UNIT)) return false;
        m_Sprite.SetTexture(m_OpenGL, TEXTURE_UNIT);
        m_Sprite.Render(m_OpenGL);

        m_OpenGL.TurnZBufferOn();
        m_OpenGL.EndScene();
        return true;
    }
}

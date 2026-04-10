namespace RastertekCS.OpenGL.Tutorial05.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private TextureShader m_TextureShader;

    public bool Initialize(GL4 OpenGL)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Data/Stone01.tga", TEXTURE_UNIT, true)) return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_TextureShader?.Shutdown(m_OpenGL);
        m_Model?.Shutdown(m_OpenGL);
        m_TextureShader = null;
        m_Model = null;
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
        var projection = m_OpenGL.GetProjectionMatrix();

        m_TextureShader.SetShader(m_OpenGL);
        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, projection, (int)TEXTURE_UNIT)) return false;

        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}

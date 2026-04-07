namespace RastertekCS.OpenGL.Tutorial17.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT_1 = 0;
    private const uint TEXTURE_UNIT_2 = 1;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private MultiTextureShader m_MultiTextureShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -5);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Data/square.txt",
            "Data/stone01.tga", TEXTURE_UNIT_1,
            "Data/dirt01.tga", TEXTURE_UNIT_2))
        return false;

        m_MultiTextureShader = new MultiTextureShader();
        if (!m_MultiTextureShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_MultiTextureShader?.Shutdown(m_OpenGL); m_MultiTextureShader = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Camera = null; m_OpenGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        m_MultiTextureShader.SetShader(m_OpenGL);
        m_Model.SetTextures(m_OpenGL, TEXTURE_UNIT_1, TEXTURE_UNIT_2);

        if (!m_MultiTextureShader.SetShaderParameters(m_OpenGL, world, view, projection,
            (int)TEXTURE_UNIT_1, (int)TEXTURE_UNIT_2)) return false;

        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}

namespace RastertekCS.OpenGL.Tutorial04.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private ColorShader m_ColorShader;

    public bool Initialize(GL4 OpenGL)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL)) return false;

        m_ColorShader = new ColorShader();
        if (!m_ColorShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_ColorShader?.Shutdown(m_OpenGL);
        m_Model?.Shutdown(m_OpenGL);
        m_ColorShader = null;
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

        m_ColorShader.SetShader(m_OpenGL);
        if (!m_ColorShader.SetShaderParameters(m_OpenGL, world, view, projection)) return false;

        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}

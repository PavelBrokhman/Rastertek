namespace RastertekCS.OpenGL.Tutorial28.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private TranslateShader m_TranslateShader;
    private float m_textureTranslation;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -5);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/square.txt", "Data/stone01.tga", 0, true))
            return false;

        m_TranslateShader = new TranslateShader();
        if (!m_TranslateShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_TranslateShader?.Shutdown(m_OpenGL);
        m_TranslateShader = null;
        m_Model?.Shutdown(m_OpenGL);
        m_Model = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame()
    {
        m_textureTranslation += 0.01f;
        if (m_textureTranslation > 1.0f)
            m_textureTranslation -= 1.0f;

        return Render(m_textureTranslation);
    }

    private bool Render(float textureTranslation)
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        m_TranslateShader.SetShader(m_OpenGL);
        m_Model.SetTexture(m_OpenGL, 0);

        if (
            !m_TranslateShader.SetShaderParameters(
                m_OpenGL,
                world,
                view,
                projection,
                textureTranslation,
                0
            )
        )
            return false;

        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}

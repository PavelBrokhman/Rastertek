using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial29.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model1;
    private Model m_Model2;
    private TextureShader m_TextureShader;
    private TransparentShader m_TransparentShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -5);
        m_Camera.Render();

        // First model uses dirt texture.
        m_Model1 = new Model();
        if (!m_Model1.Initialize(OpenGL, "Data/square.txt", "Data/dirt01.tga", 0))
            return false;

        // Second model uses stone texture.
        m_Model2 = new Model();
        if (!m_Model2.Initialize(OpenGL, "Data/square.txt", "Data/stone01.tga", 0))
            return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL)) return false;

        m_TransparentShader = new TransparentShader();
        if (!m_TransparentShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_TransparentShader?.Shutdown(m_OpenGL); m_TransparentShader = null;
        m_TextureShader?.Shutdown(m_OpenGL); m_TextureShader = null;
        m_Model2?.Shutdown(m_OpenGL); m_Model2 = null;
        m_Model1?.Shutdown(m_OpenGL); m_Model1 = null;
        m_Camera = null; m_OpenGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        float blendAmount = 0.5f;

        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        // Render the first model with the regular texture shader.
        m_TextureShader.SetShader(m_OpenGL);
        m_Model1.SetTexture(m_OpenGL, 0);

        if (!m_TextureShader.SetShaderParameters(m_OpenGL, world, view, projection, 0))
            return false;

        m_Model1.Render(m_OpenGL);

        // Translate to the right by one unit and towards the camera by one unit.
        var world2 = Matrix4X4.CreateTranslation<float>(1.0f, 0.0f, -1.0f);

        // Turn on alpha blending for the transparency to work.
        m_OpenGL.EnableAlphaBlending();

        // Render the second model with the transparent shader.
        m_TransparentShader.SetShader(m_OpenGL);
        m_Model2.SetTexture(m_OpenGL, 0);

        if (!m_TransparentShader.SetShaderParameters(m_OpenGL, world2, view, projection, blendAmount, 0))
            return false;

        m_Model2.Render(m_OpenGL);

        // Turn off alpha blending.
        m_OpenGL.DisableAlphaBlending();

        m_OpenGL.EndScene();
        return true;
    }
}

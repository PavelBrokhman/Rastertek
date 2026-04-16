using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial26.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private FogShader m_FogShader;
    private float m_rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/Cube.txt", "Data/stone01.tga", 0))
            return false;

        m_FogShader = new FogShader();
        if (!m_FogShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_FogShader?.Shutdown(m_OpenGL);
        m_FogShader = null;
        m_Model?.Shutdown(m_OpenGL);
        m_Model = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation < 0.0f)
            m_rotation += MathF.Tau;

        return Render(m_rotation);
    }

    private bool Render(float rotation)
    {
        float fogColor = 0.5f;
        float fogStart = 0.0f;
        float fogEnd = 10.0f;

        m_OpenGL.BeginScene(fogColor, fogColor, fogColor, 1.0f);

        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        m_FogShader.SetShader(m_OpenGL);
        m_Model.SetTexture(m_OpenGL, 0);

        if (
            !m_FogShader.SetShaderParameters(m_OpenGL, world, view, projection, fogStart, fogEnd, 0)
        )
            return false;

        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial27.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private ClipPlaneShader m_ClipPlaneShader;
    private float m_rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/cube.txt", "Data/stone01.tga", 0))
            return false;

        m_ClipPlaneShader = new ClipPlaneShader();
        if (!m_ClipPlaneShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_ClipPlaneShader?.Shutdown(m_OpenGL); m_ClipPlaneShader = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Camera = null; m_OpenGL = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation <= 0.0f)
            m_rotation += 360.0f;

        return Render(m_rotation);
    }

    private bool Render(float rotation)
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        // Enable clip planes.
        m_OpenGL.EnableClipping();

        m_ClipPlaneShader.SetShader(m_OpenGL);
        m_Model.SetTexture(m_OpenGL, 0);

        // Clip plane: y = 0, clips everything below y=0.
        if (!m_ClipPlaneShader.SetShaderParameters(m_OpenGL, world, view, projection,
            0.0f, -1.0f, 0.0f, 0.0f, 0))
            return false;

        m_Model.Render(m_OpenGL);

        // Disable clip planes.
        m_OpenGL.DisableClipping();

        m_OpenGL.EndScene();
        return true;
    }
}

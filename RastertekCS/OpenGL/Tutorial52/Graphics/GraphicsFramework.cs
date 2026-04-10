using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial52.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Light m_Light;
    private Model m_Model;
    private PbrShader m_PbrShader;
    private float m_rotation;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -5);
        m_Camera.Render();

        m_Light = new Light();
        m_Light.SetDirection(0.5f, 0.5f, 0.5f);

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/sphere.txt", "Data/pbr_albedo.tga", "Data/pbr_normal.tga", "Data/pbr_roughmetal.tga")) return false;

        m_PbrShader = new PbrShader();
        if (!m_PbrShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_PbrShader?.Shutdown(m_OpenGL); m_PbrShader = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Light = null; m_Camera = null; m_OpenGL = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 0.1f;
        if (m_rotation < 0.0f) m_rotation += 360.0f;
        return Render(m_rotation);
    }

    private bool Render(float rotation)
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var cameraPos = m_Camera.GetPosition();
        var lightDir = m_Light.GetDirection();

        if (!m_PbrShader.SetShaderParameters(m_OpenGL, world, view, projection, cameraPos, lightDir)) return false;
        m_Model.SetTextures(m_OpenGL, 0, 1, 2);
        m_Model.Render(m_OpenGL);

        m_OpenGL.EndScene();
        return true;
    }
}

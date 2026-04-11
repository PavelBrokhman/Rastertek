using Silk.NET.Maths;
namespace RastertekCS.OpenGL.Tutorial39.Graphics;
public class GraphicsFramework
{
    private GL4 m_OpenGL; private Camera m_Camera; private Model m_GroundModel, m_CubeModel;
    private ProjectionShader m_ProjectionShader; private Texture m_ProjectionTexture; private ViewPoint m_ViewPoint;

    public bool Initialize(GL4 gl4, int sw, int sh)
    {
        m_OpenGL = gl4;
        m_Camera = new Camera(); m_Camera.SetPosition(0, 7, -10); m_Camera.SetRotation(35, 0, 0); m_Camera.Render();
        m_GroundModel = new Model(); if (!m_GroundModel.Initialize(gl4, "Models/plane01.txt", "Data/metal001.tga")) return false;
        m_CubeModel = new Model(); if (!m_CubeModel.Initialize(gl4, "Models/Cube.txt", "Data/stone01.tga")) return false;
        m_ProjectionShader = new ProjectionShader(); if (!m_ProjectionShader.Initialize(gl4)) return false;
        m_ProjectionTexture = new Texture(); if (!m_ProjectionTexture.Initialize(gl4, "Data/opengl_logo.tga", 1, false)) return false;
        m_ViewPoint = new ViewPoint();
        m_ViewPoint.SetPosition(2, 5, -2); m_ViewPoint.SetLookAt(0, 0, 0);
        m_ViewPoint.SetProjectionParameters(MathF.PI / 2f, 1f, 0.1f, 100f);
        m_ViewPoint.GenerateViewMatrix(); m_ViewPoint.GenerateProjectionMatrix();
        return true;
    }
    public void Shutdown()
    {
        m_ProjectionTexture?.Shutdown(m_OpenGL); m_ProjectionTexture = null;
        m_ProjectionShader?.Shutdown(m_OpenGL); m_ProjectionShader = null;
        m_CubeModel?.Shutdown(m_OpenGL); m_CubeModel = null;
        m_GroundModel?.Shutdown(m_OpenGL); m_GroundModel = null;
        m_Camera = null; m_ViewPoint = null; m_OpenGL = null;
    }
    public bool Frame() => Render();
    private bool Render()
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);
        var world = m_OpenGL.GetWorldMatrix(); var view = m_Camera.GetViewMatrix(); var proj = m_OpenGL.GetProjectionMatrix();
        var v2 = m_ViewPoint.GetViewMatrix(); var p2 = m_ViewPoint.GetProjectionMatrix();

        var groundWorld = Matrix4X4.CreateTranslation<float>(0, 1, 0);
        if (!m_ProjectionShader.SetShaderParameters(m_OpenGL, groundWorld, view, proj, v2, p2)) return false;
        m_ProjectionTexture.SetTexture(m_OpenGL, 1); m_GroundModel.SetTexture(m_OpenGL, 0); m_GroundModel.Render(m_OpenGL);

        var cubeWorld = Matrix4X4.CreateTranslation<float>(0, 2, 0);
        if (!m_ProjectionShader.SetShaderParameters(m_OpenGL, cubeWorld, view, proj, v2, p2)) return false;
        m_ProjectionTexture.SetTexture(m_OpenGL, 1); m_CubeModel.SetTexture(m_OpenGL, 0); m_CubeModel.Render(m_OpenGL);

        m_OpenGL.EndScene(); return true;
    }
}

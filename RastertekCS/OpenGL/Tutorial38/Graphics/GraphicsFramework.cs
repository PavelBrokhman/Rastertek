namespace RastertekCS.OpenGL.Tutorial38.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private ParticleSystem m_ParticleSystem;
    private ParticleShader m_ParticleShader;

    public bool Initialize(GL4 OpenGL, int sw, int sh)
    {
        m_OpenGL = OpenGL;
        m_Camera = new Camera();
        m_Camera.SetPosition(0, -1, -10);
        m_Camera.Render();

        m_ParticleSystem = new ParticleSystem();
        if (!m_ParticleSystem.Initialize(OpenGL, "Data/star01.tga")) return false;

        m_ParticleShader = new ParticleShader();
        if (!m_ParticleShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_ParticleShader?.Shutdown(m_OpenGL); m_ParticleShader = null;
        m_ParticleSystem?.Shutdown(); m_ParticleSystem = null;
        m_Camera = null; m_OpenGL = null;
    }

    public bool Frame(float dt)
    {
        m_ParticleSystem.Frame(dt);
        return Render();
    }

    private bool Render()
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);
        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var proj = m_OpenGL.GetProjectionMatrix();
        m_OpenGL.EnableAlphaBlending();
        if (!m_ParticleShader.SetShaderParameters(m_OpenGL, world, view, proj)) return false;
        m_ParticleSystem.Render();
        m_OpenGL.DisableAlphaBlending();
        m_OpenGL.EndScene();
        return true;
    }
}

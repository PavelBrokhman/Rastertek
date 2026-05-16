namespace RastertekCS.OpenGL.Tutorial55.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private ParticleSystem m_ParticleSystem;
    private ParticleShader m_ParticleShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();
        m_Camera.RenderBaseViewMatrix();

        m_ParticleSystem = new ParticleSystem();
        if (!m_ParticleSystem.Initialize(OpenGL, "Data/particle_config_01.txt")) return false;

        m_ParticleShader = new ParticleShader();
        if (!m_ParticleShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_ParticleShader?.Shutdown(m_OpenGL); m_ParticleShader = null;
        m_ParticleSystem?.Shutdown(); m_ParticleSystem = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame(float frameTime)
    {
        m_ParticleSystem.Frame(frameTime);

        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        m_OpenGL.EnableParticleAlphaBlending();
        m_OpenGL.TurnZBufferOff();

        if (!m_ParticleShader.SetShaderParameters(m_OpenGL, world, view, projection)) return false;
        m_ParticleSystem.Render();

        m_OpenGL.TurnZBufferOn();
        m_OpenGL.DisableAlphaBlending();
        m_OpenGL.EndScene();
        return true;
    }
}

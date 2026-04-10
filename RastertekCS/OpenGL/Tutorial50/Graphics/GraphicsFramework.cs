using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Light m_Light;
    private Model m_Model;
    private OrthoWindow m_FullScreenWindow;
    private DeferredBuffers m_DeferredBuffers;
    private DeferredShader m_DeferredShader;
    private LightShader m_LightShader;
    private float m_rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();
        m_Camera.RenderBaseViewMatrix();

        m_Light = new Light();
        m_Light.SetDiffuseColor(1, 1, 1, 1);
        m_Light.SetDirection(0, 0, 1);

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/cube.txt", "Data/stone01.tga", 0)) return false;

        m_FullScreenWindow = new OrthoWindow();
        if (!m_FullScreenWindow.Initialize(OpenGL, screenWidth, screenHeight)) return false;

        m_DeferredBuffers = new DeferredBuffers();
        if (!m_DeferredBuffers.Initialize(OpenGL, screenWidth, screenHeight, 0.3f, 1000.0f)) return false;

        m_DeferredShader = new DeferredShader();
        if (!m_DeferredShader.Initialize(OpenGL)) return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_LightShader?.Shutdown(m_OpenGL); m_LightShader = null;
        m_DeferredShader?.Shutdown(m_OpenGL); m_DeferredShader = null;
        m_DeferredBuffers?.Shutdown(m_OpenGL); m_DeferredBuffers = null;
        m_FullScreenWindow?.Shutdown(m_OpenGL); m_FullScreenWindow = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Light = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 0.5f;
        if (m_rotation <= 0.0f) m_rotation += 360.0f;

        if (!RenderSceneToTexture(m_rotation)) return false;
        if (!Render()) return false;
        return true;
    }

    private bool RenderSceneToTexture(float rotation)
    {
        m_DeferredBuffers.SetRenderTarget(m_OpenGL);
        m_DeferredBuffers.ClearRenderTargets(m_OpenGL, 0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        world = Matrix4X4.CreateRotationY<float>(rotation);

        if (!m_DeferredShader.SetShaderParameters(m_OpenGL, world, view, projection, 0)) return false;
        m_Model.SetTexture(m_OpenGL, 0);
        m_Model.Render(m_OpenGL);

        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();

        return true;
    }

    private bool Render()
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var baseView = m_Camera.GetBaseViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        var lightDirection = m_Light.GetDirection();

        m_OpenGL.TurnZBufferOff();

        if (!m_LightShader.SetShaderParameters(m_OpenGL, world, baseView, ortho, lightDirection, 0, 1)) return false;

        m_DeferredBuffers.SetTexture(m_OpenGL, 0, 0);
        m_DeferredBuffers.SetTexture(m_OpenGL, 1, 1);

        m_FullScreenWindow.Render(m_OpenGL);

        m_OpenGL.TurnZBufferOn();

        m_OpenGL.EndScene();
        return true;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial51.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Light m_Light;
    private Model m_SphereModel, m_GroundModel;
    private DeferredBuffers m_DeferredBuffers;
    private GBufferShader m_GBufferShader;
    private RenderTexture m_SsaoRenderTexture, m_BlurSsaoRenderTexture;
    private OrthoWindow m_FullScreenWindow;
    private SsaoShader m_SsaoShader;
    private Texture m_RandomTexture;
    private SsaoBlurShader m_SsaoBlurShader;
    private LightShader m_LightShader;
    private int m_screenWidth, m_screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.RenderBaseViewMatrix();
        m_Camera.SetPosition(0, 7, -10);
        m_Camera.SetRotation(35, 0, 0);
        m_Camera.Render();

        m_Light = new Light();
        m_Light.SetDiffuseColor(1, 1, 1, 1);
        m_Light.SetDirection(1, -0.5f, 0);

        m_SphereModel = new Model();
        if (!m_SphereModel.Initialize(OpenGL, "Models/sphere.txt", "Data/ice.tga", 0)) return false;

        m_GroundModel = new Model();
        if (!m_GroundModel.Initialize(OpenGL, "Models/plane01.txt", "Data/metal001.tga", 0)) return false;

        m_DeferredBuffers = new DeferredBuffers();
        if (!m_DeferredBuffers.Initialize(OpenGL, screenWidth, screenHeight, 0.3f, 1000.0f)) return false;

        m_GBufferShader = new GBufferShader();
        if (!m_GBufferShader.Initialize(OpenGL)) return false;

        m_SsaoRenderTexture = new RenderTexture();
        if (!m_SsaoRenderTexture.Initialize(OpenGL, screenWidth, screenHeight, 0.3f, 1000.0f, 1)) return false;

        m_BlurSsaoRenderTexture = new RenderTexture();
        if (!m_BlurSsaoRenderTexture.Initialize(OpenGL, screenWidth, screenHeight, 0.3f, 1000.0f, 1)) return false;

        m_FullScreenWindow = new OrthoWindow();
        if (!m_FullScreenWindow.Initialize(OpenGL, screenWidth, screenHeight)) return false;

        m_SsaoShader = new SsaoShader();
        if (!m_SsaoShader.Initialize(OpenGL)) return false;

        m_RandomTexture = new Texture();
        if (!m_RandomTexture.Initialize(OpenGL, "Data/random_vec.tga", 0, true)) return false;

        m_SsaoBlurShader = new SsaoBlurShader();
        if (!m_SsaoBlurShader.Initialize(OpenGL)) return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_LightShader?.Shutdown(m_OpenGL); m_LightShader = null;
        m_SsaoBlurShader?.Shutdown(m_OpenGL); m_SsaoBlurShader = null;
        m_RandomTexture?.Shutdown(m_OpenGL); m_RandomTexture = null;
        m_SsaoShader?.Shutdown(m_OpenGL); m_SsaoShader = null;
        m_FullScreenWindow?.Shutdown(m_OpenGL); m_FullScreenWindow = null;
        m_BlurSsaoRenderTexture?.Shutdown(m_OpenGL); m_BlurSsaoRenderTexture = null;
        m_SsaoRenderTexture?.Shutdown(m_OpenGL); m_SsaoRenderTexture = null;
        m_GBufferShader?.Shutdown(m_OpenGL); m_GBufferShader = null;
        m_DeferredBuffers?.Shutdown(m_OpenGL); m_DeferredBuffers = null;
        m_GroundModel?.Shutdown(m_OpenGL); m_GroundModel = null;
        m_SphereModel?.Shutdown(m_OpenGL); m_SphereModel = null;
        m_Light = null; m_Camera = null; m_OpenGL = null;
    }

    public bool Frame()
    {
        if (!RenderGBuffer()) return false;
        if (!RenderSsao()) return false;
        if (!BlurSsaoTexture()) return false;
        if (!Render()) return false;
        return true;
    }

    private bool RenderGBuffer()
    {
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        m_DeferredBuffers.SetRenderTarget(m_OpenGL);
        m_DeferredBuffers.ClearRenderTargets(m_OpenGL, 0, 0, 0, 0);

        var translate = Matrix4X4.CreateTranslation<float>(2, 2, 0);
        if (!m_GBufferShader.SetShaderParameters(m_OpenGL, translate, view, projection, 0)) return false;
        m_SphereModel.SetTexture(m_OpenGL, 0);
        m_SphereModel.Render(m_OpenGL);

        translate = Matrix4X4.CreateTranslation<float>(0, 1, 0);
        if (!m_GBufferShader.SetShaderParameters(m_OpenGL, translate, view, projection, 0)) return false;
        m_GroundModel.SetTexture(m_OpenGL, 0);
        m_GroundModel.Render(m_OpenGL);

        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();
        return true;
    }

    private bool RenderSsao()
    {
        var world = m_OpenGL.GetWorldMatrix();
        var baseView = m_Camera.GetBaseViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        m_SsaoRenderTexture.SetRenderTarget(m_OpenGL);
        m_SsaoRenderTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 0);

        m_OpenGL.TurnZBufferOff();

        if (!m_SsaoShader.SetShaderParameters(m_OpenGL, world, baseView, ortho,
            (float)m_screenWidth, (float)m_screenHeight, 64.0f, 1.0f, 1.0f, 0.1f, 2.0f)) return false;

        m_DeferredBuffers.SetShaderResourcePositions(m_OpenGL, 0);
        m_DeferredBuffers.SetShaderResourceNormals(m_OpenGL, 1);
        m_RandomTexture.SetTexture(m_OpenGL, 2);

        m_FullScreenWindow.Render(m_OpenGL);

        m_OpenGL.TurnZBufferOn();
        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();
        return true;
    }

    private bool BlurSsaoTexture()
    {
        var world = m_OpenGL.GetWorldMatrix();
        var baseView = m_Camera.GetBaseViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        m_OpenGL.TurnZBufferOff();

        // Horizontal blur
        m_BlurSsaoRenderTexture.SetRenderTarget(m_OpenGL);
        m_BlurSsaoRenderTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 1);
        if (!m_SsaoBlurShader.SetShaderParameters(m_OpenGL, world, baseView, ortho, m_screenWidth, m_screenHeight, 0)) return false;
        m_SsaoRenderTexture.SetTexture(m_OpenGL, 0);
        m_DeferredBuffers.SetShaderResourceNormals(m_OpenGL, 1);
        m_FullScreenWindow.Render(m_OpenGL);

        // Vertical blur
        m_SsaoRenderTexture.SetRenderTarget(m_OpenGL);
        m_SsaoRenderTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 1);
        if (!m_SsaoBlurShader.SetShaderParameters(m_OpenGL, world, baseView, ortho, m_screenWidth, m_screenHeight, 1)) return false;
        m_BlurSsaoRenderTexture.SetTexture(m_OpenGL, 0);
        m_DeferredBuffers.SetShaderResourceNormals(m_OpenGL, 1);
        m_FullScreenWindow.Render(m_OpenGL);

        m_OpenGL.TurnZBufferOn();
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
        var view = m_Camera.GetViewMatrix();
        var lightDirection = m_Light.GetDirection();

        m_OpenGL.TurnZBufferOff();

        if (!m_LightShader.SetShaderParameters(m_OpenGL, world, baseView, ortho, view, lightDirection)) return false;

        m_DeferredBuffers.SetShaderResourceNormals(m_OpenGL, 0);
        m_SsaoRenderTexture.SetTexture(m_OpenGL, 1);
        m_DeferredBuffers.SetShaderResourceColors(m_OpenGL, 2);

        m_FullScreenWindow.Render(m_OpenGL);

        m_OpenGL.TurnZBufferOn();
        m_OpenGL.EndScene();
        return true;
    }
}

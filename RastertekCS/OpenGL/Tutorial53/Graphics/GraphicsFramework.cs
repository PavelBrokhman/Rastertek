using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial53.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Model m_Model;
    private Light m_Light;
    private LightShader m_LightShader;
    private RenderTexture m_RenderTexture;
    private OrthoWindow m_FullScreenWindow;
    private TextureShader m_TextureShader;
    private BlurShader m_BlurShader;
    private Blur m_Blur;
    private Heat m_Heat;
    private HeatShader m_HeatShader;
    private RenderTexture m_HeatTexture;
    private int m_screenWidth, m_screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_screenWidth = screenWidth; m_screenHeight = screenHeight;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();
        m_Camera.RenderBaseViewMatrix();

        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Models/sphere.txt", "Data/yellowcolor01.tga", 0)) return false;

        m_Light = new Light();
        m_Light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
        m_Light.SetDiffuseColor(1, 1, 1, 1);
        m_Light.SetDirection(0, 0, 1);

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(OpenGL)) return false;

        m_RenderTexture = new RenderTexture();
        if (!m_RenderTexture.Initialize(OpenGL, screenWidth, screenHeight, 0.3f, 1000.0f, 0)) return false;

        m_FullScreenWindow = new OrthoWindow();
        if (!m_FullScreenWindow.Initialize(OpenGL, screenWidth, screenHeight)) return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(OpenGL)) return false;

        m_BlurShader = new BlurShader();
        if (!m_BlurShader.Initialize(OpenGL)) return false;

        int downSampleWidth = screenWidth / 2, downSampleHeight = screenHeight / 2;
        m_Blur = new Blur();
        if (!m_Blur.Initialize(OpenGL, downSampleWidth, downSampleHeight, 0.3f, 1000.0f, screenWidth, screenHeight)) return false;

        m_Heat = new Heat();
        if (!m_Heat.Initialize(OpenGL)) return false;

        m_HeatShader = new HeatShader();
        if (!m_HeatShader.Initialize(OpenGL)) return false;

        m_HeatTexture = new RenderTexture();
        if (!m_HeatTexture.Initialize(OpenGL, screenWidth, screenHeight, 0.3f, 1000.0f, 0)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_HeatTexture?.Shutdown(m_OpenGL); m_HeatTexture = null;
        m_HeatShader?.Shutdown(m_OpenGL); m_HeatShader = null;
        m_Heat?.Shutdown(m_OpenGL); m_Heat = null;
        m_Blur?.Shutdown(m_OpenGL); m_Blur = null;
        m_BlurShader?.Shutdown(m_OpenGL); m_BlurShader = null;
        m_TextureShader?.Shutdown(m_OpenGL); m_TextureShader = null;
        m_FullScreenWindow?.Shutdown(m_OpenGL); m_FullScreenWindow = null;
        m_RenderTexture?.Shutdown(m_OpenGL); m_RenderTexture = null;
        m_LightShader?.Shutdown(m_OpenGL); m_LightShader = null;
        m_Light = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame(float frameTime)
    {
        m_Heat.Frame(frameTime);

        if (!RenderSceneToTexture()) return false;
        if (!RenderHeatToTexture()) return false;
        if (!m_Blur.BlurTexture(m_HeatTexture, m_OpenGL, m_Camera, m_TextureShader, m_BlurShader)) return false;
        if (!Render()) return false;
        return true;
    }

    private bool RenderSceneToTexture()
    {
        m_RenderTexture.SetRenderTarget(m_OpenGL);
        m_RenderTexture.ClearRenderTarget(m_OpenGL, 0.25f, 0.25f, 0.25f, 1.0f);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        var lightDir = m_Light.GetDirection();
        var diffuse = m_Light.GetDiffuseColor();
        var ambient = m_Light.GetAmbientLight();

        if (!m_LightShader.SetShaderParameters(m_OpenGL, world, view, projection, lightDir, diffuse, ambient)) return false;
        m_Model.SetTexture(m_OpenGL, 0);
        m_Model.Render(m_OpenGL);

        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();
        return true;
    }

    private bool RenderHeatToTexture()
    {
        m_HeatTexture.SetRenderTarget(m_OpenGL);
        m_HeatTexture.ClearRenderTarget(m_OpenGL, 0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();

        var lightDir = m_Light.GetDirection();
        var diffuse = m_Light.GetDiffuseColor();
        var ambient = m_Light.GetAmbientLight();

        if (!m_LightShader.SetShaderParameters(m_OpenGL, world, view, projection, lightDir, diffuse, ambient)) return false;
        m_Model.SetTexture(m_OpenGL, 0);
        m_Model.Render(m_OpenGL);

        m_OpenGL.SetBackBufferRenderTarget();
        m_OpenGL.ResetViewport();
        return true;
    }

    private bool Render()
    {
        m_Heat.GetNoiseValues(out var scrollSpeeds, out var scales, out var d1, out var d2, out var d3, out var emissive, out var noiseTime);

        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var baseView = m_Camera.GetBaseViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        m_OpenGL.TurnZBufferOff();

        if (!m_HeatShader.SetShaderParameters(m_OpenGL, world, baseView, ortho, emissive, noiseTime, scrollSpeeds, scales, d1, d2, d3)) return false;

        m_RenderTexture.SetTexture(m_OpenGL, 0);
        m_HeatTexture.SetTexture(m_OpenGL, 1);
        m_Heat.SetTexture(m_OpenGL, 2);

        m_FullScreenWindow.Render(m_OpenGL);

        m_OpenGL.TurnZBufferOn();
        m_OpenGL.EndScene();
        return true;
    }
}

namespace RastertekCS.OpenGL.Tutorial54.Graphics;

public class GraphicsFramework
{
    private GL4 m_OpenGL;
    private Camera m_Camera;
    private OrthoWindow m_FullScreenWindow;
    private ParallaxScroll m_ParallaxForest;
    private ScrollShader m_ScrollShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();
        m_Camera.RenderBaseViewMatrix();

        m_FullScreenWindow = new OrthoWindow();
        if (!m_FullScreenWindow.Initialize(OpenGL, screenWidth, screenHeight)) return false;

        m_ParallaxForest = new ParallaxScroll();
        if (!m_ParallaxForest.Initialize(OpenGL, "Data/parallax_config.txt")) return false;

        m_ScrollShader = new ScrollShader();
        if (!m_ScrollShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_ScrollShader?.Shutdown(m_OpenGL); m_ScrollShader = null;
        m_ParallaxForest?.Shutdown(m_OpenGL); m_ParallaxForest = null;
        m_FullScreenWindow?.Shutdown(m_OpenGL); m_FullScreenWindow = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame(float frameTime)
    {
        m_ParallaxForest.Frame(frameTime);

        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var baseView = m_Camera.GetBaseViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        m_OpenGL.EnableAlphaBlending();
        m_OpenGL.TurnZBufferOff();

        int count = m_ParallaxForest.GetTextureCount();
        for (int i = 0; i < count; i++)
        {
            if (!m_ScrollShader.SetShaderParameters(m_OpenGL, world, baseView, ortho, m_ParallaxForest.GetTranslation(i))) return false;
            m_ParallaxForest.SetTexture(m_OpenGL, i, 0);
            m_FullScreenWindow.Render(m_OpenGL);
        }

        m_OpenGL.TurnZBufferOn();
        m_OpenGL.DisableAlphaBlending();
        m_OpenGL.EndScene();
        return true;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial32.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR  = 0.3f;

    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private Model m_WinModel;
    private Texture m_NormalTexture;
    private RenderTexture m_RenderTexture;
    private TextureShader m_TextureShader;
    private GlassShader m_GlassShader;
    private float m_rotation = 360f;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", false)) return false;

        m_WinModel = new Model();
        if (!m_WinModel.Initialize(DirectX, "Models/square.txt", "Data/glass01.tga", false)) return false;

        m_NormalTexture = new Texture();
        if (!m_NormalTexture.Initialize(DirectX, "Data/normal03.tga", false)) return false;

        m_RenderTexture = new RenderTexture();
        if (!m_RenderTexture.Initialize(DirectX, screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR)) return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(DirectX)) return false;

        m_GlassShader = new GlassShader();
        if (!m_GlassShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_GlassShader?.Shutdown();
        m_TextureShader?.Shutdown();
        m_RenderTexture?.Shutdown();
        m_NormalTexture?.Shutdown();
        m_WinModel?.Shutdown();
        m_Model?.Shutdown();
        m_GlassShader = null;
        m_TextureShader = null;
        m_RenderTexture = null;
        m_NormalTexture = null;
        m_WinModel = null;
        m_Model = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f;
        if (m_rotation <= 0.0f) m_rotation += 360.0f;

        if (!RenderToTexture(m_rotation)) return false;
        return Render(m_rotation);
    }

    private bool RenderToTexture(float rot)
    {
        m_RenderTexture.SetRenderTarget(m_DirectX);
        m_RenderTexture.ClearRenderTarget(m_DirectX, 0.0f, 0.0f, 0.0f, 1.0f);

        var view       = m_Camera.GetViewMatrix();
        var projection = m_RenderTexture.GetProjectionMatrix();
        var world      = Matrix4X4.CreateRotationY<float>(rot);

        m_Model.Render(m_DirectX);
        if (!m_TextureShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection,
                m_Model.GetTextureView()))
            return false;

        m_DirectX.SetBackBufferRenderTarget();
        m_DirectX.ResetViewport();
        return true;
    }

    private bool Render(float rot)
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view       = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        // Render spinning cube.
        var world = Matrix4X4.CreateRotationY<float>(rot);
        m_Model.Render(m_DirectX);
        if (!m_TextureShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection,
                m_Model.GetTextureView()))
            return false;

        // Render glass window in front.
        world = Matrix4X4.CreateTranslation<float>(0.0f, 0.0f, -1.5f);
        m_WinModel.Render(m_DirectX);
        if (!m_GlassShader.Render(m_DirectX, m_WinModel.GetIndexCount(), world, view, projection,
                m_WinModel.GetTextureView(),
                m_NormalTexture.GetTextureView(),
                m_RenderTexture.GetShaderResourceView(),
                0.01f))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

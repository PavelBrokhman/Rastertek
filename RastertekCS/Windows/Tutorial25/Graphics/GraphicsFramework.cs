using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial25.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private TextureShader m_TextureShader;
    private RenderTexture m_RenderTexture;
    private DisplayPlane m_DisplayPlane;
    private float m_rotation = MathF.Tau;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true)) return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(DirectX)) return false;

        m_RenderTexture = new RenderTexture();
        if (!m_RenderTexture.Initialize(DirectX, 256, 256, SCREEN_DEPTH, SCREEN_NEAR)) return false;

        m_DisplayPlane = new DisplayPlane();
        if (!m_DisplayPlane.Initialize(DirectX, 1.0f, 1.0f)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_DisplayPlane?.Shutdown();
        m_RenderTexture?.Shutdown();
        m_TextureShader?.Shutdown();
        m_Model?.Shutdown();
        m_DisplayPlane = null;
        m_RenderTexture = null;
        m_TextureShader = null;
        m_Model = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation < 0.0f) m_rotation += MathF.Tau;
        if (!RenderSceneToTexture()) return false;
        return Render();
    }

    private bool RenderSceneToTexture()
    {
        m_RenderTexture.SetRenderTarget(m_DirectX);
        m_RenderTexture.ClearRenderTarget(m_DirectX, 0.0f, 0.5f, 1.0f, 1.0f);

        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        var view = m_Camera.GetViewMatrix();
        var projection = m_RenderTexture.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY(m_rotation);

        m_Model.Render(m_DirectX);
        if (!m_TextureShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection,
            m_Model.GetTextureView()))
            return false;

        m_DirectX.SetBackBufferRenderTarget();
        m_DirectX.ResetViewport();

        return true;
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();
        var rtSrv = m_RenderTexture.GetShaderResourceView();

        var world1 = Matrix4X4.CreateTranslation(0.0f, 1.5f, 0.0f);
        m_DisplayPlane.Render(m_DirectX);
        if (!m_TextureShader.Render(m_DirectX, m_DisplayPlane.GetIndexCount(), world1, view, projection, rtSrv))
            return false;

        var world2 = Matrix4X4.CreateTranslation(-1.5f, -1.5f, 0.0f);
        m_DisplayPlane.Render(m_DirectX);
        if (!m_TextureShader.Render(m_DirectX, m_DisplayPlane.GetIndexCount(), world2, view, projection, rtSrv))
            return false;

        var world3 = Matrix4X4.CreateTranslation(1.5f, -1.5f, 0.0f);
        m_DisplayPlane.Render(m_DirectX);
        if (!m_TextureShader.Render(m_DirectX, m_DisplayPlane.GetIndexCount(), world3, view, projection, rtSrv))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

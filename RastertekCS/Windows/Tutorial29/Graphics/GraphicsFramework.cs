using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial29.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model1;
    private Model m_Model2;
    private TextureShader m_TextureShader;
    private TransparentShader m_TransparentShader;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        m_Model1 = new Model();
        if (!m_Model1.Initialize(DirectX, "Models/square.txt", "Data/dirt01.tga", true))
            return false;

        m_Model2 = new Model();
        if (!m_Model2.Initialize(DirectX, "Models/square.txt", "Data/stone01.tga", true))
            return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(DirectX))
            return false;

        m_TransparentShader = new TransparentShader();
        if (!m_TransparentShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_TransparentShader?.Shutdown();
        m_TextureShader?.Shutdown();
        m_Model2?.Shutdown();
        m_Model1?.Shutdown();
        m_TransparentShader = null;
        m_TextureShader = null;
        m_Model2 = null;
        m_Model1 = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        var world1 = m_DirectX.GetWorldMatrix();
        m_Model1.Render(m_DirectX);
        if (
            !m_TextureShader.Render(
                m_DirectX,
                m_Model1.GetIndexCount(),
                world1,
                view,
                projection,
                m_Model1.GetTextureView()
            )
        )
            return false;

        var world2 = Matrix4X4.CreateTranslation(1.0f, 0.0f, -1.0f);
        m_DirectX.EnableAlphaBlending();
        m_Model2.Render(m_DirectX);
        if (
            !m_TransparentShader.Render(
                m_DirectX,
                m_Model2.GetIndexCount(),
                world2,
                view,
                projection,
                m_Model2.GetTextureView(),
                0.5f
            )
        )
            return false;
        m_DirectX.DisableAlphaBlending();

        m_DirectX.EndScene();
        return true;
    }
}

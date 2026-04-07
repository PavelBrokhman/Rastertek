namespace RastertekCS.Windows.Tutorial05.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private TextureShader m_TextureShader;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Textures/Stone01.tga", true)) return false;

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_TextureShader?.Shutdown();
        m_Model?.Shutdown();
        m_TextureShader = null;
        m_Model = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.Render();

        var world = m_DirectX.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        m_Model.Render(m_DirectX);
        m_Model.SetTexture(m_DirectX, 0);

        if (!m_TextureShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

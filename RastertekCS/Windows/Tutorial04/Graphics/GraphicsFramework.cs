namespace RastertekCS.Windows.Tutorial04.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private ColorShader m_ColorShader;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX))
            return false;

        m_ColorShader = new ColorShader();
        if (!m_ColorShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_ColorShader?.Shutdown();
        m_Model?.Shutdown();
        m_ColorShader = null;
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

        if (!m_ColorShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

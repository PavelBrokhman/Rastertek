using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial18.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private LightMapShader m_LightMapShader;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/square.txt", "Data/stone01.tga", "Data/light01.tga", true))
            return false;

        m_LightMapShader = new LightMapShader();
        if (!m_LightMapShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_LightMapShader?.Shutdown();
        m_Model?.Shutdown();
        m_LightMapShader = null;
        m_Model = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = m_DirectX.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        m_Model.Render(m_DirectX);
        m_Model.SetTextures(m_DirectX);

        if (!m_LightMapShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial27.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private ClipPlaneShader m_ClipPlaneShader;
    private readonly float[] m_clipPlane = new float[] { 0.0f, -1.0f, 0.0f, 0.0f };

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true)) return false;

        m_ClipPlaneShader = new ClipPlaneShader();
        if (!m_ClipPlaneShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_ClipPlaneShader?.Shutdown();
        m_Model?.Shutdown();
        m_ClipPlaneShader = null;
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
        m_Model.SetTexture(m_DirectX, 0);

        if (!m_ClipPlaneShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection, m_clipPlane))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

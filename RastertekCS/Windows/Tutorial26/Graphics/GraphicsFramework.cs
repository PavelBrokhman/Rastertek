using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial26.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private FogShader m_FogShader;
    private float m_rotation = MathF.Tau;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true)) return false;

        m_FogShader = new FogShader();
        if (!m_FogShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_FogShader?.Shutdown();
        m_Model?.Shutdown();
        m_FogShader = null;
        m_Model = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 0.25f;
        if (m_rotation < 0.0f) m_rotation += MathF.Tau;
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.5f, 0.5f, 0.5f, 1.0f);

        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY(m_rotation);

        m_Model.Render(m_DirectX);
        m_Model.SetTexture(m_DirectX, 0);

        if (!m_FogShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection,
            0.0f, 10.0f))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

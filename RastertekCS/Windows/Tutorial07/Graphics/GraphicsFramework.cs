using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial07.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private LightShader m_LightShader;
    private Light m_Light;
    private float m_rotation;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/Cube.txt", "Data/Stone01.tga", true)) return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(DirectX)) return false;

        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(1.0f, 0.0f, 1.0f);

        return true;
    }

    public void Shutdown()
    {
        m_LightShader?.Shutdown();
        m_Model?.Shutdown();
        m_LightShader = null;
        m_Model = null;
        m_Camera = null;
        m_Light = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.01f;
        if (m_rotation < 0.0f) m_rotation += MathF.Tau;
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.Render();

        var world = DXMath.RotationYLH(m_rotation);
        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        m_Model.Render(m_DirectX);
        m_Model.SetTexture(m_DirectX, 0);

        if (!m_LightShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection,
            m_Light.GetDirection(), m_Light.GetDiffuseColor()))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial08.Graphics;

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
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/Cube.txt", "Data/Stone01.tga", true)) return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(DirectX)) return false;

        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, 0.0f, 1.0f);

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
        m_rotation += 0.01f;
        if (m_rotation > MathF.Tau) m_rotation -= MathF.Tau;
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.Render();

        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        // Первый куб: rotation + translation (слева).
        var rotate1 = Matrix4X4.CreateRotationY(m_rotation);
        var translate1 = Matrix4X4.CreateTranslation(-2.0f, 0.0f, 0.0f);
        var world1 = rotate1 * translate1;

        m_Model.Render(m_DirectX);
        m_Model.SetTexture(m_DirectX, 0);

        if (!m_LightShader.Render(m_DirectX, m_Model.GetIndexCount(), world1, view, projection,
            m_Light.GetDirection(), m_Light.GetDiffuseColor()))
            return false;

        // Второй куб: scale + rotation + translation (справа).
        var scale2 = Matrix4X4.CreateScale(0.5f);
        var rotate2 = Matrix4X4.CreateRotationY(m_rotation);
        var translate2 = Matrix4X4.CreateTranslation(2.0f, 0.0f, 0.0f);
        var world2 = scale2 * rotate2 * translate2;

        m_Model.Render(m_DirectX);
        m_Model.SetTexture(m_DirectX, 0);

        if (!m_LightShader.Render(m_DirectX, m_Model.GetIndexCount(), world2, view, projection,
            m_Light.GetDirection(), m_Light.GetDiffuseColor()))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

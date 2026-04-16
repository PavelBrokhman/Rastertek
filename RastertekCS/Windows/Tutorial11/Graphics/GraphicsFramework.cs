using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial11.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private LightShader m_LightShader;
    private Light[] m_Lights;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 2.0f, -12.0f);
        m_Camera.SetRotation(15.0f, 0.0f, 0.0f);

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/Plane.txt", "Data/Stone01.tga", true))
            return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(DirectX))
            return false;

        m_Lights = new Light[LightShader.NUM_LIGHTS];
        m_Lights[0] = new Light();
        m_Lights[0].SetDiffuseColor(1.0f, 0.0f, 0.0f, 1.0f);
        m_Lights[0].SetPosition(-3.0f, 1.0f, 3.0f);

        m_Lights[1] = new Light();
        m_Lights[1].SetDiffuseColor(0.0f, 1.0f, 0.0f, 1.0f);
        m_Lights[1].SetPosition(3.0f, 1.0f, 3.0f);

        m_Lights[2] = new Light();
        m_Lights[2].SetDiffuseColor(0.0f, 0.0f, 1.0f, 1.0f);
        m_Lights[2].SetPosition(-3.0f, 1.0f, -3.0f);

        m_Lights[3] = new Light();
        m_Lights[3].SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Lights[3].SetPosition(3.0f, 1.0f, -3.0f);

        return true;
    }

    public void Shutdown()
    {
        m_LightShader?.Shutdown();
        m_Model?.Shutdown();
        m_LightShader = null;
        m_Model = null;
        m_Camera = null;
        m_Lights = null;
        m_DirectX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        m_Camera.Render();

        var world = Matrix4X4<float>.Identity;
        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        var positions = new float[3 * LightShader.NUM_LIGHTS];
        var colors = new float[4 * LightShader.NUM_LIGHTS];
        for (int i = 0; i < LightShader.NUM_LIGHTS; i++)
        {
            var p = m_Lights[i].GetPosition();
            var c = m_Lights[i].GetDiffuseColor();
            positions[i * 3 + 0] = p[0];
            positions[i * 3 + 1] = p[1];
            positions[i * 3 + 2] = p[2];
            colors[i * 4 + 0] = c[0];
            colors[i * 4 + 1] = c[1];
            colors[i * 4 + 2] = c[2];
            colors[i * 4 + 3] = c[3];
        }

        m_Model.Render(m_DirectX);
        m_Model.SetTexture(m_DirectX, 0);

        if (
            !m_LightShader.Render(
                m_DirectX,
                m_Model.GetIndexCount(),
                world,
                view,
                projection,
                positions,
                colors
            )
        )
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

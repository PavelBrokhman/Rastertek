using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial22.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private Light m_Light;
    private TextureShader m_TextureShader;
    private LightShader m_LightShader;
    private NormalMapShader m_NormalMapShader;
    private float m_rotation = MathF.Tau;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -8.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (
            !m_Model.Initialize(
                DirectX,
                "Models/sphere.txt",
                "Data/stone01.tga",
                "Data/normal01.tga",
                true
            )
        )
            return false;

        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, 0.0f, 1.0f);

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(DirectX))
            return false;

        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(DirectX))
            return false;

        m_NormalMapShader = new NormalMapShader();
        if (!m_NormalMapShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_NormalMapShader?.Shutdown();
        m_LightShader?.Shutdown();
        m_TextureShader?.Shutdown();
        m_Model?.Shutdown();
        m_NormalMapShader = null;
        m_LightShader = null;
        m_TextureShader = null;
        m_Model = null;
        m_Light = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_rotation -= 0.0174532925f * 1.0f;
        if (m_rotation <= 0.0f)
            m_rotation += MathF.Tau;
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();
        var rotate = Matrix4X4.CreateRotationY(m_rotation);

        var world1 = rotate * Matrix4X4.CreateTranslation(0.0f, 1.0f, 0.0f);
        m_Model.Render(m_DirectX);
        m_Model.SetTextures(m_DirectX);
        if (!m_TextureShader.Render(m_DirectX, m_Model.GetIndexCount(), world1, view, projection))
            return false;

        var world2 = rotate * Matrix4X4.CreateTranslation(-1.5f, -1.0f, 0.0f);
        m_Model.Render(m_DirectX);
        m_Model.SetTextures(m_DirectX);
        if (
            !m_LightShader.Render(
                m_DirectX,
                m_Model.GetIndexCount(),
                world2,
                view,
                projection,
                m_Light.GetDirection(),
                m_Light.GetDiffuseColor()
            )
        )
            return false;

        var world3 = rotate * Matrix4X4.CreateTranslation(1.5f, -1.0f, 0.0f);
        m_Model.Render(m_DirectX);
        m_Model.SetTextures(m_DirectX);
        if (
            !m_NormalMapShader.Render(
                m_DirectX,
                m_Model.GetIndexCount(),
                world3,
                view,
                projection,
                m_Light.GetDirection(),
                m_Light.GetDiffuseColor()
            )
        )
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

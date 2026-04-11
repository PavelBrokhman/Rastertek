using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial28.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private TranslateShader m_TranslateShader;
    private float m_textureTranslation;

    public bool Initialize(DX11 DirectX)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true)) return false;

        m_TranslateShader = new TranslateShader();
        if (!m_TranslateShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_TranslateShader?.Shutdown();
        m_Model?.Shutdown();
        m_TranslateShader = null;
        m_Model = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_textureTranslation += 0.01f;
        if (m_textureTranslation > 1.0f) m_textureTranslation -= 1.0f;
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = m_DirectX.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        m_Model.Render(m_DirectX);
        m_Model.SetTexture(m_DirectX, 0);

        if (!m_TranslateShader.Render(m_DirectX, m_Model.GetIndexCount(), world, view, projection, m_textureTranslation))
            return false;

        m_DirectX.EndScene();
        return true;
    }
}

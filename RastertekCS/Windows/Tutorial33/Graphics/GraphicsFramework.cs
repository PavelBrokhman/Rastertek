using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial33.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 m_DirectX;
    private Camera m_Camera;
    private Model m_Model;
    private Texture m_FireTexture;
    private Texture m_NoiseTexture;
    private Texture m_AlphaTexture;
    private FireShader m_FireShader;
    private float m_frameTime;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -5.0f);
        m_Camera.Render();

        m_Model = new Model();
        if (!m_Model.Initialize(DirectX, "Models/square.txt", "Data/fire01.tga", false))
            return false;

        m_FireTexture = new Texture();
        if (!m_FireTexture.Initialize(DirectX, "Data/fire01.tga", false))
            return false;
        m_NoiseTexture = new Texture();
        if (!m_NoiseTexture.Initialize(DirectX, "Data/noise01.tga", true))
            return false;
        m_AlphaTexture = new Texture();
        if (!m_AlphaTexture.Initialize(DirectX, "Data/alpha01.tga", false))
            return false;

        m_FireShader = new FireShader();
        if (!m_FireShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_FireShader?.Shutdown();
        m_AlphaTexture?.Shutdown();
        m_NoiseTexture?.Shutdown();
        m_FireTexture?.Shutdown();
        m_Model?.Shutdown();
        m_FireShader = null;
        m_AlphaTexture = null;
        m_NoiseTexture = null;
        m_FireTexture = null;
        m_Model = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        m_frameTime += 0.01f;
        if (m_frameTime > 1000.0f)
            m_frameTime = 0.0f;
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = m_DirectX.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_DirectX.GetProjectionMatrix();

        float[] scrollSpeeds = { 1.3f, 2.1f, 2.3f };
        float[] scales = { 1.0f, 2.0f, 3.0f };
        float[] d1 = { 0.1f, 0.2f };
        float[] d2 = { 0.1f, 0.3f };
        float[] d3 = { 0.1f, 0.1f };

        m_DirectX.EnableAlphaBlending();

        m_Model.Render(m_DirectX);
        if (
            !m_FireShader.Render(
                m_DirectX,
                m_Model.GetIndexCount(),
                world,
                view,
                projection,
                m_frameTime,
                scrollSpeeds,
                scales,
                d1,
                d2,
                d3,
                0.8f,
                0.5f,
                m_FireTexture.GetTextureView(),
                m_NoiseTexture.GetTextureView(),
                m_AlphaTexture.GetTextureView()
            )
        )
            return false;

        m_DirectX.DisableAlphaBlending();

        m_DirectX.EndScene();
        return true;
    }
}

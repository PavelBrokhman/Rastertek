using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial12.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private TextureShader m_TextureShader;
    private Bitmap m_Bitmap;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        m_TextureShader = new TextureShader();
        if (!m_TextureShader.Initialize(DirectX)) return false;

        m_Bitmap = new Bitmap();
        if (!m_Bitmap.Initialize(DirectX, screenWidth, screenHeight, "Data/Stone01.tga", 50, 50)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_Bitmap?.Shutdown();
        m_TextureShader?.Shutdown();
        m_Bitmap = null;
        m_TextureShader = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = m_DirectX.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var ortho = m_DirectX.GetOrthoMatrix();

        m_DirectX.TurnZBufferOff();

        if (!m_Bitmap.Render(m_DirectX)) return false;
        m_Bitmap.SetTexture(m_DirectX, 0);

        if (!m_TextureShader.Render(m_DirectX, m_Bitmap.GetIndexCount(), world, view, ortho))
            return false;

        m_DirectX.TurnZBufferOn();

        m_DirectX.EndScene();
        return true;
    }
}

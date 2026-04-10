using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial14.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private FontShader m_FontShader;
    private Font m_Font;
    private Text m_TextString1;
    private Text m_TextString2;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        m_DirectX = DirectX;

        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();

        m_FontShader = new FontShader();
        if (!m_FontShader.Initialize(DirectX)) return false;

        m_Font = new Font();
        if (!m_Font.Initialize(DirectX, 0)) return false;

        m_TextString1 = new Text();
        if (!m_TextString1.Initialize(DirectX, screenWidth, screenHeight, 32, m_Font,
                                       "Hello", 10, 10, 0.0f, 1.0f, 0.0f)) return false;

        m_TextString2 = new Text();
        if (!m_TextString2.Initialize(DirectX, screenWidth, screenHeight, 32, m_Font,
                                       "Goodbye", 10, 50, 1.0f, 1.0f, 0.0f)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_TextString2?.Shutdown();
        m_TextString1?.Shutdown();
        m_Font?.Shutdown();
        m_FontShader?.Shutdown();
        m_TextString2 = null;
        m_TextString1 = null;
        m_Font = null;
        m_FontShader = null;
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
        m_DirectX.EnableAlphaBlending();

        m_Font.SetTexture(m_DirectX, 0);

        m_TextString1.Render(m_DirectX);
        if (!m_FontShader.Render(m_DirectX, m_TextString1.GetIndexCount(), world, view, ortho,
                                  m_TextString1.GetPixelColor()))
            return false;

        m_TextString2.Render(m_DirectX);
        if (!m_FontShader.Render(m_DirectX, m_TextString2.GetIndexCount(), world, view, ortho,
                                  m_TextString2.GetPixelColor()))
            return false;

        m_DirectX.DisableAlphaBlending();
        m_DirectX.TurnZBufferOn();

        m_DirectX.EndScene();
        return true;
    }
}

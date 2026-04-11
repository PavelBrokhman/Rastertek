using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial16.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private FontShader m_FontShader;
    private Font m_Font;
    private Text[] m_MouseStrings;

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

        m_MouseStrings = new Text[3];
        m_MouseStrings[0] = new Text();
        if (!m_MouseStrings[0].Initialize(DirectX, screenWidth, screenHeight, 32, m_Font,
                                           "Mouse X: 0", 10, 10, 1, 1, 1)) return false;
        m_MouseStrings[1] = new Text();
        if (!m_MouseStrings[1].Initialize(DirectX, screenWidth, screenHeight, 32, m_Font,
                                           "Mouse Y: 0", 10, 35, 1, 1, 1)) return false;
        m_MouseStrings[2] = new Text();
        if (!m_MouseStrings[2].Initialize(DirectX, screenWidth, screenHeight, 32, m_Font,
                                           "Mouse Button: No", 10, 60, 1, 1, 1)) return false;

        return true;
    }

    public void Shutdown()
    {
        if (m_MouseStrings != null)
            foreach (var t in m_MouseStrings) t?.Shutdown();
        m_Font?.Shutdown();
        m_FontShader?.Shutdown();
        m_MouseStrings = null;
        m_Font = null;
        m_FontShader = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame(int mouseX, int mouseY, bool mouseDown)
    {
        if (!m_MouseStrings[0].UpdateText(m_DirectX, m_Font, $"Mouse X: {mouseX}", 10, 10, 1, 1, 1)) return false;
        if (!m_MouseStrings[1].UpdateText(m_DirectX, m_Font, $"Mouse Y: {mouseY}", 10, 35, 1, 1, 1)) return false;
        if (!m_MouseStrings[2].UpdateText(m_DirectX, m_Font,
            mouseDown ? "Mouse Button: Yes" : "Mouse Button: No", 10, 60, 1, 1, 1)) return false;
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = m_DirectX.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var ortho = m_DirectX.GetOrthoMatrix();

        m_DirectX.TurnZBufferOff();
        m_DirectX.EnableAlphaBlending();

        m_Font.SetTexture(m_DirectX, 0);

        for (int i = 0; i < 3; i++)
        {
            m_MouseStrings[i].Render(m_DirectX);
            if (!m_FontShader.Render(m_DirectX, m_MouseStrings[i].GetIndexCount(), world, view, ortho,
                                      m_MouseStrings[i].GetPixelColor()))
                return false;
        }

        m_DirectX.DisableAlphaBlending();
        m_DirectX.TurnZBufferOn();

        m_DirectX.EndScene();
        return true;
    }
}

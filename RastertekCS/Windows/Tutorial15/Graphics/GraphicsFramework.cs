using RastertekCS.Windows.Tutorial15.System;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial15.Graphics;

public class GraphicsFramework
{
    private DX11 m_DirectX;
    private Camera m_Camera;
    private FontShader m_FontShader;
    private Font m_Font;
    private Text m_FpsString;
    private Fps m_Fps;
    private int m_previousFps;

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

        m_Fps = new Fps();
        m_Fps.Initialize();
        m_previousFps = -1;

        m_FpsString = new Text();
        if (!m_FpsString.Initialize(DirectX, screenWidth, screenHeight, 32, m_Font,
                                     "Fps: 0", 10, 10, 0.0f, 1.0f, 0.0f)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_FpsString?.Shutdown();
        m_Font?.Shutdown();
        m_FontShader?.Shutdown();
        m_FpsString = null;
        m_Font = null;
        m_FontShader = null;
        m_Fps = null;
        m_Camera = null;
        m_DirectX = null;
    }

    public bool Frame()
    {
        if (!UpdateFps()) return false;
        return Render();
    }

    private bool UpdateFps()
    {
        m_Fps.Frame();
        int fps = m_Fps.GetFps();
        if (m_previousFps == fps) return true;
        m_previousFps = fps;

        if (fps > 99999) fps = 99999;

        float r, g, b;
        if (fps >= 60) { r = 0.0f; g = 1.0f; b = 0.0f; }
        else if (fps >= 30) { r = 1.0f; g = 1.0f; b = 0.0f; }
        else { r = 1.0f; g = 0.0f; b = 0.0f; }

        return m_FpsString.UpdateText(m_DirectX, m_Font, $"Fps: {fps}", 10, 10, r, g, b);
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

        m_FpsString.Render(m_DirectX);
        if (!m_FontShader.Render(m_DirectX, m_FpsString.GetIndexCount(), world, view, ortho,
                                  m_FpsString.GetPixelColor()))
            return false;

        m_DirectX.DisableAlphaBlending();
        m_DirectX.TurnZBufferOn();

        m_DirectX.EndScene();
        return true;
    }
}

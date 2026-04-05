using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial14.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Font m_Font;
    private FontShader m_FontShader;
    private Text m_Text1;
    private Text m_Text2;
    private int m_screenWidth, m_screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -1);

        m_Font = new Font();
        if (!m_Font.Initialize(OpenGL, "Data/Font.txt", "Textures/Font.tga", TEXTURE_UNIT)) return false;

        m_FontShader = new FontShader();
        if (!m_FontShader.Initialize(OpenGL)) return false;

        m_Text1 = new Text();
        if (!m_Text1.Initialize(OpenGL, m_Font, "Hello", 10, 10, 0, 1, 0, screenWidth, screenHeight, 32)) return false;

        m_Text2 = new Text();
        if (!m_Text2.Initialize(OpenGL, m_Font, "Goodbye", 10, 30, 1, 1, 0, screenWidth, screenHeight, 32)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_Text2?.Shutdown(m_OpenGL); m_Text2 = null;
        m_Text1?.Shutdown(m_OpenGL); m_Text1 = null;
        m_FontShader?.Shutdown(m_OpenGL); m_FontShader = null;
        m_Font?.Shutdown(m_OpenGL); m_Font = null;
        m_Camera = null; m_OpenGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);
        m_Camera.Render();
        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        m_OpenGL.TurnZBufferOff();

        // Alpha blending для прозрачности чёрных пикселей шрифта.
        m_OpenGL.Gl.Enable(EnableCap.Blend);
        m_OpenGL.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        m_FontShader.SetShader(m_OpenGL);
        m_Font.SetTexture(m_OpenGL, TEXTURE_UNIT);

        if (!m_FontShader.SetShaderParameters(m_OpenGL, world, view, ortho, (int)TEXTURE_UNIT, m_Text1.GetPixelColor())) return false;
        m_Text1.Render(m_OpenGL);

        if (!m_FontShader.SetShaderParameters(m_OpenGL, world, view, ortho, (int)TEXTURE_UNIT, m_Text2.GetPixelColor())) return false;
        m_Text2.Render(m_OpenGL);

        m_OpenGL.Gl.Disable(EnableCap.Blend);
        m_OpenGL.TurnZBufferOn();
        m_OpenGL.EndScene();
        return true;
    }
}

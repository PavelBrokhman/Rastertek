using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial15.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Font m_Font;
    private FontShader m_FontShader;
    private Text m_FpsText;
    private int m_screenWidth,
        m_screenHeight;
    private int m_previousFps = -1;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;

        m_Camera = new Camera();
        m_Camera.SetPosition(0, 0, -10);
        m_Camera.Render();

        m_Font = new Font();
        if (!m_Font.Initialize(OpenGL, "Data/font01.txt", "Data/font01.tga", TEXTURE_UNIT))
            return false;

        m_FontShader = new FontShader();
        if (!m_FontShader.Initialize(OpenGL))
            return false;

        m_FpsText = new Text();
        if (
            !m_FpsText.Initialize(
                OpenGL,
                m_Font,
                "FPS: 0",
                10,
                10,
                0,
                1,
                0,
                screenWidth,
                screenHeight,
                32
            )
        )
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_FpsText?.Shutdown(m_OpenGL);
        m_FpsText = null;
        m_FontShader?.Shutdown(m_OpenGL);
        m_FontShader = null;
        m_Font?.Shutdown(m_OpenGL);
        m_Font = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame(int fps)
    {
        if (fps != m_previousFps)
        {
            m_previousFps = fps;
            float r = 0,
                g = 1,
                b = 0;
            if (fps < 60)
            {
                r = 1;
                g = 1;
                b = 0;
            }
            if (fps < 30)
            {
                r = 1;
                g = 0;
                b = 0;
            }
            m_FpsText.UpdateText(
                m_OpenGL,
                m_Font,
                $"FPS: {fps}",
                10,
                10,
                r,
                g,
                b,
                m_screenWidth,
                m_screenHeight
            );
        }
        return Render();
    }

    private bool Render()
    {
        m_OpenGL.BeginScene(0, 0, 0, 1);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        m_OpenGL.TurnZBufferOff();

        m_OpenGL.Gl.Enable(EnableCap.Blend);
        m_OpenGL.Gl.BlendFuncSeparate(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha,
            BlendingFactor.One,
            BlendingFactor.Zero
        );

        m_FontShader.SetShader(m_OpenGL);
        m_Font.SetTexture(m_OpenGL, TEXTURE_UNIT);
        if (
            !m_FontShader.SetShaderParameters(
                m_OpenGL,
                world,
                view,
                ortho,
                (int)TEXTURE_UNIT,
                m_FpsText.GetPixelColor()
            )
        )
            return false;
        m_FpsText.Render(m_OpenGL);

        m_OpenGL.Gl.Disable(EnableCap.Blend);
        m_OpenGL.TurnZBufferOn();
        m_OpenGL.EndScene();
        return true;
    }
}

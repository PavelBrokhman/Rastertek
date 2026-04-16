using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial16.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 m_OpenGL;
    private Camera m_Camera;
    private Font m_Font;
    private FontShader m_FontShader;
    private Text[] m_MouseStrings;
    private int m_screenWidth,
        m_screenHeight;

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

        m_MouseStrings = new Text[3];
        m_MouseStrings[0] = new Text();
        if (
            !m_MouseStrings[0]
                .Initialize(
                    OpenGL,
                    m_Font,
                    "Mouse X: 0",
                    10,
                    10,
                    1,
                    1,
                    1,
                    screenWidth,
                    screenHeight,
                    32
                )
        )
            return false;
        m_MouseStrings[1] = new Text();
        if (
            !m_MouseStrings[1]
                .Initialize(
                    OpenGL,
                    m_Font,
                    "Mouse Y: 0",
                    10,
                    35,
                    1,
                    1,
                    1,
                    screenWidth,
                    screenHeight,
                    32
                )
        )
            return false;
        m_MouseStrings[2] = new Text();
        if (
            !m_MouseStrings[2]
                .Initialize(
                    OpenGL,
                    m_Font,
                    "Mouse Button: No",
                    10,
                    60,
                    1,
                    1,
                    1,
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
        if (m_MouseStrings != null)
        {
            foreach (var t in m_MouseStrings)
                t?.Shutdown(m_OpenGL);
            m_MouseStrings = null;
        }
        m_FontShader?.Shutdown(m_OpenGL);
        m_FontShader = null;
        m_Font?.Shutdown(m_OpenGL);
        m_Font = null;
        m_Camera = null;
        m_OpenGL = null;
    }

    public bool Frame(int mouseX, int mouseY, bool mouseDown)
    {
        if (!UpdateMouseStrings(mouseX, mouseY, mouseDown))
            return false;
        return Render();
    }

    private bool UpdateMouseStrings(int mouseX, int mouseY, bool mouseDown)
    {
        m_MouseStrings[0]
            .UpdateText(
                m_OpenGL,
                m_Font,
                $"Mouse X: {mouseX}",
                10,
                10,
                1,
                1,
                1,
                m_screenWidth,
                m_screenHeight
            );
        m_MouseStrings[1]
            .UpdateText(
                m_OpenGL,
                m_Font,
                $"Mouse Y: {mouseY}",
                10,
                35,
                1,
                1,
                1,
                m_screenWidth,
                m_screenHeight
            );
        m_MouseStrings[2]
            .UpdateText(
                m_OpenGL,
                m_Font,
                mouseDown ? "Mouse Button: Yes" : "Mouse Button: No",
                10,
                60,
                1,
                1,
                1,
                m_screenWidth,
                m_screenHeight
            );
        return true;
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

        var color = m_MouseStrings[0].GetPixelColor();
        if (
            !m_FontShader.SetShaderParameters(
                m_OpenGL,
                world,
                view,
                ortho,
                (int)TEXTURE_UNIT,
                color
            )
        )
            return false;

        m_MouseStrings[0].Render(m_OpenGL);
        m_MouseStrings[1].Render(m_OpenGL);
        m_MouseStrings[2].Render(m_OpenGL);

        m_OpenGL.Gl.Disable(EnableCap.Blend);
        m_OpenGL.TurnZBufferOn();
        m_OpenGL.EndScene();
        return true;
    }
}

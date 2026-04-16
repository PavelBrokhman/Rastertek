using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial16.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 _openGL;
    private Camera _camera;
    private Font _font;
    private FontShader _fontShader;
    private Text[] _mouseStrings;
    private int _screenWidth,
        _screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _camera = new Camera();
        _camera.SetPosition(0, 0, -10);
        _camera.Render();

        _font = new Font();
        if (!_font.Initialize(OpenGL, "Data/font01.txt", "Data/font01.tga", TEXTURE_UNIT))
            return false;

        _fontShader = new FontShader();
        if (!_fontShader.Initialize(OpenGL))
            return false;

        _mouseStrings = new Text[3];
        _mouseStrings[0] = new Text();
        if (
            !_mouseStrings[0]
                .Initialize(
                    OpenGL,
                    _font,
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
        _mouseStrings[1] = new Text();
        if (
            !_mouseStrings[1]
                .Initialize(
                    OpenGL,
                    _font,
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
        _mouseStrings[2] = new Text();
        if (
            !_mouseStrings[2]
                .Initialize(
                    OpenGL,
                    _font,
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
        if (_mouseStrings != null)
        {
            foreach (var t in _mouseStrings)
                t?.Shutdown(_openGL);
            _mouseStrings = null;
        }
        _fontShader?.Shutdown(_openGL);
        _fontShader = null;
        _font?.Shutdown(_openGL);
        _font = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame(int mouseX, int mouseY, bool mouseDown)
    {
        if (!UpdateMouseStrings(mouseX, mouseY, mouseDown))
            return false;
        return Render();
    }

    private bool UpdateMouseStrings(int mouseX, int mouseY, bool mouseDown)
    {
        _mouseStrings[0]
            .UpdateText(
                _openGL,
                _font,
                $"Mouse X: {mouseX}",
                10,
                10,
                1,
                1,
                1,
                _screenWidth,
                _screenHeight
            );
        _mouseStrings[1]
            .UpdateText(
                _openGL,
                _font,
                $"Mouse Y: {mouseY}",
                10,
                35,
                1,
                1,
                1,
                _screenWidth,
                _screenHeight
            );
        _mouseStrings[2]
            .UpdateText(
                _openGL,
                _font,
                mouseDown ? "Mouse Button: Yes" : "Mouse Button: No",
                10,
                60,
                1,
                1,
                1,
                _screenWidth,
                _screenHeight
            );
        return true;
    }

    private bool Render()
    {
        _openGL.BeginScene(0, 0, 0, 1);

        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var ortho = _openGL.GetOrthoMatrix();

        _openGL.TurnZBufferOff();

        _openGL.Gl.Enable(EnableCap.Blend);
        _openGL.Gl.BlendFuncSeparate(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha,
            BlendingFactor.One,
            BlendingFactor.Zero
        );

        _fontShader.SetShader(_openGL);
        _font.SetTexture(_openGL, TEXTURE_UNIT);

        var color = _mouseStrings[0].GetPixelColor();
        if (!_fontShader.SetShaderParameters(_openGL, world, view, ortho, (int)TEXTURE_UNIT, color))
            return false;

        _mouseStrings[0].Render(_openGL);
        _mouseStrings[1].Render(_openGL);
        _mouseStrings[2].Render(_openGL);

        _openGL.Gl.Disable(EnableCap.Blend);
        _openGL.TurnZBufferOn();
        _openGL.EndScene();
        return true;
    }
}

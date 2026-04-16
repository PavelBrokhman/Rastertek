using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial14.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 _openGL;
    private Camera _camera;
    private Font _font;
    private FontShader _fontShader;
    private Text _text1;
    private Text _text2;
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

        _text1 = new Text();
        if (
            !_text1.Initialize(
                OpenGL,
                _font,
                "Hello",
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

        _text2 = new Text();
        if (
            !_text2.Initialize(
                OpenGL,
                _font,
                "Goodbye",
                10,
                50,
                1,
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
        _text2?.Shutdown(_openGL);
        _text2 = null;
        _text1?.Shutdown(_openGL);
        _text1 = null;
        _fontShader?.Shutdown(_openGL);
        _fontShader = null;
        _font?.Shutdown(_openGL);
        _font = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame() => Render();

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

        if (
            !_fontShader.SetShaderParameters(
                _openGL,
                world,
                view,
                ortho,
                (int)TEXTURE_UNIT,
                _text1.GetPixelColor()
            )
        )
            return false;
        _text1.Render(_openGL);

        if (
            !_fontShader.SetShaderParameters(
                _openGL,
                world,
                view,
                ortho,
                (int)TEXTURE_UNIT,
                _text2.GetPixelColor()
            )
        )
            return false;
        _text2.Render(_openGL);

        _openGL.Gl.Disable(EnableCap.Blend);
        _openGL.TurnZBufferOn();
        _openGL.EndScene();
        return true;
    }
}

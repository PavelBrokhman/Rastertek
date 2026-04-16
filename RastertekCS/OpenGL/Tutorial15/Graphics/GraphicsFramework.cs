using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial15.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 _openGL;
    private Camera _camera;
    private Font _font;
    private FontShader _fontShader;
    private Text _fpsText;
    private int _screenWidth,
        _screenHeight;
    private int _previousFps = -1;

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

        _fpsText = new Text();
        if (
            !_fpsText.Initialize(
                OpenGL,
                _font,
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
        _fpsText?.Shutdown(_openGL);
        _fpsText = null;
        _fontShader?.Shutdown(_openGL);
        _fontShader = null;
        _font?.Shutdown(_openGL);
        _font = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame(int fps)
    {
        if (fps != _previousFps)
        {
            _previousFps = fps;
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
            _fpsText.UpdateText(
                _openGL,
                _font,
                $"FPS: {fps}",
                10,
                10,
                r,
                g,
                b,
                _screenWidth,
                _screenHeight
            );
        }
        return Render();
    }

    private bool Render()
    {
        _openGL.BeginScene(0, 0, 0, 1);

        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var ortho = _openGL.GetOrthoMatrix();

        _openGL.TurnZBufferOff();

        _openGL.Driver.Enable(EnableCap.Blend);
        _openGL.Driver.BlendFuncSeparate(
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
                _fpsText.GetPixelColor()
            )
        )
            return false;
        _fpsText.Render(_openGL);

        _openGL.Driver.Disable(EnableCap.Blend);
        _openGL.TurnZBufferOn();
        _openGL.EndScene();
        return true;
    }
}

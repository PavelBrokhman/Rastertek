using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial14.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private FontShader _fontShader;
    private Font _font;
    private Text _textString1;
    private Text _textString2;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        _fontShader = new FontShader();
        if (!_fontShader.Initialize(DirectX))
            return false;

        _font = new Font();
        if (!_font.Initialize(DirectX, 0))
            return false;

        _textString1 = new Text();
        if (
            !_textString1.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                32,
                _font,
                "Hello",
                10,
                10,
                0.0f,
                1.0f,
                0.0f
            )
        )
            return false;

        _textString2 = new Text();
        if (
            !_textString2.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                32,
                _font,
                "Goodbye",
                10,
                50,
                1.0f,
                1.0f,
                0.0f
            )
        )
            return false;

        return true;
    }

    public void Shutdown()
    {
        _textString2?.Shutdown();
        _textString1?.Shutdown();
        _font?.Shutdown();
        _fontShader?.Shutdown();
        _textString2 = null;
        _textString1 = null;
        _font = null;
        _fontShader = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var ortho = _directX.GetOrthoMatrix();

        _directX.TurnZBufferOff();
        _directX.EnableAlphaBlending();

        _font.SetTexture(_directX, 0);

        _textString1.Render(_directX);
        if (
            !_fontShader.Render(
                _directX,
                _textString1.GetIndexCount(),
                world,
                view,
                ortho,
                _textString1.GetPixelColor()
            )
        )
            return false;

        _textString2.Render(_directX);
        if (
            !_fontShader.Render(
                _directX,
                _textString2.GetIndexCount(),
                world,
                view,
                ortho,
                _textString2.GetPixelColor()
            )
        )
            return false;

        _directX.DisableAlphaBlending();
        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

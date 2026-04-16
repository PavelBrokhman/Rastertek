using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial16.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private FontShader _fontShader;
    private Font _font;
    private Text[] _mouseStrings;

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

        _mouseStrings = new Text[3];
        _mouseStrings[0] = new Text();
        if (
            !_mouseStrings[0]
                .Initialize(
                    DirectX,
                    screenWidth,
                    screenHeight,
                    32,
                    _font,
                    "Mouse X: 0",
                    10,
                    10,
                    1,
                    1,
                    1
                )
        )
            return false;
        _mouseStrings[1] = new Text();
        if (
            !_mouseStrings[1]
                .Initialize(
                    DirectX,
                    screenWidth,
                    screenHeight,
                    32,
                    _font,
                    "Mouse Y: 0",
                    10,
                    35,
                    1,
                    1,
                    1
                )
        )
            return false;
        _mouseStrings[2] = new Text();
        if (
            !_mouseStrings[2]
                .Initialize(
                    DirectX,
                    screenWidth,
                    screenHeight,
                    32,
                    _font,
                    "Mouse Button: No",
                    10,
                    60,
                    1,
                    1,
                    1
                )
        )
            return false;

        return true;
    }

    public void Shutdown()
    {
        if (_mouseStrings != null)
            foreach (var t in _mouseStrings)
                t?.Shutdown();
        _font?.Shutdown();
        _fontShader?.Shutdown();
        _mouseStrings = null;
        _font = null;
        _fontShader = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame(int mouseX, int mouseY, bool mouseDown)
    {
        if (!_mouseStrings[0].UpdateText(_directX, _font, $"Mouse X: {mouseX}", 10, 10, 1, 1, 1))
            return false;
        if (!_mouseStrings[1].UpdateText(_directX, _font, $"Mouse Y: {mouseY}", 10, 35, 1, 1, 1))
            return false;
        if (
            !_mouseStrings[2]
                .UpdateText(
                    _directX,
                    _font,
                    mouseDown ? "Mouse Button: Yes" : "Mouse Button: No",
                    10,
                    60,
                    1,
                    1,
                    1
                )
        )
            return false;
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var ortho = _directX.GetOrthoMatrix();

        _directX.TurnZBufferOff();
        _directX.EnableAlphaBlending();

        _font.SetTexture(_directX, 0);

        for (int i = 0; i < 3; i++)
        {
            _mouseStrings[i].Render(_directX);
            if (
                !_fontShader.Render(
                    _directX,
                    _mouseStrings[i].GetIndexCount(),
                    world,
                    view,
                    ortho,
                    _mouseStrings[i].GetPixelColor()
                )
            )
                return false;
        }

        _directX.DisableAlphaBlending();
        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

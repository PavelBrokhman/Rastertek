using RastertekCS.Windows.Tutorial15.System;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial15.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private FontShader _fontShader;
    private Font _font;
    private Text _fpsString;
    private Fps _fps;
    private int _previousFps;

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

        _fps = new Fps();
        _fps.Initialize();
        _previousFps = -1;

        _fpsString = new Text();
        if (
            !_fpsString.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                32,
                _font,
                "Fps: 0",
                10,
                10,
                0.0f,
                1.0f,
                0.0f
            )
        )
            return false;

        return true;
    }

    public void Shutdown()
    {
        _fpsString?.Shutdown();
        _font?.Shutdown();
        _fontShader?.Shutdown();
        _fpsString = null;
        _font = null;
        _fontShader = null;
        _fps = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        if (!UpdateFps())
            return false;
        return Render();
    }

    private bool UpdateFps()
    {
        _fps.Frame();
        int fps = _fps.GetFps();
        if (_previousFps == fps)
            return true;
        _previousFps = fps;

        if (fps > 99999)
            fps = 99999;

        float r,
            g,
            b;
        if (fps >= 60)
        {
            r = 0.0f;
            g = 1.0f;
            b = 0.0f;
        }
        else if (fps >= 30)
        {
            r = 1.0f;
            g = 1.0f;
            b = 0.0f;
        }
        else
        {
            r = 1.0f;
            g = 0.0f;
            b = 0.0f;
        }

        return _fpsString.UpdateText(_directX, _font, $"Fps: {fps}", 10, 10, r, g, b);
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

        _fpsString.Render(_directX);
        if (
            !_fontShader.Render(
                _directX,
                _fpsString.GetIndexCount(),
                world,
                view,
                ortho,
                _fpsString.GetPixelColor()
            )
        )
            return false;

        _directX.DisableAlphaBlending();
        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

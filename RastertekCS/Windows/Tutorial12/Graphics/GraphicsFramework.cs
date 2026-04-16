using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial12.Graphics;

public class GraphicsFramework
{
    private const int BITMAP_SIZE = 256;

    private DX11 _directX;
    private Camera _camera;
    private TextureShader _textureShader;
    private Bitmap _bitmap;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;

        _bitmap = new Bitmap();
        if (
            !_bitmap.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                "Data/Stone01.tga",
                BITMAP_SIZE,
                BITMAP_SIZE
            )
        )
            return false;
        _bitmap.SetRenderLocation(50, 50);

        return true;
    }

    public void Shutdown()
    {
        _bitmap?.Shutdown();
        _textureShader?.Shutdown();
        _bitmap = null;
        _textureShader = null;
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

        if (!_bitmap.Render(_directX))
            return false;
        _bitmap.SetTexture(_directX, 0);

        if (!_textureShader.Render(_directX, _bitmap.GetIndexCount(), world, view, ortho))
            return false;

        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

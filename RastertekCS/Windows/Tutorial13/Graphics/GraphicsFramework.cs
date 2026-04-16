using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial13.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private TextureShader _textureShader;
    private Sprite _sprite;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;

        _sprite = new Sprite();
        if (!_sprite.Initialize(DirectX, screenWidth, screenHeight, "Data/Sprite.txt", 50, 50))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _sprite?.Shutdown();
        _textureShader?.Shutdown();
        _sprite = null;
        _textureShader = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame(float frameTimeMs)
    {
        _sprite.Update(frameTimeMs);
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var ortho = _directX.GetOrthoMatrix();

        _directX.TurnZBufferOff();

        if (!_sprite.Render(_directX))
            return false;
        _sprite.SetTexture(_directX, 0);

        if (!_textureShader.Render(_directX, _sprite.GetIndexCount(), world, view, ortho))
            return false;

        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial28.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private TranslateShader _translateShader;
    private float _textureTranslation;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/square.txt", "Data/stone01.tga", true))
            return false;

        _translateShader = new TranslateShader();
        if (!_translateShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _translateShader?.Shutdown();
        _model?.Shutdown();
        _translateShader = null;
        _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _textureTranslation += 0.01f;
        if (_textureTranslation > 1.0f)
            _textureTranslation -= 1.0f;
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        _model.SetTexture(_directX, 0);

        if (
            !_translateShader.Render(
                _directX,
                _model.GetIndexCount(),
                world,
                view,
                projection,
                _textureTranslation
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial22.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private Light _light;
    private TextureShader _textureShader;
    private LightShader _lightShader;
    private NormalMapShader _normalMapShader;
    private float _rotation = 360.0f;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -8.0f);
        _camera.Render();

        _model = new Model();
        if (
            !_model.Initialize(
                DirectX,
                "Models/sphere.txt",
                "Data/stone01.tga",
                "Data/normal01.tga",
                true
            )
        )
            return false;

        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(DirectX))
            return false;

        _normalMapShader = new NormalMapShader();
        if (!_normalMapShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _normalMapShader?.Shutdown();
        _lightShader?.Shutdown();
        _textureShader?.Shutdown();
        _model?.Shutdown();
        _normalMapShader = null;
        _lightShader = null;
        _textureShader = null;
        _model = null;
        _light = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 0.25f;
        if (_rotation <= 0.0f)
            _rotation += 360.0f;
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();
        var rotate = Matrix4X4.CreateRotationY(_rotation);

        var world1 = rotate * Matrix4X4.CreateTranslation(0.0f, 1.0f, 0.0f);
        _model.Render(_directX);
        _model.SetTextures(_directX);
        if (!_textureShader.Render(_directX, _model.GetIndexCount(), world1, view, projection))
            return false;

        var world2 = rotate * Matrix4X4.CreateTranslation(-1.5f, -1.0f, 0.0f);
        _model.Render(_directX);
        _model.SetTextures(_directX);
        if (
            !_lightShader.Render(
                _directX,
                _model.GetIndexCount(),
                world2,
                view,
                projection,
                _light.GetDirection(),
                _light.GetDiffuseColor()
            )
        )
            return false;

        var world3 = rotate * Matrix4X4.CreateTranslation(1.5f, -1.0f, 0.0f);
        _model.Render(_directX);
        _model.SetTextures(_directX);
        if (
            !_normalMapShader.Render(
                _directX,
                _model.GetIndexCount(),
                world3,
                view,
                projection,
                _light.GetDirection(),
                _light.GetDiffuseColor()
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

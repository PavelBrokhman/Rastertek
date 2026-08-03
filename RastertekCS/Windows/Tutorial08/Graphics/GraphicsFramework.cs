using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial08.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private LightShader _lightShader;
    private Light _light;
    private float _rotation;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/Cube.txt", "Data/Stone01.tga", true))
            return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(DirectX))
            return false;

        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);

        return true;
    }

    public void Shutdown()
    {
        _lightShader?.Shutdown();
        _model?.Shutdown();
        _lightShader = null;
        _model = null;
        _camera = null;
        _light = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 0.25f;
        if (_rotation < 0.0f)
            _rotation += 360.0f;
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        _camera.Render();

        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        // Первый куб: rotation + translation (слева).
        var rotate1 = Matrix4X4.CreateRotationY(_rotation);
        var translate1 = Matrix4X4.CreateTranslation(-2.0f, 0.0f, 0.0f);
        var world1 = rotate1 * translate1;

        _model.Render(_directX);
        _model.SetTexture(_directX, 0);

        if (
            !_lightShader.Render(
                _directX,
                _model.GetIndexCount(),
                world1,
                view,
                projection,
                _light.GetDirection(),
                _light.GetDiffuseColor()
            )
        )
            return false;

        // Второй куб: scale + rotation + translation (справа).
        var scale2 = Matrix4X4.CreateScale(0.5f);
        var rotate2 = Matrix4X4.CreateRotationY(_rotation);
        var translate2 = Matrix4X4.CreateTranslation(2.0f, 0.0f, 0.0f);
        var world2 = scale2 * rotate2 * translate2;

        _model.Render(_directX);
        _model.SetTexture(_directX, 0);

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

        _directX.EndScene();
        return true;
    }
}

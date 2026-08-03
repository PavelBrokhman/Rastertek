using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial21.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private SpecMapShader _specMapShader;
    private Light _light;
    private float _rotation = 360.0f;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (
            !_model.Initialize(
                DirectX,
                "Models/Cube.txt",
                "Data/stone02.tga",
                "Data/normal02.tga",
                "Data/spec02.tga",
                true
            )
        )
            return false;

        _specMapShader = new SpecMapShader();
        if (!_specMapShader.Initialize(DirectX))
            return false;

        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);
        _light.SetSpecularColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetSpecularPower(16.0f);

        return true;
    }

    public void Shutdown()
    {
        _specMapShader?.Shutdown();
        _model?.Shutdown();
        _specMapShader = null;
        _model = null;
        _camera = null;
        _light = null;
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

        var world = Matrix4X4.CreateRotationY(_rotation);
        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        _model.SetTextures(_directX);

        if (
            !_specMapShader.Render(
                _directX,
                _model.GetIndexCount(),
                world,
                view,
                projection,
                _light.GetDirection(),
                _light.GetDiffuseColor(),
                _camera.GetPosition(),
                _light.GetSpecularColor(),
                _light.GetSpecularPower()
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

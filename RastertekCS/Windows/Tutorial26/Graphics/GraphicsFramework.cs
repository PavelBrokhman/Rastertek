using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial26.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private FogShader _fogShader;
    private float _rotation = 0.0f;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true))
            return false;

        _fogShader = new FogShader();
        if (!_fogShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _fogShader?.Shutdown();
        _model?.Shutdown();
        _fogShader = null;
        _model = null;
        _camera = null;
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
        _directX.BeginScene(0.5f, 0.5f, 0.5f, 1.0f);

        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY(_rotation);

        _model.Render(_directX);
        _model.SetTexture(_directX, 0);

        if (
            !_fogShader.Render(
                _directX,
                _model.GetIndexCount(),
                world,
                view,
                projection,
                0.0f,
                10.0f
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

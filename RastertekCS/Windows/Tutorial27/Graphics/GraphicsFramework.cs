using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial27.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private ClipPlaneShader _clipPlaneShader;
    private readonly float[] _clipPlane = new float[] { 0.0f, -1.0f, 0.0f, 0.0f };
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

        _clipPlaneShader = new ClipPlaneShader();
        if (!_clipPlaneShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _clipPlaneShader?.Shutdown();
        _model?.Shutdown();
        _clipPlaneShader = null;
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
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = Matrix4X4.CreateRotationY(_rotation);
        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        _model.SetTexture(_directX, 0);

        if (
            !_clipPlaneShader.Render(
                _directX,
                _model.GetIndexCount(),
                world,
                view,
                projection,
                _clipPlane
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

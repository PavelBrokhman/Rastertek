using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial52.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Light _light;
    private Model _model;
    private PbrShader _pbrShader;
    private float _rotation;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _light = new Light();
        _light.SetDirection(0.5f, 0.5f, 0.5f);

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/sphere.txt", "Data/pbr_albedo.tga", "Data/pbr_normal.tga", "Data/pbr_roughmetal.tga")) return false;

        _pbrShader = new PbrShader();
        if (!_pbrShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        _pbrShader?.Shutdown(); _pbrShader = null;
        _model?.Shutdown(); _model = null;
        _light = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 0.1f;
        if (_rotation < 0.0f) _rotation += 360.0f;

        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY<float>(_rotation);
        var cameraPos = _camera.GetPosition();
        var lightDir = _light.GetDirection();

        _model.Render(_directX);
        if (!_pbrShader.Render(_directX, _model.GetIndexCount(), world, view, projection,
            _model.GetTextureView1(), _model.GetTextureView2(), _model.GetTextureView3(),
            cameraPos, lightDir)) return false;

        _directX.EndScene();
        return true;
    }
}

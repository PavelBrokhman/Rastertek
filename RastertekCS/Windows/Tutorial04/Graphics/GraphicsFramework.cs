namespace RastertekCS.Windows.Tutorial04.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private ColorShader _colorShader;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);

        _model = new Model();
        if (!_model.Initialize(DirectX))
            return false;

        _colorShader = new ColorShader();
        if (!_colorShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _colorShader?.Shutdown();
        _model?.Shutdown();
        _colorShader = null;
        _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        _camera.Render();

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        _model.Render(_directX);

        if (!_colorShader.Render(_directX, _model.GetIndexCount(), world, view, projection))
            return false;

        _directX.EndScene();
        return true;
    }
}

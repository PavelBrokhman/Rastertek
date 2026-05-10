namespace RastertekCS.Windows.Tutorial49.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private ColorShader _colorShader;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX)) return false;

        _colorShader = new ColorShader();
        if (!_colorShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        _colorShader?.Shutdown(); _colorShader = null;
        _model?.Shutdown(); _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);
        var worldMatrix = _directX.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        if (!_colorShader.Render(_directX, _model.GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, 12.0f)) return false;

        _directX.EndScene();
        return true;
    }
}

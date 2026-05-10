namespace RastertekCS.Windows.Tutorial35.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private DepthShader _depthShader;
    private Model _model;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 2.0f, -10.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/floor.txt"))
            return false;

        _depthShader = new DepthShader();
        if (!_depthShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _depthShader?.Shutdown();
        _model?.Shutdown();
        _depthShader = null;
        _model = null;
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
        if (
            !_depthShader.Render(
                _directX,
                _model.GetIndexCount(),
                worldMatrix,
                viewMatrix,
                projectionMatrix
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

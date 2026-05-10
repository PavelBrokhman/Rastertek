using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial39.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _groundModel,
        _cubeModel;
    private ProjectionShader _projectionShader;
    private Texture _projectionTexture;
    private ViewPoint _viewPoint;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 7.0f, -10.0f);
        _camera.SetRotation(35.0f, 0.0f, 0.0f);
        _camera.Render();

        _groundModel = new Model();
        if (!_groundModel.Initialize(DirectX, "Models/plane01.txt", "Data/metal001.tga", true))
            return false;

        _cubeModel = new Model();
        if (!_cubeModel.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true))
            return false;

        _projectionShader = new ProjectionShader();
        if (!_projectionShader.Initialize(DirectX))
            return false;

        _projectionTexture = new Texture();
        if (!_projectionTexture.Initialize(DirectX, "Data/opengl_logo.tga", false))
            return false;

        _viewPoint = new ViewPoint();
        _viewPoint.SetPosition(2.0f, 5.0f, -2.0f);
        _viewPoint.SetLookAt(0.0f, 0.0f, 0.0f);
        _viewPoint.SetProjectionParameters(MathF.PI / 2.0f, 1.0f, 0.1f, 100.0f);
        _viewPoint.GenerateViewMatrix();
        _viewPoint.GenerateProjectionMatrix();
        return true;
    }

    public void Shutdown()
    {
        _projectionTexture?.Shutdown();
        _projectionShader?.Shutdown();
        _cubeModel?.Shutdown();
        _groundModel?.Shutdown();
        _projectionTexture = null;
        _projectionShader = null;
        _cubeModel = null;
        _groundModel = null;
        _viewPoint = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _directX.BeginScene(0, 0, 0, 1);
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();
        var viewMatrix2 = _viewPoint.GetViewMatrix();
        var projectionMatrix2 = _viewPoint.GetProjectionMatrix();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(0, 1, 0);
        _groundModel.Render(_directX);
        if (!_projectionShader.Render(
                _directX,
                _groundModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix,
                viewMatrix2, projectionMatrix2,
                _groundModel.GetTextureView(),
                _projectionTexture.GetTextureView()))
            return false;

        worldMatrix = Matrix4X4.CreateTranslation<float>(0, 2, 0);
        _cubeModel.Render(_directX);
        if (!_projectionShader.Render(
                _directX,
                _cubeModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix,
                viewMatrix2, projectionMatrix2,
                _cubeModel.GetTextureView(),
                _projectionTexture.GetTextureView()))
            return false;

        _directX.EndScene();
        return true;
    }
}

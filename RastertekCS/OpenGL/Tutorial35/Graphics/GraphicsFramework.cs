namespace RastertekCS.OpenGL.Tutorial35.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private DepthShader _depthShader;
    private Model _model;

    public bool Initialize(GL4 gl, int screenWidth, int screenHeight)
    {
        _driver = gl;
        _camera = new Camera();
        _camera.SetPosition(0, 2, -10);
        _camera.Render();
        _depthShader = new DepthShader();
        if (!_depthShader.Initialize(gl))
            return false;
        _model = new Model();
        if (!_model.Initialize(gl, "Models/floor.txt"))
            return false;
        return true;
    }

    public void Shutdown()
    {
        _depthShader?.Shutdown(_driver);
        _model?.Shutdown(_driver);
        _driver = null;
    }

    public bool Frame() => Render();

    bool Render()
    {
        _driver.BeginScene(0, 0, 0, 1);
        var worldMatrix = _driver.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        _depthShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix);
        _model.Render(_driver);
        _driver.EndScene();
        return true;
    }
}

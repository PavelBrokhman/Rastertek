namespace RastertekCS.OpenGL.Tutorial49.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private Model _model;
    private ColorShader _colorShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(OpenGL)) return false;

        _colorShader = new ColorShader();
        if (!_colorShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        _colorShader?.Shutdown(_driver); _colorShader = null;
        _model?.Shutdown(_driver); _model = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        // Subdivide the triangle into 12 sections.
        const float tessellationAmount = 12.0f;

        _driver.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);
        var worldMatrix = _driver.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        if (!_colorShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, tessellationAmount)) return false;
        _model.Render(_driver);
        _driver.EndScene();
        return true;
    }
}

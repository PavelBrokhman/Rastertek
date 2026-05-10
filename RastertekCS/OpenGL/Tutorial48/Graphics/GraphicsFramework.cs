namespace RastertekCS.OpenGL.Tutorial48.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Data/stone01.tga")) return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        _textureShader?.Shutdown(_driver); _textureShader = null;
        _model?.Shutdown(_driver); _model = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _driver.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);
        var worldMatrix = _driver.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        if (!_textureShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, 0)) return false;
        _model.SetTexture(_driver, 0);
        _model.Render(_driver);
        _driver.EndScene();
        return true;
    }
}

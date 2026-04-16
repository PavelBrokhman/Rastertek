namespace RastertekCS.OpenGL.Tutorial04.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private ColorShader _colorShader;

    public bool Initialize(GL4 OpenGL)
    {
        _openGL = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);

        _model = new Model();
        if (!_model.Initialize(OpenGL))
            return false;

        _colorShader = new ColorShader();
        if (!_colorShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _colorShader?.Shutdown(_openGL);
        _model?.Shutdown(_openGL);
        _colorShader = null;
        _model = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        _camera.Render();

        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        _colorShader.SetShader(_openGL);
        if (!_colorShader.SetShaderParameters(_openGL, world, view, projection))
            return false;

        _model.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

namespace RastertekCS.OpenGL.Tutorial05.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;

    public bool Initialize(GL4 OpenGL)
    {
        _openGL = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Data/Stone01.tga", TEXTURE_UNIT, true))
            return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _textureShader?.Shutdown(_openGL);
        _model?.Shutdown(_openGL);
        _textureShader = null;
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

        _textureShader.SetShader(_openGL);
        if (
            !_textureShader.SetShaderParameters(_openGL, world, view, projection, (int)TEXTURE_UNIT)
        )
            return false;

        _model.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

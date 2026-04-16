namespace RastertekCS.OpenGL.Tutorial28.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private TranslateShader _translateShader;
    private float _textureTranslation;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0, 0, -5);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/square.txt", "Data/stone01.tga", 0, true))
            return false;

        _translateShader = new TranslateShader();
        if (!_translateShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _translateShader?.Shutdown(_openGL);
        _translateShader = null;
        _model?.Shutdown(_openGL);
        _model = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame()
    {
        _textureTranslation += 0.01f;
        if (_textureTranslation > 1.0f)
            _textureTranslation -= 1.0f;

        return Render(_textureTranslation);
    }

    private bool Render(float textureTranslation)
    {
        _openGL.BeginScene(0, 0, 0, 1);

        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        _translateShader.SetShader(_openGL);
        _model.SetTexture(_openGL, 0);

        if (
            !_translateShader.SetShaderParameters(
                _openGL,
                world,
                view,
                projection,
                textureTranslation,
                0
            )
        )
            return false;

        _model.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

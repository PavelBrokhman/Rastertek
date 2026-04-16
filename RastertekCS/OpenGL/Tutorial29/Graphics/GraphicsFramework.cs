using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial29.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _model1;
    private Model _model2;
    private TextureShader _textureShader;
    private TransparentShader _transparentShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0, 0, -5);
        _camera.Render();

        // First model uses dirt texture.
        _model1 = new Model();
        if (!_model1.Initialize(OpenGL, "Models/square.txt", "Data/dirt01.tga", 0))
            return false;

        // Second model uses stone texture.
        _model2 = new Model();
        if (!_model2.Initialize(OpenGL, "Models/square.txt", "Data/stone01.tga", 0))
            return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;

        _transparentShader = new TransparentShader();
        if (!_transparentShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _transparentShader?.Shutdown(_openGL);
        _transparentShader = null;
        _textureShader?.Shutdown(_openGL);
        _textureShader = null;
        _model2?.Shutdown(_openGL);
        _model2 = null;
        _model1?.Shutdown(_openGL);
        _model1 = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        float blendAmount = 0.5f;

        _openGL.BeginScene(0, 0, 0, 1);

        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        // Render the first model with the regular texture shader.
        _textureShader.SetShader(_openGL);
        _model1.SetTexture(_openGL, 0);

        if (!_textureShader.SetShaderParameters(_openGL, world, view, projection, 0))
            return false;

        _model1.Render(_openGL);

        // Translate to the right by one unit and towards the camera by one unit.
        var world2 = Matrix4X4.CreateTranslation<float>(1.0f, 0.0f, -1.0f);

        // Turn on alpha blending for the transparency to work.
        _openGL.EnableAlphaBlending();

        // Render the second model with the transparent shader.
        _transparentShader.SetShader(_openGL);
        _model2.SetTexture(_openGL, 0);

        if (
            !_transparentShader.SetShaderParameters(
                _openGL,
                world2,
                view,
                projection,
                blendAmount,
                0
            )
        )
            return false;

        _model2.Render(_openGL);

        // Turn off alpha blending.
        _openGL.DisableAlphaBlending();

        _openGL.EndScene();
        return true;
    }
}

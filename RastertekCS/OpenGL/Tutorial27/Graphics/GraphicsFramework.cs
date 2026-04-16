using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial27.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private ClipPlaneShader _clipPlaneShader;
    private float _rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0, 0, -10);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/Cube.txt", "Data/stone01.tga", 0))
            return false;

        _clipPlaneShader = new ClipPlaneShader();
        if (!_clipPlaneShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _clipPlaneShader?.Shutdown(_openGL);
        _clipPlaneShader = null;
        _model?.Shutdown(_openGL);
        _model = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation < 0.0f)
            _rotation += MathF.Tau;

        return Render(_rotation);
    }

    private bool Render(float rotation)
    {
        _openGL.BeginScene(0, 0, 0, 1);

        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        // Enable clip planes.
        _openGL.EnableClipping();

        _clipPlaneShader.SetShader(_openGL);
        _model.SetTexture(_openGL, 0);

        // Clip plane: y = 0, clips everything below y=0.
        if (
            !_clipPlaneShader.SetShaderParameters(
                _openGL,
                world,
                view,
                projection,
                0.0f,
                -1.0f,
                0.0f,
                0.0f,
                0
            )
        )
            return false;

        _model.Render(_openGL);

        // Disable clip planes.
        _openGL.DisableClipping();

        _openGL.EndScene();
        return true;
    }
}

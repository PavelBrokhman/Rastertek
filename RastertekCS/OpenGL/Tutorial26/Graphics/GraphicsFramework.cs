using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial26.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private FogShader _fogShader;
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

        _fogShader = new FogShader();
        if (!_fogShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _fogShader?.Shutdown(_openGL);
        _fogShader = null;
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
        float fogColor = 0.5f;
        float fogStart = 0.0f;
        float fogEnd = 10.0f;

        _openGL.BeginScene(fogColor, fogColor, fogColor, 1.0f);

        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        _fogShader.SetShader(_openGL);
        _model.SetTexture(_openGL, 0);

        if (!_fogShader.SetShaderParameters(_openGL, world, view, projection, fogStart, fogEnd, 0))
            return false;

        _model.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

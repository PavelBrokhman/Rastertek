using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial07.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;
    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private LightShader _lightShader;
    private Light _light;
    private float _rotation;

    public bool Initialize(GL4 OpenGL)
    {
        _openGL = OpenGL;
        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/Cube.txt", "Data/Stone01.tga", TEXTURE_UNIT, true))
            return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(OpenGL))
            return false;

        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(1.0f, 0.0f, 1.0f);
        return true;
    }

    public void Shutdown()
    {
        _lightShader?.Shutdown(_openGL);
        _model?.Shutdown(_openGL);
        _lightShader = null;
        _model = null;
        _camera = null;
        _light = null;
        _openGL = null;
    }

    public bool Frame()
    {
        // Вращаем куб для наглядности освещения.
        _rotation += 0.01f;
        if (_rotation > MathF.Tau)
            _rotation -= MathF.Tau;
        return Render();
    }

    private bool Render()
    {
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        _camera.Render();
        var world = Matrix4X4.CreateRotationY(_rotation);
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        _lightShader.SetShader(_openGL);
        if (
            !_lightShader.SetShaderParameters(
                _openGL,
                world,
                view,
                projection,
                (int)TEXTURE_UNIT,
                _light.GetDirection(),
                _light.GetDiffuseColor()
            )
        )
            return false;

        _model.Render(_openGL);
        _openGL.EndScene();
        return true;
    }
}

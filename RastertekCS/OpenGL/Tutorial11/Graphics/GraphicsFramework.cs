using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial11.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;
    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private LightShader _lightShader;
    private Light[] _lights;

    public bool Initialize(GL4 OpenGL)
    {
        _openGL = OpenGL;
        _camera = new Camera();
        _camera.SetPosition(0.0f, 2.0f, -12.0f);
        _camera.SetRotation(15.0f, 0.0f, 0.0f);

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/Plane.txt", "Data/Stone01.tga", TEXTURE_UNIT, true))
            return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(OpenGL))
            return false;

        _lights = new Light[LightShader.NUM_LIGHTS];
        _lights[0] = new Light();
        _lights[0].SetDiffuseColor(1.0f, 0.0f, 0.0f, 1.0f);
        _lights[0].SetPosition(-3.0f, 1.0f, 3.0f);

        _lights[1] = new Light();
        _lights[1].SetDiffuseColor(0.0f, 1.0f, 0.0f, 1.0f);
        _lights[1].SetPosition(3.0f, 1.0f, 3.0f);

        _lights[2] = new Light();
        _lights[2].SetDiffuseColor(0.0f, 0.0f, 1.0f, 1.0f);
        _lights[2].SetPosition(-3.0f, 1.0f, -3.0f);

        _lights[3] = new Light();
        _lights[3].SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _lights[3].SetPosition(3.0f, 1.0f, -3.0f);

        return true;
    }

    public void Shutdown()
    {
        _lightShader?.Shutdown(_openGL);
        _model?.Shutdown(_openGL);
        _lightShader = null;
        _model = null;
        _camera = null;
        _lights = null;
        _openGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);
        _camera.Render();
        var world = Matrix4X4<float>.Identity;
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        // Упаковываем позиции и цвета в плоские массивы.
        var positions = new float[3 * LightShader.NUM_LIGHTS];
        var colors = new float[4 * LightShader.NUM_LIGHTS];
        for (int i = 0; i < LightShader.NUM_LIGHTS; i++)
        {
            var p = _lights[i].GetPosition();
            var c = _lights[i].GetDiffuseColor();
            positions[i * 3 + 0] = p[0];
            positions[i * 3 + 1] = p[1];
            positions[i * 3 + 2] = p[2];
            colors[i * 4 + 0] = c[0];
            colors[i * 4 + 1] = c[1];
            colors[i * 4 + 2] = c[2];
            colors[i * 4 + 3] = c[3];
        }

        _lightShader.SetShader(_openGL);
        if (
            !_lightShader.SetShaderParameters(
                _openGL,
                world,
                view,
                projection,
                (int)TEXTURE_UNIT,
                positions,
                colors
            )
        )
            return false;

        _model.Render(_openGL);
        _openGL.EndScene();
        return true;
    }
}

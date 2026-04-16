using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial20.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT_COLOR = 0;
    private const uint TEXTURE_UNIT_NORMAL = 1;

    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private NormalMapShader _normalMapShader;
    private Light _light;
    private float _rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        // Create and initialize camera.
        _camera = new Camera();
        _camera.SetPosition(0, 0, -5);
        _camera.Render();

        // Create and initialize the normal map shader.
        _normalMapShader = new NormalMapShader();
        if (!_normalMapShader.Initialize(OpenGL))
            return false;

        // Create and initialize the model with color texture and normal map.
        _model = new Model();
        if (
            !_model.Initialize(
                OpenGL,
                "Models/Cube.txt",
                "Data/stone01.tga",
                TEXTURE_UNIT_COLOR,
                "Data/normal01.tga",
                TEXTURE_UNIT_NORMAL
            )
        )
            return false;

        // Create and initialize the light.
        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);

        return true;
    }

    public void Shutdown()
    {
        _light = null;
        _normalMapShader?.Shutdown(_openGL);
        _normalMapShader = null;
        _model?.Shutdown(_openGL);
        _model = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame()
    {
        // Update rotation each frame.
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation < 0.0f)
        {
            _rotation += MathF.Tau;
        }

        return Render(_rotation);
    }

    private bool Render(float rotation)
    {
        _openGL.BeginScene(0, 0, 0, 1);

        var world = Silk.NET.Maths.Matrix4X4.CreateRotationY<float>(rotation);
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        // Get light properties.
        float[] lightDirection = _light.GetDirection();
        float[] diffuseColor = _light.GetDiffuseColor();

        // Set shader and parameters.
        _normalMapShader.SetShader(_openGL);

        // Set textures.
        _model.SetTextures(_openGL, TEXTURE_UNIT_COLOR, TEXTURE_UNIT_NORMAL);

        if (
            !_normalMapShader.SetShaderParameters(
                _openGL,
                world,
                view,
                projection,
                lightDirection,
                diffuseColor,
                (int)TEXTURE_UNIT_COLOR,
                (int)TEXTURE_UNIT_NORMAL
            )
        )
            return false;

        // Render model.
        _model.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

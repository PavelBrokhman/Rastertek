namespace RastertekCS.OpenGL.Tutorial21.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT_COLOR = 0;
    private const uint TEXTURE_UNIT_NORMAL = 1;
    private const uint TEXTURE_UNIT_SPECULAR = 2;

    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private SpecMapShader _specMapShader;
    private Light _light;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _specMapShader = new SpecMapShader();
        if (!_specMapShader.Initialize(OpenGL))
            return false;

        _model = new Model();
        if (
            !_model.Initialize(
                OpenGL,
                "Models/Cube.txt",
                "Data/stone02.tga",
                true,
                "Data/normal02.tga",
                true,
                "Data/spec02.tga",
                false
            )
        )
            return false;

        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);
        _light.SetSpecularColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetSpecularPower(16.0f);

        return true;
    }

    public void Shutdown()
    {
        _light = null;
        _model?.Shutdown(_openGL);
        _model = null;
        _specMapShader?.Shutdown(_openGL);
        _specMapShader = null;
        _camera = null;
        _openGL = null;
    }

    private float _rotation = 360.0f;

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation < 0.0f)
            _rotation += MathF.Tau;
        return Render(_rotation);
    }

    private bool Render(float rotation)
    {
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = Silk.NET.Maths.Matrix4X4.CreateRotationY<float>(rotation);
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        var lightDirection = _light.GetDirection();
        var diffuseLightColor = _light.GetDiffuseColor();
        var specularColor = _light.GetSpecularColor();
        float specularPower = _light.GetSpecularPower();
        var cameraPosition = _camera.GetPosition();

        _specMapShader.SetShader(_openGL);
        _model.SetTextures(_openGL, TEXTURE_UNIT_COLOR, TEXTURE_UNIT_NORMAL, TEXTURE_UNIT_SPECULAR);

        if (
            !_specMapShader.SetShaderParameters(
                _openGL,
                world,
                view,
                projection,
                lightDirection,
                diffuseLightColor,
                cameraPosition,
                specularColor,
                specularPower,
                (int)TEXTURE_UNIT_COLOR,
                (int)TEXTURE_UNIT_NORMAL,
                (int)TEXTURE_UNIT_SPECULAR
            )
        )
            return false;

        _model.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

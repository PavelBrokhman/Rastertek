namespace RastertekCS.OpenGL.Tutorial18.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT_1 = 0;
    private const uint TEXTURE_UNIT_2 = 1;

    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private LightMapShader _lightMapShader;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0, 0, -5);
        _camera.Render();

        _model = new Model();
        if (
            !_model.Initialize(
                OpenGL,
                "Models/square.txt",
                "Data/stone01.tga",
                TEXTURE_UNIT_1,
                "Data/light01.tga",
                TEXTURE_UNIT_2
            )
        )
            return false;

        _lightMapShader = new LightMapShader();
        if (!_lightMapShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _lightMapShader?.Shutdown(_openGL);
        _lightMapShader = null;
        _model?.Shutdown(_openGL);
        _model = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _openGL.BeginScene(0, 0, 0, 1);

        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        _lightMapShader.SetShader(_openGL);
        _model.SetTextures(_openGL, TEXTURE_UNIT_1, TEXTURE_UNIT_2);

        if (
            !_lightMapShader.SetShaderParameters(
                _openGL,
                world,
                view,
                projection,
                (int)TEXTURE_UNIT_1,
                (int)TEXTURE_UNIT_2
            )
        )
            return false;

        _model.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

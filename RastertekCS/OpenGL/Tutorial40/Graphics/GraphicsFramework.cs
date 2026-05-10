using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial40.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private Model _groundModel,
        _cubeModel;
    private ProjectionShader _projectionShader;
    private Texture _projectionTexture;
    private ViewPoint _viewPoint;
    private Light _light;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;
        _camera = new Camera();
        _camera.SetPosition(0.0f, 7.0f, -10.0f);
        _camera.SetRotation(35.0f, 0.0f, 0.0f);
        _camera.Render();

        _groundModel = new Model();
        if (!_groundModel.Initialize(OpenGL, "Models/plane01.txt", "Data/metal001.tga"))
            return false;

        _cubeModel = new Model();
        if (!_cubeModel.Initialize(OpenGL, "Models/Cube.txt", "Data/stone01.tga"))
            return false;

        _projectionShader = new ProjectionShader();
        if (!_projectionShader.Initialize(OpenGL))
            return false;

        _projectionTexture = new Texture();
        if (!_projectionTexture.Initialize(OpenGL, "Data/grate.tga", 1, false))
            return false;

        _viewPoint = new ViewPoint();
        _viewPoint.SetPosition(2.0f, 5.0f, -2.0f);
        _viewPoint.SetLookAt(0.0f, 0.0f, 0.0f);
        _viewPoint.SetProjectionParameters(MathF.PI / 2.0f, 1.0f, 0.1f, 100.0f);
        _viewPoint.GenerateViewMatrix();
        _viewPoint.GenerateProjectionMatrix();

        _light = new Light();
        _light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetPosition(2.0f, 5.0f, -2.0f);
        return true;
    }

    public void Shutdown()
    {
        _projectionTexture?.Shutdown(_driver);
        _projectionShader?.Shutdown(_driver);
        _cubeModel?.Shutdown(_driver);
        _groundModel?.Shutdown(_driver);
        _projectionTexture = null;
        _projectionShader = null;
        _cubeModel = null;
        _groundModel = null;
        _viewPoint = null;
        _light = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _driver.BeginScene(0, 0, 0, 1);
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        var viewMatrix2 = _viewPoint.GetViewMatrix();
        var projectionMatrix2 = _viewPoint.GetProjectionMatrix();
        var diffuseColor = _light.GetDiffuseColor();
        var ambientColor = _light.GetAmbientLight();
        var lightPosition = _light.GetPosition();
        const float brightness = 1.5f;

        // Ground at y=1.
        var worldMatrix = Matrix4X4.CreateTranslation<float>(0, 1, 0);
        if (!_projectionShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, viewMatrix2, projectionMatrix2,
                                                   diffuseColor, ambientColor, lightPosition, brightness))
            return false;
        _groundModel.SetTexture(_driver, 0);
        _projectionTexture.SetTexture(_driver, 1);
        _groundModel.Render(_driver);

        // Cube at y=2.
        worldMatrix = Matrix4X4.CreateTranslation<float>(0, 2, 0);
        if (!_projectionShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, viewMatrix2, projectionMatrix2,
                                                   diffuseColor, ambientColor, lightPosition, brightness))
            return false;
        _cubeModel.SetTexture(_driver, 0);
        _projectionTexture.SetTexture(_driver, 1);
        _cubeModel.Render(_driver);

        _driver.EndScene();
        return true;
    }
}

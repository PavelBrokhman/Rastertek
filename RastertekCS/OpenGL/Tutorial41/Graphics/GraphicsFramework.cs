using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial41.Graphics;

public class GraphicsFramework
{
    private const int SHADOWMAP_WIDTH = 1024;
    private const int SHADOWMAP_HEIGHT = 1024;

    private GL4 _driver;
    private Camera _camera;
    private Model _cubeModel,
        _sphereModel,
        _groundModel;
    private Light _light;
    private RenderTexture _renderTexture;
    private DepthShader _depthShader;
    private ShadowShader _shadowShader;
    private float _shadowMapBias;
    private float _lightPositionX;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 7.0f, -10.0f);
        _camera.SetRotation(35.0f, 0.0f, 0.0f);
        _camera.Render();

        _cubeModel = new Model();
        if (!_cubeModel.Initialize(OpenGL, "Models/Cube.txt", "Data/wall01.tga"))
            return false;

        _sphereModel = new Model();
        if (!_sphereModel.Initialize(OpenGL, "Models/sphere.txt", "Data/ice.tga"))
            return false;

        _groundModel = new Model();
        if (!_groundModel.Initialize(OpenGL, "Models/plane01.txt", "Data/metal001.tga"))
            return false;

        _light = new Light();
        _light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetLookAt(0.0f, 0.0f, 0.0f);
        _light.GenerateProjectionMatrix(1000.0f, 0.3f);

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, 0.3f, 1000.0f))
            return false;

        _depthShader = new DepthShader();
        if (!_depthShader.Initialize(OpenGL))
            return false;

        _shadowShader = new ShadowShader();
        if (!_shadowShader.Initialize(OpenGL))
            return false;

        _shadowMapBias = 0.0022f;
        _lightPositionX = -5.0f;
        return true;
    }

    public void Shutdown()
    {
        _shadowShader?.Shutdown(_driver);
        _depthShader?.Shutdown(_driver);
        _renderTexture?.Shutdown(_driver);
        _groundModel?.Shutdown(_driver);
        _sphereModel?.Shutdown(_driver);
        _cubeModel?.Shutdown(_driver);
        _shadowShader = null;
        _depthShader = null;
        _renderTexture = null;
        _groundModel = null;
        _sphereModel = null;
        _cubeModel = null;
        _light = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame()
    {
        _lightPositionX += 0.05f;
        if (_lightPositionX > 5.0f) _lightPositionX = -5.0f;
        _light.SetPosition(_lightPositionX, 8.0f, -5.0f);
        _light.GenerateViewMatrix();

        if (!RenderDepthToTexture()) return false;
        return Render();
    }

    private bool RenderDepthToTexture()
    {
        _renderTexture.SetRenderTarget(_driver);
        _renderTexture.ClearRenderTarget(_driver, 0, 0, 0, 1);

        var lightViewMatrix = _light.GetViewMatrix();
        var lightProjectionMatrix = _light.GetProjectionMatrix();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-2.0f, 2.0f, 0.0f);
        if (!_depthShader.SetShaderParameters(_driver, worldMatrix, lightViewMatrix, lightProjectionMatrix)) return false;
        _cubeModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(2.0f, 2.0f, 0.0f);
        if (!_depthShader.SetShaderParameters(_driver, worldMatrix, lightViewMatrix, lightProjectionMatrix)) return false;
        _sphereModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        if (!_depthShader.SetShaderParameters(_driver, worldMatrix, lightViewMatrix, lightProjectionMatrix)) return false;
        _groundModel.Render(_driver);

        _driver.SetBackBufferRenderTarget();
        _driver.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _driver.BeginScene(0, 0, 0, 1);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        var lightViewMatrix = _light.GetViewMatrix();
        var lightProjectionMatrix = _light.GetProjectionMatrix();
        var diffuseColor = _light.GetDiffuseColor();
        var ambientColor = _light.GetAmbientLight();
        var lightPosition = _light.GetPosition();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-2.0f, 2.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, diffuseColor, ambientColor, lightPosition, _shadowMapBias))
            return false;
        _cubeModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _cubeModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(2.0f, 2.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, diffuseColor, ambientColor, lightPosition, _shadowMapBias))
            return false;
        _sphereModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _sphereModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, diffuseColor, ambientColor, lightPosition, _shadowMapBias))
            return false;
        _groundModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _groundModel.Render(_driver);

        _driver.EndScene();
        return true;
    }
}

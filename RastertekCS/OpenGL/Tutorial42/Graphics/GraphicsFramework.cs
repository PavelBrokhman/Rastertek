using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial42.Graphics;

public class GraphicsFramework
{
    private const int SHADOWMAP_WIDTH = 1024;
    private const int SHADOWMAP_HEIGHT = 1024;

    private GL4 _driver;
    private Camera _camera;
    private Model _cubeModel,
        _sphereModel,
        _groundModel;
    private Light _light,
        _light2;
    private RenderTexture _renderTexture,
        _renderTexture2;
    private DepthShader _depthShader;
    private ShadowShader _shadowShader;
    private float _shadowMapBias;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(-8.0f, 7.0f, 8.0f);
        _camera.SetRotation(35.0f, 135.0f, 0.0f);
        _camera.Render();

        _cubeModel = new Model();
        if (!_cubeModel.Initialize(OpenGL, "Models/Cube.txt", "Data/wall01.tga")) return false;

        _sphereModel = new Model();
        if (!_sphereModel.Initialize(OpenGL, "Models/sphere.txt", "Data/ice.tga")) return false;

        _groundModel = new Model();
        if (!_groundModel.Initialize(OpenGL, "Models/plane01.txt", "Data/metal001.tga")) return false;

        _light = new Light();
        _light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetLookAt(0.0f, 0.0f, 0.0f);
        _light.GenerateProjectionMatrix(1000.0f, 0.3f);

        _light2 = new Light();
        _light2.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light2.SetLookAt(0.0f, 0.0f, 0.0f);
        _light2.GenerateProjectionMatrix(1000.0f, 0.3f);

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, 0.3f, 1000.0f)) return false;

        _renderTexture2 = new RenderTexture();
        if (!_renderTexture2.Initialize(OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, 0.3f, 1000.0f)) return false;

        _depthShader = new DepthShader();
        if (!_depthShader.Initialize(OpenGL)) return false;

        _shadowShader = new ShadowShader();
        if (!_shadowShader.Initialize(OpenGL)) return false;

        _shadowMapBias = 0.0022f;
        return true;
    }

    public void Shutdown()
    {
        _shadowShader?.Shutdown(_driver);
        _depthShader?.Shutdown(_driver);
        _renderTexture2?.Shutdown(_driver);
        _renderTexture?.Shutdown(_driver);
        _groundModel?.Shutdown(_driver);
        _sphereModel?.Shutdown(_driver);
        _cubeModel?.Shutdown(_driver);
        _shadowShader = null;
        _depthShader = null;
        _renderTexture2 = null;
        _renderTexture = null;
        _groundModel = null;
        _sphereModel = null;
        _cubeModel = null;
        _light2 = null;
        _light = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame()
    {
        _light.SetPosition(5.0f, 8.0f, -5.0f);
        _light.GenerateViewMatrix();

        _light2.SetPosition(-5.0f, 8.0f, -5.0f);
        _light2.GenerateViewMatrix();

        if (!RenderDepthToTexture(_renderTexture, _light)) return false;
        if (!RenderDepthToTexture(_renderTexture2, _light2)) return false;
        return Render();
    }

    private bool RenderDepthToTexture(RenderTexture rt, Light light)
    {
        rt.SetRenderTarget(_driver);
        rt.ClearRenderTarget(_driver, 0, 0, 0, 1);

        var lightViewMatrix = light.GetViewMatrix();
        var lightProjectionMatrix = light.GetProjectionMatrix();

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
        var lightViewMatrix2 = _light2.GetViewMatrix();
        var lightProjectionMatrix2 = _light2.GetProjectionMatrix();
        var diffuseColor2 = _light2.GetDiffuseColor();
        var lightPosition2 = _light2.GetPosition();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-2.0f, 2.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, diffuseColor, ambientColor, lightPosition, _shadowMapBias,
                lightViewMatrix2, lightProjectionMatrix2, diffuseColor2, lightPosition2)) return false;
        _cubeModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _renderTexture2.SetTexture(_driver, 2);
        _cubeModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(2.0f, 2.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, diffuseColor, ambientColor, lightPosition, _shadowMapBias,
                lightViewMatrix2, lightProjectionMatrix2, diffuseColor2, lightPosition2)) return false;
        _sphereModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _renderTexture2.SetTexture(_driver, 2);
        _sphereModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, diffuseColor, ambientColor, lightPosition, _shadowMapBias,
                lightViewMatrix2, lightProjectionMatrix2, diffuseColor2, lightPosition2)) return false;
        _groundModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _renderTexture2.SetTexture(_driver, 2);
        _groundModel.Render(_driver);

        _driver.EndScene();
        return true;
    }
}

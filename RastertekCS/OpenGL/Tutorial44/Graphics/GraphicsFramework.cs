using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial44.Graphics;

public class GraphicsFramework
{
    private const int SHADOWMAP_WIDTH = 1024;
    private const int SHADOWMAP_HEIGHT = 1024;
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private GL4 _driver;
    private Camera _camera;
    private Model _cubeModel,
        _sphereModel,
        _groundModel;
    private Light _light;
    private RenderTexture _renderTexture,
        _blackWhiteRenderTexture;
    private DepthShader _depthShader;
    private ShadowShader _shadowShader;
    private SoftShadowShader _softShadowShader;
    private Blur _blur;
    private TextureShader _textureShader;
    private BlurShader _blurShader;
    private float _shadowMapBias;
    private float _lightPositionX;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();
        _camera.SetPosition(0.0f, 7.0f, -10.0f);
        _camera.SetRotation(35.0f, 0.0f, 0.0f);
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
        _light.GenerateProjectionMatrix(SCREEN_DEPTH, SCREEN_NEAR);

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SCREEN_NEAR, SCREEN_DEPTH)) return false;

        _blackWhiteRenderTexture = new RenderTexture();
        if (!_blackWhiteRenderTexture.Initialize(OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SCREEN_NEAR, SCREEN_DEPTH)) return false;

        _depthShader = new DepthShader();
        if (!_depthShader.Initialize(OpenGL)) return false;

        _shadowShader = new ShadowShader();
        if (!_shadowShader.Initialize(OpenGL)) return false;

        _softShadowShader = new SoftShadowShader();
        if (!_softShadowShader.Initialize(OpenGL)) return false;

        int downSampleWidth = SHADOWMAP_WIDTH / 2;
        int downSampleHeight = SHADOWMAP_HEIGHT / 2;
        _blur = new Blur();
        if (!_blur.Initialize(OpenGL, downSampleWidth, downSampleHeight, SCREEN_NEAR, SCREEN_DEPTH, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT)) return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL)) return false;

        _blurShader = new BlurShader();
        if (!_blurShader.Initialize(OpenGL)) return false;

        _shadowMapBias = 0.0022f;
        _lightPositionX = -5.0f;
        return true;
    }

    public void Shutdown()
    {
        _blurShader?.Shutdown(_driver); _blurShader = null;
        _textureShader?.Shutdown(_driver); _textureShader = null;
        _blur?.Shutdown(_driver); _blur = null;
        _softShadowShader?.Shutdown(_driver); _softShadowShader = null;
        _shadowShader?.Shutdown(_driver); _shadowShader = null;
        _depthShader?.Shutdown(_driver); _depthShader = null;
        _blackWhiteRenderTexture?.Shutdown(_driver); _blackWhiteRenderTexture = null;
        _renderTexture?.Shutdown(_driver); _renderTexture = null;
        _groundModel?.Shutdown(_driver); _groundModel = null;
        _sphereModel?.Shutdown(_driver); _sphereModel = null;
        _cubeModel?.Shutdown(_driver); _cubeModel = null;
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
        if (!RenderBlackAndWhiteShadows()) return false;
        if (!_blur.BlurTexture(_blackWhiteRenderTexture, _driver, _camera, _textureShader, _blurShader)) return false;
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

    private bool RenderBlackAndWhiteShadows()
    {
        _blackWhiteRenderTexture.SetRenderTarget(_driver);
        _blackWhiteRenderTexture.ClearRenderTarget(_driver, 0, 0, 0, 1);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        var lightViewMatrix = _light.GetViewMatrix();
        var lightProjectionMatrix = _light.GetProjectionMatrix();
        var lightPosition = _light.GetPosition();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-2.0f, 2.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, lightPosition, _shadowMapBias)) return false;
        _renderTexture.SetTexture(_driver, 0);
        _cubeModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(2.0f, 2.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, lightPosition, _shadowMapBias)) return false;
        _renderTexture.SetTexture(_driver, 0);
        _sphereModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, lightPosition, _shadowMapBias)) return false;
        _renderTexture.SetTexture(_driver, 0);
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
        var lightPosition = _light.GetPosition();
        var ambientColor = _light.GetAmbientLight();
        var diffuseColor = _light.GetDiffuseColor();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-2.0f, 2.0f, 0.0f);
        if (!_softShadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, lightPosition, ambientColor, diffuseColor)) return false;
        _cubeModel.SetTexture(_driver, 0);
        _blackWhiteRenderTexture.SetTexture(_driver, 1);
        _cubeModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(2.0f, 2.0f, 0.0f);
        if (!_softShadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, lightPosition, ambientColor, diffuseColor)) return false;
        _sphereModel.SetTexture(_driver, 0);
        _blackWhiteRenderTexture.SetTexture(_driver, 1);
        _sphereModel.Render(_driver);

        worldMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        if (!_softShadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, lightPosition, ambientColor, diffuseColor)) return false;
        _groundModel.SetTexture(_driver, 0);
        _blackWhiteRenderTexture.SetTexture(_driver, 1);
        _groundModel.Render(_driver);

        _driver.EndScene();
        return true;
    }
}

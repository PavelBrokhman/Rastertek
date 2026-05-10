using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial45.Graphics;

public class GraphicsFramework
{
    private const int SHADOWMAP_WIDTH = 1024;
    private const int SHADOWMAP_HEIGHT = 1024;
    private const float SHADOWMAP_DEPTH = 50.0f;
    private const float SHADOWMAP_NEAR = 1.0f;

    private GL4 _driver;
    private Camera _camera;
    private Model _groundModel,
        _treeTrunkModel,
        _treeLeafModel;
    private Light _light;
    private RenderTexture _renderTexture;
    private DepthShader _depthShader;
    private TransparentDepthShader _transparentDepthShader;
    private ShadowShader _shadowShader;
    private float _shadowMapBias;
    private float _lightAngle;
    private float _lightPosX;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 7.0f, -11.0f);
        _camera.SetRotation(20.0f, 0.0f, 0.0f);
        _camera.Render();

        _groundModel = new Model();
        if (!_groundModel.Initialize(OpenGL, "Models/plane01.txt", "Data/dirt01.tga")) return false;

        _treeTrunkModel = new Model();
        if (!_treeTrunkModel.Initialize(OpenGL, "Models/trunk001.txt", "Data/trunk001.tga")) return false;

        _treeLeafModel = new Model();
        if (!_treeLeafModel.Initialize(OpenGL, "Models/leaf001.txt", "Data/leaf001.tga")) return false;

        _light = new Light();
        _light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.GenerateOrthoMatrix(20.0f, SHADOWMAP_NEAR, SHADOWMAP_DEPTH);

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(OpenGL, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SHADOWMAP_NEAR, SHADOWMAP_DEPTH)) return false;

        _depthShader = new DepthShader();
        if (!_depthShader.Initialize(OpenGL)) return false;

        _transparentDepthShader = new TransparentDepthShader();
        if (!_transparentDepthShader.Initialize(OpenGL)) return false;

        _shadowShader = new ShadowShader();
        if (!_shadowShader.Initialize(OpenGL)) return false;

        _shadowMapBias = 0.0022f;
        _lightAngle = 270.0f;
        _lightPosX = 9.0f;
        return true;
    }

    public void Shutdown()
    {
        _shadowShader?.Shutdown(_driver); _shadowShader = null;
        _transparentDepthShader?.Shutdown(_driver); _transparentDepthShader = null;
        _depthShader?.Shutdown(_driver); _depthShader = null;
        _renderTexture?.Shutdown(_driver); _renderTexture = null;
        _treeLeafModel?.Shutdown(_driver); _treeLeafModel = null;
        _treeTrunkModel?.Shutdown(_driver); _treeTrunkModel = null;
        _groundModel?.Shutdown(_driver); _groundModel = null;
        _light = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame()
    {
        const float frameTime = 10.0f;
        _lightPosX -= 0.003f * frameTime;
        _lightAngle -= 0.03f * frameTime;
        if (_lightAngle < 90.0f)
        {
            _lightAngle = 270.0f;
            _lightPosX = 9.0f;
        }
        float radians = _lightAngle * 0.0174532925f;
        _light.SetDirection(MathF.Sin(radians), MathF.Cos(radians), 0.0f);
        _light.SetPosition(_lightPosX, 10.0f, 1.0f);
        _light.SetLookAt(-_lightPosX, 0.0f, 2.0f);
        _light.GenerateViewMatrix();

        if (!RenderSceneToTexture()) return false;
        return Render();
    }

    private bool RenderSceneToTexture()
    {
        _renderTexture.SetRenderTarget(_driver);
        _renderTexture.ClearRenderTarget(_driver, 0, 0, 0, 1);

        var lightViewMatrix = _light.GetViewMatrix();
        var lightOrthoMatrix = _light.GetOrthoMatrix();

        // Tree (trunk + leaves) at scale 0.1
        var scaleMatrix = Matrix4X4.CreateScale<float>(0.1f);
        var translateMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        var worldMatrix = scaleMatrix * translateMatrix;

        if (!_depthShader.SetShaderParameters(_driver, worldMatrix, lightViewMatrix, lightOrthoMatrix)) return false;
        _treeTrunkModel.Render(_driver);

        if (!_transparentDepthShader.SetShaderParameters(_driver, worldMatrix, lightViewMatrix, lightOrthoMatrix)) return false;
        _treeLeafModel.SetTexture(_driver, 0);
        _treeLeafModel.Render(_driver);

        // Ground at scale 2.0
        scaleMatrix = Matrix4X4.CreateScale<float>(2.0f);
        worldMatrix = scaleMatrix * translateMatrix;
        if (!_depthShader.SetShaderParameters(_driver, worldMatrix, lightViewMatrix, lightOrthoMatrix)) return false;
        _groundModel.Render(_driver);

        _driver.SetBackBufferRenderTarget();
        _driver.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _driver.BeginScene(0.0f, 0.5f, 0.8f, 1.0f);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        var lightViewMatrix = _light.GetViewMatrix();
        var lightOrthoMatrix = _light.GetOrthoMatrix();
        var ambientColor = _light.GetAmbientLight();
        var diffuseColor = _light.GetDiffuseColor();
        var lightDirection = _light.GetDirection();

        // Ground
        var scaleMatrix = Matrix4X4.CreateScale<float>(2.0f);
        var translateMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        var worldMatrix = scaleMatrix * translateMatrix;
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightOrthoMatrix, diffuseColor, ambientColor, lightDirection, _shadowMapBias)) return false;
        _groundModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _groundModel.Render(_driver);

        // Tree trunk
        scaleMatrix = Matrix4X4.CreateScale<float>(0.1f);
        worldMatrix = scaleMatrix * translateMatrix;
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightOrthoMatrix, diffuseColor, ambientColor, lightDirection, _shadowMapBias)) return false;
        _treeTrunkModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _treeTrunkModel.Render(_driver);

        // Tree leaves with alpha blending
        _driver.EnableAlphaBlending();
        if (!_shadowShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightOrthoMatrix, diffuseColor, ambientColor, lightDirection, _shadowMapBias)) return false;
        _treeLeafModel.SetTexture(_driver, 0);
        _renderTexture.SetTexture(_driver, 1);
        _treeLeafModel.Render(_driver);
        _driver.DisableAlphaBlending();

        _driver.EndScene();
        return true;
    }
}

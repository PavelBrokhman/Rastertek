using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial44.Graphics;

public class GraphicsFramework
{
    private const int SHADOWMAP_WIDTH = 1024;
    private const int SHADOWMAP_HEIGHT = 1024;
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 _directX;
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

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();
        _camera.SetPosition(0.0f, 7.0f, -10.0f);
        _camera.SetRotation(35.0f, 0.0f, 0.0f);
        _camera.Render();

        _cubeModel = new Model();
        if (!_cubeModel.Initialize(DirectX, "Models/Cube.txt", "Data/wall01.tga", true)) return false;
        _sphereModel = new Model();
        if (!_sphereModel.Initialize(DirectX, "Models/sphere.txt", "Data/ice.tga", true)) return false;
        _groundModel = new Model();
        if (!_groundModel.Initialize(DirectX, "Models/plane01.txt", "Data/metal001.tga", true)) return false;

        _light = new Light();
        _light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetLookAt(0.0f, 0.0f, 0.0f);
        _light.GenerateProjectionMatrix(SCREEN_DEPTH, SCREEN_NEAR);

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(DirectX, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SCREEN_DEPTH, SCREEN_NEAR)) return false;

        _blackWhiteRenderTexture = new RenderTexture();
        if (!_blackWhiteRenderTexture.Initialize(DirectX, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SCREEN_DEPTH, SCREEN_NEAR)) return false;

        _depthShader = new DepthShader();
        if (!_depthShader.Initialize(DirectX)) return false;

        _shadowShader = new ShadowShader();
        if (!_shadowShader.Initialize(DirectX)) return false;

        _softShadowShader = new SoftShadowShader();
        if (!_softShadowShader.Initialize(DirectX)) return false;

        int dsW = SHADOWMAP_WIDTH / 2;
        int dsH = SHADOWMAP_HEIGHT / 2;
        _blur = new Blur();
        if (!_blur.Initialize(DirectX, dsW, dsH, SCREEN_NEAR, SCREEN_DEPTH, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT)) return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX)) return false;

        _blurShader = new BlurShader();
        if (!_blurShader.Initialize(DirectX)) return false;

        _shadowMapBias = 0.0022f;
        _lightPositionX = -5.0f;
        return true;
    }

    public void Shutdown()
    {
        _blurShader?.Shutdown(); _blurShader = null;
        _textureShader?.Shutdown(); _textureShader = null;
        _blur?.Shutdown(); _blur = null;
        _softShadowShader?.Shutdown(); _softShadowShader = null;
        _shadowShader?.Shutdown(); _shadowShader = null;
        _depthShader?.Shutdown(); _depthShader = null;
        _blackWhiteRenderTexture?.Shutdown(); _blackWhiteRenderTexture = null;
        _renderTexture?.Shutdown(); _renderTexture = null;
        _groundModel?.Shutdown(); _groundModel = null;
        _sphereModel?.Shutdown(); _sphereModel = null;
        _cubeModel?.Shutdown(); _cubeModel = null;
        _light = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _lightPositionX += 0.05f;
        if (_lightPositionX > 5.0f) _lightPositionX = -5.0f;
        _light.SetPosition(_lightPositionX, 8.0f, -5.0f);
        _light.GenerateViewMatrix();

        if (!RenderDepthToTexture()) return false;
        if (!RenderBlackAndWhiteShadows()) return false;
        if (!_blur.BlurTexture(_blackWhiteRenderTexture, _directX, _camera, _textureShader, _blurShader)) return false;
        return Render();
    }

    private bool RenderDepthToTexture()
    {
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0, 0, 0, 1);

        var lightViewMatrix = _light.GetViewMatrix();
        var lightProjectionMatrix = _light.GetProjectionMatrix();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-2.0f, 2.0f, 0.0f);
        _cubeModel.Render(_directX);
        if (!_depthShader.Render(_directX, _cubeModel.GetIndexCount(), worldMatrix, lightViewMatrix, lightProjectionMatrix)) return false;

        worldMatrix = Matrix4X4.CreateTranslation<float>(2.0f, 2.0f, 0.0f);
        _sphereModel.Render(_directX);
        if (!_depthShader.Render(_directX, _sphereModel.GetIndexCount(), worldMatrix, lightViewMatrix, lightProjectionMatrix)) return false;

        worldMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        _groundModel.Render(_directX);
        if (!_depthShader.Render(_directX, _groundModel.GetIndexCount(), worldMatrix, lightViewMatrix, lightProjectionMatrix)) return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool RenderBlackAndWhiteShadows()
    {
        _blackWhiteRenderTexture.SetRenderTarget(_directX);
        _blackWhiteRenderTexture.ClearRenderTarget(_directX, 0, 0, 0, 1);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();
        var lightViewMatrix = _light.GetViewMatrix();
        var lightProjectionMatrix = _light.GetProjectionMatrix();
        var lightPosition = _light.GetPosition();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-2.0f, 2.0f, 0.0f);
        _cubeModel.Render(_directX);
        if (!_shadowShader.Render(_directX, _cubeModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix,
                _renderTexture.GetShaderResourceView(), lightPosition, _shadowMapBias)) return false;

        worldMatrix = Matrix4X4.CreateTranslation<float>(2.0f, 2.0f, 0.0f);
        _sphereModel.Render(_directX);
        if (!_shadowShader.Render(_directX, _sphereModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix,
                _renderTexture.GetShaderResourceView(), lightPosition, _shadowMapBias)) return false;

        worldMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        _groundModel.Render(_directX);
        if (!_shadowShader.Render(_directX, _groundModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix,
                _renderTexture.GetShaderResourceView(), lightPosition, _shadowMapBias)) return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _directX.BeginScene(0, 0, 0, 1);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();
        var lightPosition = _light.GetPosition();
        var ambientColor = _light.GetAmbientLight();
        var diffuseColor = _light.GetDiffuseColor();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-2.0f, 2.0f, 0.0f);
        _cubeModel.Render(_directX);
        if (!_softShadowShader.Render(_directX, _cubeModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix,
                _cubeModel.GetTextureView(), _blackWhiteRenderTexture.GetShaderResourceView(),
                lightPosition, ambientColor, diffuseColor)) return false;

        worldMatrix = Matrix4X4.CreateTranslation<float>(2.0f, 2.0f, 0.0f);
        _sphereModel.Render(_directX);
        if (!_softShadowShader.Render(_directX, _sphereModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix,
                _sphereModel.GetTextureView(), _blackWhiteRenderTexture.GetShaderResourceView(),
                lightPosition, ambientColor, diffuseColor)) return false;

        worldMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        _groundModel.Render(_directX);
        if (!_softShadowShader.Render(_directX, _groundModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix,
                _groundModel.GetTextureView(), _blackWhiteRenderTexture.GetShaderResourceView(),
                lightPosition, ambientColor, diffuseColor)) return false;

        _directX.EndScene();
        return true;
    }
}

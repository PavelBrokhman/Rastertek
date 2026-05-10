using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial45.Graphics;

public class GraphicsFramework
{
    private const int SHADOWMAP_WIDTH = 1024;
    private const int SHADOWMAP_HEIGHT = 1024;
    private const float SHADOWMAP_DEPTH = 50.0f;
    private const float SHADOWMAP_NEAR = 1.0f;

    private DX11 _directX;
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

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 7.0f, -11.0f);
        _camera.SetRotation(20.0f, 0.0f, 0.0f);
        _camera.Render();

        _groundModel = new Model();
        if (!_groundModel.Initialize(DirectX, "Models/plane01.txt", "Data/dirt01.tga", true)) return false;
        _treeTrunkModel = new Model();
        if (!_treeTrunkModel.Initialize(DirectX, "Models/trunk001.txt", "Data/trunk001.tga", true)) return false;
        _treeLeafModel = new Model();
        if (!_treeLeafModel.Initialize(DirectX, "Models/leaf001.txt", "Data/leaf001.tga", true)) return false;

        _light = new Light();
        _light.SetAmbientLight(0.15f, 0.15f, 0.15f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.GenerateOrthoMatrix(20.0f, SHADOWMAP_NEAR, SHADOWMAP_DEPTH);

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(DirectX, SHADOWMAP_WIDTH, SHADOWMAP_HEIGHT, SHADOWMAP_DEPTH, SHADOWMAP_NEAR)) return false;

        _depthShader = new DepthShader();
        if (!_depthShader.Initialize(DirectX)) return false;
        _transparentDepthShader = new TransparentDepthShader();
        if (!_transparentDepthShader.Initialize(DirectX)) return false;
        _shadowShader = new ShadowShader();
        if (!_shadowShader.Initialize(DirectX)) return false;

        _shadowMapBias = 0.0022f;
        _lightAngle = 270.0f;
        _lightPosX = 9.0f;
        return true;
    }

    public void Shutdown()
    {
        _shadowShader?.Shutdown(); _shadowShader = null;
        _transparentDepthShader?.Shutdown(); _transparentDepthShader = null;
        _depthShader?.Shutdown(); _depthShader = null;
        _renderTexture?.Shutdown(); _renderTexture = null;
        _treeLeafModel?.Shutdown(); _treeLeafModel = null;
        _treeTrunkModel?.Shutdown(); _treeTrunkModel = null;
        _groundModel?.Shutdown(); _groundModel = null;
        _light = null;
        _camera = null;
        _directX = null;
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
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0, 0, 0, 1);

        var lightViewMatrix = _light.GetViewMatrix();
        var lightOrthoMatrix = _light.GetOrthoMatrix();

        var scaleMatrix = Matrix4X4.CreateScale<float>(0.1f);
        var translateMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        var worldMatrix = scaleMatrix * translateMatrix;

        _treeTrunkModel.Render(_directX);
        if (!_depthShader.Render(_directX, _treeTrunkModel.GetIndexCount(), worldMatrix, lightViewMatrix, lightOrthoMatrix)) return false;

        _treeLeafModel.Render(_directX);
        if (!_transparentDepthShader.Render(_directX, _treeLeafModel.GetIndexCount(), worldMatrix, lightViewMatrix, lightOrthoMatrix, _treeLeafModel.GetTextureView())) return false;

        scaleMatrix = Matrix4X4.CreateScale<float>(2.0f);
        worldMatrix = scaleMatrix * translateMatrix;
        _groundModel.Render(_directX);
        if (!_depthShader.Render(_directX, _groundModel.GetIndexCount(), worldMatrix, lightViewMatrix, lightOrthoMatrix)) return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.5f, 0.8f, 1.0f);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();
        var lightViewMatrix = _light.GetViewMatrix();
        var lightOrthoMatrix = _light.GetOrthoMatrix();
        var ambientColor = _light.GetAmbientLight();
        var diffuseColor = _light.GetDiffuseColor();
        var lightDirection = _light.GetDirection();

        // Ground (scale 2.0)
        var scaleMatrix = Matrix4X4.CreateScale<float>(2.0f);
        var translateMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        var worldMatrix = scaleMatrix * translateMatrix;
        _groundModel.Render(_directX);
        if (!_shadowShader.Render(_directX, _groundModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
                _groundModel.GetTextureView(), _renderTexture.GetShaderResourceView(),
                ambientColor, diffuseColor, lightDirection, _shadowMapBias)) return false;

        // Tree trunk (scale 0.1)
        scaleMatrix = Matrix4X4.CreateScale<float>(0.1f);
        worldMatrix = scaleMatrix * translateMatrix;
        _treeTrunkModel.Render(_directX);
        if (!_shadowShader.Render(_directX, _treeTrunkModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
                _treeTrunkModel.GetTextureView(), _renderTexture.GetShaderResourceView(),
                ambientColor, diffuseColor, lightDirection, _shadowMapBias)) return false;

        // Tree leaves with alpha blending
        _directX.EnableAlphaBlending();
        _treeLeafModel.Render(_directX);
        if (!_shadowShader.Render(_directX, _treeLeafModel.GetIndexCount(),
                worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightOrthoMatrix,
                _treeLeafModel.GetTextureView(), _renderTexture.GetShaderResourceView(),
                ambientColor, diffuseColor, lightDirection, _shadowMapBias)) return false;
        _directX.DisableAlphaBlending();

        _directX.EndScene();
        return true;
    }
}

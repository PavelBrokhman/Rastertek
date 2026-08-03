using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial53.Graphics;

public class GraphicsFramework
{
    private const float ScreenNear = 0.3f;
    private const float ScreenDepth = 1000.0f;

    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private Light _light;
    private Timer _timer;
    private Heat _heat;
    private RenderTexture _renderTexture;
    private RenderTexture _heatTexture;
    private OrthoWindow _fullScreenWindow;
    private LightShader _lightShader;
    private TextureShader _textureShader;
    private BlurShader _blurShader;
    private HeatShader _heatShader;
    private Blur _blur;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/sphere.txt", "Data/yellowcolor01.tga", true)) return false;

        _light = new Light();
        _light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(DirectX, screenWidth, screenHeight, ScreenDepth, ScreenNear)) return false;

        _heatTexture = new RenderTexture();
        if (!_heatTexture.Initialize(DirectX, screenWidth, screenHeight, ScreenDepth, ScreenNear)) return false;

        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(DirectX, screenWidth, screenHeight)) return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(DirectX)) return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX)) return false;

        _blurShader = new BlurShader();
        if (!_blurShader.Initialize(DirectX)) return false;

        _heatShader = new HeatShader();
        if (!_heatShader.Initialize(DirectX)) return false;

        _blur = new Blur();
        if (!_blur.Initialize(DirectX, screenWidth / 2, screenHeight / 2, ScreenNear, ScreenDepth, screenWidth, screenHeight)) return false;

        _heat = new Heat();
        if (!_heat.Initialize(DirectX, "Data/heatnoise01.tga")) return false;

        _timer = new Timer();
        _timer.Initialize();

        return true;
    }

    public void Shutdown()
    {
        _blur?.Shutdown(); _blur = null;
        _heatShader?.Shutdown(); _heatShader = null;
        _blurShader?.Shutdown(); _blurShader = null;
        _textureShader?.Shutdown(); _textureShader = null;
        _lightShader?.Shutdown(); _lightShader = null;
        _fullScreenWindow?.Shutdown(); _fullScreenWindow = null;
        _heatTexture?.Shutdown(); _heatTexture = null;
        _renderTexture?.Shutdown(); _renderTexture = null;
        _model?.Shutdown(); _model = null;
        _heat?.Shutdown(); _heat = null;
        _timer = null;
        _light = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _timer.Frame();
        _heat.Frame(_timer.GetTime());

        if (!RenderSceneToTexture()) return false;
        if (!RenderHeatToTexture()) return false;
        if (!_blur.BlurTexture(_directX, _camera, _heatTexture, _textureShader, _blurShader)) return false;
        return Render();
    }

    private bool RenderSceneToTexture()
    {
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0.25f, 0.25f, 0.25f, 1.0f);

        var worldMatrix = _directX.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        _model.SetTexture(_directX, 0);
        if (!_lightShader.Render(_directX, _model.GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix,
                _light.GetDirection(), _light.GetDiffuseColor(), _light.GetAmbientColor()))
            return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool RenderHeatToTexture()
    {
        _heatTexture.SetRenderTarget(_directX);
        _heatTexture.ClearRenderTarget(_directX, 0.0f, 0.0f, 0.0f, 1.0f);

        var worldMatrix = _directX.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        _model.SetTexture(_directX, 0);
        if (!_lightShader.Render(_directX, _model.GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix,
                _light.GetDirection(), _light.GetDiffuseColor(), _light.GetAmbientColor()))
            return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var worldMatrix = _directX.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _directX.GetOrthoMatrix();

        _directX.TurnZBufferOff();

        _fullScreenWindow.Render(_directX);
        if (!_heatShader.Render(_directX, _fullScreenWindow.GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix,
                _renderTexture.GetShaderResourceView(), _heatTexture.GetShaderResourceView(),
                _heat.GetTextureView(),
                _heat.GetEmissiveMultiplier(), _heat.GetNoiseFrameTime(),
                _heat.GetScrollSpeeds(), _heat.GetScales(),
                _heat.GetDistortion1(), _heat.GetDistortion2(), _heat.GetDistortion3(),
                _heat.GetDistortionScale(), _heat.GetDistortionBias())) return false;

        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

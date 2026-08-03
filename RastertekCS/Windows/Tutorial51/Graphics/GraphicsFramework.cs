using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial51.Graphics;

public class GraphicsFramework
{
    private const float ScreenNear = 0.3f;
    private const float ScreenDepth = 1000.0f;

    private DX11 _directX;
    private Camera _camera;
    private Light _light;
    private Model _sphereModel;
    private Model _groundModel;
    private Texture _randomTexture;
    private DeferredBuffers _deferredBuffers;
    private GBufferShader _gBufferShader;
    private RenderTexture _ssaoRenderTexture;
    private RenderTexture _blurSsaoRenderTexture;
    private OrthoWindow _fullScreenWindow;
    private SsaoShader _ssaoShader;
    private SsaoBlurShader _ssaoBlurShader;
    private LightShader _lightShader;
    private int _screenWidth,
        _screenHeight;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.RenderBaseViewMatrix();

        _camera.SetPosition(0.0f, 7.0f, -10.0f);
        _camera.SetRotation(35.0f, 0.0f, 0.0f);
        _camera.Render();

        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(1.0f, -0.5f, 0.0f);

        _sphereModel = new Model();
        if (!_sphereModel.Initialize(DirectX, "Models/sphere.txt", "Data/ice.tga", true)) return false;

        _groundModel = new Model();
        if (!_groundModel.Initialize(DirectX, "Models/plane01.txt", "Data/metal001.tga", true)) return false;

        _deferredBuffers = new DeferredBuffers();
        if (!_deferredBuffers.Initialize(DirectX, screenWidth, screenHeight, ScreenDepth, ScreenNear)) return false;

        _gBufferShader = new GBufferShader();
        if (!_gBufferShader.Initialize(DirectX)) return false;

        // Format 2 is the single-channel float target the ssao passes write to.
        _ssaoRenderTexture = new RenderTexture();
        if (!_ssaoRenderTexture.Initialize(DirectX, screenWidth, screenHeight, ScreenDepth, ScreenNear, 2)) return false;

        _blurSsaoRenderTexture = new RenderTexture();
        if (!_blurSsaoRenderTexture.Initialize(DirectX, screenWidth, screenHeight, ScreenDepth, ScreenNear, 2)) return false;

        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(DirectX, screenWidth, screenHeight)) return false;

        _randomTexture = new Texture();
        if (!_randomTexture.Initialize(DirectX, "Data/random_vec.tga", true)) return false;

        _ssaoShader = new SsaoShader();
        if (!_ssaoShader.Initialize(DirectX)) return false;

        _ssaoBlurShader = new SsaoBlurShader();
        if (!_ssaoBlurShader.Initialize(DirectX)) return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        _lightShader?.Shutdown(); _lightShader = null;
        _ssaoBlurShader?.Shutdown(); _ssaoBlurShader = null;
        _ssaoShader?.Shutdown(); _ssaoShader = null;
        _randomTexture?.Shutdown(); _randomTexture = null;
        _fullScreenWindow?.Shutdown(); _fullScreenWindow = null;
        _blurSsaoRenderTexture?.Shutdown(); _blurSsaoRenderTexture = null;
        _ssaoRenderTexture?.Shutdown(); _ssaoRenderTexture = null;
        _gBufferShader?.Shutdown(); _gBufferShader = null;
        _deferredBuffers?.Shutdown(); _deferredBuffers = null;
        _groundModel?.Shutdown(); _groundModel = null;
        _sphereModel?.Shutdown(); _sphereModel = null;
        _light = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        if (!RenderGBuffer()) return false;
        if (!RenderSsao()) return false;
        if (!BlurSsaoTexture()) return false;
        return Render();
    }

    private bool RenderGBuffer()
    {
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();

        _deferredBuffers.SetRenderTargets(_directX);
        _deferredBuffers.ClearRenderTargets(_directX, 0.0f, 0.0f, 0.0f, 0.0f);

        var translateMatrix = Matrix4X4.CreateTranslation(2.0f, 2.0f, 0.0f);
        _sphereModel.Render(_directX);
        if (!_gBufferShader.Render(_directX, _sphereModel.GetIndexCount(), translateMatrix, viewMatrix,
                projectionMatrix, _sphereModel.GetTextureView())) return false;

        translateMatrix = Matrix4X4.CreateTranslation(0.0f, 1.0f, 0.0f);
        _groundModel.Render(_directX);
        if (!_gBufferShader.Render(_directX, _groundModel.GetIndexCount(), translateMatrix, viewMatrix,
                projectionMatrix, _groundModel.GetTextureView())) return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool RenderSsao()
    {
        const float sampleRadius = 1.0f;
        const float ssaoScale = 1.0f;
        const float ssaoBias = 0.1f;
        const float ssaoIntensity = 2.0f;
        const float randomTextureSize = 64.0f;

        var worldMatrix = _directX.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _directX.GetOrthoMatrix();

        _ssaoRenderTexture.SetRenderTarget(_directX);
        _ssaoRenderTexture.ClearRenderTarget(_directX, 0.0f, 0.0f, 0.0f, 0.0f);

        _directX.TurnZBufferOff();

        _fullScreenWindow.Render(_directX);
        if (!_ssaoShader.Render(_directX, _fullScreenWindow.GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix,
                _deferredBuffers.GetShaderResourcePositions(), _deferredBuffers.GetShaderResourceNormals(),
                _randomTexture.GetTextureView(), _screenWidth, _screenHeight, randomTextureSize,
                sampleRadius, ssaoScale, ssaoBias, ssaoIntensity)) return false;

        _directX.TurnZBufferOn();

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool BlurSsaoTexture()
    {
        var worldMatrix = _directX.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _directX.GetOrthoMatrix();

        _directX.TurnZBufferOff();

        // Horizontal pass into the blur texture.
        _blurSsaoRenderTexture.SetRenderTarget(_directX);
        _blurSsaoRenderTexture.ClearRenderTarget(_directX, 0.0f, 0.0f, 0.0f, 1.0f);

        _fullScreenWindow.Render(_directX);
        if (!_ssaoBlurShader.Render(_directX, _fullScreenWindow.GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix,
                _ssaoRenderTexture.GetShaderResourceView(), _deferredBuffers.GetShaderResourceNormals(),
                _screenWidth, _screenHeight, 0)) return false;

        // Vertical pass, back into the ssao texture.
        _ssaoRenderTexture.SetRenderTarget(_directX);
        _ssaoRenderTexture.ClearRenderTarget(_directX, 0.0f, 0.0f, 0.0f, 1.0f);

        _fullScreenWindow.Render(_directX);
        if (!_ssaoBlurShader.Render(_directX, _fullScreenWindow.GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix,
                _blurSsaoRenderTexture.GetShaderResourceView(), _deferredBuffers.GetShaderResourceNormals(),
                _screenWidth, _screenHeight, 1)) return false;

        _directX.TurnZBufferOn();

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool Render()
    {
        var worldMatrix = _directX.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _directX.GetOrthoMatrix();
        var viewMatrix = _camera.GetViewMatrix();

        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        _directX.TurnZBufferOff();

        _fullScreenWindow.Render(_directX);
        if (!_lightShader.Render(_directX, _fullScreenWindow.GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix,
                _light.GetDirection(), _deferredBuffers.GetShaderResourceNormals(),
                _ssaoRenderTexture.GetShaderResourceView(), viewMatrix,
                _deferredBuffers.GetShaderResourceColors())) return false;

        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

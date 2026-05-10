using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial46.Graphics;

public class GraphicsFramework
{
    private const int DownSampleWidth = 100;
    private const int DownSampleHeight = 100;
    private const float GlowStrength = 2.0f;
    private const float ScreenNear = 0.3f;
    private const float ScreenDepth = 1000.0f;

    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private RenderTexture _renderTexture;
    private RenderTexture _glowTexture;
    private OrthoWindow _fullScreenWindow;
    private TextureShader _textureShader;
    private BlurShader _blurShader;
    private GlowShader _glowShader;
    private Blur _blur;
    private float _rotation = 0.0f;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/cube.txt", "Data/stone01.tga", "Data/glowmap001.tga", true)) return false;

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(DirectX, screenWidth, screenHeight, ScreenDepth, ScreenNear)) return false;

        _glowTexture = new RenderTexture();
        if (!_glowTexture.Initialize(DirectX, screenWidth, screenHeight, ScreenDepth, ScreenNear)) return false;

        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(DirectX, screenWidth, screenHeight)) return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX)) return false;

        _blurShader = new BlurShader();
        if (!_blurShader.Initialize(DirectX)) return false;

        _glowShader = new GlowShader();
        if (!_glowShader.Initialize(DirectX)) return false;

        _blur = new Blur();
        if (!_blur.Initialize(DirectX, DownSampleWidth, DownSampleHeight, ScreenNear, ScreenDepth, screenWidth, screenHeight)) return false;

        return true;
    }

    public void Shutdown()
    {
        _blur?.Shutdown(); _blur = null;
        _glowShader?.Shutdown(); _glowShader = null;
        _blurShader?.Shutdown(); _blurShader = null;
        _textureShader?.Shutdown(); _textureShader = null;
        _fullScreenWindow?.Shutdown(); _fullScreenWindow = null;
        _glowTexture?.Shutdown(); _glowTexture = null;
        _renderTexture?.Shutdown(); _renderTexture = null;
        _model?.Shutdown(); _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 0.25f;
        if (_rotation < 0.0f) _rotation += 360.0f;

        if (!RenderSceneToTexture(_rotation)) return false;
        if (!RenderGlowToTexture(_rotation)) return false;
        if (!_blur.BlurTexture(_directX, _camera, _glowTexture, _textureShader, _blurShader)) return false;
        return Render();
    }

    private bool RenderSceneToTexture(float rotation)
    {
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0, 0, 0, 1);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();
        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);

        _model.Render(_directX);
        if (!_textureShader.Render(_directX, _model.GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, _model.GetTexture1View())) return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool RenderGlowToTexture(float rotation)
    {
        _glowTexture.SetRenderTarget(_directX);
        _glowTexture.ClearRenderTarget(_directX, 0, 0, 0, 1);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();
        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);

        _model.Render(_directX);
        if (!_textureShader.Render(_directX, _model.GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, _model.GetTexture2View())) return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.5f, 0.8f, 1.0f);

        var worldMatrix = _directX.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _directX.GetOrthoMatrix();

        _directX.TurnZBufferOff();
        _fullScreenWindow.Render(_directX);
        if (!_glowShader.Render(_directX, _fullScreenWindow.GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix,
                _renderTexture.GetShaderResourceView(), _glowTexture.GetShaderResourceView(), GlowStrength)) return false;
        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

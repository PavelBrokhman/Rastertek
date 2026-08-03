using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial37.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;
    private const float FadeInTimeMs = 5000.0f;

    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;
    private RenderTexture _renderTexture;
    private OrthoWindow _fullScreenWindow;
    private FadeShader _fadeShader;
    private Timer _timer;
    private float _rotation;
    private float _accumulatedTime;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;
        _rotation = 360.0f;
        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();
        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;
        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", false))
            return false;
        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(DirectX, screenWidth, screenHeight, SCREEN_DEPTH, SCREEN_NEAR))
            return false;
        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(DirectX, screenWidth, screenHeight))
            return false;
        _fadeShader = new FadeShader();
        if (!_fadeShader.Initialize(DirectX))
            return false;
        _timer = new Timer();
        _timer.Initialize();
        _accumulatedTime = 0;
        return true;
    }

    public void Shutdown()
    {
        _fadeShader?.Shutdown();
        _fullScreenWindow?.Shutdown();
        _renderTexture?.Shutdown();
        _model?.Shutdown();
        _textureShader?.Shutdown();
        _fadeShader = null;
        _fullScreenWindow = null;
        _renderTexture = null;
        _model = null;
        _textureShader = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _timer.Frame();
        _rotation -= 0.0174532925f * 0.25f;
        if (_rotation < 0.0f)
            _rotation += 360.0f;
        _accumulatedTime += _timer.GetTime();
        float fadePercentage = _accumulatedTime < FadeInTimeMs ? _accumulatedTime / FadeInTimeMs : 1.0f;
        if (!RenderSceneToTexture(_rotation))
            return false;
        return Render(fadePercentage);
    }

    private bool RenderSceneToTexture(float rotation)
    {
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0, 0, 0, 1);
        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _renderTexture.GetProjectionMatrix();
        _model.Render(_directX);
        if (!_textureShader.Render(_directX, _model.GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, _model.GetTextureView()))
            return false;
        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool Render(float fadeAmount)
    {
        _directX.BeginScene(0, 0, 0, 1);
        _directX.TurnZBufferOff();
        var worldMatrix = _directX.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _directX.GetOrthoMatrix();
        _fullScreenWindow.Render(_directX);
        if (!_fadeShader.Render(_directX, _fullScreenWindow.GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix, _renderTexture.GetShaderResourceView(), fadeAmount))
            return false;
        _directX.TurnZBufferOn();
        _directX.EndScene();
        return true;
    }
}

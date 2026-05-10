using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial36.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;
    private RenderTexture _renderTexture;
    private OrthoWindow _fullScreenWindow;
    private Blur _blur;
    private BlurShader _blurShader;
    private float _rotation;

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

        int downSampleWidth = screenWidth / 2;
        int downSampleHeight = screenHeight / 2;

        _blur = new Blur();
        if (
            !_blur.Initialize(
                DirectX,
                downSampleWidth,
                downSampleHeight,
                SCREEN_DEPTH,
                SCREEN_NEAR,
                screenWidth,
                screenHeight
            )
        )
            return false;

        _blurShader = new BlurShader();
        if (!_blurShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _blurShader?.Shutdown();
        _blur?.Shutdown();
        _fullScreenWindow?.Shutdown();
        _renderTexture?.Shutdown();
        _model?.Shutdown();
        _textureShader?.Shutdown();
        _blurShader = null;
        _blur = null;
        _fullScreenWindow = null;
        _renderTexture = null;
        _model = null;
        _textureShader = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f;
        if (_rotation <= 0.0f)
            _rotation += 360.0f;
        if (!RenderSceneToTexture(_rotation))
            return false;
        if (!_blur.BlurTexture(_renderTexture, _directX, _camera, _textureShader, _blurShader))
            return true;
        return Render();
    }

    private bool RenderSceneToTexture(float rotation)
    {
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0, 0, 0, 1);

        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _renderTexture.GetProjectionMatrix();

        _model.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _model.GetIndexCount(),
                worldMatrix,
                viewMatrix,
                projectionMatrix,
                _model.GetTextureView()
            )
        )
            return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _directX.BeginScene(0, 0, 0, 1);
        _directX.TurnZBufferOff();

        var worldMatrix = _directX.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _directX.GetOrthoMatrix();

        _fullScreenWindow.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _fullScreenWindow.GetIndexCount(),
                worldMatrix,
                baseViewMatrix,
                orthoMatrix,
                _renderTexture.GetShaderResourceView()
            )
        )
            return false;

        _directX.TurnZBufferOn();
        _directX.EndScene();
        return true;
    }
}

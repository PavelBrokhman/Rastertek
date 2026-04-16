using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial32.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private Model _winModel;
    private Texture _normalTexture;
    private RenderTexture _renderTexture;
    private TextureShader _textureShader;
    private GlassShader _glassShader;
    private float _rotation = 360f;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", false))
            return false;

        _winModel = new Model();
        if (!_winModel.Initialize(DirectX, "Models/square.txt", "Data/glass01.tga", false))
            return false;

        _normalTexture = new Texture();
        if (!_normalTexture.Initialize(DirectX, "Data/normal03.tga", false))
            return false;

        _renderTexture = new RenderTexture();
        if (
            !_renderTexture.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                SCREEN_DEPTH,
                SCREEN_NEAR
            )
        )
            return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;

        _glassShader = new GlassShader();
        if (!_glassShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _glassShader?.Shutdown();
        _textureShader?.Shutdown();
        _renderTexture?.Shutdown();
        _normalTexture?.Shutdown();
        _winModel?.Shutdown();
        _model?.Shutdown();
        _glassShader = null;
        _textureShader = null;
        _renderTexture = null;
        _normalTexture = null;
        _winModel = null;
        _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f;
        if (_rotation <= 0.0f)
            _rotation += 360.0f;

        if (!RenderToTexture(_rotation))
            return false;
        return Render(_rotation);
    }

    private bool RenderToTexture(float rot)
    {
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0.0f, 0.0f, 0.0f, 1.0f);

        var view = _camera.GetViewMatrix();
        var projection = _renderTexture.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY<float>(rot);

        _model.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _model.GetIndexCount(),
                world,
                view,
                projection,
                _model.GetTextureView()
            )
        )
            return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool Render(float rot)
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        // Render spinning cube.
        var world = Matrix4X4.CreateRotationY<float>(rot);
        _model.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _model.GetIndexCount(),
                world,
                view,
                projection,
                _model.GetTextureView()
            )
        )
            return false;

        // Render glass window in front.
        world = Matrix4X4.CreateTranslation<float>(0.0f, 0.0f, -1.5f);
        _winModel.Render(_directX);
        if (
            !_glassShader.Render(
                _directX,
                _winModel.GetIndexCount(),
                world,
                view,
                projection,
                _winModel.GetTextureView(),
                _normalTexture.GetTextureView(),
                _renderTexture.GetShaderResourceView(),
                0.01f
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

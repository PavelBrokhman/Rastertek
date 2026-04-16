using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial25.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;
    private RenderTexture _renderTexture;
    private DisplayPlane _displayPlane;
    private float _rotation = MathF.Tau;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true))
            return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(DirectX, 256, 256, SCREEN_DEPTH, SCREEN_NEAR))
            return false;

        _displayPlane = new DisplayPlane();
        if (!_displayPlane.Initialize(DirectX, 1.0f, 1.0f))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _displayPlane?.Shutdown();
        _renderTexture?.Shutdown();
        _textureShader?.Shutdown();
        _model?.Shutdown();
        _displayPlane = null;
        _renderTexture = null;
        _textureShader = null;
        _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation < 0.0f)
            _rotation += MathF.Tau;
        if (!RenderSceneToTexture())
            return false;
        return Render();
    }

    private bool RenderSceneToTexture()
    {
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0.0f, 0.5f, 1.0f, 1.0f);

        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        var view = _camera.GetViewMatrix();
        var projection = _renderTexture.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY(_rotation);

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

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();
        var rtSrv = _renderTexture.GetShaderResourceView();

        var world1 = Matrix4X4.CreateTranslation(0.0f, 1.5f, 0.0f);
        _displayPlane.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _displayPlane.GetIndexCount(),
                world1,
                view,
                projection,
                rtSrv
            )
        )
            return false;

        var world2 = Matrix4X4.CreateTranslation(-1.5f, -1.5f, 0.0f);
        _displayPlane.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _displayPlane.GetIndexCount(),
                world2,
                view,
                projection,
                rtSrv
            )
        )
            return false;

        var world3 = Matrix4X4.CreateTranslation(1.5f, -1.5f, 0.0f);
        _displayPlane.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _displayPlane.GetIndexCount(),
                world3,
                view,
                projection,
                rtSrv
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

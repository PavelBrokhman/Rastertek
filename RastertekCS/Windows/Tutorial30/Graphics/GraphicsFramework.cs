using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial30.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 _directX;
    private Camera _camera;
    private Model _cubeModel;
    private Model _floorModel;
    private RenderTexture _renderTexture;
    private TextureShader _textureShader;
    private ReflectionShader _reflectionShader;
    private float _rotation = MathF.Tau;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        _cubeModel = new Model();
        if (!_cubeModel.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true))
            return false;

        _floorModel = new Model();
        if (!_floorModel.Initialize(DirectX, "Models/floor.txt", "Data/blue01.tga", true))
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

        _reflectionShader = new ReflectionShader();
        if (!_reflectionShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _reflectionShader?.Shutdown();
        _textureShader?.Shutdown();
        _renderTexture?.Shutdown();
        _floorModel?.Shutdown();
        _cubeModel?.Shutdown();
        _reflectionShader = null;
        _textureShader = null;
        _renderTexture = null;
        _floorModel = null;
        _cubeModel = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation < 0.0f)
            _rotation += MathF.Tau;
        if (!RenderReflectionToTexture())
            return false;
        return Render();
    }

    private bool RenderReflectionToTexture()
    {
        _renderTexture.SetRenderTarget(_directX);
        _renderTexture.ClearRenderTarget(_directX, 0.0f, 0.0f, 0.0f, 1.0f);

        _camera.RenderReflection(-1.5f);
        var reflectionView = _camera.GetReflectionViewMatrix();
        var projection = _directX.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY(_rotation);

        _cubeModel.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _cubeModel.GetIndexCount(),
                world,
                reflectionView,
                projection,
                _cubeModel.GetTextureView()
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

        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();
        var world = Matrix4X4.CreateRotationY(_rotation);

        _cubeModel.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _cubeModel.GetIndexCount(),
                world,
                view,
                projection,
                _cubeModel.GetTextureView()
            )
        )
            return false;

        var floorWorld = Matrix4X4.CreateTranslation(0.0f, -1.5f, 0.0f);
        var reflectionView = _camera.GetReflectionViewMatrix();
        _floorModel.Render(_directX);
        if (
            !_reflectionShader.Render(
                _directX,
                _floorModel.GetIndexCount(),
                floorWorld,
                view,
                projection,
                _floorModel.GetTextureView(),
                _renderTexture.GetShaderResourceView(),
                reflectionView
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

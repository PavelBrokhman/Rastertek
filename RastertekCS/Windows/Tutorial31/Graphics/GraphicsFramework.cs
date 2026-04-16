using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial31.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 _directX;
    private Camera _camera;
    private Model _groundModel,
        _wallModel,
        _bathModel,
        _waterModel;
    private Light _light;
    private RenderTexture _refractionTexture,
        _reflectionTexture;
    private LightShader _lightShader;
    private RefractionShader _refractionShader;
    private WaterShader _waterShader;
    private float _waterHeight = 2.75f;
    private float _waterTranslation;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(-10.0f, 6.0f, -10.0f);
        _camera.SetRotation(0.0f, 45.0f, 0.0f);
        _camera.Render();

        _groundModel = new Model();
        if (!_groundModel.Initialize(DirectX, "Models/ground.txt", "Data/ground01.tga", true))
            return false;

        _wallModel = new Model();
        if (!_wallModel.Initialize(DirectX, "Models/wall.txt", "Data/wall01.tga", true))
            return false;

        _bathModel = new Model();
        if (!_bathModel.Initialize(DirectX, "Models/bath.txt", "Data/marble01.tga", true))
            return false;

        _waterModel = new Model();
        if (!_waterModel.Initialize(DirectX, "Models/water.txt", "Data/water01.tga", true))
            return false;

        _light = new Light();
        _light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, -1.0f, 0.5f);

        _refractionTexture = new RenderTexture();
        if (
            !_refractionTexture.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                SCREEN_DEPTH,
                SCREEN_NEAR
            )
        )
            return false;

        _reflectionTexture = new RenderTexture();
        if (
            !_reflectionTexture.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                SCREEN_DEPTH,
                SCREEN_NEAR
            )
        )
            return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(DirectX))
            return false;

        _refractionShader = new RefractionShader();
        if (!_refractionShader.Initialize(DirectX))
            return false;

        _waterShader = new WaterShader();
        if (!_waterShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _waterShader?.Shutdown();
        _refractionShader?.Shutdown();
        _lightShader?.Shutdown();
        _reflectionTexture?.Shutdown();
        _refractionTexture?.Shutdown();
        _waterModel?.Shutdown();
        _bathModel?.Shutdown();
        _wallModel?.Shutdown();
        _groundModel?.Shutdown();
        _waterShader = null;
        _refractionShader = null;
        _lightShader = null;
        _reflectionTexture = null;
        _refractionTexture = null;
        _waterModel = null;
        _bathModel = null;
        _wallModel = null;
        _groundModel = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _waterTranslation += 0.001f;
        if (_waterTranslation > 1.0f)
            _waterTranslation -= 1.0f;

        if (!RenderRefractionToTexture())
            return false;
        if (!RenderReflectionToTexture())
            return false;
        return Render();
    }

    private bool RenderRefractionToTexture()
    {
        var clipPlane = new Vector4D<float>(0.0f, -1.0f, 0.0f, _waterHeight + 0.1f);

        _refractionTexture.SetRenderTarget(_directX);
        _refractionTexture.ClearRenderTarget(_directX, 0.0f, 0.0f, 0.0f, 1.0f);

        var view = _camera.GetViewMatrix();
        var projection = _refractionTexture.GetProjectionMatrix();
        var world = Matrix4X4.CreateTranslation(0.0f, 2.0f, 0.0f);

        _bathModel.Render(_directX);
        if (
            !_refractionShader.Render(
                _directX,
                _bathModel.GetIndexCount(),
                world,
                view,
                projection,
                _bathModel.GetTextureView(),
                _light.Direction,
                _light.DiffuseColor,
                _light.AmbientColor,
                clipPlane
            )
        )
            return false;

        _directX.SetBackBufferRenderTarget();
        _directX.ResetViewport();
        return true;
    }

    private bool RenderReflectionToTexture()
    {
        _reflectionTexture.SetRenderTarget(_directX);
        _reflectionTexture.ClearRenderTarget(_directX, 0.0f, 0.0f, 0.0f, 1.0f);

        _camera.RenderReflection(_waterHeight);
        var reflectionView = _camera.GetReflectionViewMatrix();
        var projection = _reflectionTexture.GetProjectionMatrix();
        var world = Matrix4X4.CreateTranslation(0.0f, 6.0f, 8.0f);

        _wallModel.Render(_directX);
        if (
            !_lightShader.Render(
                _directX,
                _wallModel.GetIndexCount(),
                world,
                reflectionView,
                projection,
                _wallModel.GetTextureView(),
                _light.Direction,
                _light.DiffuseColor,
                _light.AmbientColor
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

        var world = Matrix4X4.CreateTranslation(0.0f, 1.0f, 0.0f);
        _groundModel.Render(_directX);
        if (
            !_lightShader.Render(
                _directX,
                _groundModel.GetIndexCount(),
                world,
                view,
                projection,
                _groundModel.GetTextureView(),
                _light.Direction,
                _light.DiffuseColor,
                _light.AmbientColor
            )
        )
            return false;

        world = Matrix4X4.CreateTranslation(0.0f, 6.0f, 8.0f);
        _wallModel.Render(_directX);
        if (
            !_lightShader.Render(
                _directX,
                _wallModel.GetIndexCount(),
                world,
                view,
                projection,
                _wallModel.GetTextureView(),
                _light.Direction,
                _light.DiffuseColor,
                _light.AmbientColor
            )
        )
            return false;

        world = Matrix4X4.CreateTranslation(0.0f, 2.0f, 0.0f);
        _bathModel.Render(_directX);
        if (
            !_lightShader.Render(
                _directX,
                _bathModel.GetIndexCount(),
                world,
                view,
                projection,
                _bathModel.GetTextureView(),
                _light.Direction,
                _light.DiffuseColor,
                _light.AmbientColor
            )
        )
            return false;

        var reflectionView = _camera.GetReflectionViewMatrix();
        world = Matrix4X4.CreateTranslation(0.0f, _waterHeight, 0.0f);
        _waterModel.Render(_directX);
        if (
            !_waterShader.Render(
                _directX,
                _waterModel.GetIndexCount(),
                world,
                view,
                projection,
                reflectionView,
                _reflectionTexture.GetShaderResourceView(),
                _refractionTexture.GetShaderResourceView(),
                _waterModel.GetTextureView(),
                _waterTranslation,
                0.01f
            )
        )
            return false;

        _directX.EndScene();
        return true;
    }
}

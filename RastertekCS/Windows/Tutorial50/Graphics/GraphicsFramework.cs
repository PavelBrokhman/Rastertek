using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial50.Graphics;

public class GraphicsFramework
{
    private const float ShadowMapDepth = 1000.0f;
    private const float ShadowMapNear = 0.3f;

    private DX11 _directX;
    private Camera _camera;
    private Light _light;
    private Model _model;
    private OrthoWindow _fullScreenWindow;
    private DeferredBuffers _deferredBuffers;
    private DeferredShader _deferredShader;
    private LightShader _lightShader;
    private float _rotation = 0.0f;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();

        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/Cube.txt", "Data/stone01.tga", true)) return false;

        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(DirectX, screenWidth, screenHeight)) return false;

        _deferredBuffers = new DeferredBuffers();
        if (!_deferredBuffers.Initialize(DirectX, screenWidth, screenHeight, ShadowMapDepth, ShadowMapNear)) return false;

        _deferredShader = new DeferredShader();
        if (!_deferredShader.Initialize(DirectX)) return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        _lightShader?.Shutdown(); _lightShader = null;
        _deferredShader?.Shutdown(); _deferredShader = null;
        _deferredBuffers?.Shutdown(); _deferredBuffers = null;
        _fullScreenWindow?.Shutdown(); _fullScreenWindow = null;
        _model?.Shutdown(); _model = null;
        _light = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 0.25f;
        if (_rotation < 0.0f) _rotation += 360.0f;

        if (!RenderSceneToTexture(_rotation)) return false;
        return Render();
    }

    private bool RenderSceneToTexture(float rotation)
    {
        _deferredBuffers.SetRenderTargets(_directX);
        _deferredBuffers.ClearRenderTargets(_directX, 0.0f, 0.0f, 0.0f, 1.0f);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();
        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);

        _model.Render(_directX);
        if (!_deferredShader.Render(_directX, _model.GetIndexCount(), worldMatrix, viewMatrix, projectionMatrix, _model.GetTextureView())) return false;

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
        if (!_lightShader.Render(_directX, _fullScreenWindow.GetIndexCount(), worldMatrix, baseViewMatrix, orthoMatrix,
                _deferredBuffers.GetShaderResourceView(0), _deferredBuffers.GetShaderResourceView(1),
                _light.GetDirection())) return false;

        _directX.TurnZBufferOn();
        _directX.EndScene();
        return true;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Light _light;
    private Model _model;
    private OrthoWindow _fullScreenWindow;
    private DeferredBuffers _deferredBuffers;
    private DeferredShader _deferredShader;
    private LightShader _lightShader;
    private float _rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0, 0, -10);
        _camera.Render();
        _camera.RenderBaseViewMatrix();

        _light = new Light();
        _light.SetDiffuseColor(1, 1, 1, 1);
        _light.SetDirection(0, 0, 1);

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/Cube.txt", "Data/stone01.tga", 0)) return false;

        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(OpenGL, screenWidth, screenHeight)) return false;

        _deferredBuffers = new DeferredBuffers();
        if (!_deferredBuffers.Initialize(OpenGL, screenWidth, screenHeight, 0.3f, 1000.0f)) return false;

        _deferredShader = new DeferredShader();
        if (!_deferredShader.Initialize(OpenGL)) return false;

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        _lightShader?.Shutdown(_openGL); _lightShader = null;
        _deferredShader?.Shutdown(_openGL); _deferredShader = null;
        _deferredBuffers?.Shutdown(_openGL); _deferredBuffers = null;
        _fullScreenWindow?.Shutdown(_openGL); _fullScreenWindow = null;
        _model?.Shutdown(_openGL); _model = null;
        _light = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 0.5f;
        if (_rotation <= 0.0f) _rotation += 360.0f;

        if (!RenderSceneToTexture(_rotation)) return false;
        if (!Render()) return false;
        return true;
    }

    private bool RenderSceneToTexture(float rotation)
    {
        _deferredBuffers.SetRenderTarget(_openGL);
        _deferredBuffers.ClearRenderTargets(_openGL, 0, 0, 0, 1);

        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        world = Matrix4X4.CreateRotationY<float>(rotation);

        if (!_deferredShader.SetShaderParameters(_openGL, world, view, projection, 0)) return false;
        _model.SetTexture(_openGL, 0);
        _model.Render(_openGL);

        _openGL.SetBackBufferRenderTarget();
        _openGL.ResetViewport();

        return true;
    }

    private bool Render()
    {
        _openGL.BeginScene(0, 0, 0, 1);

        var world = _openGL.GetWorldMatrix();
        var baseView = _camera.GetBaseViewMatrix();
        var ortho = _openGL.GetOrthoMatrix();

        var lightDirection = _light.GetDirection();

        _openGL.TurnZBufferOff();

        if (!_lightShader.SetShaderParameters(_openGL, world, baseView, ortho, lightDirection, 0, 1)) return false;

        _deferredBuffers.SetTexture(_openGL, 0, 0);
        _deferredBuffers.SetTexture(_openGL, 1, 1);

        _fullScreenWindow.Render(_openGL);

        _openGL.TurnZBufferOn();

        _openGL.EndScene();
        return true;
    }
}

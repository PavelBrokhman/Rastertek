using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class GraphicsFramework
{
    private GL4 _opengl;
    private Camera _camera;
    private Light _light;
    private Model _model;
    private OrthoWindow _fullscreenwindow;
    private DeferredBuffers _deferredbuffers;
    private DeferredShader _deferredshader;
    private LightShader _lightshader;
    private float _rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _opengl = OpenGL;

        _camera = new Camera();
        _camera.SetPosition(0, 0, -10);
        _camera.Render();
        _camera.RenderBaseViewMatrix();

        _light = new Light();
        _light.SetDiffuseColor(1, 1, 1, 1);
        _light.SetDirection(0, 0, 1);

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/Cube.txt", "Data/stone01.tga", 0)) return false;

        _fullscreenwindow = new OrthoWindow();
        if (!_fullscreenwindow.Initialize(OpenGL, screenWidth, screenHeight)) return false;

        _deferredbuffers = new DeferredBuffers();
        if (!_deferredbuffers.Initialize(OpenGL, screenWidth, screenHeight, 0.3f, 1000.0f)) return false;

        _deferredshader = new DeferredShader();
        if (!_deferredshader.Initialize(OpenGL)) return false;

        _lightshader = new LightShader();
        if (!_lightshader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        _lightshader?.Shutdown(_opengl); _lightshader = null;
        _deferredshader?.Shutdown(_opengl); _deferredshader = null;
        _deferredbuffers?.Shutdown(_opengl); _deferredbuffers = null;
        _fullscreenwindow?.Shutdown(_opengl); _fullscreenwindow = null;
        _model?.Shutdown(_opengl); _model = null;
        _light = null;
        _camera = null;
        _opengl = null;
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
        _deferredbuffers.SetRenderTarget(_opengl);
        _deferredbuffers.ClearRenderTargets(_opengl, 0, 0, 0, 1);

        var world = _opengl.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _opengl.GetProjectionMatrix();

        world = Matrix4X4.CreateRotationY<float>(rotation);

        if (!_deferredshader.SetShaderParameters(_opengl, world, view, projection, 0)) return false;
        _model.SetTexture(_opengl, 0);
        _model.Render(_opengl);

        _opengl.SetBackBufferRenderTarget();
        _opengl.ResetViewport();

        return true;
    }

    private bool Render()
    {
        _opengl.BeginScene(0, 0, 0, 1);

        var world = _opengl.GetWorldMatrix();
        var baseView = _camera.GetBaseViewMatrix();
        var ortho = _opengl.GetOrthoMatrix();

        var lightDirection = _light.GetDirection();

        _opengl.TurnZBufferOff();

        if (!_lightshader.SetShaderParameters(_opengl, world, baseView, ortho, lightDirection, 0, 1)) return false;

        _deferredbuffers.SetTexture(_opengl, 0, 0);
        _deferredbuffers.SetTexture(_opengl, 1, 1);

        _fullscreenwindow.Render(_opengl);

        _opengl.TurnZBufferOn();

        _opengl.EndScene();
        return true;
    }
}

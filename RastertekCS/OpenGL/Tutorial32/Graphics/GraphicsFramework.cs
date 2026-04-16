using RastertekCS.OpenGL.Tutorial32.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class GraphicsFramework
{
    private GL4 _gl;
    private Camera _camera;
    private Model _model,
        _windowModel;
    private RenderTexture _renderTexture;
    private TextureShader _textureShader;
    private GlassShader _glassShader;
    private float _rotation = 360f;

    public bool Initialize(GL4 gl, int sw, int sh)
    {
        _gl = gl;
        _camera = new Camera();
        _camera.SetPosition(0, 0, -5);
        _camera.Render();
        _model = new Model();
        if (
            !_model.Initialize(
                gl,
                "Models/Cube.txt",
                "Data/stone01.tga",
                false,
                "Data/normal03.tga",
                false
            )
        )
            return false;
        _windowModel = new Model();
        if (
            !_windowModel.Initialize(
                gl,
                "Models/square.txt",
                "Data/glass01.tga",
                false,
                "Data/normal03.tga",
                false
            )
        )
            return false;
        _renderTexture = new RenderTexture();
        if (
            !_renderTexture.Initialize(
                gl,
                sw,
                sh,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.ScreenDepth
            )
        )
            return false;
        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(gl))
            return false;
        _glassShader = new GlassShader();
        if (!_glassShader.Initialize(gl))
            return false;
        return true;
    }

    public void Shutdown()
    {
        _glassShader?.Shutdown(_gl);
        _textureShader?.Shutdown(_gl);
        _renderTexture?.Shutdown(_gl);
        _windowModel?.Shutdown(_gl);
        _model?.Shutdown(_gl);
        _gl = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f;
        if (_rotation <= 0)
            _rotation += 360f;
        if (!RenderToTex(_rotation))
            return false;
        return Render(_rotation);
    }

    bool RenderToTex(float rot)
    {
        _renderTexture.SetRenderTarget(_gl);
        _renderTexture.ClearRenderTarget(_gl, 0, 0, 0, 1);
        var w = Matrix4X4.CreateRotationY<float>(rot);
        var v = _camera.GetViewMatrix();
        var p = _gl.GetProjectionMatrix();
        _textureShader.SetShaderParameters(_gl, w, v, p);
        _model.SetTexture1(_gl, 0);
        _model.Render(_gl);
        _gl.SetBackBufferRenderTarget();
        _gl.ResetViewport();
        return true;
    }

    bool Render(float rot)
    {
        _gl.BeginScene(0, 0, 0, 1);
        var v = _camera.GetViewMatrix();
        var p = _gl.GetProjectionMatrix();
        var w = Matrix4X4.CreateRotationY<float>(rot);
        _textureShader.SetShaderParameters(_gl, w, v, p);
        _model.SetTexture1(_gl, 0);
        _model.Render(_gl);
        w = Matrix4X4.CreateTranslation<float>(0, 0, -1.5f);
        _glassShader.SetShaderParameters(_gl, w, v, p, 0.01f);
        _renderTexture.SetTexture(_gl, 2);
        _windowModel.SetTexture1(_gl, 0);
        _windowModel.SetTexture2(_gl, 1);
        _windowModel.Render(_gl);
        _gl.EndScene();
        return true;
    }
}

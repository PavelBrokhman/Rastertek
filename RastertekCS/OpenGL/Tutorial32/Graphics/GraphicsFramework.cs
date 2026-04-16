using RastertekCS.OpenGL.Tutorial32.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private Model _model,
        _windowModel;
    private RenderTexture _renderTexture;
    private TextureShader _textureShader;
    private GlassShader _glassShader;
    private float _rotation = 360f;

    public bool Initialize(GL4 gl, int screenWidth, int screenHeight)
    {
        _driver = gl;
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
                screenWidth,
                screenHeight,
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
        _glassShader?.Shutdown(_driver);
        _textureShader?.Shutdown(_driver);
        _renderTexture?.Shutdown(_driver);
        _windowModel?.Shutdown(_driver);
        _model?.Shutdown(_driver);
        _driver = null;
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
        _renderTexture.SetRenderTarget(_driver);
        _renderTexture.ClearRenderTarget(_driver, 0, 0, 0, 1);
        var w = Matrix4X4.CreateRotationY<float>(rot);
        var v = _camera.GetViewMatrix();
        var p = _driver.GetProjectionMatrix();
        _textureShader.SetShaderParameters(_driver, w, v, p);
        _model.SetTexture1(_driver, 0);
        _model.Render(_driver);
        _driver.SetBackBufferRenderTarget();
        _driver.ResetViewport();
        return true;
    }

    bool Render(float rot)
    {
        _driver.BeginScene(0, 0, 0, 1);
        var v = _camera.GetViewMatrix();
        var p = _driver.GetProjectionMatrix();
        var w = Matrix4X4.CreateRotationY<float>(rot);
        _textureShader.SetShaderParameters(_driver, w, v, p);
        _model.SetTexture1(_driver, 0);
        _model.Render(_driver);
        w = Matrix4X4.CreateTranslation<float>(0, 0, -1.5f);
        _glassShader.SetShaderParameters(_driver, w, v, p, 0.01f);
        _renderTexture.SetTexture(_driver, 2);
        _windowModel.SetTexture1(_driver, 0);
        _windowModel.SetTexture2(_driver, 1);
        _windowModel.Render(_driver);
        _driver.EndScene();
        return true;
    }
}

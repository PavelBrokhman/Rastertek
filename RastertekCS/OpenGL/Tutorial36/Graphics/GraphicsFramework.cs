using RastertekCS.OpenGL.Tutorial36.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial36.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;
    private RenderTexture _renderTexture;
    private OrthoWindow _fullScreenWindow;
    private Blur _blur;
    private BlurShader _blurShader;
    private float _rotation;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;
        _rotation = 360.0f;
        _camera = new Camera();
        _camera.SetPosition(0, 0, -10);
        _camera.Render();
        _camera.RenderBaseViewMatrix();
        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;
        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/Cube.txt", "Data/stone01.tga"))
            return false;
        _renderTexture = new RenderTexture();
        if (
            !_renderTexture.Initialize(
                OpenGL,
                screenWidth,
                screenHeight,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.ScreenDepth
            )
        )
            return false;
        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(OpenGL, screenWidth, screenHeight))
            return false;
        int downSampleWidth = screenWidth / 2;
        int downSampleHeight = screenHeight / 2;
        _blur = new Blur();
        if (
            !_blur.Initialize(
                OpenGL,
                downSampleWidth,
                downSampleHeight,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.ScreenDepth,
                screenWidth,
                screenHeight
            )
        )
            return false;
        _blurShader = new BlurShader();
        if (!_blurShader.Initialize(OpenGL))
            return false;
        return true;
    }

    public void Shutdown()
    {
        _blurShader?.Shutdown(_driver);
        _blur?.Shutdown(_driver);
        _fullScreenWindow?.Shutdown(_driver);
        _renderTexture?.Shutdown(_driver);
        _model?.Shutdown(_driver);
        _textureShader?.Shutdown(_driver);
        _blurShader = null;
        _blur = null;
        _fullScreenWindow = null;
        _renderTexture = null;
        _model = null;
        _textureShader = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation <= 0.0f)
            _rotation += 360.0f;
        if (!RenderSceneToTexture(_rotation))
            return false;
        if (!_blur.BlurTexture(_renderTexture, _driver, _camera, _textureShader, _blurShader))
            return true;
        return Render();
    }

    private bool RenderSceneToTexture(float rotation)
    {
        _renderTexture.SetRenderTarget(_driver);
        _renderTexture.ClearRenderTarget(_driver, 0, 0, 0, 1);
        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _renderTexture.GetProjectionMatrix();
        if (!_textureShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix))
            return false;
        _model.SetTexture(_driver, 0);
        _model.Render(_driver);
        _driver.SetBackBufferRenderTarget();
        _driver.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _driver.BeginScene(0, 0, 0, 1);
        _driver.TurnZBufferOff();
        var worldMatrix = _driver.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _driver.GetOrthoMatrix();
        if (!_textureShader.SetShaderParameters(_driver, worldMatrix, baseViewMatrix, orthoMatrix))
            return false;
        _renderTexture.SetTexture(_driver, 0);
        _fullScreenWindow.Render(_driver);
        _driver.TurnZBufferOn();
        _driver.EndScene();
        return true;
    }
}

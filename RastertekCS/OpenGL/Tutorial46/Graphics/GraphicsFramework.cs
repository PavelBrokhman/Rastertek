using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial46.Graphics;

public class GraphicsFramework
{
    private const float GlowStrength = 2.0f;
    private const float ScreenNear = 0.3f;
    private const float ScreenDepth = 1000.0f;

    private GL4 _driver;
    private Camera _camera;
    private Model _model;
    private RenderTexture _renderTexture;
    private RenderTexture _glowTexture;
    private OrthoWindow _fullScreenWindow;
    private TextureShader _textureShader;
    private BlurShader _blurShader;
    private GlowShader _glowShader;
    private Blur _blur;
    private float _rotation = 360.0f;
    private int _screenWidth, _screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/cube.txt", "Data/stone01.tga", "Data/glowmap001.tga")) return false;

        _renderTexture = new RenderTexture();
        if (!_renderTexture.Initialize(OpenGL, screenWidth, screenHeight, ScreenDepth, ScreenNear)) return false;

        _glowTexture = new RenderTexture();
        if (!_glowTexture.Initialize(OpenGL, screenWidth, screenHeight, ScreenDepth, ScreenNear)) return false;

        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(OpenGL, screenWidth, screenHeight)) return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL)) return false;

        _blurShader = new BlurShader();
        if (!_blurShader.Initialize(OpenGL)) return false;

        _glowShader = new GlowShader();
        if (!_glowShader.Initialize(OpenGL)) return false;

        _blur = new Blur();
        if (!_blur.Initialize(OpenGL, screenWidth / 2, screenHeight / 2, ScreenNear, ScreenDepth, screenWidth, screenHeight)) return false;

        return true;
    }

    public void Shutdown()
    {
        _blur?.Shutdown(_driver); _blur = null;
        _glowShader?.Shutdown(_driver); _glowShader = null;
        _blurShader?.Shutdown(_driver); _blurShader = null;
        _textureShader?.Shutdown(_driver); _textureShader = null;
        _fullScreenWindow?.Shutdown(_driver); _fullScreenWindow = null;
        _glowTexture?.Shutdown(_driver); _glowTexture = null;
        _renderTexture?.Shutdown(_driver); _renderTexture = null;
        _model?.Shutdown(_driver); _model = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 0.5f;
        if (_rotation <= 0.0f) _rotation += 360.0f;

        if (!RenderSceneToTexture(_rotation)) return false;
        if (!RenderGlowToTexture(_rotation)) return false;
        if (!_blur.BlurTexture(_driver, _camera, _glowTexture, _textureShader, _blurShader)) return false;
        return Render();
    }

    private bool RenderSceneToTexture(float rotation)
    {
        _renderTexture.SetRenderTarget(_driver);
        _renderTexture.ClearRenderTarget(_driver, 0, 0, 0, 1);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);

        if (!_textureShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, 0)) return false;
        _model.SetTexture1(_driver, 0);
        _model.Render(_driver);

        _driver.SetBackBufferRenderTarget();
        _driver.ResetViewport();
        return true;
    }

    private bool RenderGlowToTexture(float rotation)
    {
        _glowTexture.SetRenderTarget(_driver);
        _glowTexture.ClearRenderTarget(_driver, 0, 0, 0, 1);

        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        var worldMatrix = Matrix4X4.CreateRotationY<float>(rotation);

        if (!_textureShader.SetShaderParameters(_driver, worldMatrix, viewMatrix, projectionMatrix, 0)) return false;
        _model.SetTexture2(_driver, 0);
        _model.Render(_driver);

        _driver.SetBackBufferRenderTarget();
        _driver.ResetViewport();
        return true;
    }

    private bool Render()
    {
        _driver.BeginScene(0.0f, 0.5f, 0.8f, 1.0f);

        var worldMatrix = _driver.GetWorldMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _driver.GetOrthoMatrix();

        _driver.TurnZBufferOff();
        if (!_glowShader.SetShaderParameters(_driver, worldMatrix, baseViewMatrix, orthoMatrix, 0, 1, GlowStrength)) return false;
        _renderTexture.SetTexture(_driver, 0);
        _glowTexture.SetTexture(_driver, 1);
        _fullScreenWindow.Render(_driver);
        _driver.TurnZBufferOn();

        _driver.EndScene();
        return true;
    }
}

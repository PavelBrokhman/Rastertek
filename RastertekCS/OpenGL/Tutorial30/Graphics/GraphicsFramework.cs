using RastertekCS.OpenGL.Tutorial30.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial30.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _cubeModel,
        _floorModel;
    private TextureShader _textureShader;
    private ReflectionShader _reflectionShader;
    private RenderTexture _renderTexture;
    private float _rotation = 360.0f;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;
        _camera = new Camera();
        _camera.SetPosition(0, 0, -10);
        _camera.Render();

        _cubeModel = new Model();
        if (!_cubeModel.Initialize(OpenGL, "Models/Cube.txt", "Data/stone01.tga", true))
            return false;

        _floorModel = new Model();
        if (!_floorModel.Initialize(OpenGL, "Models/floor.txt", "Data/blue01.tga", true))
            return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;

        _reflectionShader = new ReflectionShader();
        if (!_reflectionShader.Initialize(OpenGL))
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

        return true;
    }

    public void Shutdown()
    {
        _renderTexture?.Shutdown(_openGL);
        _renderTexture = null;
        _reflectionShader?.Shutdown(_openGL);
        _reflectionShader = null;
        _textureShader?.Shutdown(_openGL);
        _textureShader = null;
        _floorModel?.Shutdown(_openGL);
        _floorModel = null;
        _cubeModel?.Shutdown(_openGL);
        _cubeModel = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation < 0.0f)
            _rotation += MathF.Tau;
        if (!RenderReflectionToTexture(_rotation))
            return false;
        return Render(_rotation);
    }

    private bool RenderReflectionToTexture(float rotation)
    {
        _renderTexture.SetRenderTarget(_openGL);
        _renderTexture.ClearRenderTarget(_openGL, 0, 0, 0, 1);

        _camera.RenderReflection(-1.5f);
        var reflectionView = _camera.GetReflectionViewMatrix();
        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var projection = _renderTexture.GetProjectionMatrix();

        _textureShader.SetShaderParameters(_openGL, world, reflectionView, projection);
        _cubeModel.SetTexture(_openGL, 0);
        _cubeModel.Render(_openGL);

        _openGL.SetBackBufferRenderTarget();
        _openGL.ResetViewport();
        return true;
    }

    private bool Render(float rotation)
    {
        _openGL.BeginScene(0, 0, 0, 1);

        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        _textureShader.SetShaderParameters(_openGL, world, view, projection);
        _cubeModel.SetTexture(_openGL, 0);
        _cubeModel.Render(_openGL);

        world = Matrix4X4.CreateTranslation<float>(0, -1.5f, 0);
        var reflectionView = _camera.GetReflectionViewMatrix();

        _reflectionShader.SetShaderParameters(_openGL, world, view, projection, reflectionView);
        _renderTexture.SetTexture(_openGL, 1);
        _floorModel.SetTexture(_openGL, 0);
        _floorModel.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

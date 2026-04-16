using RastertekCS.OpenGL.Tutorial25.System;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial25.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;
    private RenderTexture _renderTexture;
    private DisplayPlane _displayPlane;
    private float _rotation;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        // Create and initialize the camera.
        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        // Create and initialize the model (cube with stone texture).
        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/Cube.txt", "Data/stone01.tga"))
            return false;

        // Create and initialize the texture shader.
        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;

        // Create and initialize the render-to-texture object (256x256).
        _renderTexture = new RenderTexture();
        if (
            !_renderTexture.Initialize(
                OpenGL,
                256,
                256,
                SystemConfiguration.ScreenNear,
                SystemConfiguration.ScreenDepth
            )
        )
            return false;

        // Create and initialize the display plane (1x1 quad).
        _displayPlane = new DisplayPlane();
        if (!_displayPlane.Initialize(OpenGL, 1.0f, 1.0f))
            return false;

        _rotation = 360.0f;

        return true;
    }

    public void Shutdown()
    {
        _displayPlane?.Shutdown(_openGL);
        _displayPlane = null;
        _renderTexture?.Shutdown();
        _renderTexture = null;
        _textureShader?.Shutdown(_openGL);
        _textureShader = null;
        _model?.Shutdown(_openGL);
        _model = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame()
    {
        // Update the rotation variable each frame.
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation <= 0.0f)
            _rotation += 360.0f;

        // First render the scene to the render texture.
        if (!RenderSceneToTexture(_rotation))
            return false;

        // Then render the final graphics scene.
        if (!Render())
            return false;

        return true;
    }

    private bool RenderSceneToTexture(float rotation)
    {
        // Set the render target to the render texture and clear it.
        _renderTexture.SetRenderTarget();
        _renderTexture.ClearRenderTarget(1.0f, 0.5f, 0.0f, 1.0f);

        // Set camera for rendering the cube.
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        // Get matrices.
        var world = Matrix4X4.CreateRotationY<float>(rotation);
        var view = _camera.GetViewMatrix();
        var projection = _renderTexture.GetProjectionMatrix();

        // Set shader parameters and render the model.
        if (!_textureShader.SetShaderParameters(_openGL, world, view, projection, 0))
            return false;

        _model.SetTexture(_openGL, 0);
        _model.Render(_openGL);

        // Reset back to the original back buffer and viewport.
        _openGL.SetBackBufferRenderTarget();
        _openGL.ResetViewport();

        return true;
    }

    private bool Render()
    {
        // Clear the buffers.
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        // Set camera for viewing the display planes.
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        // Get matrices.
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        // Top display plane.
        var world = Matrix4X4.CreateTranslation<float>(0.0f, 1.5f, 0.0f);
        if (!_textureShader.SetShaderParameters(_openGL, world, view, projection, 0))
            return false;
        _renderTexture.SetTexture(0);
        _displayPlane.Render(_openGL);

        // Bottom left display plane.
        world = Matrix4X4.CreateTranslation<float>(-1.5f, -1.5f, 0.0f);
        if (!_textureShader.SetShaderParameters(_openGL, world, view, projection, 0))
            return false;
        _renderTexture.SetTexture(0);
        _displayPlane.Render(_openGL);

        // Bottom right display plane.
        world = Matrix4X4.CreateTranslation<float>(1.5f, -1.5f, 0.0f);
        if (!_textureShader.SetShaderParameters(_openGL, world, view, projection, 0))
            return false;
        _renderTexture.SetTexture(0);
        _displayPlane.Render(_openGL);

        // Present the scene.
        _openGL.EndScene();

        return true;
    }
}

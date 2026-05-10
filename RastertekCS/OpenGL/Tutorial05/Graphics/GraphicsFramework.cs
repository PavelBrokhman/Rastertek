namespace RastertekCS.OpenGL.Tutorial05.Graphics;

public class GraphicsFramework
{
    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;

    public bool Initialize(GL4 OpenGL)
    {
        _openGL = OpenGL;

        // Create and initialize the camera object.
        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        // Create and initialize the model object.
        _model = new Model();
        if (!_model.Initialize(OpenGL, "Data/Stone01.tga", false))
            return false;

        // Create and initialize the texture shader object.
        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _textureShader?.Shutdown(_openGL);
        _model?.Shutdown(_openGL);
        _textureShader = null;
        _model = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        // Clear the buffers to begin the scene.
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        // Get the world, view, and projection matrices from the opengl and camera objects.
        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        // Set the texture shader as the current shader program and set the matrices that it will use for rendering.
        if (!_textureShader.SetShaderParameters(_openGL, world, view, projection))
            return false;

        // Set the texture for the model in the pixel shader.
        _model.SetTexture(_openGL, 0);

        // Render the model.
        _model.Render(_openGL);

        // Present the rendered scene to the screen.
        _openGL.EndScene();
        return true;
    }
}

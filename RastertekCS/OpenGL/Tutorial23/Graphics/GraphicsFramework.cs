using RastertekCS.OpenGL.Tutorial23.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 _openGL;
    private Input _input;
    private Camera _camera;
    private Model _model;
    private Light _light;
    private LightShader _lightShader;
    private FontShader _fontShader;
    private Font _font;
    private Text _renderCountString;
    private Position _position;
    private ModelList _modelList;
    private Frustum _frustum;
    private Matrix4X4<float> _baseViewMatrix;
    private int _screenWidth,
        _screenHeight;

    public bool Initialize(GL4 OpenGL, Input input, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;
        _input = input;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        // Create and initialize the camera.
        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _baseViewMatrix = _camera.GetViewMatrix();

        // Create and initialize the model.
        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/sphere.txt", "Data/stone01.tga", TEXTURE_UNIT))
            return false;

        // Create and initialize the light.
        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);

        // Create and initialize the light shader.
        _lightShader = new LightShader();
        if (!_lightShader.Initialize(OpenGL))
            return false;

        // Create and initialize the font shader.
        _fontShader = new FontShader();
        if (!_fontShader.Initialize(OpenGL))
            return false;

        // Create and initialize the font.
        _font = new Font();
        if (!_font.Initialize(OpenGL, "Data/font01.txt", "Data/font01.tga", TEXTURE_UNIT))
            return false;

        // Create and initialize the render count text.
        _renderCountString = new Text();
        if (
            !_renderCountString.Initialize(
                OpenGL,
                _font,
                "Render Count: 0",
                10,
                10,
                1.0f,
                1.0f,
                1.0f,
                screenWidth,
                screenHeight,
                32
            )
        )
            return false;

        // Create the position object.
        _position = new Position();

        // Create and initialize the model list.
        _modelList = new ModelList();
        _modelList.Initialize(25);

        // Create the frustum object.
        _frustum = new Frustum();

        return true;
    }

    public void Shutdown()
    {
        _frustum = null;
        _modelList?.Shutdown();
        _modelList = null;
        _position = null;
        _renderCountString?.Shutdown(_openGL);
        _renderCountString = null;
        _font?.Shutdown(_openGL);
        _font = null;
        _fontShader?.Shutdown(_openGL);
        _fontShader = null;
        _lightShader?.Shutdown(_openGL);
        _lightShader = null;
        _light = null;
        _model?.Shutdown(_openGL);
        _model = null;
        _camera = null;
        _input = null;
        _openGL = null;
    }

    public bool Frame(RastertekCS.OpenGL.Tutorial23.System.Timer timer)
    {
        // Set the frame time for position calculations.
        _position.SetFrameTime(timer.GetTime());

        // Check if left or right arrow keys are pressed.
        bool leftKey = _input.IsKeyDown(Key.Left);
        _position.TurnLeft(leftKey);

        bool rightKey = _input.IsKeyDown(Key.Right);
        _position.TurnRight(rightKey);

        // Get the current rotation and set the camera.
        float rotationY = _position.GetRotation();
        _camera.SetRotation(0.0f, rotationY, 0.0f);
        _camera.Render();

        return Render();
    }

    private bool Render()
    {
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();
        var ortho = _openGL.GetOrthoMatrix();

        // Construct the frustum.
        _frustum.ConstructFrustum(view, projection);

        // Get light properties.
        var lightDirection = _light.GetDirection();
        var diffuseLightColor = _light.GetDiffuseColor();

        // Get the number of models.
        int modelCount = _modelList.GetModelCount();
        int renderCount = 0;

        // Go through all models and render only those in the frustum.
        for (int i = 0; i < modelCount; i++)
        {
            _modelList.GetData(i, out float posX, out float posY, out float posZ);

            // Check if the sphere (radius 1.0) is in the frustum.
            if (_frustum.CheckSphere(posX, posY, posZ, 1.0f))
            {
                // Create translation matrix for this model position.
                var worldTranslated = Matrix4X4.CreateTranslation(posX, posY, posZ);

                // Set the light shader parameters.
                if (
                    !_lightShader.SetShaderParameters(
                        _openGL,
                        worldTranslated,
                        view,
                        projection,
                        (int)TEXTURE_UNIT,
                        lightDirection,
                        diffuseLightColor
                    )
                )
                    return false;

                // Render the sphere.
                _model.SetTexture(_openGL, TEXTURE_UNIT);
                _model.Render(_openGL);

                renderCount++;
            }
        }

        // Disable Z buffer and enable alpha blending for 2D rendering.
        _openGL.TurnZBufferOff();
        _openGL.EnableAlphaBlending();

        // Disable face culling for text rendering.
        _openGL.Driver.Disable(Silk.NET.OpenGL.EnableCap.CullFace);

        // Update the render count text.
        UpdateRenderCountString(renderCount);

        // Use identity as view matrix for 2D text rendering.
        var identityView = Matrix4X4<float>.Identity;

        // Set font shader parameters and render text.
        _fontShader.SetShader(_openGL);
        _font.SetTexture(_openGL, TEXTURE_UNIT);

        if (
            !_fontShader.SetShaderParameters(
                _openGL,
                world,
                identityView,
                ortho,
                (int)TEXTURE_UNIT,
                _renderCountString.GetPixelColor()
            )
        )
            return false;

        _renderCountString.Render(_openGL);

        // Re-enable face culling.
        _openGL.Driver.Enable(Silk.NET.OpenGL.EnableCap.CullFace);

        // Re-enable Z buffer and disable alpha blending.
        _openGL.TurnZBufferOn();
        _openGL.DisableAlphaBlending();

        _openGL.EndScene();
        return true;
    }

    private void UpdateRenderCountString(int renderCount)
    {
        string text = $"Render Count: {renderCount}";
        _renderCountString.UpdateText(
            _openGL,
            _font,
            text,
            10,
            10,
            1.0f,
            1.0f,
            1.0f,
            _screenWidth,
            _screenHeight
        );
    }
}

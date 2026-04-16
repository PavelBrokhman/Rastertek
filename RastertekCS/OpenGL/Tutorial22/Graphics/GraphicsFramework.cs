using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial22.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT_COLOR = 0;
    private const uint TEXTURE_UNIT_NORMAL = 1;

    private GL4 _openGL;
    private Camera _camera;
    private Model _model;
    private Light _light;
    private ShaderManager _shaderManager;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;

        // Create and initialize the camera.
        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        // Create and initialize the model with color and normal map textures.
        _model = new Model();
        if (
            !_model.Initialize(
                OpenGL,
                "Models/sphere.txt",
                "Data/stone01.tga",
                true,
                "Data/normal01.tga",
                true
            )
        )
            return false;

        // Create and initialize the light.
        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);

        // Create and initialize the shader manager.
        _shaderManager = new ShaderManager();
        if (!_shaderManager.Initialize(OpenGL))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _shaderManager?.Shutdown(_openGL);
        _shaderManager = null;
        _light = null;
        _model?.Shutdown(_openGL);
        _model = null;
        _camera = null;
        _openGL = null;
    }

    private float _rotation = 360.0f;

    public bool Frame()
    {
        _rotation -= 0.0174532925f * 1.0f;
        if (_rotation < 0.0f)
            _rotation += MathF.Tau;
        return Render(_rotation);
    }

    private bool Render(float rotation)
    {
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = _camera.GetViewMatrix();
        var projection = _openGL.GetProjectionMatrix();

        var rotateMatrix = Matrix4X4.CreateRotationY<float>(rotation);

        var lightDirection = _light.GetDirection();
        var diffuseLightColor = _light.GetDiffuseColor();

        // Render sphere 1 (top) - texture shader only.
        var translateMatrix = Matrix4X4.CreateTranslation<float>(0.0f, 1.0f, 0.0f);
        var worldMatrix = rotateMatrix * translateMatrix;

        if (
            !_shaderManager.RenderTextureShader(
                _openGL,
                worldMatrix,
                view,
                projection,
                (int)TEXTURE_UNIT_COLOR
            )
        )
            return false;
        _model.SetTexture1(_openGL, TEXTURE_UNIT_COLOR);
        _model.Render(_openGL);

        // Render sphere 2 (bottom-left) - light shader.
        translateMatrix = Matrix4X4.CreateTranslation<float>(-1.5f, -1.0f, 0.0f);
        worldMatrix = rotateMatrix * translateMatrix;

        if (
            !_shaderManager.RenderLightShader(
                _openGL,
                worldMatrix,
                view,
                projection,
                lightDirection,
                diffuseLightColor,
                (int)TEXTURE_UNIT_COLOR
            )
        )
            return false;
        _model.SetTexture1(_openGL, TEXTURE_UNIT_COLOR);
        _model.Render(_openGL);

        // Render sphere 3 (bottom-right) - normal map shader.
        translateMatrix = Matrix4X4.CreateTranslation<float>(1.5f, -1.0f, 0.0f);
        worldMatrix = rotateMatrix * translateMatrix;

        if (
            !_shaderManager.RenderNormalMapShader(
                _openGL,
                worldMatrix,
                view,
                projection,
                lightDirection,
                diffuseLightColor,
                (int)TEXTURE_UNIT_COLOR,
                (int)TEXTURE_UNIT_NORMAL
            )
        )
            return false;
        _model.SetTexture1(_openGL, TEXTURE_UNIT_COLOR);
        _model.SetTexture2(_openGL, TEXTURE_UNIT_NORMAL);
        _model.Render(_openGL);

        _openGL.EndScene();
        return true;
    }
}

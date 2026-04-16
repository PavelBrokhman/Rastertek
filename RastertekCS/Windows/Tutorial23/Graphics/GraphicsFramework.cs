using RastertekCS.Windows.Tutorial23.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial23.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
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
    private int _screenWidth,
        _screenHeight;

    public bool Initialize(DX11 DirectX, Input input, int screenWidth, int screenHeight)
    {
        _directX = DirectX;
        _input = input;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/sphere.txt", "Data/stone01.tga", true))
            return false;

        _light = new Light();
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        _light.SetDirection(0.0f, 0.0f, 1.0f);

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(DirectX))
            return false;

        _fontShader = new FontShader();
        if (!_fontShader.Initialize(DirectX))
            return false;

        _font = new Font();
        if (!_font.Initialize(DirectX, 0))
            return false;

        _renderCountString = new Text();
        if (
            !_renderCountString.Initialize(
                DirectX,
                screenWidth,
                screenHeight,
                32,
                _font,
                "Render Count: 0",
                10,
                10,
                1.0f,
                1.0f,
                1.0f
            )
        )
            return false;

        _position = new Position();
        _modelList = new ModelList();
        _modelList.Initialize(25);
        _frustum = new Frustum();

        return true;
    }

    public void Shutdown()
    {
        _renderCountString?.Shutdown();
        _font?.Shutdown();
        _fontShader?.Shutdown();
        _lightShader?.Shutdown();
        _model?.Shutdown();
        _frustum = null;
        _modelList?.Shutdown();
        _modelList = null;
        _position = null;
        _renderCountString = null;
        _font = null;
        _fontShader = null;
        _lightShader = null;
        _light = null;
        _model = null;
        _camera = null;
        _input = null;
        _directX = null;
    }

    public bool Frame(float frameTimeMs)
    {
        _position.SetFrameTime(frameTimeMs / 1000.0f);
        _position.TurnLeft(_input.IsKeyDown(Key.Left));
        _position.TurnRight(_input.IsKeyDown(Key.Right));
        _camera.SetRotation(0.0f, _position.GetRotation(), 0.0f);
        _camera.Render();
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();
        var ortho = _directX.GetOrthoMatrix();

        _frustum.ConstructFrustum(view, projection);

        int renderCount = 0;
        int modelCount = _modelList.GetModelCount();
        for (int i = 0; i < modelCount; i++)
        {
            _modelList.GetData(i, out float px, out float py, out float pz);
            if (_frustum.CheckSphere(px, py, pz, 1.0f))
            {
                var worldT = Matrix4X4.CreateTranslation(px, py, pz);
                _model.Render(_directX);
                _model.SetTexture(_directX, 0);
                if (
                    !_lightShader.Render(
                        _directX,
                        _model.GetIndexCount(),
                        worldT,
                        view,
                        projection,
                        _light.GetDirection(),
                        _light.GetDiffuseColor()
                    )
                )
                    return false;
                renderCount++;
            }
        }

        _directX.TurnZBufferOff();
        _directX.EnableAlphaBlending();

        _renderCountString.UpdateText(
            _directX,
            _font,
            $"Render Count: {renderCount}",
            10,
            10,
            1.0f,
            1.0f,
            1.0f
        );
        _font.SetTexture(_directX, 0);
        _renderCountString.Render(_directX);
        if (
            !_fontShader.Render(
                _directX,
                _renderCountString.GetIndexCount(),
                world,
                view,
                ortho,
                _renderCountString.GetPixelColor()
            )
        )
            return false;

        _directX.DisableAlphaBlending();
        _directX.TurnZBufferOn();

        _directX.EndScene();
        return true;
    }
}

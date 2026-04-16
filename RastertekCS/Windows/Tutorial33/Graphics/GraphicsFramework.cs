using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial33.Graphics;

public class GraphicsFramework
{
    private const float SCREEN_DEPTH = 1000.0f;
    private const float SCREEN_NEAR = 0.3f;

    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private Texture _fireTexture;
    private Texture _noiseTexture;
    private Texture _alphaTexture;
    private FireShader _fireShader;
    private float _frameTime;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Models/square.txt", "Data/fire01.tga", false))
            return false;

        _fireTexture = new Texture();
        if (!_fireTexture.Initialize(DirectX, "Data/fire01.tga", false))
            return false;
        _noiseTexture = new Texture();
        if (!_noiseTexture.Initialize(DirectX, "Data/noise01.tga", true))
            return false;
        _alphaTexture = new Texture();
        if (!_alphaTexture.Initialize(DirectX, "Data/alpha01.tga", false))
            return false;

        _fireShader = new FireShader();
        if (!_fireShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _fireShader?.Shutdown();
        _alphaTexture?.Shutdown();
        _noiseTexture?.Shutdown();
        _fireTexture?.Shutdown();
        _model?.Shutdown();
        _fireShader = null;
        _alphaTexture = null;
        _noiseTexture = null;
        _fireTexture = null;
        _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        _frameTime += 0.01f;
        if (_frameTime > 1000.0f)
            _frameTime = 0.0f;
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        float[] scrollSpeeds = { 1.3f, 2.1f, 2.3f };
        float[] scales = { 1.0f, 2.0f, 3.0f };
        float[] d1 = { 0.1f, 0.2f };
        float[] d2 = { 0.1f, 0.3f };
        float[] d3 = { 0.1f, 0.1f };

        _directX.EnableAlphaBlending();

        _model.Render(_directX);
        if (
            !_fireShader.Render(
                _directX,
                _model.GetIndexCount(),
                world,
                view,
                projection,
                _frameTime,
                scrollSpeeds,
                scales,
                d1,
                d2,
                d3,
                0.8f,
                0.5f,
                _fireTexture.GetTextureView(),
                _noiseTexture.GetTextureView(),
                _alphaTexture.GetTextureView()
            )
        )
            return false;

        _directX.DisableAlphaBlending();

        _directX.EndScene();
        return true;
    }
}

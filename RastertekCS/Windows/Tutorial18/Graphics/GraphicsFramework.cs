using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial18.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private LightMapShader _lightMapShader;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (
            !_model.Initialize(
                DirectX,
                "Models/square.txt",
                "Data/stone01.tga",
                "Data/light01.tga",
                true
            )
        )
            return false;

        _lightMapShader = new LightMapShader();
        if (!_lightMapShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _lightMapShader?.Shutdown();
        _model?.Shutdown();
        _lightMapShader = null;
        _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        _model.SetTextures(_directX);

        if (!_lightMapShader.Render(_directX, _model.GetIndexCount(), world, view, projection))
            return false;

        _directX.EndScene();
        return true;
    }
}

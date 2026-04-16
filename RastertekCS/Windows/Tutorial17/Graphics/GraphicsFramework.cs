using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial17.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private MultiTextureShader _multiTextureShader;

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
                "Data/dirt01.tga",
                true
            )
        )
            return false;

        _multiTextureShader = new MultiTextureShader();
        if (!_multiTextureShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _multiTextureShader?.Shutdown();
        _model?.Shutdown();
        _multiTextureShader = null;
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

        if (!_multiTextureShader.Render(_directX, _model.GetIndexCount(), world, view, projection))
            return false;

        _directX.EndScene();
        return true;
    }
}

namespace RastertekCS.Windows.Tutorial05.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);

        _model = new Model();
        if (!_model.Initialize(DirectX, "Data/Stone01.tga", true))
            return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _textureShader?.Shutdown();
        _model?.Shutdown();
        _textureShader = null;
        _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        _camera.Render();

        var world = _directX.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        _model.SetTexture(_directX, 0);

        if (!_textureShader.Render(_directX, _model.GetIndexCount(), world, view, projection))
            return false;

        _directX.EndScene();
        return true;
    }
}

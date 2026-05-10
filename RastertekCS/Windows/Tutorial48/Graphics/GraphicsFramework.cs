namespace RastertekCS.Windows.Tutorial48.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model;
    private TextureShader _textureShader;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model = new Model();
        if (!_model.Initialize(DirectX, "Data/stone01.tga")) return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        _textureShader?.Shutdown(); _textureShader = null;
        _model?.Shutdown(); _model = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame()
    {
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);
        var worldMatrix = _directX.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _directX.GetProjectionMatrix();

        _model.Render(_directX);
        if (!_textureShader.Render(_directX, _model.GetVertexCount(), _model.GetInstanceCount(),
                worldMatrix, viewMatrix, projectionMatrix, _model.GetTextureView())) return false;

        _directX.EndScene();
        return true;
    }
}

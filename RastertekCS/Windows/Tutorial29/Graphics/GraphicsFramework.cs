using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial29.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private Model _model1;
    private Model _model2;
    private TextureShader _textureShader;
    private TransparentShader _transparentShader;

    public bool Initialize(DX11 DirectX)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -5.0f);
        _camera.Render();

        _model1 = new Model();
        if (!_model1.Initialize(DirectX, "Models/square.txt", "Data/dirt01.tga", true))
            return false;

        _model2 = new Model();
        if (!_model2.Initialize(DirectX, "Models/square.txt", "Data/stone01.tga", true))
            return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(DirectX))
            return false;

        _transparentShader = new TransparentShader();
        if (!_transparentShader.Initialize(DirectX))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _transparentShader?.Shutdown();
        _textureShader?.Shutdown();
        _model2?.Shutdown();
        _model1?.Shutdown();
        _transparentShader = null;
        _textureShader = null;
        _model2 = null;
        _model1 = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _directX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var view = _camera.GetViewMatrix();
        var projection = _directX.GetProjectionMatrix();

        var world1 = _directX.GetWorldMatrix();
        _model1.Render(_directX);
        if (
            !_textureShader.Render(
                _directX,
                _model1.GetIndexCount(),
                world1,
                view,
                projection,
                _model1.GetTextureView()
            )
        )
            return false;

        var world2 = Matrix4X4.CreateTranslation(1.0f, 0.0f, -1.0f);
        _directX.EnableAlphaBlending();
        _model2.Render(_directX);
        if (
            !_transparentShader.Render(
                _directX,
                _model2.GetIndexCount(),
                world2,
                view,
                projection,
                _model2.GetTextureView(),
                0.5f
            )
        )
            return false;
        _directX.DisableAlphaBlending();

        _directX.EndScene();
        return true;
    }
}

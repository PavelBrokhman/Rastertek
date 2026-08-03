namespace RastertekCS.Windows.Tutorial54.Graphics;

public class GraphicsFramework
{
    private DX11 _directX;
    private Camera _camera;
    private OrthoWindow _fullScreenWindow;
    private ParallaxScroll _parallaxForest;
    private ScrollShader _scrollShader;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight)
    {
        _directX = DirectX;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();

        _fullScreenWindow = new OrthoWindow();
        if (!_fullScreenWindow.Initialize(DirectX, screenWidth, screenHeight)) return false;

        _parallaxForest = new ParallaxScroll();
        if (!_parallaxForest.Initialize(DirectX, "Data/parallax_config.txt")) return false;

        _scrollShader = new ScrollShader();
        if (!_scrollShader.Initialize(DirectX)) return false;

        return true;
    }

    public void Shutdown()
    {
        _scrollShader?.Shutdown(); _scrollShader = null;
        _parallaxForest?.Shutdown(); _parallaxForest = null;
        _fullScreenWindow?.Shutdown(); _fullScreenWindow = null;
        _camera = null;
        _directX = null;
    }

    public bool Frame(float frameTime)
    {
        _parallaxForest.Frame(frameTime);

        _directX.BeginScene(1.0f, 0.0f, 0.0f, 1.0f);

        var world = _directX.GetWorldMatrix();
        var baseView = _camera.GetBaseViewMatrix();
        var ortho = _directX.GetOrthoMatrix();

        _directX.EnableAlphaBlending();
        _directX.TurnZBufferOff();

        int count = _parallaxForest.GetTextureCount();
        for (int i = 0; i < count; i++)
        {
            _fullScreenWindow.Render(_directX);

            if (!_scrollShader.Render(_directX, _fullScreenWindow.GetIndexCount(), world, baseView, ortho,
                _parallaxForest.GetTexture(i), _parallaxForest.GetTranslation(i))) return false;
        }

        _directX.TurnZBufferOn();
        _directX.DisableAlphaBlending();
        _directX.EndScene();
        return true;
    }
}

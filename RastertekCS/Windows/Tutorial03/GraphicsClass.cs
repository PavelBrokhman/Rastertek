////////////////////////////////////////////////////////////////////////////////
// Filename: GraphicsClass.cs
////////////////////////////////////////////////////////////////////////////////
namespace RastertekCS.Windows.Tutorial03;

public class GraphicsClass
{
    /////////////
    // GLOBALS //
    /////////////
    public const bool FULL_SCREEN = false;
    public const bool VSYNC_ENABLED = true;
    public const float SCREEN_DEPTH = 1000.0f;
    public const float SCREEN_NEAR = 0.1f;

    private DirectXClass _directX;

    public bool Initialize(DirectXClass DirectX)
    {
        _directX = DirectX;
        return true;
    }

    public void Shutdown()
    {
        _directX = null;
    }

    public bool Frame()
    {
        return Render();
    }

    private bool Render()
    {
        _directX.BeginScene(0.5f, 0.5f, 0.5f, 1.0f);

        _directX.EndScene();

        return true;
    }
}

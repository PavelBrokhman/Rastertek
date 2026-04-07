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

    private DirectXClass m_DirectX;

    public bool Initialize(DirectXClass DirectX)
    {
        m_DirectX = DirectX;
        return true;
    }

    public void Shutdown()
    {
        m_DirectX = null;
    }

    public bool Frame()
    {
        return Render();
    }

    private bool Render()
    {
        m_DirectX.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        m_DirectX.EndScene();

        return true;
    }
}

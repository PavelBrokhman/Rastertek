////////////////////////////////////////////////////////////////////////////////
// Filename: GraphicsClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Windowing;

namespace RastertekCS.Windows.Tutorial02;

public class GraphicsClass
{
    /////////////
    // GLOBALS //
    /////////////
    public const bool FULL_SCREEN = false;
    public const bool VSYNC_ENABLED = true;
    public const float SCREEN_DEPTH = 1000.0f;
    public const float SCREEN_NEAR = 0.1f;

    public bool Initialize(DirectXClass DirectX, IWindow window)
    {
        return true;
    }

    public void Shutdown()
    {
    }

    public bool Frame()
    {
        return true;
    }

    private bool Render()
    {
        return true;
    }
}

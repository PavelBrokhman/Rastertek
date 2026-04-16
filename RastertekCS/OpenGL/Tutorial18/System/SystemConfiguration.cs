namespace RastertekCS.OpenGL.Tutorial18.System;

public class SystemConfiguration
{
    public static bool FullScreen { get; }
    public static bool VerticalSyncEnabled { get; }
    public static float ScreenDepth { get; }
    public static float ScreenNear { get; }

    static SystemConfiguration()
    {
        FullScreen = false;
        VerticalSyncEnabled = true;
        ScreenDepth = 1000.0f;
        ScreenNear = 0.1f;
    }
}

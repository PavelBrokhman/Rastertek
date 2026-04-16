namespace RastertekCS.Windows.Tutorial19.System;

public class SystemConfiguration
{
    public string Title { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool WaitVerticalBlanking { get; set; }

    public SystemConfiguration()
        : this("DirectX Tutorial") { }

    public SystemConfiguration(string title)
        : this(title, 800, 600) { }

    public SystemConfiguration(string title, int width, int height)
    {
        Title = title;
        Width = width;
        Height = height;
        WaitVerticalBlanking = false;
    }

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

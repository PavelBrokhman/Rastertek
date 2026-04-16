using System.Diagnostics;

namespace RastertekCS.OpenGL.Tutorial15.System;

public class Fps
{
    private int _fps;
    private int _count;
    private long _startTicks;
    private readonly Stopwatch _stopwatch = new();

    public void Initialize()
    {
        _fps = 0;
        _count = 0;
        _stopwatch.Start();
        _startTicks = _stopwatch.ElapsedTicks;
    }

    public void Frame()
    {
        _count++;
        long current = _stopwatch.ElapsedTicks;
        float elapsed = (float)(current - _startTicks) / Stopwatch.Frequency;
        if (elapsed >= 1.0f)
        {
            _fps = _count;
            _count = 0;
            _startTicks = current;
        }
    }

    public int GetFps() => _fps;
}

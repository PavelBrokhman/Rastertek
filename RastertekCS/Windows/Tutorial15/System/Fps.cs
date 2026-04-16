using System.Diagnostics;

namespace RastertekCS.Windows.Tutorial15.System;

public class Fps
{
    private int _fps;
    private int _count;
    private long _startMs;
    private readonly Stopwatch _stopwatch = new();

    public void Initialize()
    {
        _fps = 0;
        _count = 0;
        _stopwatch.Start();
        _startMs = _stopwatch.ElapsedMilliseconds;
    }

    public void Frame()
    {
        _count++;
        if (_stopwatch.ElapsedMilliseconds >= _startMs + 1000)
        {
            _fps = _count;
            _count = 0;
            _startMs = _stopwatch.ElapsedMilliseconds;
        }
    }

    public int GetFps() => _fps;
}

using System.Diagnostics;

namespace RastertekCS.Windows.Tutorial59.Graphics;

public class Timer
{
    private readonly Stopwatch _stopwatch = new();
    private long _previousTicks;
    private float _frameTime;

    public void Initialize()
    {
        _stopwatch.Start();
        _previousTicks = _stopwatch.ElapsedTicks;
        _frameTime = 0;
    }

    public void Frame()
    {
        long current = _stopwatch.ElapsedTicks;
        long delta = current - _previousTicks;
        _previousTicks = current;
        _frameTime = (float)delta / Stopwatch.Frequency * 1000.0f;
    }

    public float GetTime() => _frameTime;
}

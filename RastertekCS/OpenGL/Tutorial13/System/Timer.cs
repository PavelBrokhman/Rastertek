using System.Diagnostics;

namespace RastertekCS.OpenGL.Tutorial13.System;

public class Timer
{
    private readonly Stopwatch m_stopwatch = new();
    private long m_previousTicks;
    private float m_frameTime;

    public void Initialize()
    {
        m_stopwatch.Start();
        m_previousTicks = m_stopwatch.ElapsedTicks;
        m_frameTime = 0;
    }

    public void Frame()
    {
        long current = m_stopwatch.ElapsedTicks;
        long delta = current - m_previousTicks;
        m_previousTicks = current;
        m_frameTime = (float)delta / Stopwatch.Frequency * 1000.0f;
    }

    public float GetTime() => m_frameTime / 1000.0f;

    public int GetFrameTime() => (int)m_frameTime;
}

using System.Diagnostics;

namespace RastertekCS.OpenGL.Tutorial55.Graphics;

public class Timer
{
    private Stopwatch m_stopwatch;
    private long m_lastTick;
    private float m_frameTime;

    public void Initialize()
    {
        m_stopwatch = Stopwatch.StartNew();
        m_lastTick = m_stopwatch.ElapsedMilliseconds;
    }

    public void Frame()
    {
        long current = m_stopwatch.ElapsedMilliseconds;
        m_frameTime = (current - m_lastTick) / 1000.0f;
        m_lastTick = current;
    }

    public float GetTime() => m_frameTime;
}

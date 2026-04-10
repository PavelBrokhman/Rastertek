using System.Diagnostics;

namespace RastertekCS.Windows.Tutorial15.System;

public class Fps
{
    private int m_fps;
    private int m_count;
    private long m_startMs;
    private readonly Stopwatch m_stopwatch = new();

    public void Initialize()
    {
        m_fps = 0;
        m_count = 0;
        m_stopwatch.Start();
        m_startMs = m_stopwatch.ElapsedMilliseconds;
    }

    public void Frame()
    {
        m_count++;
        if (m_stopwatch.ElapsedMilliseconds >= m_startMs + 1000)
        {
            m_fps = m_count;
            m_count = 0;
            m_startMs = m_stopwatch.ElapsedMilliseconds;
        }
    }

    public int GetFps() => m_fps;
}

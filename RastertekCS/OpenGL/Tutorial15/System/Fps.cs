using System.Diagnostics;

namespace RastertekCS.OpenGL.Tutorial15.System;

public class Fps
{
    private int m_fps;
    private int m_count;
    private long m_startTicks;
    private readonly Stopwatch m_stopwatch = new();

    public void Initialize()
    {
        m_fps = 0;
        m_count = 0;
        m_stopwatch.Start();
        m_startTicks = m_stopwatch.ElapsedTicks;
    }

    public void Frame()
    {
        m_count++;
        long current = m_stopwatch.ElapsedTicks;
        float elapsed = (float)(current - m_startTicks) / Stopwatch.Frequency;
        if (elapsed >= 1.0f)
        {
            m_fps = m_count;
            m_count = 0;
            m_startTicks = current;
        }
    }

    public int GetFps() => m_fps;
}

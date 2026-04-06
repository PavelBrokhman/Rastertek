using System.Diagnostics;
namespace RastertekCS.OpenGL.Tutorial34.Graphics;
public class Timer
{
    private Stopwatch m_sw = new();
    private long m_lastTicks;
    private float m_frameTime;
    public void Initialize() { m_sw.Start(); m_lastTicks = m_sw.ElapsedTicks; }
    public void Frame() { long cur = m_sw.ElapsedTicks; m_frameTime = (float)(cur - m_lastTicks) / Stopwatch.Frequency; m_lastTicks = cur; }
    public float GetTime() => m_frameTime;
}

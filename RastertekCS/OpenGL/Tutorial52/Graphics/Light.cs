namespace RastertekCS.OpenGL.Tutorial52.Graphics;

public class Light
{
    private float[] m_direction = new float[3];
    public void SetDirection(float x, float y, float z) { m_direction[0] = x; m_direction[1] = y; m_direction[2] = z; }
    public float[] GetDirection() => new[] { m_direction[0], m_direction[1], m_direction[2] };
}

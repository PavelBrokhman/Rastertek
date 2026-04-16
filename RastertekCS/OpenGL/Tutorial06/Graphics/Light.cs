namespace RastertekCS.OpenGL.Tutorial06.Graphics;

public class Light
{
    private readonly float[] m_diffuseColor = new float[4];
    private readonly float[] m_direction = new float[3];

    public void SetDiffuseColor(float r, float g, float b, float a)
    {
        m_diffuseColor[0] = r;
        m_diffuseColor[1] = g;
        m_diffuseColor[2] = b;
        m_diffuseColor[3] = a;
    }

    public void SetDirection(float x, float y, float z)
    {
        m_direction[0] = x;
        m_direction[1] = y;
        m_direction[2] = z;
    }

    public float[] GetDiffuseColor() => m_diffuseColor;

    public float[] GetDirection() => m_direction;
}

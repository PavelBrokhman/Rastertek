namespace RastertekCS.Windows.Tutorial11.Graphics;

// Point light: position + diffuse color.
public class Light
{
    private readonly float[] m_diffuseColor = new float[4];
    private readonly float[] m_position = new float[3];

    public void SetDiffuseColor(float r, float g, float b, float a)
    {
        m_diffuseColor[0] = r;
        m_diffuseColor[1] = g;
        m_diffuseColor[2] = b;
        m_diffuseColor[3] = a;
    }

    public void SetPosition(float x, float y, float z)
    {
        m_position[0] = x;
        m_position[1] = y;
        m_position[2] = z;
    }

    public float[] GetDiffuseColor() => m_diffuseColor;

    public float[] GetPosition() => m_position;
}

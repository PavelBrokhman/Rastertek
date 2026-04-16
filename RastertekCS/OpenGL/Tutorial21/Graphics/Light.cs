namespace RastertekCS.OpenGL.Tutorial21.Graphics;

public class Light
{
    private float[] m_diffuseColor = new float[4];
    private float[] m_direction = new float[3];
    private float[] m_specularColor = new float[4];
    private float m_specularPower;

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

    public void SetSpecularColor(float r, float g, float b, float a)
    {
        m_specularColor[0] = r;
        m_specularColor[1] = g;
        m_specularColor[2] = b;
        m_specularColor[3] = a;
    }

    public void SetSpecularPower(float power)
    {
        m_specularPower = power;
    }

    public float[] GetDiffuseColor() => (float[])m_diffuseColor.Clone();

    public float[] GetDirection() => (float[])m_direction.Clone();

    public float[] GetSpecularColor() => (float[])m_specularColor.Clone();

    public float GetSpecularPower() => m_specularPower;
}

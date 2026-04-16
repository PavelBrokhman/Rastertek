namespace RastertekCS.OpenGL.Tutorial10.Graphics;

public class Light
{
    private readonly float[] m_ambientLight = new float[4];
    private readonly float[] m_diffuseColor = new float[4];
    private readonly float[] m_direction = new float[3];
    private readonly float[] m_specularColor = new float[4];
    private float m_specularPower;

    public void SetAmbientLight(float r, float g, float b, float a)
    {
        m_ambientLight[0] = r;
        m_ambientLight[1] = g;
        m_ambientLight[2] = b;
        m_ambientLight[3] = a;
    }

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

    public float[] GetAmbientLight() => m_ambientLight;

    public float[] GetDiffuseColor() => m_diffuseColor;

    public float[] GetDirection() => m_direction;

    public float[] GetSpecularColor() => m_specularColor;

    public float GetSpecularPower() => m_specularPower;
}

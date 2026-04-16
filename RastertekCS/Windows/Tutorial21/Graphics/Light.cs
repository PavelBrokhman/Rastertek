namespace RastertekCS.Windows.Tutorial21.Graphics;

public class Light
{
    private readonly float[] _diffuseColor = new float[4];
    private readonly float[] _direction = new float[3];
    private readonly float[] _specularColor = new float[4];
    private float _specularPower;

    public void SetDiffuseColor(float r, float g, float b, float a)
    {
        _diffuseColor[0] = r;
        _diffuseColor[1] = g;
        _diffuseColor[2] = b;
        _diffuseColor[3] = a;
    }

    public void SetDirection(float x, float y, float z)
    {
        _direction[0] = x;
        _direction[1] = y;
        _direction[2] = z;
    }

    public void SetSpecularColor(float r, float g, float b, float a)
    {
        _specularColor[0] = r;
        _specularColor[1] = g;
        _specularColor[2] = b;
        _specularColor[3] = a;
    }

    public void SetSpecularPower(float power)
    {
        _specularPower = power;
    }

    public float[] GetDiffuseColor() => _diffuseColor;

    public float[] GetDirection() => _direction;

    public float[] GetSpecularColor() => _specularColor;

    public float GetSpecularPower() => _specularPower;
}

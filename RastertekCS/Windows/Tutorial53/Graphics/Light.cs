namespace RastertekCS.Windows.Tutorial53.Graphics;

public class Light
{
    private readonly float[] _ambientColor = new float[4];
    private readonly float[] _diffuseColor = new float[4];
    private readonly float[] _direction = new float[3];

    public void SetAmbientColor(float r, float g, float b, float a)
    {
        _ambientColor[0] = r;
        _ambientColor[1] = g;
        _ambientColor[2] = b;
        _ambientColor[3] = a;
    }

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

    public float[] GetAmbientColor() => _ambientColor;

    public float[] GetDiffuseColor() => _diffuseColor;

    public float[] GetDirection() => _direction;
}

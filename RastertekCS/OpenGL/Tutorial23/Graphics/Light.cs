namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class Light
{
    private float[] _diffuseColor = new float[4];
    private float[] _direction = new float[3];

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

    public float[] GetDiffuseColor() => _diffuseColor;

    public float[] GetDirection() => _direction;
}

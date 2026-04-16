namespace RastertekCS.OpenGL.Tutorial09.Graphics;

public class Light
{
    private readonly float[] _ambientLight = new float[4];
    private readonly float[] _diffuseColor = new float[4];
    private readonly float[] _direction = new float[3];

    public void SetAmbientLight(float r, float g, float b, float a)
    {
        _ambientLight[0] = r;
        _ambientLight[1] = g;
        _ambientLight[2] = b;
        _ambientLight[3] = a;
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

    public float[] GetAmbientLight() => _ambientLight;

    public float[] GetDiffuseColor() => _diffuseColor;

    public float[] GetDirection() => _direction;
}

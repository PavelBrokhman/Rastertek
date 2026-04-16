namespace RastertekCS.OpenGL.Tutorial11.Graphics;

// Point light: position + diffuse color (no direction for this tutorial).
public class Light
{
    private readonly float[] _diffuseColor = new float[4];
    private readonly float[] _position = new float[3];

    public void SetDiffuseColor(float r, float g, float b, float a)
    {
        _diffuseColor[0] = r;
        _diffuseColor[1] = g;
        _diffuseColor[2] = b;
        _diffuseColor[3] = a;
    }

    public void SetPosition(float x, float y, float z)
    {
        _position[0] = x;
        _position[1] = y;
        _position[2] = z;
    }

    public float[] GetDiffuseColor() => _diffuseColor;

    public float[] GetPosition() => _position;
}

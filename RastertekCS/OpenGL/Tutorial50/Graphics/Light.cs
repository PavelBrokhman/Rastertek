namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class Light
{
    private float[] _diffusecolor = new float[4];
    private float[] _direction = new float[3];

    public void SetDiffuseColor(float r, float g, float b, float a) { _diffusecolor[0] = r; _diffusecolor[1] = g; _diffusecolor[2] = b; _diffusecolor[3] = a; }
    public void SetDirection(float x, float y, float z) { _direction[0] = x; _direction[1] = y; _direction[2] = z; }

    public float[] GetDirection() => new[] { _direction[0], _direction[1], _direction[2] };
}

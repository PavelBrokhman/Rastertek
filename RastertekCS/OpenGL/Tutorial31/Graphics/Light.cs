namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class Light
{
    public float[] AmbientLight { get; private set; } = new float[4];
    public float[] DiffuseColor { get; private set; } = new float[4];
    public float[] Direction { get; private set; } = new float[3];

    public void SetAmbientLight(float r, float g, float b, float a)
    {
        AmbientLight[0] = r;
        AmbientLight[1] = g;
        AmbientLight[2] = b;
        AmbientLight[3] = a;
    }

    public void SetDiffuseColor(float r, float g, float b, float a)
    {
        DiffuseColor[0] = r;
        DiffuseColor[1] = g;
        DiffuseColor[2] = b;
        DiffuseColor[3] = a;
    }

    public void SetDirection(float x, float y, float z)
    {
        Direction[0] = x;
        Direction[1] = y;
        Direction[2] = z;
    }
}

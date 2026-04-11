using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial31.Graphics;

public class Light
{
    public Vector4D<float> AmbientColor { get; private set; }
    public Vector4D<float> DiffuseColor { get; private set; }
    public Vector3D<float> Direction { get; private set; }

    public void SetAmbientColor(float r, float g, float b, float a) => AmbientColor = new Vector4D<float>(r, g, b, a);
    public void SetDiffuseColor(float r, float g, float b, float a) => DiffuseColor = new Vector4D<float>(r, g, b, a);
    public void SetDirection(float x, float y, float z) => Direction = new Vector3D<float>(x, y, z);
}

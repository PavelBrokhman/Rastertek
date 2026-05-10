using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial40.Graphics;

public class Light
{
    private Vector4D<float> _ambientColor;
    private Vector4D<float> _diffuseColor;
    private Vector3D<float> _position;

    public void SetAmbientLight(float r, float g, float b, float a) => _ambientColor = new Vector4D<float>(r, g, b, a);
    public void SetDiffuseColor(float r, float g, float b, float a) => _diffuseColor = new Vector4D<float>(r, g, b, a);
    public void SetPosition(float x, float y, float z) => _position = new Vector3D<float>(x, y, z);
    public Vector4D<float> GetAmbientLight() => _ambientColor;
    public Vector4D<float> GetDiffuseColor() => _diffuseColor;
    public Vector3D<float> GetPosition() => _position;
}

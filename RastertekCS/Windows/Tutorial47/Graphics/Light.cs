using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial47.Graphics;

public class Light
{
    private Vector4D<float> _ambientColor;
    private Vector4D<float> _diffuseColor;
    private Vector3D<float> _direction;
    private Vector3D<float> _position;
    private Vector3D<float> _lookAt;
    private Matrix4X4<float> _viewMatrix;
    private Matrix4X4<float> _orthoMatrix;

    public void SetAmbientLight(float r, float g, float b, float a) => _ambientColor = new Vector4D<float>(r, g, b, a);
    public void SetDiffuseColor(float r, float g, float b, float a) => _diffuseColor = new Vector4D<float>(r, g, b, a);
    public void SetDirection(float x, float y, float z) => _direction = new Vector3D<float>(x, y, z);
    public void SetPosition(float x, float y, float z) => _position = new Vector3D<float>(x, y, z);
    public void SetLookAt(float x, float y, float z) => _lookAt = new Vector3D<float>(x, y, z);

    public Vector4D<float> GetAmbientLight() => _ambientColor;
    public Vector4D<float> GetDiffuseColor() => _diffuseColor;
    public Vector3D<float> GetDirection() => _direction;
    public Vector3D<float> GetPosition() => _position;
    public Matrix4X4<float> GetViewMatrix() => _viewMatrix;
    public Matrix4X4<float> GetOrthoMatrix() => _orthoMatrix;

    public void GenerateViewMatrix()
    {
        var up = new Vector3D<float>(0, 1, 0);
        _viewMatrix = LookAtLH(_position, _lookAt, up);
    }

    public void GenerateOrthoMatrix(float width, float screenNear, float screenDepth)
    {
        float w = 2.0f / width;
        float h = 2.0f / width;
        float r = 1.0f / (screenDepth - screenNear);
        _orthoMatrix = new Matrix4X4<float>(
            w, 0, 0, 0,
            0, h, 0, 0,
            0, 0, r, 0,
            0, 0, -r * screenNear, 1
        );
    }

    private static Matrix4X4<float> LookAtLH(Vector3D<float> eye, Vector3D<float> target, Vector3D<float> up)
    {
        var zAxis = Vector3D.Normalize(target - eye);
        var xAxis = Vector3D.Normalize(Vector3D.Cross(up, zAxis));
        var yAxis = Vector3D.Cross(zAxis, xAxis);
        return new Matrix4X4<float>(
            xAxis.X, yAxis.X, zAxis.X, 0,
            xAxis.Y, yAxis.Y, zAxis.Y, 0,
            xAxis.Z, yAxis.Z, zAxis.Z, 0,
            -Vector3D.Dot(xAxis, eye), -Vector3D.Dot(yAxis, eye), -Vector3D.Dot(zAxis, eye), 1
        );
    }
}

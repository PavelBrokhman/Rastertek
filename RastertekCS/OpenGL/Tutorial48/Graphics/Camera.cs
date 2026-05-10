using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial48.Graphics;

public class Camera
{
    private float _positionX,
        _positionY,
        _positionZ;
    private float _rotationX,
        _rotationY,
        _rotationZ;
    private Matrix4X4<float> _viewMatrix;

    public void SetPosition(float x, float y, float z) { _positionX = x; _positionY = y; _positionZ = z; }
    public void SetRotation(float x, float y, float z) { _rotationX = x; _rotationY = y; _rotationZ = z; }

    public void Render()
    {
        var up = new Vector3D<float>(0, 1, 0);
        var position = new Vector3D<float>(_positionX, _positionY, _positionZ);
        var lookAt = new Vector3D<float>(0, 0, 1);
        var rot = Matrix4X4.CreateFromYawPitchRoll(
            _rotationY * (MathF.PI / 180.0f),
            _rotationX * (MathF.PI / 180.0f),
            _rotationZ * (MathF.PI / 180.0f)
        );
        lookAt = Vector3D.Transform(lookAt, rot);
        up = Vector3D.Transform(up, rot);
        lookAt = position + lookAt;
        _viewMatrix = LookAtLH(position, lookAt, up);
    }

    public Matrix4X4<float> GetViewMatrix() => _viewMatrix;

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

using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial40.Graphics;

public class ViewPoint
{
    private Vector3D<float> _position;
    private Vector3D<float> _lookAt;
    private float _fieldOfView,
        _aspectRatio,
        _nearPlane,
        _farPlane;
    private Matrix4X4<float> _viewMatrix;
    private Matrix4X4<float> _projectionMatrix;

    public void SetPosition(float x, float y, float z) => _position = new Vector3D<float>(x, y, z);

    public void SetLookAt(float x, float y, float z) => _lookAt = new Vector3D<float>(x, y, z);

    public void SetProjectionParameters(float fieldOfView, float aspectRatio, float nearPlane, float farPlane)
    {
        _fieldOfView = fieldOfView;
        _aspectRatio = aspectRatio;
        _nearPlane = nearPlane;
        _farPlane = farPlane;
    }

    public void GenerateViewMatrix()
    {
        var up = new Vector3D<float>(0, 1, 0);
        _viewMatrix = LookAtLH(_position, _lookAt, up);
    }

    public void GenerateProjectionMatrix()
    {
        float h = 1.0f / MathF.Tan(_fieldOfView * 0.5f);
        float w = h / _aspectRatio;
        float range = _farPlane / (_farPlane - _nearPlane);
        _projectionMatrix = new Matrix4X4<float>(w, 0, 0, 0, 0, h, 0, 0, 0, 0, range, 1, 0, 0, -range * _nearPlane, 0);
    }

    public Matrix4X4<float> GetViewMatrix() => _viewMatrix;
    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

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

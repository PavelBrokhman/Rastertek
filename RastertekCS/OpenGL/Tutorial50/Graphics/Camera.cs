using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class Camera
{
    private float _positionx, _positiony, _positionz;
    private float _rotationx, _rotationy, _rotationz;
    private Matrix4X4<float> _viewmatrix;
    private Matrix4X4<float> _baseviewmatrix;

    public void SetPosition(float x, float y, float z) { _positionx = x; _positiony = y; _positionz = z; }
    public void SetRotation(float x, float y, float z) { _rotationx = x; _rotationy = y; _rotationz = z; }

    public void Render()
    {
        var up = new Vector3D<float>(0, 1, 0);
        var position = new Vector3D<float>(_positionx, _positiony, _positionz);
        var lookAt = new Vector3D<float>(0, 0, 1);
        var rot = Matrix4X4.CreateFromYawPitchRoll(
            _rotationy * (MathF.PI / 180.0f),
            _rotationx * (MathF.PI / 180.0f),
            _rotationz * (MathF.PI / 180.0f));
        lookAt = Vector3D.Transform(lookAt, rot);
        up = Vector3D.Transform(up, rot);
        lookAt = position + lookAt;
        _viewmatrix = LookAtLH(position, lookAt, up);
    }

    public void RenderBaseViewMatrix()
    {
        var up = new Vector3D<float>(0, 1, 0);
        var position = new Vector3D<float>(_positionx, _positiony, _positionz);
        var lookAt = new Vector3D<float>(0, 0, 1);
        var rot = Matrix4X4.CreateFromYawPitchRoll(
            _rotationy * (MathF.PI / 180.0f),
            _rotationx * (MathF.PI / 180.0f),
            _rotationz * (MathF.PI / 180.0f));
        lookAt = Vector3D.Transform(lookAt, rot);
        up = Vector3D.Transform(up, rot);
        lookAt = position + lookAt;
        _baseviewmatrix = LookAtLH(position, lookAt, up);
    }

    public Matrix4X4<float> GetViewMatrix() => _viewmatrix;
    public Matrix4X4<float> GetBaseViewMatrix() => _baseviewmatrix;

    private static Matrix4X4<float> LookAtLH(Vector3D<float> eye, Vector3D<float> target, Vector3D<float> up)
    {
        var zAxis = Vector3D.Normalize(target - eye);
        var xAxis = Vector3D.Normalize(Vector3D.Cross(up, zAxis));
        var yAxis = Vector3D.Cross(zAxis, xAxis);
        return new Matrix4X4<float>(
            xAxis.X, yAxis.X, zAxis.X, 0,
            xAxis.Y, yAxis.Y, zAxis.Y, 0,
            xAxis.Z, yAxis.Z, zAxis.Z, 0,
            -Vector3D.Dot(xAxis, eye), -Vector3D.Dot(yAxis, eye), -Vector3D.Dot(zAxis, eye), 1);
    }
}

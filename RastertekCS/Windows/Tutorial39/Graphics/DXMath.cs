using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial39.Graphics;

public static class DXMath
{
    public static Matrix4X4<float> LookAtLH(
        Vector3D<float> eye,
        Vector3D<float> target,
        Vector3D<float> up
    )
    {
        var zAxis = Vector3D.Normalize(target - eye);
        var xAxis = Vector3D.Normalize(Vector3D.Cross(up, zAxis));
        var yAxis = Vector3D.Cross(zAxis, xAxis);
        return new Matrix4X4<float>(
            xAxis.X, yAxis.X, zAxis.X, 0,
            xAxis.Y, yAxis.Y, zAxis.Y, 0,
            xAxis.Z, yAxis.Z, zAxis.Z, 0,
            -Vector3D.Dot(xAxis, eye),
            -Vector3D.Dot(yAxis, eye),
            -Vector3D.Dot(zAxis, eye),
            1
        );
    }

    public static Matrix4X4<float> PerspectiveFovLH(
        float fov,
        float aspect,
        float nearZ,
        float farZ
    )
    {
        float h = 1.0f / MathF.Tan(fov * 0.5f);
        float w = h / aspect;
        float range = farZ / (farZ - nearZ);
        return new Matrix4X4<float>(
            w, 0, 0, 0,
            0, h, 0, 0,
            0, 0, range, 1,
            0, 0, -range * nearZ, 0
        );
    }

    public static Matrix4X4<float> OrthographicLH(float width, float height, float nearZ, float farZ)
    {
        float range = 1.0f / (farZ - nearZ);
        return new Matrix4X4<float>(
            2f / width, 0, 0, 0,
            0, 2f / height, 0, 0,
            0, 0, range, 0,
            0, 0, -range * nearZ, 1
        );
    }
}

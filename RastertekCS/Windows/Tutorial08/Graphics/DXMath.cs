using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial08.Graphics;

/// <summary>
/// Left-handed matrix helpers matching DirectXMath (XMMatrixLookAtLH, XMMatrixPerspectiveFovLH).
/// Silk.NET.Maths only provides right-handed variants.
/// </summary>
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
            xAxis.X,
            yAxis.X,
            zAxis.X,
            0,
            xAxis.Y,
            yAxis.Y,
            zAxis.Y,
            0,
            xAxis.Z,
            yAxis.Z,
            zAxis.Z,
            0,
            -Vector3D.Dot(xAxis, eye),
            -Vector3D.Dot(yAxis, eye),
            -Vector3D.Dot(zAxis, eye),
            1
        );
    }

    public static Matrix4X4<float> RotationYLH(float angle)
    {
        float c = MathF.Cos(angle);
        float s = MathF.Sin(angle);
        return new Matrix4X4<float>(c, 0, -s, 0, 0, 1, 0, 0, s, 0, c, 0, 0, 0, 0, 1);
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
            w,
            0,
            0,
            0,
            0,
            h,
            0,
            0,
            0,
            0,
            range,
            1,
            0,
            0,
            -range * nearZ,
            0
        );
    }
}

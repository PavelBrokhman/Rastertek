using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial06.Graphics;

public class Camera
{
    private float _positionX, _positionY, _positionZ;
    private float _rotationX, _rotationY, _rotationZ;
    private Matrix4X4<float> _viewMatrix;

    public void SetPosition(float x, float y, float z) { _positionX = x; _positionY = y; _positionZ = z; }
    public void SetRotation(float x, float y, float z) { _rotationX = x; _rotationY = y; _rotationZ = z; }

    public void Render()
    {
        var up = new Vector3D<float>(0.0f, 1.0f, 0.0f);
        var position = new Vector3D<float>(_positionX, _positionY, _positionZ);
        var lookAt = new Vector3D<float>(0.0f, 0.0f, 1.0f);

        float pitch = _rotationX * 0.0174532925f;
        float yaw = _rotationY * 0.0174532925f;
        float roll = _rotationZ * 0.0174532925f;

        // C++ cameraclass.cpp MatrixRotationYawPitchRoll — 3x3 row-major.
        Span<float> rot = stackalloc float[9];
        MatrixRotationYawPitchRoll(rot, yaw, pitch, roll);

        TransformCoord(ref lookAt, rot);
        TransformCoord(ref up, rot);

        lookAt = position + lookAt;
        _viewMatrix = BuildViewMatrix(position, lookAt, up);
    }

    public Matrix4X4<float> GetViewMatrix() => _viewMatrix;

    // C++ cameraclass.cpp MatrixRotationYawPitchRoll
    private static void MatrixRotationYawPitchRoll(Span<float> m, float yaw, float pitch, float roll)
    {
        float cYaw = MathF.Cos(yaw);
        float cPitch = MathF.Cos(pitch);
        float cRoll = MathF.Cos(roll);
        float sYaw = MathF.Sin(yaw);
        float sPitch = MathF.Sin(pitch);
        float sRoll = MathF.Sin(roll);

        m[0] = (cRoll * cYaw) + (sRoll * sPitch * sYaw);
        m[1] = (sRoll * cPitch);
        m[2] = (cRoll * -sYaw) + (sRoll * sPitch * cYaw);

        m[3] = (-sRoll * cYaw) + (cRoll * sPitch * sYaw);
        m[4] = (cRoll * cPitch);
        m[5] = (sRoll * sYaw) + (cRoll * sPitch * cYaw);

        m[6] = (cPitch * sYaw);
        m[7] = -sPitch;
        m[8] = (cPitch * cYaw);
    }

    // C++ cameraclass.cpp TransformCoord — row-vec * 3x3 row-major
    private static void TransformCoord(ref Vector3D<float> v, ReadOnlySpan<float> m)
    {
        float x = (v.X * m[0]) + (v.Y * m[3]) + (v.Z * m[6]);
        float y = (v.X * m[1]) + (v.Y * m[4]) + (v.Z * m[7]);
        float z = (v.X * m[2]) + (v.Y * m[5]) + (v.Z * m[8]);
        v = new Vector3D<float>(x, y, z);
    }

    // C++ cameraclass.cpp BuildViewMatrix
    private static Matrix4X4<float> BuildViewMatrix(Vector3D<float> position, Vector3D<float> lookAt, Vector3D<float> up)
    {
        // zAxis = normal(lookAt - position)
        var zAxis = lookAt - position;
        zAxis = Vector3D.Normalize(zAxis);

        // xAxis = normal(cross(up, zAxis))
        var xAxis = new Vector3D<float>(
            (up.Y * zAxis.Z) - (up.Z * zAxis.Y),
            (up.Z * zAxis.X) - (up.X * zAxis.Z),
            (up.X * zAxis.Y) - (up.Y * zAxis.X));
        xAxis = Vector3D.Normalize(xAxis);

        // yAxis = cross(zAxis, xAxis)
        var yAxis = new Vector3D<float>(
            (zAxis.Y * xAxis.Z) - (zAxis.Z * xAxis.Y),
            (zAxis.Z * xAxis.X) - (zAxis.X * xAxis.Z),
            (zAxis.X * xAxis.Y) - (zAxis.Y * xAxis.X));

        float r1 = -((xAxis.X * position.X) + (xAxis.Y * position.Y) + (xAxis.Z * position.Z));
        float r2 = -((yAxis.X * position.X) + (yAxis.Y * position.Y) + (yAxis.Z * position.Z));
        float r3 = -((zAxis.X * position.X) + (zAxis.Y * position.Y) + (zAxis.Z * position.Z));

        // Row-major storage: matrix[0..3]=row0, matrix[4..7]=row1, etc.
        // M11=xAxis.x, M12=yAxis.x, M13=zAxis.x, M14=0
        // M21=xAxis.y, M22=yAxis.y, M23=zAxis.y, M24=0
        // M31=xAxis.z, M32=yAxis.z, M33=zAxis.z, M34=0
        // M41=r1,      M42=r2,      M43=r3,      M44=1
        return new Matrix4X4<float>(
            xAxis.X, yAxis.X, zAxis.X, 0,
            xAxis.Y, yAxis.Y, zAxis.Y, 0,
            xAxis.Z, yAxis.Z, zAxis.Z, 0,
            r1, r2, r3, 1);
    }
}

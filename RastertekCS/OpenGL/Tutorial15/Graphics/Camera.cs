using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial15.Graphics;

public class Camera
{
    private float _positionX,
        _positionY,
        _positionZ;
    private float _rotationX,
        _rotationY,
        _rotationZ;
    private Matrix4X4<float> _viewMatrix;

    public void SetPosition(float x, float y, float z)
    {
        _positionX = x;
        _positionY = y;
        _positionZ = z;
    }

    public void SetRotation(float x, float y, float z)
    {
        _rotationX = x;
        _rotationY = y;
        _rotationZ = z;
    }

    public float[] GetPosition() => new[] { _positionX, _positionY, _positionZ };

    public void Render()
    {
        float pitch = _rotationX * (MathF.PI / 180.0f);
        float yaw = _rotationY * (MathF.PI / 180.0f);
        float roll = _rotationZ * (MathF.PI / 180.0f);

        float cY = MathF.Cos(yaw),
            sY = MathF.Sin(yaw);
        float cP = MathF.Cos(pitch),
            sP = MathF.Sin(pitch);
        float cR = MathF.Cos(roll),
            sR = MathF.Sin(roll);

        float r00 = cR * cY + sR * sP * sY;
        float r01 = sR * cP;
        float r02 = cR * -sY + sR * sP * cY;
        float r10 = -sR * cY + cR * sP * sY;
        float r11 = cR * cP;
        float r12 = sR * sY + cR * sP * cY;
        float r20 = cP * sY;
        float r21 = -sP;
        float r22 = cP * cY;

        float lx = r20 + _positionX,
            ly = r21 + _positionY,
            lz = r22 + _positionZ;
        float ux = r10,
            uy = r11,
            uz = r12;

        BuildViewMatrixLH(
            out _viewMatrix,
            _positionX,
            _positionY,
            _positionZ,
            lx,
            ly,
            lz,
            ux,
            uy,
            uz
        );
    }

    public Matrix4X4<float> GetViewMatrix() => _viewMatrix;

    private static void BuildViewMatrixLH(
        out Matrix4X4<float> m,
        float px,
        float py,
        float pz,
        float lx,
        float ly,
        float lz,
        float ux,
        float uy,
        float uz
    )
    {
        float zx = lx - px,
            zy = ly - py,
            zz = lz - pz;
        float zLen = MathF.Sqrt(zx * zx + zy * zy + zz * zz);
        zx /= zLen;
        zy /= zLen;
        zz /= zLen;

        float xx = uy * zz - uz * zy;
        float xy = uz * zx - ux * zz;
        float xz = ux * zy - uy * zx;
        float xLen = MathF.Sqrt(xx * xx + xy * xy + xz * xz);
        xx /= xLen;
        xy /= xLen;
        xz /= xLen;

        float yx = zy * xz - zz * xy;
        float yy = zz * xx - zx * xz;
        float yz = zx * xy - zy * xx;

        float d1 = -(xx * px + xy * py + xz * pz);
        float d2 = -(yx * px + yy * py + yz * pz);
        float d3 = -(zx * px + zy * py + zz * pz);

        m = default;
        m.M11 = xx;
        m.M21 = yx;
        m.M31 = zx;
        m.M41 = 0;
        m.M12 = xy;
        m.M22 = yy;
        m.M32 = zy;
        m.M42 = 0;
        m.M13 = xz;
        m.M23 = yz;
        m.M33 = zz;
        m.M43 = 0;
        m.M14 = d1;
        m.M24 = d2;
        m.M34 = d3;
        m.M44 = 1;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial17.Graphics;

public class Camera
{
    private float m_positionX, m_positionY, m_positionZ;
    private float m_rotationX, m_rotationY, m_rotationZ;
    private Matrix4X4<float> m_viewMatrix;

    public void SetPosition(float x, float y, float z) { m_positionX = x; m_positionY = y; m_positionZ = z; }
    public void SetRotation(float x, float y, float z) { m_rotationX = x; m_rotationY = y; m_rotationZ = z; }

    public float[] GetPosition() => new[] { m_positionX, m_positionY, m_positionZ };

    public void Render()
    {
        float pitch = m_rotationX * (MathF.PI / 180.0f);
        float yaw = m_rotationY * (MathF.PI / 180.0f);
        float roll = m_rotationZ * (MathF.PI / 180.0f);

        // Rotation matrix (matches original Rastertek MatrixRotationYawPitchRoll).
        float cY = MathF.Cos(yaw), sY = MathF.Sin(yaw);
        float cP = MathF.Cos(pitch), sP = MathF.Sin(pitch);
        float cR = MathF.Cos(roll), sR = MathF.Sin(roll);

        float r00 = cR * cY + sR * sP * sY;
        float r01 = sR * cP;
        float r02 = cR * -sY + sR * sP * cY;
        float r10 = -sR * cY + cR * sP * sY;
        float r11 = cR * cP;
        float r12 = sR * sY + cR * sP * cY;
        float r20 = cP * sY;
        float r21 = -sP;
        float r22 = cP * cY;

        // Default lookAt = (0,0,1), up = (0,1,0) transformed by rotation.
        float lx = r20, ly = r21, lz = r22; // lookAt direction
        float ux = r10, uy = r11, uz = r12; // up direction

        // Translate to camera position.
        lx += m_positionX; ly += m_positionY; lz += m_positionZ;

        // Build LH view matrix (matches original Rastertek BuildViewMatrix).
        BuildViewMatrixLH(out m_viewMatrix,
            m_positionX, m_positionY, m_positionZ,
            lx, ly, lz, ux, uy, uz);
    }

    public Matrix4X4<float> GetViewMatrix() => m_viewMatrix;

    private static void BuildViewMatrixLH(out Matrix4X4<float> m,
        float px, float py, float pz,
        float lx, float ly, float lz,
        float ux, float uy, float uz)
    {
        // zAxis = normalize(lookAt - position) -- LH convention
        float zx = lx - px, zy = ly - py, zz = lz - pz;
        float zLen = MathF.Sqrt(zx * zx + zy * zy + zz * zz);
        zx /= zLen; zy /= zLen; zz /= zLen;

        // xAxis = normalize(cross(up, zAxis))
        float xx = uy * zz - uz * zy;
        float xy = uz * zx - ux * zz;
        float xz = ux * zy - uy * zx;
        float xLen = MathF.Sqrt(xx * xx + xy * xy + xz * xz);
        xx /= xLen; xy /= xLen; xz /= xLen;

        // yAxis = cross(zAxis, xAxis)
        float yx = zy * xz - zz * xy;
        float yy = zz * xx - zx * xz;
        float yz = zx * xy - zy * xx;

        float d1 = -(xx * px + xy * py + xz * pz);
        float d2 = -(yx * py + yy * py + yz * pz);
        float d3 = -(zx * px + zy * py + zz * pz);

        // Column-major layout matching original Rastertek.
        m = default;
        m.M11 = xx; m.M21 = yx; m.M31 = zx; m.M41 = 0;
        m.M12 = xy; m.M22 = yy; m.M32 = zy; m.M42 = 0;
        m.M13 = xz; m.M23 = yz; m.M33 = zz; m.M43 = 0;
        m.M14 = d1; m.M24 = d2; m.M34 = d3; m.M44 = 1;
    }
}

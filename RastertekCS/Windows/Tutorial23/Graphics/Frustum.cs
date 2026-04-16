using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial23.Graphics;

public class Frustum
{
    private readonly float[][] _planes = new float[6][];

    public Frustum()
    {
        for (int i = 0; i < 6; i++)
            _planes[i] = new float[4];
    }

    public void ConstructFrustum(Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix)
    {
        var m = viewMatrix * projectionMatrix;

        // Left: col4 + col1
        _planes[0][0] = m.M14 + m.M11;
        _planes[0][1] = m.M24 + m.M21;
        _planes[0][2] = m.M34 + m.M31;
        _planes[0][3] = m.M44 + m.M41;
        NormalizePlane(_planes[0]);
        // Right: col4 - col1
        _planes[1][0] = m.M14 - m.M11;
        _planes[1][1] = m.M24 - m.M21;
        _planes[1][2] = m.M34 - m.M31;
        _planes[1][3] = m.M44 - m.M41;
        NormalizePlane(_planes[1]);
        // Bottom: col4 + col2
        _planes[2][0] = m.M14 + m.M12;
        _planes[2][1] = m.M24 + m.M22;
        _planes[2][2] = m.M34 + m.M32;
        _planes[2][3] = m.M44 + m.M42;
        NormalizePlane(_planes[2]);
        // Top: col4 - col2
        _planes[3][0] = m.M14 - m.M12;
        _planes[3][1] = m.M24 - m.M22;
        _planes[3][2] = m.M34 - m.M32;
        _planes[3][3] = m.M44 - m.M42;
        NormalizePlane(_planes[3]);
        // Near: col3
        _planes[4][0] = m.M13;
        _planes[4][1] = m.M23;
        _planes[4][2] = m.M33;
        _planes[4][3] = m.M43;
        NormalizePlane(_planes[4]);
        // Far: col4 - col3
        _planes[5][0] = m.M14 - m.M13;
        _planes[5][1] = m.M24 - m.M23;
        _planes[5][2] = m.M34 - m.M33;
        _planes[5][3] = m.M44 - m.M43;
        NormalizePlane(_planes[5]);
    }

    public bool CheckSphere(float xCenter, float yCenter, float zCenter, float radius)
    {
        for (int i = 0; i < 6; i++)
            if (
                _planes[i][0] * xCenter
                    + _planes[i][1] * yCenter
                    + _planes[i][2] * zCenter
                    + _planes[i][3]
                < -radius
            )
                return false;
        return true;
    }

    private static void NormalizePlane(float[] plane)
    {
        float t = MathF.Sqrt(plane[0] * plane[0] + plane[1] * plane[1] + plane[2] * plane[2]);
        plane[0] /= t;
        plane[1] /= t;
        plane[2] /= t;
        plane[3] /= t;
    }
}

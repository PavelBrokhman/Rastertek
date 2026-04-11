using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial23.Graphics;

public class Frustum
{
    private readonly float[][] m_planes = new float[6][];

    public Frustum()
    {
        for (int i = 0; i < 6; i++) m_planes[i] = new float[4];
    }

    public void ConstructFrustum(Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix)
    {
        var m = viewMatrix * projectionMatrix;

        // Left: col4 + col1
        m_planes[0][0] = m.M14 + m.M11; m_planes[0][1] = m.M24 + m.M21; m_planes[0][2] = m.M34 + m.M31; m_planes[0][3] = m.M44 + m.M41;
        NormalizePlane(m_planes[0]);
        // Right: col4 - col1
        m_planes[1][0] = m.M14 - m.M11; m_planes[1][1] = m.M24 - m.M21; m_planes[1][2] = m.M34 - m.M31; m_planes[1][3] = m.M44 - m.M41;
        NormalizePlane(m_planes[1]);
        // Bottom: col4 + col2
        m_planes[2][0] = m.M14 + m.M12; m_planes[2][1] = m.M24 + m.M22; m_planes[2][2] = m.M34 + m.M32; m_planes[2][3] = m.M44 + m.M42;
        NormalizePlane(m_planes[2]);
        // Top: col4 - col2
        m_planes[3][0] = m.M14 - m.M12; m_planes[3][1] = m.M24 - m.M22; m_planes[3][2] = m.M34 - m.M32; m_planes[3][3] = m.M44 - m.M42;
        NormalizePlane(m_planes[3]);
        // Near: col3
        m_planes[4][0] = m.M13; m_planes[4][1] = m.M23; m_planes[4][2] = m.M33; m_planes[4][3] = m.M43;
        NormalizePlane(m_planes[4]);
        // Far: col4 - col3
        m_planes[5][0] = m.M14 - m.M13; m_planes[5][1] = m.M24 - m.M23; m_planes[5][2] = m.M34 - m.M33; m_planes[5][3] = m.M44 - m.M43;
        NormalizePlane(m_planes[5]);
    }

    public bool CheckSphere(float xCenter, float yCenter, float zCenter, float radius)
    {
        for (int i = 0; i < 6; i++)
            if (m_planes[i][0] * xCenter + m_planes[i][1] * yCenter + m_planes[i][2] * zCenter + m_planes[i][3] < -radius)
                return false;
        return true;
    }

    private static void NormalizePlane(float[] plane)
    {
        float t = MathF.Sqrt(plane[0] * plane[0] + plane[1] * plane[1] + plane[2] * plane[2]);
        plane[0] /= t; plane[1] /= t; plane[2] /= t; plane[3] /= t;
    }
}

using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class Frustum
{
    private readonly float[][] m_planes = new float[6][];

    public Frustum()
    {
        for (int i = 0; i < 6; i++) m_planes[i] = new float[4];
    }

    public void ConstructFrustum(Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix)
    {
        // Multiply view * projection to get the combined matrix.
        var matrix = viewMatrix * projectionMatrix;

        // Extract frustum planes from the combined matrix (column-major, row-vector convention).
        // Using Gribb/Hartmann method for column-major matrices.
        // Left plane: col4 + col1
        m_planes[0][0] = matrix.M14 + matrix.M11;
        m_planes[0][1] = matrix.M24 + matrix.M21;
        m_planes[0][2] = matrix.M34 + matrix.M31;
        m_planes[0][3] = matrix.M44 + matrix.M41;
        NormalizePlane(m_planes[0]);

        // Right plane: col4 - col1
        m_planes[1][0] = matrix.M14 - matrix.M11;
        m_planes[1][1] = matrix.M24 - matrix.M21;
        m_planes[1][2] = matrix.M34 - matrix.M31;
        m_planes[1][3] = matrix.M44 - matrix.M41;
        NormalizePlane(m_planes[1]);

        // Bottom plane: col4 + col2
        m_planes[2][0] = matrix.M14 + matrix.M12;
        m_planes[2][1] = matrix.M24 + matrix.M22;
        m_planes[2][2] = matrix.M34 + matrix.M32;
        m_planes[2][3] = matrix.M44 + matrix.M42;
        NormalizePlane(m_planes[2]);

        // Top plane: col4 - col2
        m_planes[3][0] = matrix.M14 - matrix.M12;
        m_planes[3][1] = matrix.M24 - matrix.M22;
        m_planes[3][2] = matrix.M34 - matrix.M32;
        m_planes[3][3] = matrix.M44 - matrix.M42;
        NormalizePlane(m_planes[3]);

        // Near plane: col3
        m_planes[4][0] = matrix.M13;
        m_planes[4][1] = matrix.M23;
        m_planes[4][2] = matrix.M33;
        m_planes[4][3] = matrix.M43;
        NormalizePlane(m_planes[4]);

        // Far plane: col4 - col3
        m_planes[5][0] = matrix.M14 - matrix.M13;
        m_planes[5][1] = matrix.M24 - matrix.M23;
        m_planes[5][2] = matrix.M34 - matrix.M33;
        m_planes[5][3] = matrix.M44 - matrix.M43;
        NormalizePlane(m_planes[5]);
    }

    public bool CheckPoint(float x, float y, float z)
    {
        for (int i = 0; i < 6; i++)
        {
            if (m_planes[i][0] * x + m_planes[i][1] * y + m_planes[i][2] * z + m_planes[i][3] < 0.0f)
                return false;
        }
        return true;
    }

    public bool CheckSphere(float xCenter, float yCenter, float zCenter, float radius)
    {
        for (int i = 0; i < 6; i++)
        {
            if (m_planes[i][0] * xCenter + m_planes[i][1] * yCenter + m_planes[i][2] * zCenter + m_planes[i][3] < -radius)
                return false;
        }
        return true;
    }

    public bool CheckCube(float xCenter, float yCenter, float zCenter, float radius)
    {
        for (int i = 0; i < 6; i++)
        {
            if (m_planes[i][0] * (xCenter - radius) + m_planes[i][1] * (yCenter - radius) + m_planes[i][2] * (zCenter - radius) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter + radius) + m_planes[i][1] * (yCenter - radius) + m_planes[i][2] * (zCenter - radius) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter - radius) + m_planes[i][1] * (yCenter + radius) + m_planes[i][2] * (zCenter - radius) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter + radius) + m_planes[i][1] * (yCenter + radius) + m_planes[i][2] * (zCenter - radius) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter - radius) + m_planes[i][1] * (yCenter - radius) + m_planes[i][2] * (zCenter + radius) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter + radius) + m_planes[i][1] * (yCenter - radius) + m_planes[i][2] * (zCenter + radius) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter - radius) + m_planes[i][1] * (yCenter + radius) + m_planes[i][2] * (zCenter + radius) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter + radius) + m_planes[i][1] * (yCenter + radius) + m_planes[i][2] * (zCenter + radius) + m_planes[i][3] >= 0.0f) continue;
            return false;
        }
        return true;
    }

    public bool CheckRectangle(float xCenter, float yCenter, float zCenter, float xSize, float ySize, float zSize)
    {
        for (int i = 0; i < 6; i++)
        {
            if (m_planes[i][0] * (xCenter - xSize) + m_planes[i][1] * (yCenter - ySize) + m_planes[i][2] * (zCenter - zSize) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter + xSize) + m_planes[i][1] * (yCenter - ySize) + m_planes[i][2] * (zCenter - zSize) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter - xSize) + m_planes[i][1] * (yCenter + ySize) + m_planes[i][2] * (zCenter - zSize) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter - xSize) + m_planes[i][1] * (yCenter - ySize) + m_planes[i][2] * (zCenter + zSize) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter + xSize) + m_planes[i][1] * (yCenter + ySize) + m_planes[i][2] * (zCenter - zSize) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter + xSize) + m_planes[i][1] * (yCenter - ySize) + m_planes[i][2] * (zCenter + zSize) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter - xSize) + m_planes[i][1] * (yCenter + ySize) + m_planes[i][2] * (zCenter + zSize) + m_planes[i][3] >= 0.0f) continue;
            if (m_planes[i][0] * (xCenter + xSize) + m_planes[i][1] * (yCenter + ySize) + m_planes[i][2] * (zCenter + zSize) + m_planes[i][3] >= 0.0f) continue;
            return false;
        }
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

using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

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
        // Multiply view * projection to get the combined matrix.
        var matrix = viewMatrix * projectionMatrix;

        // Extract frustum planes from the combined matrix (column-major, row-vector convention).
        // Using Gribb/Hartmann method for column-major matrices.
        // Left plane: col4 + col1
        _planes[0][0] = matrix.M14 + matrix.M11;
        _planes[0][1] = matrix.M24 + matrix.M21;
        _planes[0][2] = matrix.M34 + matrix.M31;
        _planes[0][3] = matrix.M44 + matrix.M41;
        NormalizePlane(_planes[0]);

        // Right plane: col4 - col1
        _planes[1][0] = matrix.M14 - matrix.M11;
        _planes[1][1] = matrix.M24 - matrix.M21;
        _planes[1][2] = matrix.M34 - matrix.M31;
        _planes[1][3] = matrix.M44 - matrix.M41;
        NormalizePlane(_planes[1]);

        // Bottom plane: col4 + col2
        _planes[2][0] = matrix.M14 + matrix.M12;
        _planes[2][1] = matrix.M24 + matrix.M22;
        _planes[2][2] = matrix.M34 + matrix.M32;
        _planes[2][3] = matrix.M44 + matrix.M42;
        NormalizePlane(_planes[2]);

        // Top plane: col4 - col2
        _planes[3][0] = matrix.M14 - matrix.M12;
        _planes[3][1] = matrix.M24 - matrix.M22;
        _planes[3][2] = matrix.M34 - matrix.M32;
        _planes[3][3] = matrix.M44 - matrix.M42;
        NormalizePlane(_planes[3]);

        // Near plane: col3
        _planes[4][0] = matrix.M13;
        _planes[4][1] = matrix.M23;
        _planes[4][2] = matrix.M33;
        _planes[4][3] = matrix.M43;
        NormalizePlane(_planes[4]);

        // Far plane: col4 - col3
        _planes[5][0] = matrix.M14 - matrix.M13;
        _planes[5][1] = matrix.M24 - matrix.M23;
        _planes[5][2] = matrix.M34 - matrix.M33;
        _planes[5][3] = matrix.M44 - matrix.M43;
        NormalizePlane(_planes[5]);
    }

    public bool CheckPoint(float x, float y, float z)
    {
        for (int i = 0; i < 6; i++)
        {
            if (_planes[i][0] * x + _planes[i][1] * y + _planes[i][2] * z + _planes[i][3] < 0.0f)
                return false;
        }
        return true;
    }

    public bool CheckSphere(float xCenter, float yCenter, float zCenter, float radius)
    {
        for (int i = 0; i < 6; i++)
        {
            if (
                _planes[i][0] * xCenter
                    + _planes[i][1] * yCenter
                    + _planes[i][2] * zCenter
                    + _planes[i][3]
                < -radius
            )
                return false;
        }
        return true;
    }

    public bool CheckCube(float xCenter, float yCenter, float zCenter, float radius)
    {
        for (int i = 0; i < 6; i++)
        {
            if (
                _planes[i][0] * (xCenter - radius)
                    + _planes[i][1] * (yCenter - radius)
                    + _planes[i][2] * (zCenter - radius)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter + radius)
                    + _planes[i][1] * (yCenter - radius)
                    + _planes[i][2] * (zCenter - radius)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter - radius)
                    + _planes[i][1] * (yCenter + radius)
                    + _planes[i][2] * (zCenter - radius)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter + radius)
                    + _planes[i][1] * (yCenter + radius)
                    + _planes[i][2] * (zCenter - radius)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter - radius)
                    + _planes[i][1] * (yCenter - radius)
                    + _planes[i][2] * (zCenter + radius)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter + radius)
                    + _planes[i][1] * (yCenter - radius)
                    + _planes[i][2] * (zCenter + radius)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter - radius)
                    + _planes[i][1] * (yCenter + radius)
                    + _planes[i][2] * (zCenter + radius)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter + radius)
                    + _planes[i][1] * (yCenter + radius)
                    + _planes[i][2] * (zCenter + radius)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            return false;
        }
        return true;
    }

    public bool CheckRectangle(
        float xCenter,
        float yCenter,
        float zCenter,
        float xSize,
        float ySize,
        float zSize
    )
    {
        for (int i = 0; i < 6; i++)
        {
            if (
                _planes[i][0] * (xCenter - xSize)
                    + _planes[i][1] * (yCenter - ySize)
                    + _planes[i][2] * (zCenter - zSize)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter + xSize)
                    + _planes[i][1] * (yCenter - ySize)
                    + _planes[i][2] * (zCenter - zSize)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter - xSize)
                    + _planes[i][1] * (yCenter + ySize)
                    + _planes[i][2] * (zCenter - zSize)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter - xSize)
                    + _planes[i][1] * (yCenter - ySize)
                    + _planes[i][2] * (zCenter + zSize)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter + xSize)
                    + _planes[i][1] * (yCenter + ySize)
                    + _planes[i][2] * (zCenter - zSize)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter + xSize)
                    + _planes[i][1] * (yCenter - ySize)
                    + _planes[i][2] * (zCenter + zSize)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter - xSize)
                    + _planes[i][1] * (yCenter + ySize)
                    + _planes[i][2] * (zCenter + zSize)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
            if (
                _planes[i][0] * (xCenter + xSize)
                    + _planes[i][1] * (yCenter + ySize)
                    + _planes[i][2] * (zCenter + zSize)
                    + _planes[i][3]
                >= 0.0f
            )
                continue;
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

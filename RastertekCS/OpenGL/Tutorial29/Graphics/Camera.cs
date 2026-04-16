using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial29.Graphics;

public class Camera
{
    private float m_positionX,
        m_positionY,
        m_positionZ;
    private float m_rotationX,
        m_rotationY,
        m_rotationZ;
    private Matrix4X4<float> m_viewMatrix;

    public void SetPosition(float x, float y, float z)
    {
        m_positionX = x;
        m_positionY = y;
        m_positionZ = z;
    }

    public void SetRotation(float x, float y, float z)
    {
        m_rotationX = x;
        m_rotationY = y;
        m_rotationZ = z;
    }

    public float[] GetPosition() => new[] { m_positionX, m_positionY, m_positionZ };

    public void Render()
    {
        var up = new Vector3D<float>(0, 1, 0);
        var position = new Vector3D<float>(m_positionX, m_positionY, m_positionZ);
        var lookAt = new Vector3D<float>(0, 0, 1);
        var rot = Matrix4X4.CreateFromYawPitchRoll(
            m_rotationY * (MathF.PI / 180.0f),
            m_rotationX * (MathF.PI / 180.0f),
            m_rotationZ * (MathF.PI / 180.0f)
        );
        lookAt = Vector3D.Transform(lookAt, rot);
        up = Vector3D.Transform(up, rot);
        lookAt = position + lookAt;
        m_viewMatrix = LookAtLH(position, lookAt, up);
    }

    public Matrix4X4<float> GetViewMatrix() => m_viewMatrix;

    private static Matrix4X4<float> LookAtLH(
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
}

using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial30.Graphics;

public class Camera
{
    private float m_positionX,
        m_positionY,
        m_positionZ;
    private float m_rotationX,
        m_rotationY,
        m_rotationZ;
    private Matrix4X4<float> m_viewMatrix;
    private Matrix4X4<float> m_reflectionViewMatrix;

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

    public void Render()
    {
        var up = new Vector3D<float>(0.0f, 1.0f, 0.0f);
        var position = new Vector3D<float>(m_positionX, m_positionY, m_positionZ);
        var lookAt = new Vector3D<float>(0.0f, 0.0f, 1.0f);

        float pitch = m_rotationX * (MathF.PI / 180.0f);
        float yaw = m_rotationY * (MathF.PI / 180.0f);
        float roll = m_rotationZ * (MathF.PI / 180.0f);

        var rotationMatrix = Matrix4X4.CreateFromYawPitchRoll(yaw, pitch, roll);
        lookAt = Vector3D.Transform(lookAt, rotationMatrix);
        up = Vector3D.Transform(up, rotationMatrix);

        lookAt = position + lookAt;

        m_viewMatrix = DXMath.LookAtLH(position, lookAt, up);
    }

    public Matrix4X4<float> GetViewMatrix() => m_viewMatrix;

    public void RenderReflection(float height)
    {
        var up = new Vector3D<float>(0.0f, 1.0f, 0.0f);
        var position = new Vector3D<float>(m_positionX, -m_positionY + height * 2.0f, m_positionZ);
        var lookAt = new Vector3D<float>(0.0f, 0.0f, 1.0f);

        float pitch = -m_rotationX * (MathF.PI / 180.0f);
        float yaw = m_rotationY * (MathF.PI / 180.0f);
        float roll = m_rotationZ * (MathF.PI / 180.0f);

        var rotationMatrix = Matrix4X4.CreateFromYawPitchRoll(yaw, pitch, roll);
        lookAt = Vector3D.Transform(lookAt, rotationMatrix);
        up = Vector3D.Transform(up, rotationMatrix);

        lookAt = position + lookAt;
        m_reflectionViewMatrix = DXMath.LookAtLH(position, lookAt, up);
    }

    public Matrix4X4<float> GetReflectionViewMatrix() => m_reflectionViewMatrix;
}

using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial35.Graphics;

public class Camera
{
    private float m_positionX, m_positionY, m_positionZ;
    private Matrix4X4<float> m_viewMatrix;

    public void SetPosition(float x, float y, float z) { m_positionX = x; m_positionY = y; m_positionZ = z; }

    public void Render()
    {
        var up = new Vector3D<float>(0, 1, 0);
        var position = new Vector3D<float>(m_positionX, m_positionY, m_positionZ);
        var lookAt = new Vector3D<float>(0, 0, 1);
        lookAt = position + lookAt;
        m_viewMatrix = Matrix4X4.CreateLookAt(position, lookAt, up);
    }

    public Matrix4X4<float> GetViewMatrix() => m_viewMatrix;
}

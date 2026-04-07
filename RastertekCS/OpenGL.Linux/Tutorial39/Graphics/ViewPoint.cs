using Silk.NET.Maths;
namespace RastertekCS.OpenGL.Tutorial39.Graphics;
public class ViewPoint
{
    private Vector3D<float> m_position, m_lookAt;
    private float m_fov, m_aspect, m_near, m_far;
    private Matrix4X4<float> m_viewMatrix, m_projectionMatrix;

    public void SetPosition(float x, float y, float z) { m_position = new Vector3D<float>(x, y, z); }
    public void SetLookAt(float x, float y, float z) { m_lookAt = new Vector3D<float>(x, y, z); }
    public void SetProjectionParameters(float fov, float aspect, float near, float far) { m_fov = fov; m_aspect = aspect; m_near = near; m_far = far; }
    public void GenerateViewMatrix() { m_viewMatrix = Matrix4X4.CreateLookAt(m_position, m_lookAt, new Vector3D<float>(0, 1, 0)); }
    public void GenerateProjectionMatrix() { m_projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(m_fov, m_aspect, m_near, m_far); }
    public Matrix4X4<float> GetViewMatrix() => m_viewMatrix;
    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;
}

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
    public void GenerateViewMatrix() { m_viewMatrix = LookAtLH(m_position, m_lookAt, new Vector3D<float>(0, 1, 0)); }
    public void GenerateProjectionMatrix() { m_projectionMatrix = PerspectiveFovLH(m_fov, m_aspect, m_near, m_far); }
    public Matrix4X4<float> GetViewMatrix() => m_viewMatrix;
    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;

    private static Matrix4X4<float> LookAtLH(Vector3D<float> eye, Vector3D<float> target, Vector3D<float> up)
    {
        var zAxis = Vector3D.Normalize(target - eye);
        var xAxis = Vector3D.Normalize(Vector3D.Cross(up, zAxis));
        var yAxis = Vector3D.Cross(zAxis, xAxis);
        return new Matrix4X4<float>(
            xAxis.X, yAxis.X, zAxis.X, 0,
            xAxis.Y, yAxis.Y, zAxis.Y, 0,
            xAxis.Z, yAxis.Z, zAxis.Z, 0,
            -Vector3D.Dot(xAxis, eye), -Vector3D.Dot(yAxis, eye), -Vector3D.Dot(zAxis, eye), 1);
    }

    private static Matrix4X4<float> PerspectiveFovLH(float fov, float aspect, float nearZ, float farZ)
    {
        float h = 1.0f / MathF.Tan(fov * 0.5f);
        float w = h / aspect;
        float range = farZ / (farZ - nearZ);
        return new Matrix4X4<float>(
            w, 0, 0, 0,
            0, h, 0, 0,
            0, 0, range, 1,
            0, 0, -range * nearZ, 0);
    }
}

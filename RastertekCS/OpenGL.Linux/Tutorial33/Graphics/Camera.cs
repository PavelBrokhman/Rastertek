using Silk.NET.Maths;
namespace RastertekCS.OpenGL.Tutorial33.Graphics;
public class Camera
{
    private float m_px, m_py, m_pz; private Matrix4X4<float> m_view;
    public void SetPosition(float x, float y, float z) { m_px = x; m_py = y; m_pz = z; }
    public void Render() { var pos = new Vector3D<float>(m_px, m_py, m_pz); m_view = Matrix4X4.CreateLookAt(pos, pos + new Vector3D<float>(0, 0, 1), new Vector3D<float>(0, 1, 0)); }
    public Matrix4X4<float> GetViewMatrix() => m_view;
}

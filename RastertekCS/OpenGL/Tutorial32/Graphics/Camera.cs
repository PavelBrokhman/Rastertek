using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class Camera
{
    private float m_px,
        m_py,
        m_pz,
        m_rx,
        m_ry,
        m_rz;
    private Matrix4X4<float> m_view;

    public void SetPosition(float x, float y, float z)
    {
        m_px = x;
        m_py = y;
        m_pz = z;
    }

    public void Render()
    {
        var up = new Vector3D<float>(0, 1, 0);
        var pos = new Vector3D<float>(m_px, m_py, m_pz);
        var la = new Vector3D<float>(0, 0, 1);
        var rot = Matrix4X4.CreateFromYawPitchRoll(
            m_ry * MathF.PI / 180f,
            m_rx * MathF.PI / 180f,
            m_rz * MathF.PI / 180f
        );
        la = Vector3D.Transform(la, rot);
        up = Vector3D.Transform(up, rot);
        la = pos + la;
        m_view = LookAtLH(pos, la, up);
    }

    public Matrix4X4<float> GetViewMatrix() => m_view;

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

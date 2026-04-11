using Silk.NET.Maths;
namespace RastertekCS.OpenGL.Tutorial39.Graphics;
public class Camera
{
    private float m_posX, m_posY, m_posZ, m_rotX, m_rotY, m_rotZ;
    private Matrix4X4<float> m_viewMatrix;
    public void SetPosition(float x, float y, float z) { m_posX=x; m_posY=y; m_posZ=z; }
    public void SetRotation(float x, float y, float z) { m_rotX=x; m_rotY=y; m_rotZ=z; }
    public void Render()
    {
        var up=new Vector3D<float>(0,1,0); var pos=new Vector3D<float>(m_posX,m_posY,m_posZ); var la=new Vector3D<float>(0,0,1);
        var rot=Matrix4X4.CreateFromYawPitchRoll(m_rotY*(MathF.PI/180f),m_rotX*(MathF.PI/180f),m_rotZ*(MathF.PI/180f));
        la=Vector3D.Transform(la,rot); up=Vector3D.Transform(up,rot); la=pos+la;
        m_viewMatrix=LookAtLH(pos,la,up);
    }
    public Matrix4X4<float> GetViewMatrix() => m_viewMatrix;

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
}

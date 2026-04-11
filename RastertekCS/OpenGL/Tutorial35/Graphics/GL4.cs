using Silk.NET.Maths; using Silk.NET.OpenGL; using Silk.NET.Windowing;
namespace RastertekCS.OpenGL.Tutorial35.Graphics;
public class GL4 { private GL m_gl; private Matrix4X4<float> m_w, m_p; public GL Gl => m_gl; public bool Initialize(IWindow w, int sw, int sh, float sd, float sn, bool vs) { m_gl = GL.GetApi(w); m_gl.ClearDepth(1f); m_gl.Enable(EnableCap.DepthTest); m_gl.FrontFace(FrontFaceDirection.CW); m_gl.Enable(EnableCap.CullFace); m_gl.CullFace(TriangleFace.Back); m_gl.Viewport(0, 0, (uint)sw, (uint)sh); m_w = Matrix4X4<float>.Identity; m_p = Matrix4X4.CreatePerspectiveFieldOfView<float>(MathF.PI / 4f, (float)sw / sh, sn, sd); return true; } public void Shutdown() { m_gl?.Dispose(); m_gl = null; } public void BeginScene(float r, float g, float b, float a) { m_gl.ClearColor(r, g, b, a); m_gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)); } public void EndScene() { } public Matrix4X4<float> GetWorldMatrix() => m_w; public Matrix4X4<float> GetProjectionMatrix() => m_p; 
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

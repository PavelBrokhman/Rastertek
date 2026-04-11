using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class GL4
{
    private GL m_gl;
    private Matrix4X4<float> m_worldMatrix;
    private Matrix4X4<float> m_projectionMatrix;
    private int m_screenWidth, m_screenHeight;

    public GL Gl => m_gl;

    public bool Initialize(IWindow window, int sw, int sh, float sd, float sn, bool vsync)
    {
        m_gl = GL.GetApi(window);
        m_screenWidth = sw; m_screenHeight = sh;
        m_gl.ClearDepth(1.0f);
        m_gl.Enable(EnableCap.DepthTest);
        m_gl.FrontFace(FrontFaceDirection.CW);
        m_gl.Enable(EnableCap.CullFace);
        m_gl.CullFace(TriangleFace.Back);
        m_gl.Viewport(0, 0, (uint)sw, (uint)sh);
        m_worldMatrix = Matrix4X4<float>.Identity;
        m_projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>(MathF.PI / 4.0f, (float)sw / sh, sn, sd);
        return true;
    }

    public void Shutdown() { m_gl?.Dispose(); m_gl = null; }
    public void BeginScene(float r, float g, float b, float a) { m_gl.ClearColor(r, g, b, a); m_gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)); }
    public void EndScene() { }
    public Matrix4X4<float> GetWorldMatrix() => m_worldMatrix;
    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;
    public void SetBackBufferRenderTarget() { m_gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0); }
    public void ResetViewport() { m_gl.Viewport(0, 0, (uint)m_screenWidth, (uint)m_screenHeight); }
    public void EnableClipping() { m_gl.Enable(EnableCap.ClipDistance0); }
    public void DisableClipping() { m_gl.Disable(EnableCap.ClipDistance0); }

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

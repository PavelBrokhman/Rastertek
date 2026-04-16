using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class GL4
{
    private GL m_gl;
    private Matrix4X4<float> m_world,
        m_proj;
    private int m_sw,
        m_sh;
    public GL Gl => m_gl;

    public bool Initialize(IWindow w, int sw, int sh, float sd, float sn, bool vs)
    {
        m_gl = GL.GetApi(w);
        m_sw = sw;
        m_sh = sh;
        m_gl.ClearDepth(1.0f);
        m_gl.Enable(EnableCap.DepthTest);
        m_gl.FrontFace(FrontFaceDirection.CW);
        m_gl.Enable(EnableCap.CullFace);
        m_gl.CullFace(TriangleFace.Back);
        m_gl.Viewport(0, 0, (uint)sw, (uint)sh);
        m_world = Matrix4X4<float>.Identity;
        m_proj = PerspectiveFovLH(MathF.PI / 4f, (float)sw / sh, sn, sd);
        return true;
    }

    public void Shutdown()
    {
        m_gl?.Dispose();
        m_gl = null;
    }

    public void BeginScene(float r, float g, float b, float a)
    {
        m_gl.ClearColor(r, g, b, a);
        m_gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void EndScene() { }

    public Matrix4X4<float> GetWorldMatrix() => m_world;

    public Matrix4X4<float> GetProjectionMatrix() => m_proj;

    public void SetBackBufferRenderTarget()
    {
        m_gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void ResetViewport()
    {
        m_gl.Viewport(0, 0, (uint)m_sw, (uint)m_sh);
    }

    private static Matrix4X4<float> PerspectiveFovLH(
        float fov,
        float aspect,
        float nearZ,
        float farZ
    )
    {
        float h = 1.0f / MathF.Tan(fov * 0.5f);
        float w = h / aspect;
        float range = farZ / (farZ - nearZ);
        return new Matrix4X4<float>(
            w,
            0,
            0,
            0,
            0,
            h,
            0,
            0,
            0,
            0,
            range,
            1,
            0,
            0,
            -range * nearZ,
            0
        );
    }
}

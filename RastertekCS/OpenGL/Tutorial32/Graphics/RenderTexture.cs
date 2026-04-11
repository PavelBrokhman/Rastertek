using Silk.NET.Maths; using Silk.NET.OpenGL;
namespace RastertekCS.OpenGL.Tutorial32.Graphics;
public class RenderTexture
{
    private uint m_fbo, m_tex, m_depth; private int m_tw, m_th; private Matrix4X4<float> m_proj;
    public unsafe bool Initialize(GL4 gl, int tw, int th, float sn, float sd)
    { var g = gl.Gl; m_tw = tw; m_th = th; m_fbo = g.GenFramebuffer(); g.BindFramebuffer(FramebufferTarget.Framebuffer, m_fbo); g.ActiveTexture(TextureUnit.Texture0); m_tex = g.GenTexture(); g.BindTexture(TextureTarget.Texture2D, m_tex); g.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, (uint)tw, (uint)th, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null); g.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear); g.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear); g.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, m_tex, 0); m_depth = g.GenRenderbuffer(); g.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depth); g.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent, (uint)tw, (uint)th); g.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, m_depth); GLEnum[] db = { GLEnum.ColorAttachment0 }; fixed (GLEnum* p = db) g.DrawBuffers(1, p); g.BindFramebuffer(FramebufferTarget.Framebuffer, 0); m_proj = PerspectiveFovLH(MathF.PI / 4f, (float)tw / th, sn, sd); return true; }
    public void Shutdown(GL4 gl) { var g = gl.Gl; g.DeleteRenderbuffer(m_depth); g.DeleteTexture(m_tex); g.DeleteFramebuffer(m_fbo); }
    public void SetRenderTarget(GL4 gl) { gl.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_fbo); gl.Gl.Viewport(0, 0, (uint)m_tw, (uint)m_th); }
    public void ClearRenderTarget(GL4 gl, float r, float g2, float b, float a) { gl.Gl.ClearColor(r, g2, b, a); gl.Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)); }
    public void SetTexture(GL4 gl, uint tu) { gl.Gl.ActiveTexture(TextureUnit.Texture0 + (int)tu); gl.Gl.BindTexture(TextureTarget.Texture2D, m_tex); }
    public Matrix4X4<float> GetProjectionMatrix() => m_proj;

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

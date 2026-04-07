using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial36.Graphics;

public class RenderTexture
{
    private uint m_fbo, m_tex, m_depth;
    private int m_tw, m_th;
    private Matrix4X4<float> m_projectionMatrix;
    private Matrix4X4<float> m_orthoMatrix;

    public unsafe bool Initialize(GL4 gl4, int tw, int th, float sn, float sd)
    {
        var gl = gl4.Gl;
        m_tw = tw;
        m_th = th;

        m_fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_fbo);

        gl.ActiveTexture(TextureUnit.Texture0);
        m_tex = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_tex);
        gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba,
            (uint)tw, (uint)th, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, m_tex, 0);

        m_depth = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depth);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent, (uint)tw, (uint)th);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, m_depth);

        GLEnum[] drawBuffers = { GLEnum.ColorAttachment0 };
        fixed (GLEnum* p = drawBuffers)
            gl.DrawBuffers(1, p);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        m_projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 4f, (float)tw / th, sn, sd);
        m_orthoMatrix = Matrix4X4.CreateOrthographicOffCenter(-tw / 2f, tw / 2f, -th / 2f, th / 2f, sn, sd);

        return true;
    }

    public void Shutdown(GL4 gl4)
    {
        var gl = gl4.Gl;
        gl.DeleteRenderbuffer(m_depth);
        gl.DeleteTexture(m_tex);
        gl.DeleteFramebuffer(m_fbo);
    }

    public void SetRenderTarget(GL4 gl4)
    {
        gl4.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_fbo);
        gl4.Gl.Viewport(0, 0, (uint)m_tw, (uint)m_th);
    }

    public void ClearRenderTarget(GL4 gl4, float r, float g, float b, float a)
    {
        gl4.Gl.ClearColor(r, g, b, a);
        gl4.Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetTexture(GL4 gl4, uint textureUnit)
    {
        gl4.Gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        gl4.Gl.BindTexture(TextureTarget.Texture2D, m_tex);
    }

    public int GetTextureWidth() => m_tw;
    public int GetTextureHeight() => m_th;
    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;
    public Matrix4X4<float> GetOrthoMatrix() => m_orthoMatrix;
}

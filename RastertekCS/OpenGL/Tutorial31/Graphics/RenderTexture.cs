using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class RenderTexture
{
    private uint m_fbo,
        m_texId,
        m_depthId;
    private int m_tw,
        m_th;
    private Matrix4X4<float> m_projMatrix;

    public unsafe bool Initialize(GL4 OpenGL, int tw, int th, float sn, float sd)
    {
        var gl = OpenGL.Gl;
        m_tw = tw;
        m_th = th;
        m_fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_fbo);
        gl.ActiveTexture(TextureUnit.Texture0);
        m_texId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_texId);
        gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            (int)InternalFormat.Rgba,
            (uint)tw,
            (uint)th,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            null
        );
        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
        );
        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear
        );
        gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            m_texId,
            0
        );
        m_depthId = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthId);
        gl.RenderbufferStorage(
            RenderbufferTarget.Renderbuffer,
            InternalFormat.DepthComponent,
            (uint)tw,
            (uint)th
        );
        gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            m_depthId
        );
        GLEnum[] db = { GLEnum.ColorAttachment0 };
        fixed (GLEnum* p = db)
            gl.DrawBuffers(1, p);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        m_projMatrix = PerspectiveFovLH(MathF.PI / 4.0f, (float)tw / th, sn, sd);
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DeleteRenderbuffer(m_depthId);
        gl.DeleteTexture(m_texId);
        gl.DeleteFramebuffer(m_fbo);
    }

    public void SetRenderTarget(GL4 OpenGL)
    {
        OpenGL.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_fbo);
        OpenGL.Gl.Viewport(0, 0, (uint)m_tw, (uint)m_th);
    }

    public void ClearRenderTarget(GL4 OpenGL, float r, float g, float b, float a)
    {
        OpenGL.Gl.ClearColor(r, g, b, a);
        OpenGL.Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetTexture(GL4 OpenGL, uint tu)
    {
        OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)tu);
        OpenGL.Gl.BindTexture(TextureTarget.Texture2D, m_texId);
    }

    public Matrix4X4<float> GetProjectionMatrix() => m_projMatrix;

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

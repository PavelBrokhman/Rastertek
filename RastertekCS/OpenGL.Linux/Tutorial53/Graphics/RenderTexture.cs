using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial53.Graphics;

public class RenderTexture
{
    private int m_textureWidth, m_textureHeight;
    private uint m_frameBufferId, m_depthBufferId, m_textureID;

    public unsafe bool Initialize(GL4 OpenGL, int w, int h, float sn, float sd, int format)
    {
        var gl = OpenGL.Gl; m_textureWidth = w; m_textureHeight = h;
        m_frameBufferId = gl.GenFramebuffer(); gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId);
        gl.ActiveTexture(TextureUnit.Texture0);
        m_textureID = gl.GenTexture(); gl.BindTexture(TextureTarget.Texture2D, m_textureID);
        gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, (uint)w, (uint)h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, m_textureID, 0);
        m_depthBufferId = gl.GenRenderbuffer(); gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthBufferId);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)w, (uint)h);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, m_depthBufferId);
        var db = new[] { DrawBufferMode.ColorAttachment0 }; fixed (DrawBufferMode* p = db) gl.DrawBuffers(1, p);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return true;
    }

    public void Shutdown(GL4 OpenGL) { var gl = OpenGL.Gl; gl.DeleteRenderbuffer(m_depthBufferId); gl.DeleteTexture(m_textureID); gl.DeleteFramebuffer(m_frameBufferId); }
    public void SetRenderTarget(GL4 OpenGL) { OpenGL.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId); OpenGL.Gl.Viewport(0, 0, (uint)m_textureWidth, (uint)m_textureHeight); }
    public void ClearRenderTarget(GL4 OpenGL, float r, float g, float b, float a) { OpenGL.Gl.ClearColor(r, g, b, a); OpenGL.Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)); }
    public void SetTexture(GL4 OpenGL, uint unit) { OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)unit); OpenGL.Gl.BindTexture(TextureTarget.Texture2D, m_textureID); }
}

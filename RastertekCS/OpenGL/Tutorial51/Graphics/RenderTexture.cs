using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial51.Graphics;

public class RenderTexture
{
    private int m_textureWidth, m_textureHeight;
    private uint m_frameBufferId, m_depthBufferId, m_textureID;

    public unsafe bool Initialize(GL4 OpenGL, int textureWidth, int textureHeight, float screenNear, float screenDepth, int format)
    {
        var gl = OpenGL.Gl;
        m_textureWidth = textureWidth;
        m_textureHeight = textureHeight;

        InternalFormat internalFmt;
        PixelFormat texFmt;
        PixelType pixelType;
        int filter;

        switch (format)
        {
            case 1:
                internalFmt = InternalFormat.R32f;
                texFmt = PixelFormat.Red;
                pixelType = PixelType.Float;
                filter = (int)TextureMinFilter.Nearest;
                break;
            default:
                internalFmt = InternalFormat.Rgba;
                texFmt = PixelFormat.Rgba;
                pixelType = PixelType.UnsignedByte;
                filter = (int)TextureMinFilter.Linear;
                break;
        }

        m_frameBufferId = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId);

        gl.ActiveTexture(TextureUnit.Texture0);
        m_textureID = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_textureID);
        gl.TexImage2D(TextureTarget.Texture2D, 0, (int)internalFmt, (uint)m_textureWidth, (uint)m_textureHeight, 0, texFmt, pixelType, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, filter);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, filter);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, m_textureID, 0);

        m_depthBufferId = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthBufferId);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)m_textureWidth, (uint)m_textureHeight);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, m_depthBufferId);

        var drawBuffers = new[] { DrawBufferMode.ColorAttachment0 };
        fixed (DrawBufferMode* ptr = drawBuffers) gl.DrawBuffers(1, ptr);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DeleteRenderbuffer(m_depthBufferId);
        gl.DeleteTexture(m_textureID);
        gl.DeleteFramebuffer(m_frameBufferId);
    }

    public void SetRenderTarget(GL4 OpenGL)
    {
        OpenGL.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId);
        OpenGL.Gl.Viewport(0, 0, (uint)m_textureWidth, (uint)m_textureHeight);
    }

    public void ClearRenderTarget(GL4 OpenGL, float r, float g, float b, float a)
    {
        OpenGL.Gl.ClearColor(r, g, b, a);
        OpenGL.Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        OpenGL.Gl.BindTexture(TextureTarget.Texture2D, m_textureID);
    }
}

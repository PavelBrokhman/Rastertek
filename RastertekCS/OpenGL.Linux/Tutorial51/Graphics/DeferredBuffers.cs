using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial51.Graphics;

public class DeferredBuffers
{
    private int m_textureWidth, m_textureHeight;
    private uint m_frameBufferId, m_depthBufferId;
    private uint[] m_textureIDArray = new uint[3]; // positions, normals, color

    public unsafe bool Initialize(GL4 OpenGL, int textureWidth, int textureHeight, float screenNear, float screenDepth)
    {
        var gl = OpenGL.Gl;
        m_textureWidth = textureWidth;
        m_textureHeight = textureHeight;

        m_frameBufferId = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId);

        m_depthBufferId = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthBufferId);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)m_textureWidth, (uint)m_textureHeight);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, m_depthBufferId);

        // Positions texture (GL_COLOR_ATTACHMENT0) - float format
        gl.ActiveTexture(TextureUnit.Texture0);
        m_textureIDArray[0] = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_textureIDArray[0]);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba32f, (uint)m_textureWidth, (uint)m_textureHeight, 0, PixelFormat.Rgba, PixelType.Float, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, m_textureIDArray[0], 0);

        // Normals texture (GL_COLOR_ATTACHMENT1) - float format
        gl.ActiveTexture(TextureUnit.Texture0);
        m_textureIDArray[1] = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_textureIDArray[1]);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba32f, (uint)m_textureWidth, (uint)m_textureHeight, 0, PixelFormat.Rgba, PixelType.Float, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, m_textureIDArray[1], 0);

        // Color texture (GL_COLOR_ATTACHMENT2) - RGBA unsigned byte
        gl.ActiveTexture(TextureUnit.Texture0);
        m_textureIDArray[2] = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_textureIDArray[2]);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)m_textureWidth, (uint)m_textureHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, m_textureIDArray[2], 0);

        var drawBuffers = new[] { DrawBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1, DrawBufferMode.ColorAttachment2 };
        fixed (DrawBufferMode* ptr = drawBuffers)
            gl.DrawBuffers(3, ptr);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DeleteRenderbuffer(m_depthBufferId);
        for (int i = 0; i < 3; i++) gl.DeleteTexture(m_textureIDArray[i]);
        gl.DeleteFramebuffer(m_frameBufferId);
    }

    public void SetRenderTarget(GL4 OpenGL)
    {
        OpenGL.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId);
        OpenGL.Gl.Viewport(0, 0, (uint)m_textureWidth, (uint)m_textureHeight);
    }

    public void ClearRenderTargets(GL4 OpenGL, float r, float g, float b, float a)
    {
        OpenGL.Gl.ClearColor(r, g, b, a);
        OpenGL.Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetShaderResourcePositions(GL4 OpenGL, uint textureUnit)
    {
        OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        OpenGL.Gl.BindTexture(TextureTarget.Texture2D, m_textureIDArray[0]);
    }

    public void SetShaderResourceNormals(GL4 OpenGL, uint textureUnit)
    {
        OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        OpenGL.Gl.BindTexture(TextureTarget.Texture2D, m_textureIDArray[1]);
    }

    public void SetShaderResourceColors(GL4 OpenGL, uint textureUnit)
    {
        OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        OpenGL.Gl.BindTexture(TextureTarget.Texture2D, m_textureIDArray[2]);
    }
}

using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial30.Graphics;

public class RenderTexture
{
    private uint m_frameBufferId, m_textureId, m_depthBufferId;
    private int m_textureWidth, m_textureHeight;
    private Matrix4X4<float> m_projectionMatrix;

    public unsafe bool Initialize(GL4 OpenGL, int textureWidth, int textureHeight, float screenNear, float screenDepth)
    {
        var gl = OpenGL.Gl;
        m_textureWidth = textureWidth;
        m_textureHeight = textureHeight;

        m_frameBufferId = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId);

        gl.ActiveTexture(TextureUnit.Texture0);
        m_textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_textureId);
        gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, (uint)m_textureWidth, (uint)m_textureHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, m_textureId, 0);

        m_depthBufferId = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthBufferId);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent, (uint)m_textureWidth, (uint)m_textureHeight);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, m_depthBufferId);

        GLEnum[] drawBuffers = { GLEnum.ColorAttachment0 };
        fixed (GLEnum* p = drawBuffers) gl.DrawBuffers(1, p);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        float fov = MathF.PI / 4.0f;
        float aspect = (float)m_textureWidth / m_textureHeight;
        m_projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>(fov, aspect, screenNear, screenDepth);

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DeleteRenderbuffer(m_depthBufferId);
        gl.DeleteTexture(m_textureId);
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
        OpenGL.Gl.BindTexture(TextureTarget.Texture2D, m_textureId);
    }

    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;
}

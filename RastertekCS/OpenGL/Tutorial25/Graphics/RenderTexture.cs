using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial25.Graphics;

public class RenderTexture
{
    private GL4 m_OpenGLPtr;
    private int m_textureWidth, m_textureHeight;
    private uint m_frameBufferId;
    private uint m_textureId;
    private uint m_depthBufferId;
    private Matrix4X4<float> m_projectionMatrix;

    public unsafe bool Initialize(GL4 OpenGL, int textureWidth, int textureHeight, float screenNear, float screenDepth)
    {
        m_OpenGLPtr = OpenGL;
        m_textureWidth = textureWidth;
        m_textureHeight = textureHeight;

        var gl = OpenGL.Gl;

        // Generate and bind the framebuffer.
        m_frameBufferId = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId);

        // Create the color texture attachment.
        gl.ActiveTexture(TextureUnit.Texture0);
        m_textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_textureId);
        gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba,
            (uint)textureWidth, (uint)textureHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (void*)0);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        // Attach the texture to the framebuffer.
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, m_textureId, 0);

        // Create the depth renderbuffer.
        m_depthBufferId = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthBufferId);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24,
            (uint)textureWidth, (uint)textureHeight);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, m_depthBufferId);

        // Set the draw buffer.
        GLEnum drawBuffer = GLEnum.ColorAttachment0;
        gl.DrawBuffers(1, &drawBuffer);

        // Unbind the framebuffer.
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Build the projection matrix for this render texture.
        float screenAspect = (float)textureWidth / (float)textureHeight;
        m_projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 4.0f, screenAspect, screenNear, screenDepth);

        return true;
    }

    public void Shutdown()
    {
        var gl = m_OpenGLPtr.Gl;
        gl.DeleteRenderbuffer(m_depthBufferId);
        gl.DeleteTexture(m_textureId);
        gl.DeleteFramebuffer(m_frameBufferId);
        m_OpenGLPtr = null;
    }

    public void SetRenderTarget()
    {
        var gl = m_OpenGLPtr.Gl;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, m_frameBufferId);
        gl.Viewport(0, 0, (uint)m_textureWidth, (uint)m_textureHeight);
    }

    public void ClearRenderTarget(float red, float green, float blue, float alpha)
    {
        var gl = m_OpenGLPtr.Gl;
        gl.ClearColor(red, green, blue, alpha);
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetTexture(uint textureUnit)
    {
        var gl = m_OpenGLPtr.Gl;
        gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        gl.BindTexture(TextureTarget.Texture2D, m_textureId);
    }

    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;
}

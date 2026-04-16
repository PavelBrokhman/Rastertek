using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial25.Graphics;

public class RenderTexture
{
    private GL4 _openGLPtr;
    private int _textureWidth,
        _textureHeight;
    private uint _frameBufferId;
    private uint _textureId;
    private uint _depthBufferId;
    private Matrix4X4<float> _projectionMatrix;

    public unsafe bool Initialize(
        GL4 OpenGL,
        int textureWidth,
        int textureHeight,
        float screenNear,
        float screenDepth
    )
    {
        _openGLPtr = OpenGL;
        _textureWidth = textureWidth;
        _textureHeight = textureHeight;

        var gl = OpenGL.Gl;

        // Generate and bind the framebuffer.
        _frameBufferId = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);

        // Create the color texture attachment.
        gl.ActiveTexture(TextureUnit.Texture0);
        _textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _textureId);
        gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            (int)InternalFormat.Rgba,
            (uint)textureWidth,
            (uint)textureHeight,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            (void*)0
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

        // Attach the texture to the framebuffer.
        gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            _textureId,
            0
        );

        // Create the depth renderbuffer.
        _depthBufferId = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBufferId);
        gl.RenderbufferStorage(
            RenderbufferTarget.Renderbuffer,
            InternalFormat.DepthComponent24,
            (uint)textureWidth,
            (uint)textureHeight
        );
        gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            _depthBufferId
        );

        // Set the draw buffer.
        GLEnum drawBuffer = GLEnum.ColorAttachment0;
        gl.DrawBuffers(1, &drawBuffer);

        // Unbind the framebuffer.
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Build the projection matrix for this render texture.
        float screenAspect = (float)textureWidth / (float)textureHeight;
        _projectionMatrix = PerspectiveFovLH(
            MathF.PI / 4.0f,
            screenAspect,
            screenNear,
            screenDepth
        );

        return true;
    }

    public void Shutdown()
    {
        var gl = _openGLPtr.Gl;
        gl.DeleteRenderbuffer(_depthBufferId);
        gl.DeleteTexture(_textureId);
        gl.DeleteFramebuffer(_frameBufferId);
        _openGLPtr = null;
    }

    public void SetRenderTarget()
    {
        var gl = _openGLPtr.Gl;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);
        gl.Viewport(0, 0, (uint)_textureWidth, (uint)_textureHeight);
    }

    public void ClearRenderTarget(float red, float green, float blue, float alpha)
    {
        var gl = _openGLPtr.Gl;
        gl.ClearColor(red, green, blue, alpha);
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetTexture(uint textureUnit)
    {
        var gl = _openGLPtr.Gl;
        gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        gl.BindTexture(TextureTarget.Texture2D, _textureId);
    }

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

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

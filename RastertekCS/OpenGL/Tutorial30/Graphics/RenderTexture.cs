using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial30.Graphics;

public class RenderTexture
{
    private uint _frameBufferId,
        _textureId,
        _depthBufferId;
    private int _textureWidth,
        _textureHeight;
    private Matrix4X4<float> _projectionMatrix;

    public unsafe bool Initialize(
        GL4 OpenGL,
        int textureWidth,
        int textureHeight,
        float screenNear,
        float screenDepth
    )
    {
        var gl = OpenGL.Gl;
        _textureWidth = textureWidth;
        _textureHeight = textureHeight;

        _frameBufferId = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);

        gl.ActiveTexture(TextureUnit.Texture0);
        _textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _textureId);
        gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            (int)InternalFormat.Rgba,
            (uint)_textureWidth,
            (uint)_textureHeight,
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
            _textureId,
            0
        );

        _depthBufferId = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBufferId);
        gl.RenderbufferStorage(
            RenderbufferTarget.Renderbuffer,
            InternalFormat.DepthComponent,
            (uint)_textureWidth,
            (uint)_textureHeight
        );
        gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            _depthBufferId
        );

        GLEnum[] drawBuffers = { GLEnum.ColorAttachment0 };
        fixed (GLEnum* p = drawBuffers)
            gl.DrawBuffers(1, p);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        float fov = MathF.PI / 4.0f;
        float aspect = (float)_textureWidth / _textureHeight;
        _projectionMatrix = PerspectiveFovLH(fov, aspect, screenNear, screenDepth);

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DeleteRenderbuffer(_depthBufferId);
        gl.DeleteTexture(_textureId);
        gl.DeleteFramebuffer(_frameBufferId);
    }

    public void SetRenderTarget(GL4 OpenGL)
    {
        OpenGL.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);
        OpenGL.Gl.Viewport(0, 0, (uint)_textureWidth, (uint)_textureHeight);
    }

    public void ClearRenderTarget(GL4 OpenGL, float r, float g, float b, float a)
    {
        OpenGL.Gl.ClearColor(r, g, b, a);
        OpenGL.Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        OpenGL.Gl.BindTexture(TextureTarget.Texture2D, _textureId);
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

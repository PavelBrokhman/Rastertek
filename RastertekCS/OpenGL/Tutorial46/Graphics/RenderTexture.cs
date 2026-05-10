using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial46.Graphics;

public class RenderTexture
{
    private uint _frameBufferId,
        _textureId,
        _depthBufferId;
    private int _textureWidth,
        _textureHeight;
    private Matrix4X4<float> _projectionMatrix;
    private Matrix4X4<float> _orthoMatrix;

    public unsafe bool Initialize(
        GL4 OpenGL,
        int textureWidth,
        int textureHeight,
        float screenNear,
        float screenDepth
    )
    {
        var gl = OpenGL.Driver;
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
            (uint)textureWidth,
            (uint)textureHeight,
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
            (uint)textureWidth,
            (uint)textureHeight
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

        _projectionMatrix = PerspectiveFovLH(
            MathF.PI / 4f,
            (float)textureWidth / textureHeight,
            screenNear,
            screenDepth
        );
        _orthoMatrix = OrthographicLH(textureWidth, textureHeight, screenNear, screenDepth);
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.DeleteRenderbuffer(_depthBufferId);
        gl.DeleteTexture(_textureId);
        gl.DeleteFramebuffer(_frameBufferId);
    }

    public void SetRenderTarget(GL4 OpenGL)
    {
        OpenGL.Driver.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);
        OpenGL.Driver.Viewport(0, 0, (uint)_textureWidth, (uint)_textureHeight);
    }

    public void ClearRenderTarget(GL4 OpenGL, float r, float g, float b, float a)
    {
        OpenGL.Driver.ClearColor(r, g, b, a);
        OpenGL.Driver.Clear(
            (uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)
        );
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        OpenGL.Driver.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        OpenGL.Driver.BindTexture(TextureTarget.Texture2D, _textureId);
    }

    public int GetTextureWidth() => _textureWidth;

    public int GetTextureHeight() => _textureHeight;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

    public Matrix4X4<float> GetOrthoMatrix() => _orthoMatrix;

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
            w, 0, 0, 0,
            0, h, 0, 0,
            0, 0, range, 1,
            0, 0, -range * nearZ, 0
        );
    }

    private static Matrix4X4<float> OrthographicLH(float width, float height, float nearZ, float farZ)
    {
        float range = 1.0f / (farZ - nearZ);
        return new Matrix4X4<float>(
            2.0f / width, 0, 0, 0,
            0, 2.0f / height, 0, 0,
            0, 0, range, 0,
            0, 0, -range * nearZ, 1);
    }
}

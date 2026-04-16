using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class RenderTexture
{
    private uint _frameBufferId,
        _textureId,
        _depthId;
    private int _textureWidth,
        _textureHeight;
    private Matrix4X4<float> _projectionMatrix;

    public unsafe bool Initialize(GL4 OpenGL, int tw, int th, float sn, float sd)
    {
        var gl = OpenGL.Gl;
        _textureWidth = tw;
        _textureHeight = th;
        _frameBufferId = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);
        gl.ActiveTexture(TextureUnit.Texture0);
        _textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _textureId);
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
            _textureId,
            0
        );
        _depthId = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthId);
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
            _depthId
        );
        GLEnum[] db = { GLEnum.ColorAttachment0 };
        fixed (GLEnum* p = db)
            gl.DrawBuffers(1, p);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _projectionMatrix = PerspectiveFovLH(MathF.PI / 4.0f, (float)tw / th, sn, sd);
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DeleteRenderbuffer(_depthId);
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

    public void SetTexture(GL4 OpenGL, uint tu)
    {
        OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)tu);
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

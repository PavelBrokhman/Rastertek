using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class RenderTexture
{
    private uint _frameBufferId,
        _textures,
        _depth;
    private int _textureWidth,
        _textureHeight;
    private Matrix4X4<float> _projectionMatrix;

    public unsafe bool Initialize(GL4 gl, int tw, int th, float screenNear, float screenDepth)
    {
        var g = gl.Gl;
        _textureWidth = tw;
        _textureHeight = th;
        _frameBufferId = g.GenFramebuffer();
        g.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);
        g.ActiveTexture(TextureUnit.Texture0);
        _textures = g.GenTexture();
        g.BindTexture(TextureTarget.Texture2D, _textures);
        g.TexImage2D(
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
        g.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
        );
        g.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear
        );
        g.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            _textures,
            0
        );
        _depth = g.GenRenderbuffer();
        g.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depth);
        g.RenderbufferStorage(
            RenderbufferTarget.Renderbuffer,
            InternalFormat.DepthComponent,
            (uint)tw,
            (uint)th
        );
        g.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            _depth
        );
        GLEnum[] db = { GLEnum.ColorAttachment0 };
        fixed (GLEnum* p = db)
            g.DrawBuffers(1, p);
        g.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _projectionMatrix = PerspectiveFovLH(
            MathF.PI / 4f,
            (float)tw / th,
            screenNear,
            screenDepth
        );
        return true;
    }

    public void Shutdown(GL4 gl)
    {
        var g = gl.Gl;
        g.DeleteRenderbuffer(_depth);
        g.DeleteTexture(_textures);
        g.DeleteFramebuffer(_frameBufferId);
    }

    public void SetRenderTarget(GL4 gl)
    {
        gl.Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferId);
        gl.Gl.Viewport(0, 0, (uint)_textureWidth, (uint)_textureHeight);
    }

    public void ClearRenderTarget(GL4 gl, float r, float g2, float b, float a)
    {
        gl.Gl.ClearColor(r, g2, b, a);
        gl.Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetTexture(GL4 gl, uint tu)
    {
        gl.Gl.ActiveTexture(TextureUnit.Texture0 + (int)tu);
        gl.Gl.BindTexture(TextureTarget.Texture2D, _textures);
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

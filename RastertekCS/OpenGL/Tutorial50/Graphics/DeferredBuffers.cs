using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class DeferredBuffers
{
    private int _texturewidth, _textureheight;
    private uint _framebufferid, _depthbufferid;
    private uint[] _textureidarray = new uint[2];

    public unsafe bool Initialize(GL4 OpenGL, int textureWidth, int textureHeight, float screenNear, float screenDepth)
    {
        var gl = OpenGL.Gl;
        _texturewidth = textureWidth;
        _textureheight = textureHeight;

        _framebufferid = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferid);

        _depthbufferid = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthbufferid);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)_texturewidth, (uint)_textureheight);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthbufferid);

        // Color texture (GL_COLOR_ATTACHMENT0)
        gl.ActiveTexture(TextureUnit.Texture0);
        _textureidarray[0] = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _textureidarray[0]);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)_texturewidth, (uint)_textureheight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _textureidarray[0], 0);

        // Normal texture (GL_COLOR_ATTACHMENT1) - float format
        gl.ActiveTexture(TextureUnit.Texture0);
        _textureidarray[1] = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _textureidarray[1]);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba32f, (uint)_texturewidth, (uint)_textureheight, 0, PixelFormat.Rgba, PixelType.Float, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, _textureidarray[1], 0);

        // Set draw buffers
        var drawBuffers = new[] { DrawBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1 };
        fixed (DrawBufferMode* ptr = drawBuffers)
            gl.DrawBuffers(2, ptr);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DeleteRenderbuffer(_depthbufferid);
        gl.DeleteTexture(_textureidarray[0]);
        gl.DeleteTexture(_textureidarray[1]);
        gl.DeleteFramebuffer(_framebufferid);
    }

    public void SetRenderTarget(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferid);
        gl.Viewport(0, 0, (uint)_texturewidth, (uint)_textureheight);
    }

    public void ClearRenderTargets(GL4 OpenGL, float r, float g, float b, float a)
    {
        var gl = OpenGL.Gl;
        gl.ClearColor(r, g, b, a);
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit, int index)
    {
        var gl = OpenGL.Gl;
        gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        gl.BindTexture(TextureTarget.Texture2D, _textureidarray[index]);
    }
}

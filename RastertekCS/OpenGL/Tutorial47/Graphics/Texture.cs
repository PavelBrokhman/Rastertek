using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial47.Graphics;

public class Texture
{
    private uint _textureId;
    private bool _loaded;
    private int _width, _height;

    public int GetWidth() => _width;
    public int GetHeight() => _height;

    public unsafe bool Initialize(GL4 OpenGL, string filename, uint textureUnit, bool wrap)
    {
        var gl = OpenGL.Driver;
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Texture not found: {filename}");
            return false;
        }
        if (!LoadTga(filename, out _width, out _height, out byte[] pixels))
        {
            Console.WriteLine($"Failed to load texture: {filename}");
            return false;
        }
        gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        _textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _textureId);
        fixed (byte* p = pixels)
            gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, (uint)_width, (uint)_height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, p);
        gl.GenerateMipmap(TextureTarget.Texture2D);
        var wm = wrap ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge;
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wm);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wm);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _loaded = true;
        return true;
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        if (_loaded)
        {
            OpenGL.Driver.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
            OpenGL.Driver.BindTexture(TextureTarget.Texture2D, _textureId);
        }
    }

    public void Shutdown(GL4 OpenGL)
    {
        if (_loaded) { OpenGL.Driver.DeleteTexture(_textureId); _loaded = false; }
    }

    private static bool LoadTga(string filename, out int width, out int height, out byte[] rgba)
    {
        width = 0; height = 0; rgba = null;
        byte[] data = File.ReadAllBytes(filename);
        if (data.Length < 18) return false;
        int idLength = data[0], imageType = data[2], bpp = data[16];
        width = data[12] | (data[13] << 8);
        height = data[14] | (data[15] << 8);
        if (imageType != 2) return false;
        if (bpp != 24 && bpp != 32) return false;
        int offset = 18 + idLength, channels = bpp / 8, pixelCount = width * height;
        if (data.Length < offset + pixelCount * channels) return false;
        rgba = new byte[pixelCount * 4];
        for (int destRow = 0; destRow < height; destRow++)
        {
            int srcRow = height - 1 - destRow;
            int srcOff = offset + srcRow * width * channels;
            int dstOff = destRow * width * 4;
            for (int x = 0; x < width; x++)
            {
                rgba[dstOff + x * 4 + 0] = data[srcOff + x * channels + 2];
                rgba[dstOff + x * 4 + 1] = data[srcOff + x * channels + 1];
                rgba[dstOff + x * 4 + 2] = data[srcOff + x * channels + 0];
                rgba[dstOff + x * 4 + 3] = channels == 4 ? data[srcOff + x * channels + 3] : (byte)255;
            }
        }
        return true;
    }
}

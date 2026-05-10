using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial46.Graphics;

public class Texture
{
    private uint _textureId;
    private bool _loaded;

    public unsafe bool Initialize(GL4 OpenGL, string filename, uint textureUnit, bool wrap)
    {
        var gl = OpenGL.Driver;

        if (!File.Exists(filename))
        {
            Console.WriteLine($"Texture not found: {filename}");
            return false;
        }

        // Open the targa file and read 18-byte header (C++ textureclass.cpp LoadTarga32Bit).
        using var fs = File.OpenRead(filename);
        using var reader = new BinaryReader(fs);

        byte[] header = reader.ReadBytes(18);
        if (header.Length != 18) return false;

        int width = header[12] | (header[13] << 8);
        int height = header[14] | (header[15] << 8);
        int bpp = header[16];

        // Check that it is 32 bit and not 24 bit.
        if (bpp != 32) return false;

        int imageSize = width * height * 4;
        byte[] targaImage = reader.ReadBytes(imageSize);
        if (targaImage.Length != imageSize) return false;

        // BGRA -> RGBA linear copy, no V-flip (matches C++ textureclass.cpp).
        byte[] targaData = new byte[imageSize];
        int index = 0;
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                targaData[index + 0] = targaImage[index + 2]; // Red.
                targaData[index + 1] = targaImage[index + 1]; // Green.
                targaData[index + 2] = targaImage[index + 0]; // Blue.
                targaData[index + 3] = targaImage[index + 3]; // Alpha.
                index += 4;
            }
        }

        gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        _textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _textureId);

        fixed (byte* p = targaData)
            gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba,
                (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, p);

        if (wrap)
        {
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }
        else
        {
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);

        gl.GenerateMipmap(TextureTarget.Texture2D);

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
}

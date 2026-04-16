using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial21.Graphics;

public class Texture
{
    private uint _textureId;
    private bool _loaded;

    public unsafe bool Initialize(GL4 OpenGL, string filename, uint textureUnit, bool wrap)
    {
        var gl = OpenGL.Gl;
        if (!File.Exists(filename))
            GenerateCheckerboardTga(filename, 64);
        if (!LoadTga(filename, out int width, out int height, out byte[] pixels))
        {
            Console.WriteLine($"Failed to load texture: {filename}");
            return false;
        }
        gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        _textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _textureId);
        fixed (byte* p = pixels)
            gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                (int)InternalFormat.Rgba,
                (uint)width,
                (uint)height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                p
            );
        gl.GenerateMipmap(TextureTarget.Texture2D);
        var wm = wrap ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge;
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wm);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wm);
        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear
        );
        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
        );
        _loaded = true;
        return true;
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        if (_loaded)
        {
            OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
            OpenGL.Gl.BindTexture(TextureTarget.Texture2D, _textureId);
        }
    }

    public void Shutdown(GL4 OpenGL)
    {
        if (_loaded)
        {
            OpenGL.Gl.DeleteTexture(_textureId);
            _loaded = false;
        }
    }

    private static bool LoadTga(string filename, out int width, out int height, out byte[] rgba)
    {
        width = 0;
        height = 0;
        rgba = null;
        byte[] data = File.ReadAllBytes(filename);
        if (data.Length < 18)
            return false;
        int idLength = data[0],
            imageType = data[2],
            bpp = data[16],
            descriptor = data[17];
        width = data[12] | (data[13] << 8);
        height = data[14] | (data[15] << 8);
        if (imageType != 2)
            return false;
        if (bpp != 24 && bpp != 32)
            return false;
        int offset = 18 + idLength,
            channels = bpp / 8,
            pixelCount = width * height;
        if (data.Length < offset + pixelCount * channels)
            return false;
        rgba = new byte[pixelCount * 4];
        bool topLeft = (descriptor & 0x20) != 0;
        for (int y = 0; y < height; y++)
        {
            int srcRow = topLeft ? y : (height - 1 - y);
            int srcOff = offset + srcRow * width * channels;
            int dstOff = y * width * 4;
            for (int x = 0; x < width; x++)
            {
                byte b = data[srcOff + x * channels + 0];
                byte g = data[srcOff + x * channels + 1];
                byte r = data[srcOff + x * channels + 2];
                byte a = channels == 4 ? data[srcOff + x * channels + 3] : (byte)255;
                rgba[dstOff + x * 4 + 0] = r;
                rgba[dstOff + x * 4 + 1] = g;
                rgba[dstOff + x * 4 + 2] = b;
                rgba[dstOff + x * 4 + 3] = a;
            }
        }
        return true;
    }

    private static void GenerateCheckerboardTga(string filename, int size)
    {
        var dir = Path.GetDirectoryName(filename);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        byte cr = 220,
            cg = 220,
            cb = 220;
        var name = Path.GetFileNameWithoutExtension(filename).ToLower();
        if (name.Contains("01") || name.Contains("red"))
        {
            cr = 220;
            cg = 40;
            cb = 40;
        }
        else if (name.Contains("02") || name.Contains("green"))
        {
            cr = 40;
            cg = 220;
            cb = 40;
        }
        else if (name.Contains("03") || name.Contains("blue"))
        {
            cr = 40;
            cg = 40;
            cb = 220;
        }
        else if (name.Contains("04") || name.Contains("yellow"))
        {
            cr = 220;
            cg = 220;
            cb = 40;
        }

        byte[] header = new byte[18];
        header[2] = 2;
        header[12] = (byte)(size & 0xFF);
        header[13] = (byte)((size >> 8) & 0xFF);
        header[14] = (byte)(size & 0xFF);
        header[15] = (byte)((size >> 8) & 0xFF);
        header[16] = 32;
        header[17] = 0x28;
        byte[] pixels = new byte[size * size * 4];
        int cell = size / 8;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool lit = (((x / cell) + (y / cell)) & 1) == 0;
            int i = (y * size + x) * 4;
            pixels[i + 0] = lit ? cb : (byte)20; // B
            pixels[i + 1] = lit ? cg : (byte)20; // G
            pixels[i + 2] = lit ? cr : (byte)20; // R
            pixels[i + 3] = 255;
        }
        using var fs = File.Create(filename);
        fs.Write(header, 0, header.Length);
        fs.Write(pixels, 0, pixels.Length);
    }
}

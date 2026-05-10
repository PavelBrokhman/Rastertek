using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial34.Graphics;

public class Texture
{
    private uint _id;
    private bool _loaded;

    public unsafe bool Initialize(GL4 gl, string filename, uint textureUnit, bool wrap)
    {
        var glApi = gl.Driver;
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Texture not found: {filename}");
            return false;
        }
        if (!LoadTga(filename, out int width, out int height, out byte[] pixels))
        {
            Console.WriteLine($"Failed: {filename}");
            return false;
        }
        Console.WriteLine($"Loaded {filename} ({width}x{height})");
        glApi.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        _id = glApi.GenTexture();
        glApi.BindTexture(TextureTarget.Texture2D, _id);
        fixed (byte* p = pixels)
            glApi.TexImage2D(
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
        var wrapMode = wrap ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge;
        glApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrapMode);
        glApi.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrapMode);
        glApi.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
        );
        glApi.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear
        );
        glApi.GenerateMipmap(TextureTarget.Texture2D);
        _loaded = true;
        return true;
    }

    public void SetTexture(GL4 gl, uint textureUnit)
    {
        if (_loaded)
        {
            gl.Driver.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
            gl.Driver.BindTexture(TextureTarget.Texture2D, _id);
        }
    }

    public void Shutdown(GL4 gl)
    {
        if (_loaded)
        {
            gl.Driver.DeleteTexture(_id);
            _loaded = false;
        }
    }

    static bool LoadTga(string filename, out int width, out int height, out byte[] rgbaData)
    {
        width = 0;
        height = 0;
        rgbaData = null;
        var fileData = File.ReadAllBytes(filename);
        if (fileData.Length < 18)
            return false;
        int imageIdLength = fileData[0];
        int imageType = fileData[2];
        int bpp = fileData[16];
        width = fileData[12] | (fileData[13] << 8);
        height = fileData[14] | (fileData[15] << 8);
        if (imageType != 2 || (bpp != 24 && bpp != 32))
            return false;
        int pixelDataOffset = 18 + imageIdLength;
        int channels = bpp / 8;
        int pixelCount = width * height;
        if (fileData.Length < pixelDataOffset + pixelCount * channels)
            return false;
        rgbaData = new byte[pixelCount * 4];
        // TGA stores rows bottom-to-top. Our LH projection maps V=0 to the visual top,
        // so we flip vertically here to compensate (C++ GL skips this flip because V=0=bottom).
        for (int destRow = 0; destRow < height; destRow++)
        {
            int srcRow = height - 1 - destRow;
            int srcOffset = pixelDataOffset + srcRow * width * channels;
            int destOffset = destRow * width * 4;
            for (int x = 0; x < width; x++)
            {
                rgbaData[destOffset + x * 4 + 0] = fileData[srcOffset + x * channels + 2]; // R <- B
                rgbaData[destOffset + x * 4 + 1] = fileData[srcOffset + x * channels + 1]; // G <- G
                rgbaData[destOffset + x * 4 + 2] = fileData[srcOffset + x * channels + 0]; // B <- R
                rgbaData[destOffset + x * 4 + 3] = channels == 4 ? fileData[srcOffset + x * channels + 3] : (byte)255;
            }
        }
        return true;
    }
}

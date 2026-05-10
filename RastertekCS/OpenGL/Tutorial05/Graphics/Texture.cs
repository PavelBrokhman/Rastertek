using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial05.Graphics;

public class Texture
{
    private uint _textureId;
    private int _width;
    private int _height;
    private bool _loaded;

    public bool Initialize(GL4 OpenGL, string filename, bool wrap)
    {
        // Load the texture from the file.
        if (!LoadTarga32Bit(OpenGL, filename, wrap))
            return false;

        // Set that the texture is loaded.
        _loaded = true;
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        if (_loaded)
        {
            OpenGL.Driver.DeleteTexture(_textureId);
            _loaded = false;
        }
    }

    private unsafe bool LoadTarga32Bit(GL4 OpenGL, string filename, bool wrap)
    {
        var gl = OpenGL.Driver;

        // Если файл отсутствует, генерируем шахматный паттерн TGA на лету.
        if (!File.Exists(filename))
            GenerateCheckerboardTga(filename, 64);

        // Open the targa file for reading in binary.
        using var fs = File.OpenRead(filename);
        using var reader = new BinaryReader(fs);

        // Read in the file header (18 bytes).
        byte[] header = reader.ReadBytes(18);
        if (header.Length != 18)
            return false;

        // Get the important information from the header.
        _width = header[12] | (header[13] << 8);
        _height = header[14] | (header[15] << 8);
        int bpp = header[16];

        // Check that it is 32 bit and not 24 bit.
        if (bpp != 32)
            return false;

        // Calculate the size of the 32 bit image data.
        int imageSize = _width * _height * 4;

        // Read in the targa image data.
        byte[] targaImage = reader.ReadBytes(imageSize);
        if (targaImage.Length != imageSize)
            return false;

        // Allocate memory for the targa destination data.
        byte[] targaData = new byte[imageSize];

        // Initialize the index into the targa destination data array.
        int index = 0;

        // Now copy the targa image data into the targa destination array in the correct order since the targa format is not stored in the RGBA order.
        for (int j = 0; j < _height; j++)
        {
            for (int i = 0; i < _width; i++)
            {
                targaData[index + 0] = targaImage[index + 2]; // Red.
                targaData[index + 1] = targaImage[index + 1]; // Green.
                targaData[index + 2] = targaImage[index + 0]; // Blue.
                targaData[index + 3] = targaImage[index + 3]; // Alpha.

                index += 4;
            }
        }

        // Set the active texture unit in which to store the data.
        gl.ActiveTexture(TextureUnit.Texture0 + 0);

        // Generate an ID for the texture.
        _textureId = gl.GenTexture();

        // Bind the texture as a 2D texture.
        gl.BindTexture(TextureTarget.Texture2D, _textureId);

        // Load the image data into the texture unit.
        fixed (byte* p = targaData)
        {
            gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                (int)InternalFormat.Rgba,
                (uint)_width,
                (uint)_height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                p
            );
        }

        // Set the texture color to either wrap around or clamp to the edge.
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

        // Set the texture filtering.
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);

        // Generate mipmaps for the texture.
        gl.GenerateMipmap(TextureTarget.Texture2D);

        return true;
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        if (_loaded)
        {
            var gl = OpenGL.Driver;
            // Set the texture unit we are working with.
            gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
            // Bind the texture as a 2D texture.
            gl.BindTexture(TextureTarget.Texture2D, _textureId);
        }
    }

    public int GetWidth() => _width;
    public int GetHeight() => _height;

    // Генерирует 32-bit TGA шахматный паттерн (как замена для stone01.tga).
    private static void GenerateCheckerboardTga(string filename, int size)
    {
        var dir = Path.GetDirectoryName(filename);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

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
        {
            for (int x = 0; x < size; x++)
            {
                bool white = (((x / cell) + (y / cell)) & 1) == 0;
                int i = (y * size + x) * 4;
                pixels[i + 0] = white ? (byte)220 : (byte)40;
                pixels[i + 1] = white ? (byte)220 : (byte)40;
                pixels[i + 2] = white ? (byte)220 : (byte)40;
                pixels[i + 3] = 255;
            }
        }

        using var fs = File.Create(filename);
        fs.Write(header, 0, header.Length);
        fs.Write(pixels, 0, pixels.Length);
    }
}

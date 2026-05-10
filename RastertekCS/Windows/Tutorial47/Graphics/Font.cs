using System.Globalization;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace RastertekCS.Windows.Tutorial47.Graphics;

public unsafe class Font
{
    private struct FontType { public float left, right; public int size; }

    public struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private FontType[] _font;
    private Texture _texture;
    private float _fontHeight;
    private int _spaceSize;

    public bool Initialize(DX11 DirectX, int fontChoice)
    {
        string fontFilename, fontTextureFilename;
        switch (fontChoice)
        {
            default:
                fontFilename = "Data/font01.txt";
                fontTextureFilename = "Data/font01.tga";
                _fontHeight = 32.0f;
                _spaceSize = 3;
                break;
        }
        if (!LoadFontData(fontFilename)) return false;
        _texture = new Texture();
        return _texture.Initialize(DirectX, fontTextureFilename, false);
    }

    public void Shutdown()
    {
        _texture?.Shutdown();
        _texture = null;
        _font = null;
    }

    public ComPtr<ID3D11ShaderResourceView> GetTextureView() => _texture.GetTextureView();

    public int GetFontHeight() => (int)_fontHeight;

    public int GetSentencePixelLength(string sentence)
    {
        int pixelLength = 0;
        foreach (char c in sentence)
        {
            int letter = c - 32;
            if (letter == 0) pixelLength += _spaceSize;
            else pixelLength += _font[letter].size + 1;
        }
        return pixelLength;
    }

    public void BuildVertexArray(VertexType[] vertices, string sentence, float drawX, float drawY)
    {
        int index = 0;
        foreach (char c in sentence)
        {
            int letter = c - 32;
            if (letter == 0)
            {
                drawX += _spaceSize;
            }
            else
            {
                vertices[index++] = new VertexType { x = drawX, y = drawY, z = 0, tu = _font[letter].left, tv = 0 };
                vertices[index++] = new VertexType { x = drawX + _font[letter].size, y = drawY - _fontHeight, z = 0, tu = _font[letter].right, tv = 1 };
                vertices[index++] = new VertexType { x = drawX, y = drawY - _fontHeight, z = 0, tu = _font[letter].left, tv = 1 };
                vertices[index++] = new VertexType { x = drawX, y = drawY, z = 0, tu = _font[letter].left, tv = 0 };
                vertices[index++] = new VertexType { x = drawX + _font[letter].size, y = drawY, z = 0, tu = _font[letter].right, tv = 0 };
                vertices[index++] = new VertexType { x = drawX + _font[letter].size, y = drawY - _fontHeight, z = 0, tu = _font[letter].right, tv = 1 };
                drawX += _font[letter].size + 1.0f;
            }
        }
    }

    private bool LoadFontData(string filename)
    {
        if (!File.Exists(filename)) { Console.WriteLine($"Font file not found: {filename}"); return false; }
        _font = new FontType[95];
        var lines = File.ReadAllLines(filename);
        int i = 0;
        foreach (var line in lines)
        {
            if (i >= 95) break;
            if (line.Length == 0) continue;
            // Last 3 whitespace-separated tokens are: left, right, size.
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) continue;
            _font[i].left = float.Parse(parts[parts.Length - 3], CultureInfo.InvariantCulture);
            _font[i].right = float.Parse(parts[parts.Length - 2], CultureInfo.InvariantCulture);
            _font[i].size = int.Parse(parts[parts.Length - 1], CultureInfo.InvariantCulture);
            i++;
        }
        return i == 95;
    }
}

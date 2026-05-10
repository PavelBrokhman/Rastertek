using System.Globalization;

namespace RastertekCS.OpenGL.Tutorial47.Graphics;

public class Font
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

    public bool Initialize(GL4 OpenGL, int fontChoice)
    {
        string fontFilename, fontTextureFilename;
        switch (fontChoice)
        {
            default:
                fontFilename = "Data/font/font01.txt";
                fontTextureFilename = "Data/font/font01.tga";
                _fontHeight = 32.0f;
                _spaceSize = 3;
                break;
        }
        if (!LoadFontData(fontFilename)) return false;
        _texture = new Texture();
        return _texture.Initialize(OpenGL, fontTextureFilename, 0, false);
    }

    public void Shutdown(GL4 OpenGL)
    {
        _texture?.Shutdown(OpenGL);
        _texture = null;
        _font = null;
    }

    public void SetTexture(GL4 OpenGL, uint slot) => _texture.SetTexture(OpenGL, slot);

    public int GetFontHeight() => (int)_fontHeight;

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

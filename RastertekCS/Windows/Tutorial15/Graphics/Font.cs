using System.Globalization;

namespace RastertekCS.Windows.Tutorial15.Graphics;

public class Font
{
    public struct FontType
    {
        public float Left,
            Right;
        public int Size;
    }

    private FontType[] _font;
    private Texture _texture;
    private float _fontHeight;
    private int _spaceSize;

    public bool Initialize(DX11 DirectX, int fontChoice)
    {
        string fontFilename,
            fontTextureFilename;
        switch (fontChoice)
        {
            default:
            case 0:
                fontFilename = "Data/font/font01.txt";
                fontTextureFilename = "Data/font/font01.tga";
                _fontHeight = 32.0f;
                _spaceSize = 3;
                break;
        }

        if (!LoadFontData(fontFilename))
            return false;

        _texture = new Texture();
        if (!_texture.Initialize(DirectX, fontTextureFilename, false))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _texture?.Shutdown();
        _texture = null;
        _font = null;
    }

    public void SetTexture(DX11 DirectX, uint slot) => _texture?.SetTexture(DirectX, slot);

    public int GetFontHeight() => (int)_fontHeight;

    public int GetSentencePixelLength(string sentence)
    {
        int length = 0;
        foreach (char c in sentence)
        {
            int letter = c - 32;
            if (letter == 0)
                length += _spaceSize;
            else if (letter > 0 && letter < 95)
                length += _font[letter].Size + 1;
        }
        return length;
    }

    // Writes 6 vertices per non-space character into vertices (each VertexType is 5 floats: x y z tu tv).
    public void BuildVertexArray(float[] vertices, string sentence, float drawX, float drawY)
    {
        int index = 0;
        foreach (char c in sentence)
        {
            int letter = c - 32;
            if (letter < 0 || letter >= 95)
                letter = 0;

            if (letter == 0)
            {
                drawX += _spaceSize;
                continue;
            }

            float left = _font[letter].Left;
            float right = _font[letter].Right;
            int size = _font[letter].Size;

            // Tri 1: TL, BR, BL
            vertices[index++] = drawX;
            vertices[index++] = drawY;
            vertices[index++] = 0;
            vertices[index++] = left;
            vertices[index++] = 0.0f;
            vertices[index++] = drawX + size;
            vertices[index++] = drawY - _fontHeight;
            vertices[index++] = 0;
            vertices[index++] = right;
            vertices[index++] = 1.0f;
            vertices[index++] = drawX;
            vertices[index++] = drawY - _fontHeight;
            vertices[index++] = 0;
            vertices[index++] = left;
            vertices[index++] = 1.0f;
            // Tri 2: TL, TR, BR
            vertices[index++] = drawX;
            vertices[index++] = drawY;
            vertices[index++] = 0;
            vertices[index++] = left;
            vertices[index++] = 0.0f;
            vertices[index++] = drawX + size;
            vertices[index++] = drawY;
            vertices[index++] = 0;
            vertices[index++] = right;
            vertices[index++] = 0.0f;
            vertices[index++] = drawX + size;
            vertices[index++] = drawY - _fontHeight;
            vertices[index++] = 0;
            vertices[index++] = right;
            vertices[index++] = 1.0f;

            drawX += size + 1.0f;
        }
    }

    private bool LoadFontData(string filename)
    {
        if (!File.Exists(filename))
            return false;

        _font = new FontType[95];
        var lines = File.ReadAllLines(filename);
        var inv = CultureInfo.InvariantCulture;

        // Format per line: "<ascii> <char> <left> <right> <size>"
        // Space line looks like: "32   0.0  0.0  0" (char missing/absent).
        for (int i = 0; i < lines.Length && i < 95; i++)
        {
            var t = lines[i]
                .Trim()
                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (t.Length < 4)
                continue;
            int off = t.Length >= 5 ? 2 : 1;
            _font[i].Left = float.Parse(t[off], inv);
            _font[i].Right = float.Parse(t[off + 1], inv);
            _font[i].Size = int.Parse(t[off + 2], inv);
        }

        return true;
    }
}

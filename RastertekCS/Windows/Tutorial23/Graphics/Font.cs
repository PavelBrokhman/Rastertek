using System.Globalization;

namespace RastertekCS.Windows.Tutorial23.Graphics;

public class Font
{
    public struct FontType
    {
        public float Left, Right;
        public int Size;
    }

    private FontType[] m_Font;
    private Texture m_Texture;
    private float m_fontHeight;
    private int m_spaceSize;

    public bool Initialize(DX11 DirectX, int fontChoice)
    {
        string fontFilename, fontTextureFilename;
        switch (fontChoice)
        {
            default:
            case 0:
                fontFilename = "Data/font/font01.txt";
                fontTextureFilename = "Data/font/font01.tga";
                m_fontHeight = 32.0f;
                m_spaceSize = 3;
                break;
        }

        if (!LoadFontData(fontFilename)) return false;

        m_Texture = new Texture();
        if (!m_Texture.Initialize(DirectX, fontTextureFilename, false)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_Texture?.Shutdown();
        m_Texture = null;
        m_Font = null;
    }

    public void SetTexture(DX11 DirectX, uint slot) => m_Texture?.SetTexture(DirectX, slot);

    public int GetFontHeight() => (int)m_fontHeight;

    public int GetSentencePixelLength(string sentence)
    {
        int length = 0;
        foreach (char c in sentence)
        {
            int letter = c - 32;
            if (letter == 0) length += m_spaceSize;
            else if (letter > 0 && letter < 95) length += m_Font[letter].Size + 1;
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
            if (letter < 0 || letter >= 95) letter = 0;

            if (letter == 0)
            {
                drawX += m_spaceSize;
                continue;
            }

            float left = m_Font[letter].Left;
            float right = m_Font[letter].Right;
            int size = m_Font[letter].Size;

            // Tri 1: TL, BR, BL
            vertices[index++] = drawX;              vertices[index++] = drawY;               vertices[index++] = 0;
            vertices[index++] = left;               vertices[index++] = 0.0f;
            vertices[index++] = drawX + size;       vertices[index++] = drawY - m_fontHeight; vertices[index++] = 0;
            vertices[index++] = right;              vertices[index++] = 1.0f;
            vertices[index++] = drawX;              vertices[index++] = drawY - m_fontHeight; vertices[index++] = 0;
            vertices[index++] = left;               vertices[index++] = 1.0f;
            // Tri 2: TL, TR, BR
            vertices[index++] = drawX;              vertices[index++] = drawY;               vertices[index++] = 0;
            vertices[index++] = left;               vertices[index++] = 0.0f;
            vertices[index++] = drawX + size;       vertices[index++] = drawY;               vertices[index++] = 0;
            vertices[index++] = right;              vertices[index++] = 0.0f;
            vertices[index++] = drawX + size;       vertices[index++] = drawY - m_fontHeight; vertices[index++] = 0;
            vertices[index++] = right;              vertices[index++] = 1.0f;

            drawX += size + 1.0f;
        }
    }

    private bool LoadFontData(string filename)
    {
        if (!File.Exists(filename)) return false;

        m_Font = new FontType[95];
        var lines = File.ReadAllLines(filename);
        var inv = CultureInfo.InvariantCulture;

        // Format per line: "<ascii> <char> <left> <right> <size>"
        // Space line looks like: "32   0.0  0.0  0" (char missing/absent).
        for (int i = 0; i < lines.Length && i < 95; i++)
        {
            var t = lines[i].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (t.Length < 4) continue;
            int off = t.Length >= 5 ? 2 : 1;
            m_Font[i].Left = float.Parse(t[off], inv);
            m_Font[i].Right = float.Parse(t[off + 1], inv);
            m_Font[i].Size = int.Parse(t[off + 2], inv);
        }

        return true;
    }
}

using System.Globalization;

namespace RastertekCS.OpenGL.Tutorial14.Graphics;

public class Font
{
    private struct FontType
    {
        public float Left,
            Right,
            Top,
            Bottom;
        public int Size;
    }

    private FontType[] m_Font;
    private Texture m_Texture;
    private float m_fontHeight;

    public bool Initialize(
        GL4 OpenGL,
        string fontDataFile,
        string fontTextureFile,
        uint textureUnit
    )
    {
        if (!File.Exists(fontDataFile) || !File.Exists(fontTextureFile))
            GenerateDefaultFont(fontDataFile, fontTextureFile);

        if (!LoadFontData(fontDataFile))
            return false;

        m_Texture = new Texture();
        if (!m_Texture.Initialize(OpenGL, fontTextureFile, textureUnit, false))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_Texture?.Shutdown(OpenGL);
        m_Texture = null;
        m_Font = null;
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit) =>
        m_Texture?.SetTexture(OpenGL, textureUnit);

    public float GetFontHeight() => m_fontHeight;

    public int GetSentencePixelLength(string sentence)
    {
        int length = 0;
        foreach (char c in sentence)
        {
            int idx = c - 32;
            if (idx >= 0 && idx < m_Font.Length)
                length += m_Font[idx].Size;
        }
        return length;
    }

    public void BuildVertexArray(float[] vertices, string sentence, float drawX, float drawY)
    {
        int idx = 0;
        foreach (char c in sentence)
        {
            int letter = c - 32;
            if (letter < 0 || letter >= m_Font.Length)
                letter = 0;

            if (c != ' ')
            {
                float left = drawX;
                float right = drawX + m_Font[letter].Size;
                float top = drawY;
                float bottom = drawY - m_fontHeight;
                float tl = m_Font[letter].Left;
                float tr = m_Font[letter].Right;
                float tt = m_Font[letter].Top;
                float tb = m_Font[letter].Bottom;

                // Tri 1
                vertices[idx++] = left;
                vertices[idx++] = top;
                vertices[idx++] = 0;
                vertices[idx++] = tl;
                vertices[idx++] = tt;
                vertices[idx++] = right;
                vertices[idx++] = bottom;
                vertices[idx++] = 0;
                vertices[idx++] = tr;
                vertices[idx++] = tb;
                vertices[idx++] = left;
                vertices[idx++] = bottom;
                vertices[idx++] = 0;
                vertices[idx++] = tl;
                vertices[idx++] = tb;
                // Tri 2
                vertices[idx++] = left;
                vertices[idx++] = top;
                vertices[idx++] = 0;
                vertices[idx++] = tl;
                vertices[idx++] = tt;
                vertices[idx++] = right;
                vertices[idx++] = top;
                vertices[idx++] = 0;
                vertices[idx++] = tr;
                vertices[idx++] = tt;
                vertices[idx++] = right;
                vertices[idx++] = bottom;
                vertices[idx++] = 0;
                vertices[idx++] = tr;
                vertices[idx++] = tb;
            }

            drawX += m_Font[letter].Size + 1.0f;
        }
    }

    private bool LoadFontData(string filename)
    {
        var lines = File.ReadAllLines(filename);
        m_Font = new FontType[95]; // ASCII 32-126
        m_fontHeight = 32.0f;

        foreach (var line in lines)
        {
            var t = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (t.Length < 4)
                continue;
            int ascii = int.Parse(t[0], CultureInfo.InvariantCulture);
            int idx = ascii - 32;
            if (idx < 0 || idx >= 95)
                continue;
            // Format: "ascii char left right size" or "ascii left right size" (for space)
            int off = t.Length >= 5 ? 2 : 1;
            m_Font[idx].Left = float.Parse(t[off], CultureInfo.InvariantCulture);
            m_Font[idx].Right = float.Parse(t[off + 1], CultureInfo.InvariantCulture);
            m_Font[idx].Size = int.Parse(t[off + 2], CultureInfo.InvariantCulture);
            m_Font[idx].Top = 0.0f;
            m_Font[idx].Bottom = 1.0f;
        }
        return true;
    }

    // Generate a simple 256x256 monospaced bitmap font (16x16 grid, cells 16x16px).
    private static void GenerateDefaultFont(string dataFile, string textureFile)
    {
        var dataDir = Path.GetDirectoryName(dataFile);
        if (!string.IsNullOrEmpty(dataDir))
            Directory.CreateDirectory(dataDir);
        var texDir = Path.GetDirectoryName(textureFile);
        if (!string.IsNullOrEmpty(texDir))
            Directory.CreateDirectory(texDir);

        int texSize = 256,
            cols = 16,
            cellW = texSize / cols,
            cellH = 16;
        var inv = CultureInfo.InvariantCulture;

        using (var w = new StreamWriter(dataFile))
        {
            for (int i = 0; i < 95; i++)
            {
                int ascii = 32 + i;
                char ch = (char)ascii;
                int col = i % cols;
                int row = i / cols;
                float leftU = (float)(col * cellW) / texSize;
                float rightU = (float)((col + 1) * cellW) / texSize;
                w.WriteLine(
                    $"{ascii} {ch} {leftU.ToString("F6", inv)} {rightU.ToString("F6", inv)} {cellW - 2}"
                );
            }
        }

        // TGA 256x96 (6 rows * 16px)
        int texH = 6 * cellH;
        byte[] header = new byte[18];
        header[2] = 2;
        header[12] = (byte)(texSize & 0xFF);
        header[13] = (byte)((texSize >> 8) & 0xFF);
        header[14] = (byte)(texH & 0xFF);
        header[15] = (byte)((texH >> 8) & 0xFF);
        header[16] = 32;
        header[17] = 0x28;
        byte[] px = new byte[texSize * texH * 4];

        for (int i = 0; i < 95; i++)
        {
            int col = i % cols;
            int row = i / cols;
            int cx = col * cellW;
            int cy = row * cellH;
            // White block with 2px border
            for (int y = 2; y < cellH - 2; y++)
            for (int x = 2; x < cellW - 2; x++)
            {
                int pi = ((cy + y) * texSize + (cx + x)) * 4;
                px[pi + 0] = 255;
                px[pi + 1] = 255;
                px[pi + 2] = 255;
                px[pi + 3] = 255;
            }
        }

        using var fs = File.Create(textureFile);
        fs.Write(header);
        fs.Write(px);
    }
}

namespace RastertekCS.OpenGL.Tutorial54.Graphics;

public class ParallaxScroll
{
    private Texture[] m_textures;
    private float[] m_scrollSpeed;
    private float[] m_translation;
    private int m_textureCount;

    public bool Initialize(GL4 OpenGL, string configFilename)
    {
        if (!File.Exists(configFilename)) { Console.WriteLine($"Config not found: {configFilename}"); return false; }
        var lines = File.ReadAllLines(configFilename);
        // First non-empty line: "Count: N"
        int li = 0;
        while (li < lines.Length && string.IsNullOrWhiteSpace(lines[li])) li++;
        if (li >= lines.Length) return false;
        var countLine = lines[li].Trim(); li++;
        var colon = countLine.IndexOf(':');
        if (colon < 0 || !int.TryParse(countLine[(colon + 1)..].Trim(), out m_textureCount)) return false;
        if (m_textureCount < 1 || m_textureCount > 64) return false;

        m_textures = new Texture[m_textureCount];
        m_scrollSpeed = new float[m_textureCount];
        m_translation = new float[m_textureCount];

        for (int i = 0; i < m_textureCount; i++)
        {
            while (li < lines.Length && string.IsNullOrWhiteSpace(lines[li])) li++;
            if (li >= lines.Length) return false;
            var parts = lines[li].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            li++;
            if (parts.Length < 2) return false;
            var file = parts[0];
            if (!float.TryParse(parts[1], global::System.Globalization.NumberStyles.Float, global::System.Globalization.CultureInfo.InvariantCulture, out var speed)) return false;
            m_textures[i] = new Texture();
            if (!m_textures[i].Initialize(OpenGL, file, 0, true)) return false;
            m_scrollSpeed[i] = speed;
            m_translation[i] = 0.0f;
        }
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        if (m_textures != null) { for (int i = 0; i < m_textureCount; i++) m_textures[i]?.Shutdown(OpenGL); m_textures = null; }
    }

    public void Frame(float frameTime)
    {
        for (int i = 0; i < m_textureCount; i++)
        {
            m_translation[i] += frameTime * m_scrollSpeed[i];
            if (m_translation[i] > 1.0f) m_translation[i] -= 1.0f;
        }
    }

    public int GetTextureCount() => m_textureCount;
    public void SetTexture(GL4 OpenGL, int index, uint unit) => m_textures[index].SetTexture(OpenGL, unit);
    public float GetTranslation(int index) => m_translation[index];
}

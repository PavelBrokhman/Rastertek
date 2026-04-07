namespace RastertekCS.OpenGL.Tutorial53.Graphics;

public class Heat
{
    private Texture m_noiseTexture;
    private float m_noiseFrameTime;
    private float[] m_scrollSpeeds = { 1.3f, 2.1f, 2.3f };
    private float[] m_scales = { 1.0f, 2.0f, 3.0f };
    private float[] m_distortion1 = { 0.1f, 0.2f };
    private float[] m_distortion2 = { 0.1f, 0.3f };
    private float[] m_distortion3 = { 0.1f, 0.1f };
    private float m_emissiveMultiplier = 0.35f;

    public bool Initialize(GL4 OpenGL)
    {
        m_noiseTexture = new Texture();
        if (!m_noiseTexture.Initialize(OpenGL, "Data/heatnoise01.tga", 0, true)) return false;
        return true;
    }

    public void Shutdown(GL4 OpenGL) { m_noiseTexture?.Shutdown(OpenGL); }

    public void Frame(float frameTime) { m_noiseFrameTime += frameTime * 0.3f; if (m_noiseFrameTime > 1000.0f) m_noiseFrameTime = 0.0f; }

    public void SetTexture(GL4 OpenGL, uint unit) => m_noiseTexture?.SetTexture(OpenGL, unit);

    public void GetNoiseValues(out float[] scrollSpeeds, out float[] scales, out float[] d1, out float[] d2, out float[] d3, out float emissive, out float noiseTime)
    {
        scrollSpeeds = m_scrollSpeeds; scales = m_scales;
        d1 = m_distortion1; d2 = m_distortion2; d3 = m_distortion3;
        emissive = m_emissiveMultiplier; noiseTime = m_noiseFrameTime;
    }
}

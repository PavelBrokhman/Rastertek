using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial55.Graphics;

public class ParticleSystem
{
    private struct VertexType { public float x, y, z, tu, tv, lifeTime, scroll1X, scroll1Y; }

    private struct ParticleType
    {
        public float positionX, positionY, positionZ;
        public bool active;
        public float lifeTime, scroll1X, scroll1Y;
    }

    private GL4 m_OpenGL;
    private string m_configFilename;

    private int m_maxParticles, m_currentParticleCount;
    private float m_particlesPerSecond, m_particleSize, m_particleLifeTime;
    private float m_accumulatedTime, m_emitAngle;
    private string m_textureFilename;

    private ParticleType[] m_particleList;
    private VertexType[] m_vertices;
    private int m_vertexCount, m_indexCount;
    private uint m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    private Texture m_Texture;

    private static readonly Random Rng = new();

    public bool Initialize(GL4 OpenGL, string configFilename)
    {
        m_OpenGL = OpenGL;
        m_configFilename = configFilename;
        if (!LoadParticleConfiguration()) return false;
        InitializeParticleSystem();
        InitializeBuffers();
        if (!LoadTexture()) return false;
        return true;
    }

    public void Shutdown()
    {
        m_Texture?.Shutdown(m_OpenGL); m_Texture = null;
        ShutdownBuffers();
        m_particleList = null;
    }

    public bool Reload()
    {
        Shutdown();
        if (!LoadParticleConfiguration()) return false;
        InitializeParticleSystem();
        InitializeBuffers();
        return LoadTexture();
    }

    public void Frame(float frameTime)
    {
        KillParticles();
        EmitParticles(frameTime);
        UpdateParticles(frameTime);
        UpdateBuffers();
    }

    public unsafe void Render()
    {
        m_Texture.SetTexture(m_OpenGL, 0);
        m_OpenGL.Gl.BindVertexArray(m_vertexArrayId);
        m_OpenGL.Gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    private bool LoadParticleConfiguration()
    {
        if (!File.Exists(m_configFilename)) { Console.WriteLine($"Config not found: {m_configFilename}"); return false; }
        var lines = File.ReadAllLines(m_configFilename);
        if (lines.Length < 5) return false;
        m_maxParticles      = int.Parse(ValueAfterColon(lines[0]));
        m_particlesPerSecond = float.Parse(ValueAfterColon(lines[1]), global::System.Globalization.CultureInfo.InvariantCulture);
        m_particleSize       = float.Parse(ValueAfterColon(lines[2]), global::System.Globalization.CultureInfo.InvariantCulture);
        m_particleLifeTime   = float.Parse(ValueAfterColon(lines[3]), global::System.Globalization.CultureInfo.InvariantCulture);
        m_textureFilename    = ValueAfterColon(lines[4]);
        return true;

        static string ValueAfterColon(string s) { var i = s.IndexOf(':'); return i < 0 ? s.Trim() : s[(i + 1)..].Trim(); }
    }

    private void InitializeParticleSystem()
    {
        m_particleList = new ParticleType[m_maxParticles];
        for (int i = 0; i < m_maxParticles; i++) m_particleList[i].active = false;
        m_accumulatedTime = 0.0f;
        m_currentParticleCount = 0;
        m_emitAngle = 0.0f;
    }

    private void EmitParticles(float frameTime)
    {
        m_emitAngle += frameTime * 2.0f;
        float originX = MathF.Sin(m_emitAngle);
        float originY = MathF.Cos(m_emitAngle);
        float originZ = 0.0f;

        m_accumulatedTime += frameTime;
        bool emit = false;
        if (m_accumulatedTime > (1.0f / m_particlesPerSecond)) { m_accumulatedTime = 0.0f; emit = true; }

        if (emit && m_currentParticleCount < (m_maxParticles - 1))
        {
            m_currentParticleCount++;
            float positionX = originX, positionY = originY, positionZ = originZ;
            float s = (float)Math.Abs(Rng.NextDouble() - Rng.NextDouble());
            float scroll1X = s, scroll1Y = s;

            // Sort: insert by Z (back-to-front)
            int index = 0; bool found = false;
            while (!found)
            {
                if (!m_particleList[index].active || m_particleList[index].positionZ < positionZ) found = true;
                else index++;
            }
            // Shift array from currentCount down to index to make room
            int i = m_currentParticleCount, j = i - 1;
            while (i != index) { CopyParticle(i, j); i--; j--; }

            m_particleList[index].positionX = positionX;
            m_particleList[index].positionY = positionY;
            m_particleList[index].positionZ = positionZ;
            m_particleList[index].active = true;
            m_particleList[index].lifeTime = m_particleLifeTime;
            m_particleList[index].scroll1X = scroll1X;
            m_particleList[index].scroll1Y = scroll1Y;
        }
    }

    private void UpdateParticles(float frameTime)
    {
        for (int i = 0; i < m_currentParticleCount; i++)
        {
            m_particleList[i].lifeTime -= frameTime;
            m_particleList[i].scroll1X += frameTime * 0.5f;
            if (m_particleList[i].scroll1X > 1.0f) m_particleList[i].scroll1X -= 1.0f;
            m_particleList[i].scroll1Y += frameTime * 0.5f;
            if (m_particleList[i].scroll1Y > 1.0f) m_particleList[i].scroll1Y -= 1.0f;
        }
    }

    private void KillParticles()
    {
        for (int i = 0; i < m_maxParticles; i++)
        {
            if (m_particleList[i].active && m_particleList[i].lifeTime <= 0.0f)
            {
                m_particleList[i].active = false;
                m_currentParticleCount--;
                for (int j = i; j < m_maxParticles - 1; j++) CopyParticle(j, j + 1);
            }
        }
    }

    private void CopyParticle(int dst, int src)
    {
        m_particleList[dst].positionX = m_particleList[src].positionX;
        m_particleList[dst].positionY = m_particleList[src].positionY;
        m_particleList[dst].positionZ = m_particleList[src].positionZ;
        m_particleList[dst].active    = m_particleList[src].active;
        m_particleList[dst].lifeTime  = m_particleList[src].lifeTime;
        m_particleList[dst].scroll1X  = m_particleList[src].scroll1X;
        m_particleList[dst].scroll1Y  = m_particleList[src].scroll1Y;
    }

    private unsafe void InitializeBuffers()
    {
        var gl = m_OpenGL.Gl;
        m_vertexCount = m_maxParticles * 6;
        m_indexCount = m_vertexCount;
        m_vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];
        for (int i = 0; i < m_indexCount; i++) indices[i] = (uint)i;

        m_vertexArrayId = gl.GenVertexArray(); gl.BindVertexArray(m_vertexArrayId);
        m_vertexBufferId = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = m_vertices) gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * m_vertexCount), p, BufferUsageARB.DynamicDraw);
        gl.EnableVertexAttribArray(0); gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1); gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(2); gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(5 * sizeof(float)));
        m_indexBufferId = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices) gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * m_indexCount), p, BufferUsageARB.StaticDraw);
    }

    private void ShutdownBuffers()
    {
        if (m_vertexArrayId == 0) return;
        var gl = m_OpenGL.Gl;
        gl.BindVertexArray(0); gl.DeleteVertexArray(m_vertexArrayId); m_vertexArrayId = 0;
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0); gl.DeleteBuffer(m_vertexBufferId); m_vertexBufferId = 0;
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0); gl.DeleteBuffer(m_indexBufferId); m_indexBufferId = 0;
        m_vertices = null;
    }

    private unsafe void UpdateBuffers()
    {
        for (int n = 0; n < m_vertexCount; n++) m_vertices[n] = default;
        int index = 0;
        for (int i = 0; i < m_currentParticleCount; i++)
        {
            float lt = m_particleList[i].lifeTime / m_particleLifeTime;
            float sx = m_particleList[i].scroll1X, sy = m_particleList[i].scroll1Y;
            float px = m_particleList[i].positionX, py = m_particleList[i].positionY, pz = m_particleList[i].positionZ;
            // BL
            m_vertices[index++] = new VertexType { x = px - m_particleSize, y = py - m_particleSize, z = pz, tu = 0, tv = 0, lifeTime = lt, scroll1X = sx, scroll1Y = sy };
            // TL
            m_vertices[index++] = new VertexType { x = px - m_particleSize, y = py + m_particleSize, z = pz, tu = 0, tv = 1, lifeTime = lt, scroll1X = sx, scroll1Y = sy };
            // BR
            m_vertices[index++] = new VertexType { x = px + m_particleSize, y = py - m_particleSize, z = pz, tu = 1, tv = 0, lifeTime = lt, scroll1X = sx, scroll1Y = sy };
            // BR
            m_vertices[index++] = new VertexType { x = px + m_particleSize, y = py - m_particleSize, z = pz, tu = 1, tv = 0, lifeTime = lt, scroll1X = sx, scroll1Y = sy };
            // TL
            m_vertices[index++] = new VertexType { x = px - m_particleSize, y = py + m_particleSize, z = pz, tu = 0, tv = 1, lifeTime = lt, scroll1X = sx, scroll1Y = sy };
            // TR
            m_vertices[index++] = new VertexType { x = px + m_particleSize, y = py + m_particleSize, z = pz, tu = 1, tv = 1, lifeTime = lt, scroll1X = sx, scroll1Y = sy };
        }
        var gl = m_OpenGL.Gl;
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = m_vertices)
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(sizeof(VertexType) * m_vertexCount), p);
    }

    private bool LoadTexture()
    {
        m_Texture = new Texture();
        return m_Texture.Initialize(m_OpenGL, m_textureFilename, 0, true);
    }
}

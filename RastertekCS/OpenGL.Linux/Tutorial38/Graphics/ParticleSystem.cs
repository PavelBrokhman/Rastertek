using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial38.Graphics;

public class ParticleSystem
{
    private struct VertexType { public float x, y, z, tu, tv, r, g, b, a; }
    private struct ParticleType { public float posX, posY, posZ, r, g, b, vel; public bool active; }

    private GL4 m_gl4;
    private Texture m_Texture;
    private ParticleType[] m_particles;
    private VertexType[] m_vertices;
    private uint m_vao, m_vbo, m_ibo;
    private int m_vertexCount, m_indexCount;
    private float m_devX, m_devY, m_devZ, m_velocity, m_velVar, m_size;
    private int m_perSec, m_maxParticles, m_currentCount;
    private float m_accTime;
    private Random m_rand = new Random();

    public unsafe bool Initialize(GL4 gl4, string texFn)
    {
        m_gl4 = gl4;
        m_Texture = new Texture();
        if (!m_Texture.Initialize(gl4, texFn, 0, false)) return false;
        InitParticleSystem();
        InitBuffers();
        return true;
    }

    public void Shutdown()
    {
        var gl = m_gl4.Gl;
        gl.BindVertexArray(0); gl.DeleteVertexArray(m_vao);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0); gl.DeleteBuffer(m_vbo);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0); gl.DeleteBuffer(m_ibo);
        m_Texture?.Shutdown(m_gl4); m_Texture = null;
        m_particles = null; m_vertices = null;
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
        m_Texture.SetTexture(m_gl4, 0);
        m_gl4.Gl.BindVertexArray(m_vao);
        m_gl4.Gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    private void InitParticleSystem()
    {
        m_devX = 0.5f; m_devY = 0.1f; m_devZ = 2.0f;
        m_velocity = 1.0f; m_velVar = 0.2f; m_size = 0.2f;
        m_perSec = 100; m_maxParticles = 1000;
        m_particles = new ParticleType[m_maxParticles];
        m_currentCount = 0; m_accTime = 0;
    }

    private unsafe void InitBuffers()
    {
        var gl = m_gl4.Gl;
        m_vertexCount = m_maxParticles * 6; m_indexCount = m_vertexCount;
        m_vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];
        for (int i = 0; i < m_indexCount; i++) indices[i] = (uint)i;

        m_vao = gl.GenVertexArray(); gl.BindVertexArray(m_vao);
        m_vbo = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vbo);
        fixed (VertexType* p = m_vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * m_vertexCount), p, BufferUsageARB.DynamicDraw);
        gl.EnableVertexAttribArray(0); gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1); gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(2); gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(5 * sizeof(float)));
        m_ibo = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_ibo);
        fixed (uint* p = indices) gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * m_indexCount), p, BufferUsageARB.StaticDraw);
    }

    private float RandDev() => (float)(m_rand.NextDouble() * 2.0 - 1.0);

    private void EmitParticles(float frameTime)
    {
        m_accTime += frameTime;
        bool emit = m_accTime > (1.0f / m_perSec);
        if (emit) m_accTime = 0;
        if (emit && m_currentCount < m_maxParticles - 1)
        {
            m_currentCount++;
            float px = RandDev() * m_devX, py = RandDev() * m_devY, pz = RandDev() * m_devZ;
            float vel = m_velocity + RandDev() * m_velVar;
            float cr = RandDev() * 0.5f + 0.5f, cg = RandDev() * 0.5f + 0.5f, cb = RandDev() * 0.5f + 0.5f;
            int idx = 0; while (idx < m_currentCount - 1 && m_particles[idx].active && m_particles[idx].posZ >= pz) idx++;
            for (int i = m_currentCount - 1; i > idx; i--) m_particles[i] = m_particles[i - 1];
            m_particles[idx] = new ParticleType { posX = px, posY = py, posZ = pz, r = cr, g = cg, b = cb, vel = vel, active = true };
        }
    }

    private void UpdateParticles(float frameTime)
    {
        for (int i = 0; i < m_currentCount; i++)
            m_particles[i].posY -= m_particles[i].vel * frameTime;
    }

    private void KillParticles()
    {
        for (int i = 0; i < m_currentCount; i++)
        {
            if (m_particles[i].active && m_particles[i].posY < -3.0f)
            {
                for (int j = i; j < m_currentCount - 1; j++) m_particles[j] = m_particles[j + 1];
                m_particles[m_currentCount - 1].active = false;
                m_currentCount--; i--;
            }
        }
    }

    private unsafe void UpdateBuffers()
    {
        Array.Clear(m_vertices, 0, m_vertexCount);
        int vi = 0;
        for (int i = 0; i < m_currentCount; i++)
        {
            ref var p = ref m_particles[i];
            float l = p.posX - m_size, r = p.posX + m_size, b = p.posY - m_size, t = p.posY + m_size;
            m_vertices[vi++] = new VertexType { x = l, y = b, z = p.posZ, tu = 0, tv = 0, r = p.r, g = p.g, b = p.b, a = 1 };
            m_vertices[vi++] = new VertexType { x = l, y = t, z = p.posZ, tu = 0, tv = 1, r = p.r, g = p.g, b = p.b, a = 1 };
            m_vertices[vi++] = new VertexType { x = r, y = b, z = p.posZ, tu = 1, tv = 0, r = p.r, g = p.g, b = p.b, a = 1 };
            m_vertices[vi++] = new VertexType { x = r, y = b, z = p.posZ, tu = 1, tv = 0, r = p.r, g = p.g, b = p.b, a = 1 };
            m_vertices[vi++] = new VertexType { x = l, y = t, z = p.posZ, tu = 0, tv = 1, r = p.r, g = p.g, b = p.b, a = 1 };
            m_vertices[vi++] = new VertexType { x = r, y = t, z = p.posZ, tu = 1, tv = 1, r = p.r, g = p.g, b = p.b, a = 1 };
        }
        var gl = m_gl4.Gl;
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vbo);
        fixed (VertexType* ptr = m_vertices)
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(sizeof(VertexType) * m_vertexCount), ptr);
    }
}

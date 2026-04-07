using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial52.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
        public float nx, ny, nz;
        public float tx, ty, tz;
        public float bx, by, bz;
    }

    private struct TempVertex { public float x, y, z, tu, tv; }
    private struct Vec3 { public float x, y, z; }

    private uint m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    private int m_vertexCount, m_indexCount;
    private Texture m_Texture1, m_Texture2, m_Texture3;

    public unsafe bool Initialize(GL4 OpenGL, string modelFilename, string tex1, string tex2, string tex3)
    {
        if (!LoadModel(modelFilename)) return false;
        CalculateModelVectors();
        if (!InitializeBuffers(OpenGL)) return false;
        m_Texture1 = new Texture(); if (!m_Texture1.Initialize(OpenGL, tex1, 0, true)) return false;
        m_Texture2 = new Texture(); if (!m_Texture2.Initialize(OpenGL, tex2, 1, true)) return false;
        m_Texture3 = new Texture(); if (!m_Texture3.Initialize(OpenGL, tex3, 2, true)) return false;
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_Texture3?.Shutdown(OpenGL); m_Texture2?.Shutdown(OpenGL); m_Texture1?.Shutdown(OpenGL);
        ShutdownBuffers(OpenGL);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        OpenGL.Gl.BindVertexArray(m_vertexArrayId);
        OpenGL.Gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    public void SetTextures(GL4 OpenGL, uint u1, uint u2, uint u3)
    {
        m_Texture1?.SetTexture(OpenGL, u1);
        m_Texture2?.SetTexture(OpenGL, u2);
        m_Texture3?.SetTexture(OpenGL, u3);
    }

    private float[] m_pos, m_tex, m_norm;
    private float[] m_tan, m_bin;
    private int m_modelVertexCount;

    private bool LoadModel(string filename)
    {
        if (!File.Exists(filename)) { Console.WriteLine($"Model not found: {filename}"); return false; }
        var lines = File.ReadAllLines(filename);
        int vc = 0, ds = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var l = lines[i].Trim();
            if (l.StartsWith("Vertex Count:")) vc = int.Parse(l.Substring("Vertex Count:".Length).Trim());
            if (l == "Data:") { ds = i + 1; break; }
        }
        if (vc == 0 || ds < 0) return false;
        m_vertexCount = vc; m_indexCount = vc; m_modelVertexCount = vc;
        m_pos = new float[vc * 3]; m_tex = new float[vc * 2]; m_norm = new float[vc * 3];
        m_tan = new float[vc * 3]; m_bin = new float[vc * 3];
        int vi = 0;
        for (int i = ds; i < lines.Length && vi < vc; i++)
        {
            var l = lines[i].Trim();
            if (string.IsNullOrEmpty(l)) continue;
            var p = l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length < 8) continue;
            m_pos[vi * 3] = float.Parse(p[0]); m_pos[vi * 3 + 1] = float.Parse(p[1]); m_pos[vi * 3 + 2] = float.Parse(p[2]);
            m_tex[vi * 2] = float.Parse(p[3]); m_tex[vi * 2 + 1] = 1.0f - float.Parse(p[4]);
            m_norm[vi * 3] = float.Parse(p[5]); m_norm[vi * 3 + 1] = float.Parse(p[6]); m_norm[vi * 3 + 2] = float.Parse(p[7]);
            vi++;
        }
        return vi == vc;
    }

    private void CalculateModelVectors()
    {
        int faceCount = m_modelVertexCount / 3;
        for (int f = 0; f < faceCount; f++)
        {
            int i0 = f * 3, i1 = f * 3 + 1, i2 = f * 3 + 2;
            float dx1 = m_pos[i1 * 3] - m_pos[i0 * 3], dy1 = m_pos[i1 * 3 + 1] - m_pos[i0 * 3 + 1], dz1 = m_pos[i1 * 3 + 2] - m_pos[i0 * 3 + 2];
            float dx2 = m_pos[i2 * 3] - m_pos[i0 * 3], dy2 = m_pos[i2 * 3 + 1] - m_pos[i0 * 3 + 1], dz2 = m_pos[i2 * 3 + 2] - m_pos[i0 * 3 + 2];
            float du1 = m_tex[i1 * 2] - m_tex[i0 * 2], dv1 = m_tex[i1 * 2 + 1] - m_tex[i0 * 2 + 1];
            float du2 = m_tex[i2 * 2] - m_tex[i0 * 2], dv2 = m_tex[i2 * 2 + 1] - m_tex[i0 * 2 + 1];
            float den = 1.0f / (du1 * dv2 - du2 * dv1 + 0.00001f);
            float tx = (dv2 * dx1 - dv1 * dx2) * den, ty = (dv2 * dy1 - dv1 * dy2) * den, tz = (dv2 * dz1 - dv1 * dz2) * den;
            float bx = (du1 * dx2 - du2 * dx1) * den, by = (du1 * dy2 - du2 * dy1) * den, bz = (du1 * dz2 - du2 * dz1) * den;
            float tl = MathF.Sqrt(tx * tx + ty * ty + tz * tz) + 0.00001f;
            tx /= tl; ty /= tl; tz /= tl;
            float bl = MathF.Sqrt(bx * bx + by * by + bz * bz) + 0.00001f;
            bx /= bl; by /= bl; bz /= bl;
            for (int k = 0; k < 3; k++)
            {
                int idx = f * 3 + k;
                m_tan[idx * 3] = tx; m_tan[idx * 3 + 1] = ty; m_tan[idx * 3 + 2] = tz;
                m_bin[idx * 3] = bx; m_bin[idx * 3 + 1] = by; m_bin[idx * 3 + 2] = bz;
            }
        }
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        var vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];
        for (int i = 0; i < m_vertexCount; i++)
        {
            vertices[i].x = m_pos[i * 3]; vertices[i].y = m_pos[i * 3 + 1]; vertices[i].z = m_pos[i * 3 + 2];
            vertices[i].tu = m_tex[i * 2]; vertices[i].tv = m_tex[i * 2 + 1];
            vertices[i].nx = m_norm[i * 3]; vertices[i].ny = m_norm[i * 3 + 1]; vertices[i].nz = m_norm[i * 3 + 2];
            vertices[i].tx = m_tan[i * 3]; vertices[i].ty = m_tan[i * 3 + 1]; vertices[i].tz = m_tan[i * 3 + 2];
            vertices[i].bx = m_bin[i * 3]; vertices[i].by = m_bin[i * 3 + 1]; vertices[i].bz = m_bin[i * 3 + 2];
            indices[i] = (uint)i;
        }
        m_vertexArrayId = gl.GenVertexArray(); gl.BindVertexArray(m_vertexArrayId);
        m_vertexBufferId = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = vertices) gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0); gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1); gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(2); gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(5 * sizeof(float)));
        gl.EnableVertexAttribArray(3); gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(8 * sizeof(float)));
        gl.EnableVertexAttribArray(4); gl.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(11 * sizeof(float)));
        m_indexBufferId = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices) gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);
        m_pos = null; m_tex = null; m_norm = null; m_tan = null; m_bin = null;
        return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        for (uint i = 0; i < 5; i++) gl.DisableVertexAttribArray(i);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0); gl.DeleteBuffer(m_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0); gl.DeleteBuffer(m_indexBufferId);
        gl.BindVertexArray(0); gl.DeleteVertexArray(m_vertexArrayId);
    }
}

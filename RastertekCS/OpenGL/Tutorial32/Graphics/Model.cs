using Silk.NET.OpenGL;
namespace RastertekCS.OpenGL.Tutorial32.Graphics;
public class Model
{
    private struct VT { public float x, y, z, tu, tv, nx, ny, nz; }
    private uint m_vao, m_vbo, m_ibo; private int m_vc, m_ic; private Texture[] m_tex; private float[] m_md;
    public unsafe bool Initialize(GL4 gl, string mf, string t1, bool w1, string t2 = null, bool w2 = false)
    {
        if (!LoadModel(mf)) return false; if (!InitBuf(gl)) return false;
        m_tex = new Texture[3];
        if (t1 != null) { m_tex[0] = new Texture(); if (!m_tex[0].Initialize(gl, t1, 0, w1)) return false; }
        if (t2 != null) { m_tex[1] = new Texture(); if (!m_tex[1].Initialize(gl, t2, 0, w2)) return false; }
        return true;
    }
    public void Shutdown(GL4 gl) { if (m_tex != null) { foreach (var t in m_tex) t?.Shutdown(gl); m_tex = null; } ShutBuf(gl); }
    public unsafe void Render(GL4 gl) { gl.Gl.BindVertexArray(m_vao); gl.Gl.DrawElements(PrimitiveType.Triangles, (uint)m_ic, DrawElementsType.UnsignedInt, (void*)0); }
    public void SetTexture1(GL4 gl, uint tu) { m_tex?[0]?.SetTexture(gl, tu); }
    public void SetTexture2(GL4 gl, uint tu) { m_tex?[1]?.SetTexture(gl, tu); }
    bool LoadModel(string fn) { if (!File.Exists(fn)) return false; var lines = File.ReadAllLines(fn); int vc = 0, ds = -1; for (int i = 0; i < lines.Length; i++) { var l = lines[i].Trim(); if (l.StartsWith("Vertex Count:")) vc = int.Parse(l.Substring(13).Trim()); if (l == "Data:") { ds = i + 1; break; } } if (vc == 0 || ds < 0) return false; m_vc = vc; m_ic = vc; m_md = new float[vc * 8]; int vi = 0; for (int i = ds; i < lines.Length && vi < vc; i++) { var l = lines[i].Trim(); if (string.IsNullOrEmpty(l)) continue; var p = l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries); if (p.Length < 8) continue; int o = vi * 8; m_md[o] = float.Parse(p[0]); m_md[o + 1] = float.Parse(p[1]); m_md[o + 2] = float.Parse(p[2]); m_md[o + 3] = float.Parse(p[3]); m_md[o + 4] = 1f - float.Parse(p[4]); m_md[o + 5] = float.Parse(p[5]); m_md[o + 6] = float.Parse(p[6]); m_md[o + 7] = float.Parse(p[7]); vi++; } return vi == vc; }
    unsafe bool InitBuf(GL4 gl) { var g = gl.Gl; var v = new VT[m_vc]; var idx = new uint[m_ic]; for (int i = 0; i < m_vc; i++) { int o = i * 8; v[i].x = m_md[o]; v[i].y = m_md[o + 1]; v[i].z = m_md[o + 2]; v[i].tu = m_md[o + 3]; v[i].tv = m_md[o + 4]; v[i].nx = m_md[o + 5]; v[i].ny = m_md[o + 6]; v[i].nz = m_md[o + 7]; idx[i] = (uint)i; } m_vao = g.GenVertexArray(); g.BindVertexArray(m_vao); m_vbo = g.GenBuffer(); g.BindBuffer(BufferTargetARB.ArrayBuffer, m_vbo); fixed (VT* p = v) g.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VT) * v.Length), p, BufferUsageARB.StaticDraw); g.EnableVertexAttribArray(0); g.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VT), (void*)0); g.EnableVertexAttribArray(1); g.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VT), (void*)(3 * sizeof(float))); g.EnableVertexAttribArray(2); g.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VT), (void*)(5 * sizeof(float))); m_ibo = g.GenBuffer(); g.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_ibo); fixed (uint* p = idx) g.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * idx.Length), p, BufferUsageARB.StaticDraw); m_md = null; return true; }
    void ShutBuf(GL4 gl) { var g = gl.Gl; g.DisableVertexAttribArray(0); g.DisableVertexAttribArray(1); g.DisableVertexAttribArray(2); g.BindBuffer(BufferTargetARB.ArrayBuffer, 0); g.DeleteBuffer(m_vbo); g.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0); g.DeleteBuffer(m_ibo); g.BindVertexArray(0); g.DeleteVertexArray(m_vao); }
}

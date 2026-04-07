using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial37.Graphics;

public class Model
{
    private struct VertexType { public float x, y, z, tu, tv, nx, ny, nz; }
    private uint m_vao, m_vbo, m_ibo; private int m_vertexCount, m_indexCount;
    private Texture m_Texture; private float[] m_data;

    public unsafe bool Initialize(GL4 gl4, string modelFn, string texFn)
    {
        if (!LoadModel(modelFn)) return false;
        if (!InitBuffers(gl4)) return false;
        m_Texture = new Texture();
        return m_Texture.Initialize(gl4, texFn, 0, true);
    }
    public void Shutdown(GL4 gl4) { m_Texture?.Shutdown(gl4); m_Texture = null; var gl = gl4.Gl; gl.DisableVertexAttribArray(0); gl.DisableVertexAttribArray(1); gl.DisableVertexAttribArray(2); gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0); gl.DeleteBuffer(m_vbo); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0); gl.DeleteBuffer(m_ibo); gl.BindVertexArray(0); gl.DeleteVertexArray(m_vao); }
    public unsafe void Render(GL4 gl4) { gl4.Gl.BindVertexArray(m_vao); gl4.Gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0); }
    public void SetTexture(GL4 gl4, uint tu) { m_Texture?.SetTexture(gl4, tu); }

    private bool LoadModel(string fn)
    {
        if (!File.Exists(fn)) { Console.WriteLine($"Model not found: {fn}"); return false; }
        var lines = File.ReadAllLines(fn); int vc = 0, ds = -1;
        for (int i = 0; i < lines.Length; i++) { var l = lines[i].Trim(); if (l.StartsWith("Vertex Count:")) vc = int.Parse(l.Substring(13).Trim()); if (l == "Data:") { ds = i + 1; break; } }
        if (vc == 0 || ds < 0) return false; m_vertexCount = vc; m_indexCount = vc; m_data = new float[vc * 8]; int vi = 0;
        for (int i = ds; i < lines.Length && vi < vc; i++) { var l = lines[i].Trim(); if (string.IsNullOrEmpty(l)) continue; var p = l.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries); if (p.Length < 8) continue; int o = vi*8; m_data[o]=float.Parse(p[0]); m_data[o+1]=float.Parse(p[1]); m_data[o+2]=float.Parse(p[2]); m_data[o+3]=float.Parse(p[3]); m_data[o+4]=1f-float.Parse(p[4]); m_data[o+5]=float.Parse(p[5]); m_data[o+6]=float.Parse(p[6]); m_data[o+7]=float.Parse(p[7]); vi++; }
        return vi == vc;
    }

    private unsafe bool InitBuffers(GL4 gl4)
    {
        var gl = gl4.Gl; var verts = new VertexType[m_vertexCount]; var idx = new uint[m_indexCount];
        for (int i = 0; i < m_vertexCount; i++) { int o = i*8; verts[i].x=m_data[o]; verts[i].y=m_data[o+1]; verts[i].z=m_data[o+2]; verts[i].tu=m_data[o+3]; verts[i].tv=m_data[o+4]; verts[i].nx=m_data[o+5]; verts[i].ny=m_data[o+6]; verts[i].nz=m_data[o+7]; idx[i]=(uint)i; }
        m_vao = gl.GenVertexArray(); gl.BindVertexArray(m_vao);
        m_vbo = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vbo);
        fixed (VertexType* p = verts) gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType)*verts.Length), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0); gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1); gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3*sizeof(float)));
        gl.EnableVertexAttribArray(2); gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(5*sizeof(float)));
        m_ibo = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_ibo);
        fixed (uint* p = idx) gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint)*idx.Length), p, BufferUsageARB.StaticDraw);
        m_data = null; return true;
    }
}

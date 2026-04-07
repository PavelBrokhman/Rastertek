using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial53.Graphics;

public class Model
{
    private struct VertexType { public float x, y, z, tu, tv, nx, ny, nz; }
    private uint m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    private int m_vertexCount, m_indexCount;
    private Texture m_Texture;

    public unsafe bool Initialize(GL4 OpenGL, string modelFile, string texFile, uint texUnit)
    {
        if (!LoadModel(modelFile)) return false;
        if (!InitializeBuffers(OpenGL)) return false;
        m_Texture = new Texture();
        if (!m_Texture.Initialize(OpenGL, texFile, texUnit, true)) return false;
        return true;
    }

    public void Shutdown(GL4 OpenGL) { m_Texture?.Shutdown(OpenGL); ShutdownBuffers(OpenGL); }

    public unsafe void Render(GL4 OpenGL) { OpenGL.Gl.BindVertexArray(m_vertexArrayId); OpenGL.Gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0); }

    public void SetTexture(GL4 OpenGL, uint u) => m_Texture?.SetTexture(OpenGL, u);

    private float[] m_modelData;
    private bool LoadModel(string f)
    {
        if (!File.Exists(f)) return false;
        var lines = File.ReadAllLines(f); int vc = 0, ds = -1;
        for (int i = 0; i < lines.Length; i++) { var l = lines[i].Trim(); if (l.StartsWith("Vertex Count:")) vc = int.Parse(l.Substring(13).Trim()); if (l == "Data:") { ds = i + 1; break; } }
        if (vc == 0 || ds < 0) return false;
        m_vertexCount = vc; m_indexCount = vc; m_modelData = new float[vc * 8]; int vi = 0;
        for (int i = ds; i < lines.Length && vi < vc; i++) { var l = lines[i].Trim(); if (string.IsNullOrEmpty(l)) continue;
            var p = l.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries); if (p.Length < 8) continue;
            int o = vi * 8; m_modelData[o]=float.Parse(p[0]); m_modelData[o+1]=float.Parse(p[1]); m_modelData[o+2]=float.Parse(p[2]);
            m_modelData[o+3]=float.Parse(p[3]); m_modelData[o+4]=1f-float.Parse(p[4]);
            m_modelData[o+5]=float.Parse(p[5]); m_modelData[o+6]=float.Parse(p[6]); m_modelData[o+7]=float.Parse(p[7]); vi++; }
        return vi == vc;
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl; var verts = new VertexType[m_vertexCount]; var idx = new uint[m_indexCount];
        for (int i = 0; i < m_vertexCount; i++) { int o = i * 8; verts[i].x=m_modelData[o]; verts[i].y=m_modelData[o+1]; verts[i].z=m_modelData[o+2];
            verts[i].tu=m_modelData[o+3]; verts[i].tv=m_modelData[o+4]; verts[i].nx=m_modelData[o+5]; verts[i].ny=m_modelData[o+6]; verts[i].nz=m_modelData[o+7]; idx[i]=(uint)i; }
        m_vertexArrayId = gl.GenVertexArray(); gl.BindVertexArray(m_vertexArrayId);
        m_vertexBufferId = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = verts) gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType)*verts.Length), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0); gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1); gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3*sizeof(float)));
        gl.EnableVertexAttribArray(2); gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(5*sizeof(float)));
        m_indexBufferId = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = idx) gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint)*idx.Length), p, BufferUsageARB.StaticDraw);
        m_modelData = null; return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
    { var gl = OpenGL.Gl; gl.DisableVertexAttribArray(0); gl.DisableVertexAttribArray(1); gl.DisableVertexAttribArray(2);
      gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0); gl.DeleteBuffer(m_vertexBufferId);
      gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0); gl.DeleteBuffer(m_indexBufferId);
      gl.BindVertexArray(0); gl.DeleteVertexArray(m_vertexArrayId); }
}

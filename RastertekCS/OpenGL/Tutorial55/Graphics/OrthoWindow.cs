using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial55.Graphics;

public class OrthoWindow
{
    private struct VT { public float x, y, z, tu, tv; }
    private uint m_vao, m_vbo, m_ibo;
    private int m_indexCount;

    public unsafe bool Initialize(GL4 OpenGL, int w, int h)
    {
        var gl = OpenGL.Gl; float l = -(w/2f), r2 = l+w, t = h/2f, b = t-h; m_indexCount = 6;
        var v = new VT[6]; var idx = new uint[6];
        v[0]=new VT{x=l,y=t,z=0,tu=0,tv=1}; v[1]=new VT{x=r2,y=b,z=0,tu=1,tv=0}; v[2]=new VT{x=l,y=b,z=0,tu=0,tv=0};
        v[3]=new VT{x=l,y=t,z=0,tu=0,tv=1}; v[4]=new VT{x=r2,y=t,z=0,tu=1,tv=1}; v[5]=new VT{x=r2,y=b,z=0,tu=1,tv=0};
        for (int i=0;i<6;i++) idx[i]=(uint)i;
        m_vao=gl.GenVertexArray(); gl.BindVertexArray(m_vao);
        m_vbo=gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vbo);
        fixed (VT* p=v) gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VT)*6), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0); gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VT), (void*)0);
        gl.EnableVertexAttribArray(1); gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VT), (void*)(3*sizeof(float)));
        m_ibo=gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_ibo);
        fixed (uint* p=idx) gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint)*6), p, BufferUsageARB.StaticDraw);
        return true;
    }

    public void Shutdown(GL4 OpenGL) { var gl=OpenGL.Gl; gl.DisableVertexAttribArray(0); gl.DisableVertexAttribArray(1);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer,0); gl.DeleteBuffer(m_vbo); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer,0); gl.DeleteBuffer(m_ibo);
        gl.BindVertexArray(0); gl.DeleteVertexArray(m_vao); }

    public unsafe void Render(GL4 OpenGL) { OpenGL.Gl.BindVertexArray(m_vao); OpenGL.Gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0); }
}

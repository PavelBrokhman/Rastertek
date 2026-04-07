using Silk.NET.OpenGL;
namespace RastertekCS.OpenGL.Tutorial37.Graphics;
public class OrthoWindow
{
    private struct VertexType { public float x,y,z,tu,tv; }
    private uint m_vao, m_vbo, m_ibo; private int m_indexCount;
    public unsafe bool Initialize(GL4 gl4, int ww, int wh)
    {
        var gl=gl4.Gl; float l=-(ww/2f),r=l+ww,t=wh/2f,b=t-wh; m_indexCount=6;
        var v=new VertexType[6]; var idx=new uint[6];
        v[0]=new(){x=l,y=t,z=0,tu=0,tv=1}; v[1]=new(){x=r,y=b,z=0,tu=1,tv=0}; v[2]=new(){x=l,y=b,z=0,tu=0,tv=0};
        v[3]=new(){x=l,y=t,z=0,tu=0,tv=1}; v[4]=new(){x=r,y=t,z=0,tu=1,tv=1}; v[5]=new(){x=r,y=b,z=0,tu=1,tv=0};
        for(int i=0;i<6;i++) idx[i]=(uint)i;
        m_vao=gl.GenVertexArray(); gl.BindVertexArray(m_vao);
        m_vbo=gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ArrayBuffer,m_vbo);
        fixed(VertexType*p=v) gl.BufferData(BufferTargetARB.ArrayBuffer,(nuint)(sizeof(VertexType)*6),p,BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0); gl.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false,(uint)sizeof(VertexType),(void*)0);
        gl.EnableVertexAttribArray(1); gl.VertexAttribPointer(1,2,VertexAttribPointerType.Float,false,(uint)sizeof(VertexType),(void*)(3*sizeof(float)));
        m_ibo=gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer,m_ibo);
        fixed(uint*p=idx) gl.BufferData(BufferTargetARB.ElementArrayBuffer,(nuint)(sizeof(uint)*6),p,BufferUsageARB.StaticDraw);
        return true;
    }
    public void Shutdown(GL4 gl4) { var gl=gl4.Gl; gl.BindVertexArray(0); gl.DeleteVertexArray(m_vao); gl.BindBuffer(BufferTargetARB.ArrayBuffer,0); gl.DeleteBuffer(m_vbo); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer,0); gl.DeleteBuffer(m_ibo); }
    public unsafe void Render(GL4 gl4) { gl4.Gl.BindVertexArray(m_vao); gl4.Gl.DrawElements(PrimitiveType.Triangles,(uint)m_indexCount,DrawElementsType.UnsignedInt,(void*)0); }
}

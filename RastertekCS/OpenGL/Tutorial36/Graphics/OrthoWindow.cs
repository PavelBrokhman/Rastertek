using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial36.Graphics;

public class OrthoWindow
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private uint m_vertexArrayId, m_vertexBufferId, m_indexBufferId;
    private int m_indexCount;

    public unsafe bool Initialize(GL4 gl4, int windowWidth, int windowHeight)
    {
        var gl = gl4.Gl;

        float left = -(windowWidth / 2f);
        float right = left + windowWidth;
        float top = windowHeight / 2f;
        float bottom = top - windowHeight;

        m_indexCount = 6;

        var vertices = new VertexType[6];
        var indices = new uint[6];

        // First triangle
        vertices[0] = new VertexType { x = left, y = top, z = 0, tu = 0, tv = 1 };
        vertices[1] = new VertexType { x = right, y = bottom, z = 0, tu = 1, tv = 0 };
        vertices[2] = new VertexType { x = left, y = bottom, z = 0, tu = 0, tv = 0 };
        // Second triangle
        vertices[3] = new VertexType { x = left, y = top, z = 0, tu = 0, tv = 1 };
        vertices[4] = new VertexType { x = right, y = top, z = 0, tu = 1, tv = 1 };
        vertices[5] = new VertexType { x = right, y = bottom, z = 0, tu = 1, tv = 0 };

        for (int i = 0; i < 6; i++) indices[i] = (uint)i;

        m_vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(m_vertexArrayId);

        m_vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * 6), p, BufferUsageARB.StaticDraw);

        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * 6), p, BufferUsageARB.StaticDraw);

        return true;
    }

    public void Shutdown(GL4 gl4)
    {
        var gl = gl4.Gl;
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(m_vertexArrayId);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(m_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(m_indexBufferId);
    }

    public unsafe void Render(GL4 gl4)
    {
        gl4.Gl.BindVertexArray(m_vertexArrayId);
        gl4.Gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }
}

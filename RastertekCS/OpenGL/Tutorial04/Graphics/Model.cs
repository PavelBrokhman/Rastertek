using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial04.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x, y, z;
        public float r, g, b;
    }

    private uint m_vertexArrayId;
    private uint m_vertexBufferId;
    private uint m_indexBufferId;
    private int m_indexCount;

    public unsafe bool Initialize(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        m_indexCount = 3;

        var vertices = new VertexType[]
        {
            new() { x = -1.0f, y = -1.0f, z = 0.0f, r = 0.0f, g = 1.0f, b = 0.0f },
            new() { x =  0.0f, y =  1.0f, z = 0.0f, r = 0.0f, g = 1.0f, b = 0.0f },
            new() { x =  1.0f, y = -1.0f, z = 0.0f, r = 0.0f, g = 1.0f, b = 0.0f },
        };
        var indices = new uint[] { 0, 1, 2 };

        m_vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(m_vertexArrayId);

        m_vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = vertices)
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.StaticDraw);
        }

        gl.EnableVertexAttribArray(0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
        {
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);
        }

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(m_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(m_indexBufferId);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(m_vertexArrayId);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.BindVertexArray(m_vertexArrayId);
        gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }
}

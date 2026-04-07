using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial25.Graphics;

public class DisplayPlane
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private uint m_vertexArrayId;
    private uint m_vertexBufferId;
    private uint m_indexBufferId;
    private int m_vertexCount;
    private int m_indexCount;

    public unsafe bool Initialize(GL4 OpenGL, float width, float height)
    {
        return InitializeBuffers(OpenGL, width, height);
    }

    public void Shutdown(GL4 OpenGL)
    {
        ShutdownBuffers(OpenGL);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.BindVertexArray(m_vertexArrayId);
        gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL, float width, float height)
    {
        var gl = OpenGL.Gl;

        m_vertexCount = 6;
        m_indexCount = m_vertexCount;

        var vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];

        // First triangle.
        vertices[0].x = -width;  vertices[0].y =  height; vertices[0].z = 0.0f;
        vertices[0].tu = 0.0f;  vertices[0].tv = 1.0f;

        vertices[1].x =  width;  vertices[1].y = -height; vertices[1].z = 0.0f;
        vertices[1].tu = 1.0f;  vertices[1].tv = 0.0f;

        vertices[2].x = -width;  vertices[2].y = -height; vertices[2].z = 0.0f;
        vertices[2].tu = 0.0f;  vertices[2].tv = 0.0f;

        // Second triangle.
        vertices[3].x = -width;  vertices[3].y =  height; vertices[3].z = 0.0f;
        vertices[3].tu = 0.0f;  vertices[3].tv = 1.0f;

        vertices[4].x =  width;  vertices[4].y =  height; vertices[4].z = 0.0f;
        vertices[4].tu = 1.0f;  vertices[4].tv = 1.0f;

        vertices[5].x =  width;  vertices[5].y = -height; vertices[5].z = 0.0f;
        vertices[5].tu = 1.0f;  vertices[5].tv = 0.0f;

        for (int i = 0; i < m_indexCount; i++)
            indices[i] = (uint)i;

        m_vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(m_vertexArrayId);

        m_vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.StaticDraw);

        // Attribute 0: position (3 floats)
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
            (uint)sizeof(VertexType), (void*)0);

        // Attribute 1: texcoord (2 floats)
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
            (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);

        return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
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
}

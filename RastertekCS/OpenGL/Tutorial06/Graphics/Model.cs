using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial06.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
        public float nx, ny, nz;
    }

    private uint m_vertexArrayId;
    private uint m_vertexBufferId;
    private uint m_indexBufferId;
    private int m_indexCount;
    private Texture m_Texture;

    public unsafe bool Initialize(GL4 OpenGL, string textureFilename, uint textureUnit, bool wrap)
    {
        var gl = OpenGL.Gl;
        m_indexCount = 3;

        var vertices = new VertexType[]
        {
            new() { x = -1, y = -1, z = 0, tu = 0.0f, tv = 0.0f, nx = 0, ny = 0, nz = -1 },
            new() { x =  0, y =  1, z = 0, tu = 0.5f, tv = 1.0f, nx = 0, ny = 0, nz = -1 },
            new() { x =  1, y = -1, z = 0, tu = 1.0f, tv = 0.0f, nx = 0, ny = 0, nz = -1 },
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

        // location 0 = position, 1 = texcoord, 2 = normal
        gl.EnableVertexAttribArray(0);
        gl.EnableVertexAttribArray(1);
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(5 * sizeof(float)));

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
        {
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);
        }

        m_Texture = new Texture();
        if (!m_Texture.Initialize(OpenGL, textureFilename, textureUnit, wrap)) return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_Texture?.Shutdown(OpenGL);
        m_Texture = null;
        var gl = OpenGL.Gl;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.DisableVertexAttribArray(2);
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

using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial50.Graphics;

public class OrthoWindow
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private uint _vertexarrayid, _vertexbufferid, _indexbufferid;
    private int _vertexcount, _indexcount;

    public unsafe bool Initialize(GL4 OpenGL, int windowWidth, int windowHeight)
    {
        var gl = OpenGL.Gl;

        float left = -(windowWidth / 2.0f);
        float right = left + windowWidth;
        float top = windowHeight / 2.0f;
        float bottom = top - windowHeight;

        _vertexcount = 6;
        _indexcount = 6;

        var vertices = new VertexType[6];
        var indices = new uint[6];

        // First triangle (CCW)
        vertices[0] = new VertexType { x = left, y = top, z = 0, tu = 0, tv = 1 };
        vertices[1] = new VertexType { x = right, y = bottom, z = 0, tu = 1, tv = 0 };
        vertices[2] = new VertexType { x = left, y = bottom, z = 0, tu = 0, tv = 0 };
        // Second triangle (CCW)
        vertices[3] = new VertexType { x = left, y = top, z = 0, tu = 0, tv = 1 };
        vertices[4] = new VertexType { x = right, y = top, z = 0, tu = 1, tv = 1 };
        vertices[5] = new VertexType { x = right, y = bottom, z = 0, tu = 1, tv = 0 };

        for (int i = 0; i < 6; i++) indices[i] = (uint)i;

        _vertexarrayid = gl.GenVertexArray();
        gl.BindVertexArray(_vertexarrayid);

        _vertexbufferid = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexbufferid);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.StaticDraw);

        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));

        _indexbufferid = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexbufferid);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(_vertexbufferid);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(_indexbufferid);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(_vertexarrayid);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.BindVertexArray(_vertexarrayid);
        gl.DrawElements(PrimitiveType.Triangles, (uint)_indexcount, DrawElementsType.UnsignedInt, (void*)0);
    }
}

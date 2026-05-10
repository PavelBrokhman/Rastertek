using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial46.Graphics;

public class OrthoWindow
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private uint _vao, _vbo, _ibo;
    private int _indexCount;

    public unsafe bool Initialize(GL4 OpenGL, int windowWidth, int windowHeight)
    {
        var gl = OpenGL.Driver;
        float left = -windowWidth / 2.0f;
        float right = left + windowWidth;
        float top = windowHeight / 2.0f;
        float bottom = top - windowHeight;

        _indexCount = 6;
        var vertices = new VertexType[6]
        {
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
            new() { x = left,  y = bottom, z = 0, tu = 0, tv = 1 },
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = top,    z = 0, tu = 1, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
        };
        var indices = new uint[] { 0, 1, 2, 3, 4, 5 };

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * 6), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)12);

        _ibo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ibo);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * 6), p, BufferUsageARB.StaticDraw);

        gl.BindVertexArray(0);
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.DeleteBuffer(_ibo);
        gl.DeleteBuffer(_vbo);
        gl.DeleteVertexArray(_vao);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.BindVertexArray(_vao);
        gl.DrawElements(PrimitiveType.Triangles, (uint)_indexCount, DrawElementsType.UnsignedInt, (void*)0);
        gl.BindVertexArray(0);
    }

    public int GetIndexCount() => _indexCount;
}

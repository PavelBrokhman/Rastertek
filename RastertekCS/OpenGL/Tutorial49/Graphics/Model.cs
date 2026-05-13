using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial49.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x, y, z;
        public float r, g, b;
    }

    private uint _vao, _vbo, _ibo;
    private int _indexCount;

    public unsafe bool Initialize(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        _indexCount = 3;

        var vertices = new VertexType[]
        {
            new() { x = -1.0f, y = -1.0f, z = 0.0f, r = 0.0f, g = 1.0f, b = 0.0f },
            new() { x =  0.0f, y =  1.0f, z = 0.0f, r = 0.0f, g = 1.0f, b = 0.0f },
            new() { x =  1.0f, y = -1.0f, z = 0.0f, r = 0.0f, g = 1.0f, b = 0.0f },
        };
        var indices = new uint[] { 0, 1, 2 };

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.StaticDraw);

        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)12);

        _ibo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ibo);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);

        gl.BindVertexArray(0);
        return true;
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.BindVertexArray(_vao);

        // Three control points per patch — matches the DX11 3-control-point patchlist topology.
        gl.PatchParameter(PatchParameterName.Vertices, 3);

        // Each patch is dispatched through the tessellator and rendered as triangles.
        gl.DrawElements(PrimitiveType.Patches, (uint)_indexCount, DrawElementsType.UnsignedInt, (void*)0);

        gl.BindVertexArray(0);
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.DeleteBuffer(_ibo);
        gl.DeleteBuffer(_vbo);
        gl.DeleteVertexArray(_vao);
    }

    public int GetIndexCount() => _indexCount;
}

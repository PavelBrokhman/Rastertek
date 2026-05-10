using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial37.Graphics;

public class OrthoWindow
{
    private struct VertexType
    {
        public float x,
            y,
            z,
            tu,
            tv;
    }

    private uint _vertexArrayId,
        _vertexBufferId,
        _indexBufferId;
    private int _indexCount;

    public unsafe bool Initialize(GL4 OpenGL, int windowWidth, int windowHeight)
    {
        var gl = OpenGL.Driver;
        float left = -(windowWidth / 2f);
        float right = left + windowWidth;
        float top = windowHeight / 2f;
        float bottom = top - windowHeight;
        _indexCount = 6;
        var vertices = new VertexType[6];
        var indices = new uint[6];
        vertices[0] = new VertexType { x = left, y = top, z = 0, tu = 0, tv = 1 };
        vertices[1] = new VertexType { x = right, y = bottom, z = 0, tu = 1, tv = 0 };
        vertices[2] = new VertexType { x = left, y = bottom, z = 0, tu = 0, tv = 0 };
        vertices[3] = new VertexType { x = left, y = top, z = 0, tu = 0, tv = 1 };
        vertices[4] = new VertexType { x = right, y = top, z = 0, tu = 1, tv = 1 };
        vertices[5] = new VertexType { x = right, y = bottom, z = 0, tu = 1, tv = 0 };
        for (int i = 0; i < 6; i++)
            indices[i] = (uint)i;

        _vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(_vertexArrayId);
        _vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * 6), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));
        _indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * 6), p, BufferUsageARB.StaticDraw);
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(_vertexArrayId);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(_indexBufferId);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        OpenGL.Driver.BindVertexArray(_vertexArrayId);
        OpenGL.Driver.DrawElements(PrimitiveType.Triangles, (uint)_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }
}

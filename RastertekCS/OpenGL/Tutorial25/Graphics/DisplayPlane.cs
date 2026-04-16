using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial25.Graphics;

public class DisplayPlane
{
    private struct VertexType
    {
        public float x,
            y,
            z;
        public float tu,
            tv;
    }

    private uint _vertexArrayId;
    private uint _vertexBufferId;
    private uint _indexBufferId;
    private int _vertexCount;
    private int _indexCount;

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
        gl.BindVertexArray(_vertexArrayId);
        gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)_indexCount,
            DrawElementsType.UnsignedInt,
            (void*)0
        );
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL, float width, float height)
    {
        var gl = OpenGL.Gl;

        _vertexCount = 6;
        _indexCount = _vertexCount;

        var vertices = new VertexType[_vertexCount];
        var indices = new uint[_indexCount];

        // First triangle.
        vertices[0].x = -width;
        vertices[0].y = height;
        vertices[0].z = 0.0f;
        vertices[0].tu = 0.0f;
        vertices[0].tv = 1.0f;

        vertices[1].x = width;
        vertices[1].y = -height;
        vertices[1].z = 0.0f;
        vertices[1].tu = 1.0f;
        vertices[1].tv = 0.0f;

        vertices[2].x = -width;
        vertices[2].y = -height;
        vertices[2].z = 0.0f;
        vertices[2].tu = 0.0f;
        vertices[2].tv = 0.0f;

        // Second triangle.
        vertices[3].x = -width;
        vertices[3].y = height;
        vertices[3].z = 0.0f;
        vertices[3].tu = 0.0f;
        vertices[3].tv = 1.0f;

        vertices[4].x = width;
        vertices[4].y = height;
        vertices[4].z = 0.0f;
        vertices[4].tu = 1.0f;
        vertices[4].tv = 1.0f;

        vertices[5].x = width;
        vertices[5].y = -height;
        vertices[5].z = 0.0f;
        vertices[5].tu = 1.0f;
        vertices[5].tv = 0.0f;

        for (int i = 0; i < _indexCount; i++)
            indices[i] = (uint)i;

        _vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(_vertexArrayId);

        _vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length),
                p,
                BufferUsageARB.StaticDraw
            );

        // Attribute 0: position (3 floats)
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)0
        );

        // Attribute 1: texcoord (2 floats)
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)(3 * sizeof(float))
        );

        _indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length),
                p,
                BufferUsageARB.StaticDraw
            );

        return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(_indexBufferId);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(_vertexArrayId);
    }
}

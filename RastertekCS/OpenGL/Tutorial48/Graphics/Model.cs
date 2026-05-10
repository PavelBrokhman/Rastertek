using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial48.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private struct InstanceType
    {
        public float x, y, z;
    }

    private uint _vao, _vbo, _ibo;
    private int _vertexCount;
    private int _instanceCount;
    private Texture _texture;

    public unsafe bool Initialize(GL4 OpenGL, string textureFilename)
    {
        var gl = OpenGL.Driver;
        _vertexCount = 3;
        _instanceCount = 4;

        var vertices = new VertexType[]
        {
            new() { x = -1.0f, y = -1.0f, z = 0.0f, tu = 0.0f, tv = 0.0f },
            new() { x =  0.0f, y =  1.0f, z = 0.0f, tu = 0.5f, tv = 1.0f },
            new() { x =  1.0f, y = -1.0f, z = 0.0f, tu = 1.0f, tv = 0.0f },
        };
        var instances = new InstanceType[]
        {
            new() { x = -1.5f, y = -1.5f, z = 5.0f },
            new() { x = -1.5f, y =  1.5f, z = 5.0f },
            new() { x =  1.5f, y = -1.5f, z = 5.0f },
            new() { x =  1.5f, y =  1.5f, z = 5.0f },
        };

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)12);

        _ibo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _ibo);
        fixed (InstanceType* p = instances)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(InstanceType) * instances.Length), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(InstanceType), (void*)0);
        gl.VertexAttribDivisor(2, 1);

        gl.BindVertexArray(0);

        _texture = new Texture();
        return _texture.Initialize(OpenGL, textureFilename, 0);
    }

    public void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.BindVertexArray(_vao);
        gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, (uint)_vertexCount, (uint)_instanceCount);
        gl.BindVertexArray(0);
    }

    public void SetTexture(GL4 OpenGL, uint slot) => _texture.SetTexture(OpenGL, slot);

    public void Shutdown(GL4 OpenGL)
    {
        _texture?.Shutdown(OpenGL); _texture = null;
        var gl = OpenGL.Driver;
        gl.DeleteBuffer(_ibo);
        gl.DeleteBuffer(_vbo);
        gl.DeleteVertexArray(_vao);
    }

    public int GetVertexCount() => _vertexCount;
    public int GetInstanceCount() => _instanceCount;
}

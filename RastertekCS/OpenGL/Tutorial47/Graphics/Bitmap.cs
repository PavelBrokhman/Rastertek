using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial47.Graphics;

public class Bitmap
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private uint _vao, _vbo, _ibo;
    private int _vertexCount, _indexCount;
    private int _screenWidth, _screenHeight;
    private int _bitmapWidth, _bitmapHeight;
    private int _renderX, _renderY;
    private int _prevPosX = -1, _prevPosY = -1;
    private Texture _texture;

    public unsafe bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight, string textureFilename, int renderX, int renderY)
    {
        _screenWidth = screenWidth; _screenHeight = screenHeight;
        _renderX = renderX; _renderY = renderY;
        _vertexCount = 6;
        _indexCount = 6;

        var gl = OpenGL.Driver;
        var indices = new uint[6] { 0, 1, 2, 3, 4, 5 };
        var emptyVerts = new VertexType[6];

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (VertexType* p = emptyVerts)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * 6), p, BufferUsageARB.DynamicDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)12);

        _ibo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ibo);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * 6), p, BufferUsageARB.StaticDraw);

        gl.BindVertexArray(0);

        _texture = new Texture();
        if (!_texture.Initialize(OpenGL, textureFilename, 0, false)) return false;
        _bitmapWidth = _texture.GetWidth();
        _bitmapHeight = _texture.GetHeight();
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _texture?.Shutdown(OpenGL); _texture = null;
        var gl = OpenGL.Driver;
        gl.DeleteBuffer(_ibo);
        gl.DeleteBuffer(_vbo);
        gl.DeleteVertexArray(_vao);
    }

    public unsafe bool Render(GL4 OpenGL)
    {
        if (!UpdateBuffers(OpenGL)) return false;
        var gl = OpenGL.Driver;
        gl.BindVertexArray(_vao);
        gl.DrawElements(PrimitiveType.Triangles, (uint)_indexCount, DrawElementsType.UnsignedInt, (void*)0);
        gl.BindVertexArray(0);
        return true;
    }

    public void SetTexture(GL4 OpenGL, uint slot) => _texture.SetTexture(OpenGL, slot);

    public int GetIndexCount() => _indexCount;

    public void SetRenderLocation(int x, int y) { _renderX = x; _renderY = y; }

    private unsafe bool UpdateBuffers(GL4 OpenGL)
    {
        if (_prevPosX == _renderX && _prevPosY == _renderY) return true;
        _prevPosX = _renderX; _prevPosY = _renderY;

        float left = (_screenWidth / 2) * -1.0f + _renderX;
        float right = left + _bitmapWidth;
        float top = (_screenHeight / 2) - (float)_renderY;
        float bottom = top - _bitmapHeight;

        var vertices = new VertexType[6]
        {
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
            new() { x = left,  y = bottom, z = 0, tu = 0, tv = 1 },
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = top,    z = 0, tu = 1, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
        };

        var gl = OpenGL.Driver;
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (VertexType* p = vertices)
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(sizeof(VertexType) * 6), p);
        return true;
    }
}

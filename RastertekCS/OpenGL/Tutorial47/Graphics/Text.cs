using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial47.Graphics;

public class Text
{
    private uint _vao, _vbo, _ibo;
    private int _vertexCount, _indexCount;
    private int _screenWidth, _screenHeight;
    private int _maxLength;
    private Vector4D<float> _pixelColor;

    public unsafe bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight, int maxLength,
        Font font, string text, int positionX, int positionY, float r, float g, float b)
    {
        _screenWidth = screenWidth; _screenHeight = screenHeight; _maxLength = maxLength;
        _vertexCount = 6 * maxLength;
        _indexCount = _vertexCount;

        var gl = OpenGL.Driver;
        var emptyVerts = new Font.VertexType[_vertexCount];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _indexCount; i++) indices[i] = (uint)i;

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (Font.VertexType* p = emptyVerts)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(Font.VertexType) * _vertexCount), p, BufferUsageARB.DynamicDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Font.VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Font.VertexType), (void*)12);

        _ibo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ibo);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * _indexCount), p, BufferUsageARB.StaticDraw);

        gl.BindVertexArray(0);

        return UpdateText(OpenGL, font, text, positionX, positionY, r, g, b);
    }

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.DeleteBuffer(_ibo);
        gl.DeleteBuffer(_vbo);
        gl.DeleteVertexArray(_vao);
    }

    public unsafe bool UpdateText(GL4 OpenGL, Font font, string text, int positionX, int positionY, float r, float g, float b)
    {
        if (text.Length > _maxLength) return false;
        _pixelColor = new Vector4D<float>(r, g, b, 1.0f);

        var vertices = new Font.VertexType[_vertexCount];
        float drawX = (-_screenWidth / 2.0f) + positionX;
        float drawY = (_screenHeight / 2.0f) - positionY;
        font.BuildVertexArray(vertices, text, drawX, drawY);

        var gl = OpenGL.Driver;
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (Font.VertexType* p = vertices)
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(sizeof(Font.VertexType) * _vertexCount), p);
        return true;
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.BindVertexArray(_vao);
        gl.DrawElements(PrimitiveType.Triangles, (uint)_indexCount, DrawElementsType.UnsignedInt, (void*)0);
        gl.BindVertexArray(0);
    }

    public int GetIndexCount() => _indexCount;
    public Vector4D<float> GetPixelColor() => _pixelColor;
}

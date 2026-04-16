using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial12.Graphics;

public class Bitmap
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
    private Texture _texture;

    private int _screenWidth,
        _screenHeight;
    private int _bitmapWidth,
        _bitmapHeight;
    private int _previousPosX = -1,
        _previousPosY = -1;

    public unsafe bool Initialize(
        GL4 OpenGL,
        int screenWidth,
        int screenHeight,
        string textureFilename,
        uint textureUnit,
        int bitmapWidth,
        int bitmapHeight
    )
    {
        var gl = OpenGL.Gl;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _bitmapWidth = bitmapWidth;
        _bitmapHeight = bitmapHeight;
        _vertexCount = 6;
        _indexCount = 6;

        // Вершины стартуют как нули — обновятся в Render() через UpdateBuffers.
        var vertices = new VertexType[_vertexCount];
        var indices = new uint[] { 0, 1, 2, 3, 4, 5 };

        _vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(_vertexArrayId);

        _vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length),
                p,
                BufferUsageARB.DynamicDraw
            );

        gl.EnableVertexAttribArray(0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)0
        );
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

        _texture = new Texture();
        if (!_texture.Initialize(OpenGL, textureFilename, textureUnit, false))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _texture?.Shutdown(OpenGL);
        _texture = null;
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

    public bool SetRenderLocation(int posX, int posY)
    {
        return posX != _previousPosX || posY != _previousPosY ? SetNewPos(posX, posY) : true;
    }

    private bool SetNewPos(int posX, int posY)
    {
        _previousPosX = posX;
        _previousPosY = posY;
        return true;
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;

        // Обновляем позиции вершин исходя из screen coords.
        float left = -((float)_screenWidth / 2.0f) + _previousPosX;
        float right = left + _bitmapWidth;
        float top = ((float)_screenHeight / 2.0f) - _previousPosY;
        float bottom = top - _bitmapHeight;

        var v = new VertexType[6]
        {
            new()
            {
                x = left,
                y = top,
                z = 0,
                tu = 0,
                tv = 0,
            }, // top-left
            new()
            {
                x = right,
                y = bottom,
                z = 0,
                tu = 1,
                tv = 1,
            }, // bottom-right
            new()
            {
                x = left,
                y = bottom,
                z = 0,
                tu = 0,
                tv = 1,
            }, // bottom-left
            new()
            {
                x = left,
                y = top,
                z = 0,
                tu = 0,
                tv = 0,
            }, // top-left
            new()
            {
                x = right,
                y = top,
                z = 0,
                tu = 1,
                tv = 0,
            }, // top-right
            new()
            {
                x = right,
                y = bottom,
                z = 0,
                tu = 1,
                tv = 1,
            }, // bottom-right
        };

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = v)
            gl.BufferSubData(
                BufferTargetARB.ArrayBuffer,
                0,
                (nuint)(sizeof(VertexType) * v.Length),
                p
            );

        gl.BindVertexArray(_vertexArrayId);
        gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)_indexCount,
            DrawElementsType.UnsignedInt,
            (void*)0
        );
    }

    public int GetIndexCount() => _indexCount;
}

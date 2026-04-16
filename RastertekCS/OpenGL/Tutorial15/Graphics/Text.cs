using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial15.Graphics;

public class Text
{
    private uint _vertexArrayId;
    private uint _vertexBufferId;
    private uint _indexBufferId;
    private int _vertexCount;
    private int _indexCount;
    private int _maxLength;
    private float _red,
        _green,
        _blue;

    public unsafe bool Initialize(
        GL4 OpenGL,
        Font font,
        string sentence,
        int posX,
        int posY,
        float r,
        float g,
        float b,
        int screenWidth,
        int screenHeight,
        int maxLength
    )
    {
        var gl = OpenGL.Gl;
        _maxLength = maxLength;
        _red = r;
        _green = g;
        _blue = b;

        _vertexCount = 6 * _maxLength;
        _indexCount = _vertexCount;
        int floatsPerVertex = 5; // x,y,z,tu,tv

        var vertices = new float[_vertexCount * floatsPerVertex];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _indexCount; i++)
            indices[i] = (uint)i;

        float startX = -(screenWidth / 2.0f) + posX;
        float startY = (screenHeight / 2.0f) - posY;
        font.BuildVertexArray(vertices, sentence, startX, startY);

        _vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(_vertexArrayId);

        _vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (float* p = vertices)
            gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(float) * vertices.Length),
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
            (uint)(floatsPerVertex * sizeof(float)),
            (void*)0
        );
        gl.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            (uint)(floatsPerVertex * sizeof(float)),
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

    public unsafe void UpdateText(
        GL4 OpenGL,
        Font font,
        string sentence,
        int posX,
        int posY,
        float r,
        float g,
        float b,
        int screenWidth,
        int screenHeight
    )
    {
        _red = r;
        _green = g;
        _blue = b;
        int floatsPerVertex = 5;
        var vertices = new float[_vertexCount * floatsPerVertex];
        float startX = -(screenWidth / 2.0f) + posX;
        float startY = (screenHeight / 2.0f) - posY;
        font.BuildVertexArray(vertices, sentence, startX, startY);

        var gl = OpenGL.Gl;
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (float* p = vertices)
            gl.BufferSubData(
                BufferTargetARB.ArrayBuffer,
                0,
                (nuint)(sizeof(float) * vertices.Length),
                p
            );
    }

    public void Shutdown(GL4 OpenGL)
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

    public float[] GetPixelColor() => new[] { _red, _green, _blue, 1.0f };

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
}

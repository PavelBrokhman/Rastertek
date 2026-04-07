using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial12.Graphics;

public class Bitmap
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private uint m_vertexArrayId;
    private uint m_vertexBufferId;
    private uint m_indexBufferId;
    private int m_vertexCount;
    private int m_indexCount;
    private Texture m_Texture;

    private int m_screenWidth, m_screenHeight;
    private int m_bitmapWidth, m_bitmapHeight;
    private int m_previousPosX = -1, m_previousPosY = -1;

    public unsafe bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight,
                                   string textureFilename, uint textureUnit,
                                   int bitmapWidth, int bitmapHeight)
    {
        var gl = OpenGL.Gl;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;
        m_bitmapWidth = bitmapWidth;
        m_bitmapHeight = bitmapHeight;
        m_vertexCount = 6;
        m_indexCount = 6;

        // Вершины стартуют как нули — обновятся в Render() через UpdateBuffers.
        var vertices = new VertexType[m_vertexCount];
        var indices = new uint[] { 0, 1, 2, 3, 4, 5 };

        m_vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(m_vertexArrayId);

        m_vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.DynamicDraw);

        gl.EnableVertexAttribArray(0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);

        m_Texture = new Texture();
        if (!m_Texture.Initialize(OpenGL, textureFilename, textureUnit, false)) return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_Texture?.Shutdown(OpenGL); m_Texture = null;
        var gl = OpenGL.Gl;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(m_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(m_indexBufferId);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(m_vertexArrayId);
    }

    public bool SetRenderLocation(int posX, int posY)
    {
        return posX != m_previousPosX || posY != m_previousPosY
            ? SetNewPos(posX, posY) : true;
    }

    private bool SetNewPos(int posX, int posY)
    {
        m_previousPosX = posX;
        m_previousPosY = posY;
        return true;
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;

        // Обновляем позиции вершин исходя из screen coords.
        float left = -((float)m_screenWidth / 2.0f) + m_previousPosX;
        float right = left + m_bitmapWidth;
        float top = ((float)m_screenHeight / 2.0f) - m_previousPosY;
        float bottom = top - m_bitmapHeight;

        var v = new VertexType[6]
        {
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 }, // top-left
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 }, // bottom-right
            new() { x = left,  y = bottom, z = 0, tu = 0, tv = 1 }, // bottom-left
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 }, // top-left
            new() { x = right, y = top,    z = 0, tu = 1, tv = 0 }, // top-right
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 }, // bottom-right
        };

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = v)
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                (nuint)(sizeof(VertexType) * v.Length), p);

        gl.BindVertexArray(m_vertexArrayId);
        gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    public int GetIndexCount() => m_indexCount;
}

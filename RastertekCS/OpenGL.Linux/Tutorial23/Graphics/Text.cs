using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class Text
{
    private uint m_vertexArrayId;
    private uint m_vertexBufferId;
    private uint m_indexBufferId;
    private int m_vertexCount;
    private int m_indexCount;
    private int m_maxLength;
    private float m_red, m_green, m_blue;

    public unsafe bool Initialize(GL4 OpenGL, Font font, string sentence,
                                   int posX, int posY, float r, float g, float b,
                                   int screenWidth, int screenHeight, int maxLength)
    {
        var gl = OpenGL.Gl;
        m_maxLength = maxLength;
        m_red = r; m_green = g; m_blue = b;

        m_vertexCount = 6 * m_maxLength;
        m_indexCount = m_vertexCount;
        int floatsPerVertex = 5;

        var vertices = new float[m_vertexCount * floatsPerVertex];
        var indices = new uint[m_indexCount];
        for (int i = 0; i < m_indexCount; i++) indices[i] = (uint)i;

        float startX = -(screenWidth / 2.0f) + posX;
        float startY = (screenHeight / 2.0f) - posY;
        font.BuildVertexArray(vertices, sentence, startX, startY);

        m_vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(m_vertexArrayId);

        m_vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (float* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(float) * vertices.Length), p, BufferUsageARB.DynamicDraw);

        gl.EnableVertexAttribArray(0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)(floatsPerVertex * sizeof(float)), (void*)0);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)(floatsPerVertex * sizeof(float)), (void*)(3 * sizeof(float)));

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);

        return true;
    }

    public unsafe void UpdateText(GL4 OpenGL, Font font, string sentence,
                                   int posX, int posY, float r, float g, float b,
                                   int screenWidth, int screenHeight)
    {
        m_red = r; m_green = g; m_blue = b;
        int floatsPerVertex = 5;
        var vertices = new float[m_vertexCount * floatsPerVertex];
        float startX = -(screenWidth / 2.0f) + posX;
        float startY = (screenHeight / 2.0f) - posY;
        font.BuildVertexArray(vertices, sentence, startX, startY);

        var gl = OpenGL.Gl;
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (float* p = vertices)
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(sizeof(float) * vertices.Length), p);
    }

    public void Shutdown(GL4 OpenGL)
    {
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

    public float[] GetPixelColor() => new[] { m_red, m_green, m_blue, 1.0f };

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.BindVertexArray(m_vertexArrayId);
        gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }
}

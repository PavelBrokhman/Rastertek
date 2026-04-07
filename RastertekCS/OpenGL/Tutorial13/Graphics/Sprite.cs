using Silk.NET.OpenGL;
using System.Globalization;

namespace RastertekCS.OpenGL.Tutorial13.Graphics;

public class Sprite
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
    private Texture[] m_Textures;
    private int m_textureCount;
    private int m_currentTexture;
    private float m_cycleTime;
    private float m_frameTime;

    private int m_screenWidth, m_screenHeight;
    private int m_bitmapWidth, m_bitmapHeight;
    private int m_renderX, m_renderY;

    public unsafe bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight,
                                   string spriteFilename, uint textureUnit)
    {
        var gl = OpenGL.Gl;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;
        m_currentTexture = 0;
        m_frameTime = 0;

        if (!LoadSpriteFile(spriteFilename)) return false;

        m_Textures = new Texture[m_textureCount];
        var lines = File.ReadAllLines(spriteFilename);
        int startLine = 3;
        for (int i = 0; i < m_textureCount; i++)
        {
            var texFile = lines[startLine + i].Trim();
            m_Textures[i] = new Texture();
            if (!m_Textures[i].Initialize(OpenGL, texFile, textureUnit, false)) return false;
        }

        m_vertexCount = 6;
        m_indexCount = 6;
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

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        if (m_Textures != null)
        {
            foreach (var t in m_Textures) t?.Shutdown(OpenGL);
            m_Textures = null;
        }
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

    public void Update(float frameTimeMs)
    {
        m_frameTime += frameTimeMs;
        if (m_frameTime >= m_cycleTime)
        {
            m_frameTime -= m_cycleTime;
            m_currentTexture++;
            if (m_currentTexture >= m_textureCount) m_currentTexture = 0;
        }
    }

    public void SetRenderLocation(int x, int y) { m_renderX = x; m_renderY = y; }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        m_Textures[m_currentTexture].SetTexture(OpenGL, textureUnit);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;

        float left = -((float)m_screenWidth / 2.0f) + m_renderX;
        float right = left + m_bitmapWidth;
        float top = ((float)m_screenHeight / 2.0f) - m_renderY;
        float bottom = top - m_bitmapHeight;

        var v = new VertexType[6]
        {
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
            new() { x = left,  y = bottom, z = 0, tu = 0, tv = 1 },
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = top,    z = 0, tu = 1, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
        };

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = v)
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                (nuint)(sizeof(VertexType) * v.Length), p);

        gl.BindVertexArray(m_vertexArrayId);
        gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    private bool LoadSpriteFile(string filename)
    {
        if (!File.Exists(filename)) return false;
        var lines = File.ReadAllLines(filename);
        // Format:
        //   TextureCount: N
        //   CycleTime: Ms
        //   BitmapSize: W H
        //   file1.tga
        //   file2.tga
        //   ...
        m_textureCount = int.Parse(lines[0].Split(':')[1].Trim(), CultureInfo.InvariantCulture);
        m_cycleTime = float.Parse(lines[1].Split(':')[1].Trim(), CultureInfo.InvariantCulture);
        var size = lines[2].Split(':')[1].Trim().Split(' ');
        m_bitmapWidth = int.Parse(size[0], CultureInfo.InvariantCulture);
        m_bitmapHeight = int.Parse(size[1], CultureInfo.InvariantCulture);
        return true;
    }
}

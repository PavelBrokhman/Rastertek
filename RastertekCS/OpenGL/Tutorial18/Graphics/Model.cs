using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial18.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x,
            y,
            z;
        public float tu,
            tv;
        public float nx,
            ny,
            nz;
    }

    private uint m_vertexArrayId;
    private uint m_vertexBufferId;
    private uint m_indexBufferId;
    private int m_vertexCount;
    private int m_indexCount;
    private Texture m_Texture1;
    private Texture m_Texture2;

    public unsafe bool Initialize(
        GL4 OpenGL,
        string modelFilename,
        string textureFilename1,
        uint textureUnit1,
        string textureFilename2,
        uint textureUnit2
    )
    {
        if (!LoadModel(modelFilename))
            return false;
        if (!InitializeBuffers(OpenGL))
            return false;

        m_Texture1 = new Texture();
        if (!m_Texture1.Initialize(OpenGL, textureFilename1, textureUnit1, true))
            return false;

        m_Texture2 = new Texture();
        if (!m_Texture2.Initialize(OpenGL, textureFilename2, textureUnit2, true))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_Texture2?.Shutdown(OpenGL);
        m_Texture2 = null;
        m_Texture1?.Shutdown(OpenGL);
        m_Texture1 = null;
        ShutdownBuffers(OpenGL);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.BindVertexArray(m_vertexArrayId);
        gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)m_indexCount,
            DrawElementsType.UnsignedInt,
            (void*)0
        );
    }

    public void SetTextures(GL4 OpenGL, uint textureUnit1, uint textureUnit2)
    {
        m_Texture1?.SetTexture(OpenGL, textureUnit1);
        m_Texture2?.SetTexture(OpenGL, textureUnit2);
    }

    public int GetIndexCount() => m_indexCount;

    private float[] m_modelData; // flat array: x y z tu tv nx ny nz per vertex

    private bool LoadModel(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Model file not found: {filename}");
            return false;
        }
        var lines = File.ReadAllLines(filename);

        // Find "Vertex Count:" line.
        int vertexCount = 0;
        int dataStart = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("Vertex Count:"))
            {
                vertexCount = int.Parse(line.Substring("Vertex Count:".Length).Trim());
            }
            if (line == "Data:")
            {
                dataStart = i + 1;
                break;
            }
        }
        if (vertexCount == 0 || dataStart < 0)
            return false;

        m_vertexCount = vertexCount;
        m_indexCount = vertexCount;
        m_modelData = new float[vertexCount * 8];

        int vi = 0;
        for (int i = dataStart; i < lines.Length && vi < vertexCount; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 8)
                continue;

            int off = vi * 8;
            m_modelData[off + 0] = float.Parse(parts[0]); // x
            m_modelData[off + 1] = float.Parse(parts[1]); // y
            m_modelData[off + 2] = float.Parse(parts[2]); // z
            m_modelData[off + 3] = float.Parse(parts[3]); // tu
            m_modelData[off + 4] = float.Parse(parts[4]); // tv
            m_modelData[off + 5] = float.Parse(parts[5]); // nx
            m_modelData[off + 6] = float.Parse(parts[6]); // ny
            m_modelData[off + 7] = float.Parse(parts[7]); // nz
            vi++;
        }

        return vi == vertexCount;
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        var vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];

        for (int i = 0; i < m_vertexCount; i++)
        {
            int off = i * 8;
            vertices[i].x = m_modelData[off + 0];
            vertices[i].y = m_modelData[off + 1];
            vertices[i].z = m_modelData[off + 2];
            vertices[i].tu = m_modelData[off + 3];
            vertices[i].tv = m_modelData[off + 4];
            vertices[i].nx = m_modelData[off + 5];
            vertices[i].ny = m_modelData[off + 6];
            vertices[i].nz = m_modelData[off + 7];
            indices[i] = (uint)i;
        }

        m_vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(m_vertexArrayId);

        m_vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
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

        // Attribute 2: normal (3 floats)
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(
            2,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)(5 * sizeof(float))
        );

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length),
                p,
                BufferUsageARB.StaticDraw
            );

        m_modelData = null; // Free raw data.
        return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.DisableVertexAttribArray(2);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(m_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(m_indexBufferId);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(m_vertexArrayId);
    }
}

using System.Globalization;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial08.Graphics;

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

    private struct ModelType
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
    private ModelType[] m_model;
    private Texture m_Texture;

    public unsafe bool Initialize(
        GL4 OpenGL,
        string modelFilename,
        string textureFilename,
        uint textureUnit,
        bool wrap
    )
    {
        var gl = OpenGL.Gl;

        // Загружаем геометрию из текстового файла.
        if (!LoadModel(modelFilename))
            return false;

        // Инициализируем GL-буферы из загруженной геометрии.
        var vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];
        for (int i = 0; i < m_vertexCount; i++)
        {
            vertices[i].x = m_model[i].x;
            vertices[i].y = m_model[i].y;
            vertices[i].z = m_model[i].z;
            vertices[i].tu = m_model[i].tu;
            // Инвертируем tv — OpenGL начинает UV снизу-слева, а файл — сверху-слева.
            vertices[i].tv = m_model[i].tv;
            vertices[i].nx = m_model[i].nx;
            vertices[i].ny = m_model[i].ny;
            vertices[i].nz = m_model[i].nz;
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

        gl.EnableVertexAttribArray(0);
        gl.EnableVertexAttribArray(1);
        gl.EnableVertexAttribArray(2);
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

        m_Texture = new Texture();
        if (!m_Texture.Initialize(OpenGL, textureFilename, textureUnit, wrap))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_Texture?.Shutdown(OpenGL);
        m_Texture = null;
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
        ReleaseModel();
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

    public int GetIndexCount() => m_indexCount;

    private bool LoadModel(string filename)
    {
        if (!File.Exists(filename))
        {
            global::System.Console.WriteLine($"Файл модели не найден: {filename}");
            return false;
        }

        var lines = File.ReadAllLines(filename);
        int idx = 0;

        // Первая непустая строка "Vertex Count: N".
        while (
            idx < lines.Length
            && !lines[idx].StartsWith("Vertex Count", StringComparison.OrdinalIgnoreCase)
        )
            idx++;
        if (idx >= lines.Length)
            return false;
        var parts = lines[idx].Split(':');
        if (parts.Length < 2)
            return false;
        m_vertexCount = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
        m_indexCount = m_vertexCount;
        m_model = new ModelType[m_vertexCount];
        idx++;

        // Пропускаем до "Data:" и дальше — до первой строки с числами.
        while (
            idx < lines.Length
            && !lines[idx].Trim().StartsWith("Data", StringComparison.OrdinalIgnoreCase)
        )
            idx++;
        idx++;

        int vi = 0;
        while (idx < lines.Length && vi < m_vertexCount)
        {
            var line = lines[idx].Trim();
            idx++;
            if (line.Length == 0)
                continue;
            var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 8)
                continue;
            m_model[vi].x = float.Parse(tokens[0], CultureInfo.InvariantCulture);
            m_model[vi].y = float.Parse(tokens[1], CultureInfo.InvariantCulture);
            m_model[vi].z = float.Parse(tokens[2], CultureInfo.InvariantCulture);
            m_model[vi].tu = float.Parse(tokens[3], CultureInfo.InvariantCulture);
            m_model[vi].tv = float.Parse(tokens[4], CultureInfo.InvariantCulture);
            m_model[vi].nx = float.Parse(tokens[5], CultureInfo.InvariantCulture);
            m_model[vi].ny = float.Parse(tokens[6], CultureInfo.InvariantCulture);
            m_model[vi].nz = float.Parse(tokens[7], CultureInfo.InvariantCulture);
            vi++;
        }
        return vi == m_vertexCount;
    }

    private void ReleaseModel() => m_model = null;
}

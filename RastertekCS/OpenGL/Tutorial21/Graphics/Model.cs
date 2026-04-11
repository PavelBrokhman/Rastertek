using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial21.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
        public float nx, ny, nz;
        public float tx, ty, tz;
        public float bx, by, bz;
    }

    private struct ModelType
    {
        public float x, y, z;
        public float tu, tv;
        public float nx, ny, nz;
        public float tx, ty, tz;
        public float bx, by, bz;
    }

    private struct TempVertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private struct VectorType
    {
        public float x, y, z;
    }

    private uint m_vertexArrayId;
    private uint m_vertexBufferId;
    private uint m_indexBufferId;
    private int m_vertexCount;
    private int m_indexCount;
    private Texture m_Texture1;
    private Texture m_Texture2;
    private Texture m_Texture3;
    private ModelType[] m_model;

    public unsafe bool Initialize(GL4 OpenGL, string modelFilename,
        string textureFilename1, bool wrap1,
        string textureFilename2, bool wrap2,
        string textureFilename3, bool wrap3)
    {
        if (!LoadModel(modelFilename)) return false;
        CalculateModelVectors();
        if (!InitializeBuffers(OpenGL)) return false;
        if (!LoadTextures(OpenGL, textureFilename1, wrap1, textureFilename2, wrap2, textureFilename3, wrap3)) return false;
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        m_Texture3?.Shutdown(OpenGL); m_Texture3 = null;
        m_Texture2?.Shutdown(OpenGL); m_Texture2 = null;
        m_Texture1?.Shutdown(OpenGL); m_Texture1 = null;
        ShutdownBuffers(OpenGL);
        m_model = null;
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.BindVertexArray(m_vertexArrayId);
        gl.DrawElements(PrimitiveType.Triangles, (uint)m_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    public void SetTextures(GL4 OpenGL, uint unit1, uint unit2, uint unit3)
    {
        m_Texture1?.SetTexture(OpenGL, unit1);
        m_Texture2?.SetTexture(OpenGL, unit2);
        m_Texture3?.SetTexture(OpenGL, unit3);
    }

    private bool LoadModel(string filename)
    {
        if (!File.Exists(filename)) { Console.WriteLine($"Model file not found: {filename}"); return false; }
        var lines = File.ReadAllLines(filename);

        int vertexCount = 0;
        int dataStart = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("Vertex Count:"))
                vertexCount = int.Parse(line.Substring("Vertex Count:".Length).Trim());
            if (line == "Data:")
            {
                dataStart = i + 1;
                break;
            }
        }
        if (vertexCount == 0 || dataStart < 0) return false;

        m_vertexCount = vertexCount;
        m_indexCount = vertexCount;
        m_model = new ModelType[vertexCount];

        int vi = 0;
        for (int i = dataStart; i < lines.Length && vi < vertexCount; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 8) continue;

            m_model[vi].x = float.Parse(parts[0]);
            m_model[vi].y = float.Parse(parts[1]);
            m_model[vi].z = float.Parse(parts[2]);
            m_model[vi].tu = float.Parse(parts[3]);
            m_model[vi].tv = float.Parse(parts[4]); // Invert V for OpenGL
            m_model[vi].nx = float.Parse(parts[5]);
            m_model[vi].ny = float.Parse(parts[6]);
            m_model[vi].nz = float.Parse(parts[7]);
            vi++;
        }

        return vi == vertexCount;
    }

    private void CalculateModelVectors()
    {
        int faceCount = m_vertexCount / 3;
        int index = 0;

        for (int i = 0; i < faceCount; i++)
        {
            TempVertexType vertex1, vertex2, vertex3;

            vertex1.x = m_model[index].x; vertex1.y = m_model[index].y; vertex1.z = m_model[index].z;
            vertex1.tu = m_model[index].tu; vertex1.tv = m_model[index].tv;
            index++;

            vertex2.x = m_model[index].x; vertex2.y = m_model[index].y; vertex2.z = m_model[index].z;
            vertex2.tu = m_model[index].tu; vertex2.tv = m_model[index].tv;
            index++;

            vertex3.x = m_model[index].x; vertex3.y = m_model[index].y; vertex3.z = m_model[index].z;
            vertex3.tu = m_model[index].tu; vertex3.tv = m_model[index].tv;
            index++;

            CalculateTangentBinormal(vertex1, vertex2, vertex3, out VectorType tangent, out VectorType binormal);

            m_model[index - 1].tx = tangent.x; m_model[index - 1].ty = tangent.y; m_model[index - 1].tz = tangent.z;
            m_model[index - 1].bx = binormal.x; m_model[index - 1].by = binormal.y; m_model[index - 1].bz = binormal.z;

            m_model[index - 2].tx = tangent.x; m_model[index - 2].ty = tangent.y; m_model[index - 2].tz = tangent.z;
            m_model[index - 2].bx = binormal.x; m_model[index - 2].by = binormal.y; m_model[index - 2].bz = binormal.z;

            m_model[index - 3].tx = tangent.x; m_model[index - 3].ty = tangent.y; m_model[index - 3].tz = tangent.z;
            m_model[index - 3].bx = binormal.x; m_model[index - 3].by = binormal.y; m_model[index - 3].bz = binormal.z;
        }
    }

    private static void CalculateTangentBinormal(TempVertexType vertex1, TempVertexType vertex2, TempVertexType vertex3,
        out VectorType tangent, out VectorType binormal)
    {
        float[] vector1 = new float[3], vector2 = new float[3];
        float[] tuVector = new float[2], tvVector = new float[2];

        vector1[0] = vertex2.x - vertex1.x;
        vector1[1] = vertex2.y - vertex1.y;
        vector1[2] = vertex2.z - vertex1.z;

        vector2[0] = vertex3.x - vertex1.x;
        vector2[1] = vertex3.y - vertex1.y;
        vector2[2] = vertex3.z - vertex1.z;

        tuVector[0] = vertex2.tu - vertex1.tu;
        tvVector[0] = vertex2.tv - vertex1.tv;

        tuVector[1] = vertex3.tu - vertex1.tu;
        tvVector[1] = vertex3.tv - vertex1.tv;

        float den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

        tangent.x = (tvVector[1] * vector1[0] - tvVector[0] * vector2[0]) * den;
        tangent.y = (tvVector[1] * vector1[1] - tvVector[0] * vector2[1]) * den;
        tangent.z = (tvVector[1] * vector1[2] - tvVector[0] * vector2[2]) * den;

        binormal.x = (tuVector[0] * vector2[0] - tuVector[1] * vector1[0]) * den;
        binormal.y = (tuVector[0] * vector2[1] - tuVector[1] * vector1[1]) * den;
        binormal.z = (tuVector[0] * vector2[2] - tuVector[1] * vector1[2]) * den;

        float length = MathF.Sqrt(tangent.x * tangent.x + tangent.y * tangent.y + tangent.z * tangent.z);
        tangent.x /= length; tangent.y /= length; tangent.z /= length;

        length = MathF.Sqrt(binormal.x * binormal.x + binormal.y * binormal.y + binormal.z * binormal.z);
        binormal.x /= length; binormal.y /= length; binormal.z /= length;
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        var vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];

        for (int i = 0; i < m_vertexCount; i++)
        {
            vertices[i].x = m_model[i].x;
            vertices[i].y = m_model[i].y;
            vertices[i].z = m_model[i].z;
            vertices[i].tu = m_model[i].tu;
            vertices[i].tv = m_model[i].tv;
            vertices[i].nx = m_model[i].nx;
            vertices[i].ny = m_model[i].ny;
            vertices[i].nz = m_model[i].nz;
            vertices[i].tx = m_model[i].tx;
            vertices[i].ty = m_model[i].ty;
            vertices[i].tz = m_model[i].tz;
            vertices[i].bx = m_model[i].bx;
            vertices[i].by = m_model[i].by;
            vertices[i].bz = m_model[i].bz;
            indices[i] = (uint)i;
        }

        m_vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(m_vertexArrayId);

        m_vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.StaticDraw);

        // Attribute 0: position (3 floats)
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
            (uint)sizeof(VertexType), (void*)0);

        // Attribute 1: texcoord (2 floats)
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
            (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));

        // Attribute 2: normal (3 floats)
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false,
            (uint)sizeof(VertexType), (void*)(5 * sizeof(float)));

        // Attribute 3: tangent (3 floats)
        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false,
            (uint)sizeof(VertexType), (void*)(8 * sizeof(float)));

        // Attribute 4: binormal (3 floats)
        gl.EnableVertexAttribArray(4);
        gl.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false,
            (uint)sizeof(VertexType), (void*)(11 * sizeof(float)));

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);

        return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.DisableVertexAttribArray(2);
        gl.DisableVertexAttribArray(3);
        gl.DisableVertexAttribArray(4);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(m_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(m_indexBufferId);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(m_vertexArrayId);
    }

    private bool LoadTextures(GL4 OpenGL, string filename1, bool wrap1, string filename2, bool wrap2, string filename3, bool wrap3)
    {
        m_Texture1 = new Texture();
        if (!m_Texture1.Initialize(OpenGL, filename1, 0, wrap1)) return false;

        m_Texture2 = new Texture();
        if (!m_Texture2.Initialize(OpenGL, filename2, 1, wrap2)) return false;

        m_Texture3 = new Texture();
        if (!m_Texture3.Initialize(OpenGL, filename3, 2, wrap3)) return false;

        return true;
    }
}

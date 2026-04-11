using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial20.Graphics;

public class Model
{
    // Vertex with tangent and binormal for normal mapping.
    // Layout: position(3) + texcoord(2) + normal(3) + tangent(3) + binormal(3) = 14 floats.
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

    private uint m_vertexArrayId;
    private uint m_vertexBufferId;
    private uint m_indexBufferId;
    private int m_vertexCount;
    private int m_indexCount;
    private Texture m_Texture1;
    private Texture m_Texture2;
    private ModelType[] m_model;

    public unsafe bool Initialize(GL4 OpenGL, string modelFilename,
        string textureFilename1, uint textureUnit1,
        string textureFilename2, uint textureUnit2)
    {
        if (!LoadModel(modelFilename)) return false;
        CalculateModelVectors();
        if (!InitializeBuffers(OpenGL)) return false;

        m_Texture1 = new Texture();
        if (!m_Texture1.Initialize(OpenGL, textureFilename1, textureUnit1, true)) return false;

        m_Texture2 = new Texture();
        if (!m_Texture2.Initialize(OpenGL, textureFilename2, textureUnit2, true)) return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
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

    public void SetTextures(GL4 OpenGL, uint textureUnit1, uint textureUnit2)
    {
        m_Texture1?.SetTexture(OpenGL, textureUnit1);
        m_Texture2?.SetTexture(OpenGL, textureUnit2);
    }

    public int GetIndexCount() => m_indexCount;

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
            {
                vertexCount = int.Parse(line.Substring("Vertex Count:".Length).Trim());
            }
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
            // Tangent and binormal will be calculated later.
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
            // Get three vertices for this face.
            float v1x = m_model[index].x, v1y = m_model[index].y, v1z = m_model[index].z;
            float v1tu = m_model[index].tu, v1tv = m_model[index].tv;
            index++;
            float v2x = m_model[index].x, v2y = m_model[index].y, v2z = m_model[index].z;
            float v2tu = m_model[index].tu, v2tv = m_model[index].tv;
            index++;
            float v3x = m_model[index].x, v3y = m_model[index].y, v3z = m_model[index].z;
            float v3tu = m_model[index].tu, v3tv = m_model[index].tv;
            index++;

            // Calculate tangent and binormal.
            CalculateTangentBinormal(
                v1x, v1y, v1z, v1tu, v1tv,
                v2x, v2y, v2z, v2tu, v2tv,
                v3x, v3y, v3z, v3tu, v3tv,
                out float tanX, out float tanY, out float tanZ,
                out float binX, out float binY, out float binZ);

            // Store tangent and binormal for all three vertices of this face.
            m_model[index - 1].tx = tanX; m_model[index - 1].ty = tanY; m_model[index - 1].tz = tanZ;
            m_model[index - 1].bx = binX; m_model[index - 1].by = binY; m_model[index - 1].bz = binZ;
            m_model[index - 2].tx = tanX; m_model[index - 2].ty = tanY; m_model[index - 2].tz = tanZ;
            m_model[index - 2].bx = binX; m_model[index - 2].by = binY; m_model[index - 2].bz = binZ;
            m_model[index - 3].tx = tanX; m_model[index - 3].ty = tanY; m_model[index - 3].tz = tanZ;
            m_model[index - 3].bx = binX; m_model[index - 3].by = binY; m_model[index - 3].bz = binZ;
        }
    }

    private static void CalculateTangentBinormal(
        float v1x, float v1y, float v1z, float v1tu, float v1tv,
        float v2x, float v2y, float v2z, float v2tu, float v2tv,
        float v3x, float v3y, float v3z, float v3tu, float v3tv,
        out float tanX, out float tanY, out float tanZ,
        out float binX, out float binY, out float binZ)
    {
        // Two edge vectors.
        float e1x = v2x - v1x, e1y = v2y - v1y, e1z = v2z - v1z;
        float e2x = v3x - v1x, e2y = v3y - v1y, e2z = v3z - v1z;

        // Texture space vectors.
        float du1 = v2tu - v1tu, dv1 = v2tv - v1tv;
        float du2 = v3tu - v1tu, dv2 = v3tv - v1tv;

        float den = 1.0f / (du1 * dv2 - du2 * dv1);

        // Tangent.
        tanX = (dv2 * e1x - dv1 * e2x) * den;
        tanY = (dv2 * e1y - dv1 * e2y) * den;
        tanZ = (dv2 * e1z - dv1 * e2z) * den;

        // Binormal.
        binX = (du1 * e2x - du2 * e1x) * den;
        binY = (du1 * e2y - du2 * e1y) * den;
        binZ = (du1 * e2z - du2 * e1z) * den;

        // Normalize tangent.
        float len = MathF.Sqrt(tanX * tanX + tanY * tanY + tanZ * tanZ);
        tanX /= len; tanY /= len; tanZ /= len;

        // Normalize binormal.
        len = MathF.Sqrt(binX * binX + binY * binY + binZ * binZ);
        binX /= len; binY /= len; binZ /= len;
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

        uint stride = (uint)sizeof(VertexType);

        // Attribute 0: position (3 floats)
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

        // Attribute 1: texcoord (2 floats)
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

        // Attribute 2: normal (3 floats)
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, (void*)(5 * sizeof(float)));

        // Attribute 3: tangent (3 floats)
        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, stride, (void*)(8 * sizeof(float)));

        // Attribute 4: binormal (3 floats)
        gl.EnableVertexAttribArray(4);
        gl.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, stride, (void*)(11 * sizeof(float)));

        m_indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);

        m_model = null; // Free raw data.
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
}

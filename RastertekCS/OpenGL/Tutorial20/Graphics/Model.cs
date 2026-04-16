using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial20.Graphics;

public class Model
{
    // Vertex with tangent and binormal for normal mapping.
    // Layout: position(3) + texcoord(2) + normal(3) + tangent(3) + binormal(3) = 14 floats.
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
        public float tx,
            ty,
            tz;
        public float bx,
            by,
            bz;
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
        public float tx,
            ty,
            tz;
        public float bx,
            by,
            bz;
    }

    private uint _vertexArrayId;
    private uint _vertexBufferId;
    private uint _indexBufferId;
    private int _vertexCount;
    private int _indexCount;
    private Texture _texture1;
    private Texture _texture2;
    private ModelType[] _model;

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
        CalculateModelVectors();
        if (!InitializeBuffers(OpenGL))
            return false;

        _texture1 = new Texture();
        if (!_texture1.Initialize(OpenGL, textureFilename1, textureUnit1, true))
            return false;

        _texture2 = new Texture();
        if (!_texture2.Initialize(OpenGL, textureFilename2, textureUnit2, true))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _texture2?.Shutdown(OpenGL);
        _texture2 = null;
        _texture1?.Shutdown(OpenGL);
        _texture1 = null;
        ShutdownBuffers(OpenGL);
        _model = null;
    }

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

    public void SetTextures(GL4 OpenGL, uint textureUnit1, uint textureUnit2)
    {
        _texture1?.SetTexture(OpenGL, textureUnit1);
        _texture2?.SetTexture(OpenGL, textureUnit2);
    }

    public int GetIndexCount() => _indexCount;

    private bool LoadModel(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Model file not found: {filename}");
            return false;
        }
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
        if (vertexCount == 0 || dataStart < 0)
            return false;

        _vertexCount = vertexCount;
        _indexCount = vertexCount;
        _model = new ModelType[vertexCount];

        int vi = 0;
        for (int i = dataStart; i < lines.Length && vi < vertexCount; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 8)
                continue;

            _model[vi].x = float.Parse(parts[0]);
            _model[vi].y = float.Parse(parts[1]);
            _model[vi].z = float.Parse(parts[2]);
            _model[vi].tu = float.Parse(parts[3]);
            _model[vi].tv = float.Parse(parts[4]); // Invert V for OpenGL
            _model[vi].nx = float.Parse(parts[5]);
            _model[vi].ny = float.Parse(parts[6]);
            _model[vi].nz = float.Parse(parts[7]);
            // Tangent and binormal will be calculated later.
            vi++;
        }

        return vi == vertexCount;
    }

    private void CalculateModelVectors()
    {
        int faceCount = _vertexCount / 3;
        int index = 0;

        for (int i = 0; i < faceCount; i++)
        {
            // Get three vertices for this face.
            float v1x = _model[index].x,
                v1y = _model[index].y,
                v1z = _model[index].z;
            float v1tu = _model[index].tu,
                v1tv = _model[index].tv;
            index++;
            float v2x = _model[index].x,
                v2y = _model[index].y,
                v2z = _model[index].z;
            float v2tu = _model[index].tu,
                v2tv = _model[index].tv;
            index++;
            float v3x = _model[index].x,
                v3y = _model[index].y,
                v3z = _model[index].z;
            float v3tu = _model[index].tu,
                v3tv = _model[index].tv;
            index++;

            // Calculate tangent and binormal.
            CalculateTangentBinormal(
                v1x,
                v1y,
                v1z,
                v1tu,
                v1tv,
                v2x,
                v2y,
                v2z,
                v2tu,
                v2tv,
                v3x,
                v3y,
                v3z,
                v3tu,
                v3tv,
                out float tanX,
                out float tanY,
                out float tanZ,
                out float binX,
                out float binY,
                out float binZ
            );

            // Store tangent and binormal for all three vertices of this face.
            _model[index - 1].tx = tanX;
            _model[index - 1].ty = tanY;
            _model[index - 1].tz = tanZ;
            _model[index - 1].bx = binX;
            _model[index - 1].by = binY;
            _model[index - 1].bz = binZ;
            _model[index - 2].tx = tanX;
            _model[index - 2].ty = tanY;
            _model[index - 2].tz = tanZ;
            _model[index - 2].bx = binX;
            _model[index - 2].by = binY;
            _model[index - 2].bz = binZ;
            _model[index - 3].tx = tanX;
            _model[index - 3].ty = tanY;
            _model[index - 3].tz = tanZ;
            _model[index - 3].bx = binX;
            _model[index - 3].by = binY;
            _model[index - 3].bz = binZ;
        }
    }

    private static void CalculateTangentBinormal(
        float v1x,
        float v1y,
        float v1z,
        float v1tu,
        float v1tv,
        float v2x,
        float v2y,
        float v2z,
        float v2tu,
        float v2tv,
        float v3x,
        float v3y,
        float v3z,
        float v3tu,
        float v3tv,
        out float tanX,
        out float tanY,
        out float tanZ,
        out float binX,
        out float binY,
        out float binZ
    )
    {
        // Two edge vectors.
        float e1x = v2x - v1x,
            e1y = v2y - v1y,
            e1z = v2z - v1z;
        float e2x = v3x - v1x,
            e2y = v3y - v1y,
            e2z = v3z - v1z;

        // Texture space vectors.
        float du1 = v2tu - v1tu,
            dv1 = v2tv - v1tv;
        float du2 = v3tu - v1tu,
            dv2 = v3tv - v1tv;

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
        tanX /= len;
        tanY /= len;
        tanZ /= len;

        // Normalize binormal.
        len = MathF.Sqrt(binX * binX + binY * binY + binZ * binZ);
        binX /= len;
        binY /= len;
        binZ /= len;
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        var vertices = new VertexType[_vertexCount];
        var indices = new uint[_indexCount];

        for (int i = 0; i < _vertexCount; i++)
        {
            vertices[i].x = _model[i].x;
            vertices[i].y = _model[i].y;
            vertices[i].z = _model[i].z;
            vertices[i].tu = _model[i].tu;
            vertices[i].tv = _model[i].tv;
            vertices[i].nx = _model[i].nx;
            vertices[i].ny = _model[i].ny;
            vertices[i].nz = _model[i].nz;
            vertices[i].tx = _model[i].tx;
            vertices[i].ty = _model[i].ty;
            vertices[i].tz = _model[i].tz;
            vertices[i].bx = _model[i].bx;
            vertices[i].by = _model[i].by;
            vertices[i].bz = _model[i].bz;
            indices[i] = (uint)i;
        }

        _vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(_vertexArrayId);

        _vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length),
                p,
                BufferUsageARB.StaticDraw
            );

        uint stride = (uint)sizeof(VertexType);

        // Attribute 0: position (3 floats)
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

        // Attribute 1: texcoord (2 floats)
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            stride,
            (void*)(3 * sizeof(float))
        );

        // Attribute 2: normal (3 floats)
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(
            2,
            3,
            VertexAttribPointerType.Float,
            false,
            stride,
            (void*)(5 * sizeof(float))
        );

        // Attribute 3: tangent (3 floats)
        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(
            3,
            3,
            VertexAttribPointerType.Float,
            false,
            stride,
            (void*)(8 * sizeof(float))
        );

        // Attribute 4: binormal (3 floats)
        gl.EnableVertexAttribArray(4);
        gl.VertexAttribPointer(
            4,
            3,
            VertexAttribPointerType.Float,
            false,
            stride,
            (void*)(11 * sizeof(float))
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

        _model = null; // Free raw data.
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
        gl.DeleteBuffer(_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(_indexBufferId);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(_vertexArrayId);
    }
}

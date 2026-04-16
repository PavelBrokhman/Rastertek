using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial19.Graphics;

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

    private uint _vertexArrayId;
    private uint _vertexBufferId;
    private uint _indexBufferId;
    private int _vertexCount;
    private int _indexCount;
    private Texture _texture1;
    private Texture _texture2;
    private Texture _texture3;

    public unsafe bool Initialize(
        GL4 OpenGL,
        string modelFilename,
        string textureFilename1,
        uint textureUnit1,
        string textureFilename2,
        uint textureUnit2,
        string textureFilename3,
        uint textureUnit3
    )
    {
        if (!LoadModel(modelFilename))
            return false;
        if (!InitializeBuffers(OpenGL))
            return false;

        _texture1 = new Texture();
        if (!_texture1.Initialize(OpenGL, textureFilename1, textureUnit1, true))
            return false;

        _texture2 = new Texture();
        if (!_texture2.Initialize(OpenGL, textureFilename2, textureUnit2, true))
            return false;

        _texture3 = new Texture();
        if (!_texture3.Initialize(OpenGL, textureFilename3, textureUnit3, true))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _texture3?.Shutdown(OpenGL);
        _texture3 = null;
        _texture2?.Shutdown(OpenGL);
        _texture2 = null;
        _texture1?.Shutdown(OpenGL);
        _texture1 = null;
        ShutdownBuffers(OpenGL);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.BindVertexArray(_vertexArrayId);
        gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)_indexCount,
            DrawElementsType.UnsignedInt,
            (void*)0
        );
    }

    public void SetTextures(GL4 OpenGL, uint textureUnit1, uint textureUnit2, uint textureUnit3)
    {
        _texture1?.SetTexture(OpenGL, textureUnit1);
        _texture2?.SetTexture(OpenGL, textureUnit2);
        _texture3?.SetTexture(OpenGL, textureUnit3);
    }

    public int GetIndexCount() => _indexCount;

    private float[] _modelData; // flat array: x y z tu tv nx ny nz per vertex

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

        _vertexCount = vertexCount;
        _indexCount = vertexCount;
        _modelData = new float[vertexCount * 8];

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
            _modelData[off + 0] = float.Parse(parts[0]); // x
            _modelData[off + 1] = float.Parse(parts[1]); // y
            _modelData[off + 2] = float.Parse(parts[2]); // z
            _modelData[off + 3] = float.Parse(parts[3]); // tu
            _modelData[off + 4] = float.Parse(parts[4]); // tv
            _modelData[off + 5] = float.Parse(parts[5]); // nx
            _modelData[off + 6] = float.Parse(parts[6]); // ny
            _modelData[off + 7] = float.Parse(parts[7]); // nz
            vi++;
        }

        return vi == vertexCount;
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        var vertices = new VertexType[_vertexCount];
        var indices = new uint[_indexCount];

        for (int i = 0; i < _vertexCount; i++)
        {
            int off = i * 8;
            vertices[i].x = _modelData[off + 0];
            vertices[i].y = _modelData[off + 1];
            vertices[i].z = _modelData[off + 2];
            vertices[i].tu = _modelData[off + 3];
            vertices[i].tv = _modelData[off + 4];
            vertices[i].nx = _modelData[off + 5];
            vertices[i].ny = _modelData[off + 6];
            vertices[i].nz = _modelData[off + 7];
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

        _indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length),
                p,
                BufferUsageARB.StaticDraw
            );

        _modelData = null; // Free raw data.
        return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.DisableVertexAttribArray(2);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(_indexBufferId);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(_vertexArrayId);
    }
}

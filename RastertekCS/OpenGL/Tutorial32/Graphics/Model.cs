using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x,
            y,
            z,
            tu,
            tv,
            nx,
            ny,
            nz;
    }

    private uint _vertexArrayId,
        _vertexBufferId,
        _indexBufferId;
    private int _vertexCount,
        _indexCount;
    private Texture[] _textures;
    private float[] _modelData;

    public unsafe bool Initialize(
        GL4 gl,
        string modelFile,
        string texture1File,
        bool wrap1,
        string texture2File = null,
        bool wrap2 = false
    )
    {
        if (!LoadModel(modelFile))
            return false;
        if (!InitializeBuffers(gl))
            return false;
        _textures = new Texture[3];
        if (texture1File != null)
        {
            _textures[0] = new Texture();
            if (!_textures[0].Initialize(gl, texture1File, 0, wrap1))
                return false;
        }
        if (texture2File != null)
        {
            _textures[1] = new Texture();
            if (!_textures[1].Initialize(gl, texture2File, 0, wrap2))
                return false;
        }
        return true;
    }

    public void Shutdown(GL4 gl)
    {
        if (_textures != null)
        {
            foreach (var t in _textures)
                t?.Shutdown(gl);
            _textures = null;
        }
        ShutdownBuffers(gl);
    }

    public unsafe void Render(GL4 gl)
    {
        gl.Driver.BindVertexArray(_vertexArrayId);
        gl.Driver.DrawElements(
            PrimitiveType.Triangles,
            (uint)_indexCount,
            DrawElementsType.UnsignedInt,
            (void*)0
        );
    }

    public void SetTexture1(GL4 gl, uint textureUnit)
    {
        _textures?[0]?.SetTexture(gl, textureUnit);
    }

    public void SetTexture2(GL4 gl, uint textureUnit)
    {
        _textures?[1]?.SetTexture(gl, textureUnit);
    }

    bool LoadModel(string filename)
    {
        if (!File.Exists(filename))
            return false;
        var lines = File.ReadAllLines(filename);
        int vertexCount = 0,
            dataStartIndex = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("Vertex Count:"))
                vertexCount = int.Parse(line.Substring(13).Trim());
            if (line == "Data:")
            {
                dataStartIndex = i + 1;
                break;
            }
        }
        if (vertexCount == 0 || dataStartIndex < 0)
            return false;
        _vertexCount = vertexCount;
        _indexCount = vertexCount;
        _modelData = new float[vertexCount * 8];
        int vertexIndex = 0;
        for (int i = dataStartIndex; i < lines.Length && vertexIndex < vertexCount; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 8)
                continue;
            int offset = vertexIndex * 8;
            _modelData[offset] = float.Parse(parts[0]);
            _modelData[offset + 1] = float.Parse(parts[1]);
            _modelData[offset + 2] = float.Parse(parts[2]);
            _modelData[offset + 3] = float.Parse(parts[3]);
            _modelData[offset + 4] = float.Parse(parts[4]);
            _modelData[offset + 5] = float.Parse(parts[5]);
            _modelData[offset + 6] = float.Parse(parts[6]);
            _modelData[offset + 7] = float.Parse(parts[7]);
            vertexIndex++;
        }
        return vertexIndex == vertexCount;
    }

    unsafe bool InitializeBuffers(GL4 gl)
    {
        var glApi = gl.Driver;
        var vertices = new VertexType[_vertexCount];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _vertexCount; i++)
        {
            int offset = i * 8;
            vertices[i].x = _modelData[offset];
            vertices[i].y = _modelData[offset + 1];
            vertices[i].z = _modelData[offset + 2];
            vertices[i].tu = _modelData[offset + 3];
            vertices[i].tv = _modelData[offset + 4];
            vertices[i].nx = _modelData[offset + 5];
            vertices[i].ny = _modelData[offset + 6];
            vertices[i].nz = _modelData[offset + 7];
            indices[i] = (uint)i;
        }
        _vertexArrayId = glApi.GenVertexArray();
        glApi.BindVertexArray(_vertexArrayId);
        _vertexBufferId = glApi.GenBuffer();
        glApi.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = vertices)
            glApi.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length),
                p,
                BufferUsageARB.StaticDraw
            );
        glApi.EnableVertexAttribArray(0);
        glApi.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)0
        );
        glApi.EnableVertexAttribArray(1);
        glApi.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)(3 * sizeof(float))
        );
        glApi.EnableVertexAttribArray(2);
        glApi.VertexAttribPointer(
            2,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)(5 * sizeof(float))
        );
        _indexBufferId = glApi.GenBuffer();
        glApi.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = indices)
            glApi.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length),
                p,
                BufferUsageARB.StaticDraw
            );
        _modelData = null;
        return true;
    }

    void ShutdownBuffers(GL4 gl)
    {
        var glApi = gl.Driver;
        glApi.DisableVertexAttribArray(0);
        glApi.DisableVertexAttribArray(1);
        glApi.DisableVertexAttribArray(2);
        glApi.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        glApi.DeleteBuffer(_vertexBufferId);
        glApi.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        glApi.DeleteBuffer(_indexBufferId);
        glApi.BindVertexArray(0);
        glApi.DeleteVertexArray(_vertexArrayId);
    }
}

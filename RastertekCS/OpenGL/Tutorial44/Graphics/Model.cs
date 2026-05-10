using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial44.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x, y, z, tu, tv, nx, ny, nz;
    }

    private uint _vertexArrayId,
        _vertexBufferId,
        _indexBufferId;
    private int _vertexCount,
        _indexCount;
    private Texture _texture;
    private float[] _modelData;

    public unsafe bool Initialize(GL4 OpenGL, string modelFilename, string textureFilename)
    {
        if (!LoadModel(modelFilename)) return false;
        if (!InitializeBuffers(OpenGL)) return false;
        _texture = new Texture();
        if (!_texture.Initialize(OpenGL, textureFilename, 0, true)) return false;
        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _texture?.Shutdown(OpenGL);
        _texture = null;
        ShutdownBuffers(OpenGL);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;
        gl.BindVertexArray(_vertexArrayId);
        gl.DrawElements(PrimitiveType.Triangles, (uint)_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit) => _texture?.SetTexture(OpenGL, textureUnit);

    public int GetIndexCount() => _indexCount;

    private bool LoadModel(string filename)
    {
        if (!File.Exists(filename)) { Console.WriteLine($"Model file not found: {filename}"); return false; }
        var lines = File.ReadAllLines(filename);
        int vertexCount = 0, dataStartIndex = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("Vertex Count:")) vertexCount = int.Parse(line.Substring(13).Trim());
            if (line == "Data:") { dataStartIndex = i + 1; break; }
        }
        if (vertexCount == 0 || dataStartIndex < 0) return false;
        _vertexCount = vertexCount;
        _indexCount = vertexCount;
        _modelData = new float[vertexCount * 8];
        int vi = 0;
        for (int i = dataStartIndex; i < lines.Length && vi < vertexCount; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 8) continue;
            int o = vi * 8;
            for (int k = 0; k < 8; k++)
                _modelData[o + k] = float.Parse(parts[k]);
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
            int o = i * 8;
            vertices[i].x = _modelData[o];
            vertices[i].y = _modelData[o + 1];
            vertices[i].z = _modelData[o + 2];
            vertices[i].tu = _modelData[o + 3];
            vertices[i].tv = _modelData[o + 4];
            vertices[i].nx = _modelData[o + 5];
            vertices[i].ny = _modelData[o + 6];
            vertices[i].nz = _modelData[o + 7];
            indices[i] = (uint)i;
        }
        _vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(_vertexArrayId);
        _vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(VertexType) * vertices.Length), p, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(5 * sizeof(float)));
        _indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * indices.Length), p, BufferUsageARB.StaticDraw);
        _modelData = null;
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

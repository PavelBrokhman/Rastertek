using System.Globalization;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial11.Graphics;

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

    private uint _vertexArrayId;
    private uint _vertexBufferId;
    private uint _indexBufferId;
    private int _vertexCount;
    private int _indexCount;
    private ModelType[] _model;
    private Texture _texture;

    public unsafe bool Initialize(
        GL4 OpenGL,
        string modelFilename,
        string textureFilename,
        uint textureUnit,
        bool wrap
    )
    {
        var gl = OpenGL.Driver;

        // Загружаем геометрию из текстового файла.
        if (!LoadModel(modelFilename))
            return false;

        // Инициализируем GL-буферы из загруженной геометрии.
        var vertices = new VertexType[_vertexCount];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _vertexCount; i++)
        {
            vertices[i].x = _model[i].x;
            vertices[i].y = _model[i].y;
            vertices[i].z = _model[i].z;
            vertices[i].tu = _model[i].tu;
            // Инвертируем tv — OpenGL начинает UV снизу-слева, а файл — сверху-слева.
            vertices[i].tv = _model[i].tv;
            vertices[i].nx = _model[i].nx;
            vertices[i].ny = _model[i].ny;
            vertices[i].nz = _model[i].nz;
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

        _indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length),
                p,
                BufferUsageARB.StaticDraw
            );

        _texture = new Texture();
        if (!_texture.Initialize(OpenGL, textureFilename, textureUnit, wrap))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        _texture?.Shutdown(OpenGL);
        _texture = null;
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
        ReleaseModel();
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

    public int GetIndexCount() => _indexCount;

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
        _vertexCount = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
        _indexCount = _vertexCount;
        _model = new ModelType[_vertexCount];
        idx++;

        // Пропускаем до "Data:" и дальше — до первой строки с числами.
        while (
            idx < lines.Length
            && !lines[idx].Trim().StartsWith("Data", StringComparison.OrdinalIgnoreCase)
        )
            idx++;
        idx++;

        int vi = 0;
        while (idx < lines.Length && vi < _vertexCount)
        {
            var line = lines[idx].Trim();
            idx++;
            if (line.Length == 0)
                continue;
            var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 8)
                continue;
            _model[vi].x = float.Parse(tokens[0], CultureInfo.InvariantCulture);
            _model[vi].y = float.Parse(tokens[1], CultureInfo.InvariantCulture);
            _model[vi].z = float.Parse(tokens[2], CultureInfo.InvariantCulture);
            _model[vi].tu = float.Parse(tokens[3], CultureInfo.InvariantCulture);
            _model[vi].tv = float.Parse(tokens[4], CultureInfo.InvariantCulture);
            _model[vi].nx = float.Parse(tokens[5], CultureInfo.InvariantCulture);
            _model[vi].ny = float.Parse(tokens[6], CultureInfo.InvariantCulture);
            _model[vi].nz = float.Parse(tokens[7], CultureInfo.InvariantCulture);
            vi++;
        }
        return vi == _vertexCount;
    }

    private void ReleaseModel() => _model = null;
}

using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

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
    private Texture _texture;
    private float[] _modelData;

    public unsafe bool Initialize(GL4 OpenGL, string modelFile, string texFile, bool wrap)
    {
        if (!LoadModel(modelFile))
            return false;
        if (!InitBuffers(OpenGL))
            return false;
        _texture = new Texture();
        if (!_texture.Initialize(OpenGL, texFile, 0, wrap))
            return false;
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
        OpenGL.Gl.BindVertexArray(_vertexArrayId);
        OpenGL.Gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)_indexCount,
            DrawElementsType.UnsignedInt,
            (void*)0
        );
    }

    public void SetTexture(GL4 OpenGL, uint tu)
    {
        _texture?.SetTexture(OpenGL, tu);
    }

    private bool LoadModel(string fn)
    {
        if (!File.Exists(fn))
        {
            Console.WriteLine($"Model not found: {fn}");
            return false;
        }
        var lines = File.ReadAllLines(fn);
        int vc = 0,
            ds = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var l = lines[i].Trim();
            if (l.StartsWith("Vertex Count:"))
                vc = int.Parse(l.Substring(13).Trim());
            if (l == "Data:")
            {
                ds = i + 1;
                break;
            }
        }
        if (vc == 0 || ds < 0)
            return false;
        _vertexCount = vc;
        _indexCount = vc;
        _modelData = new float[vc * 8];
        int vi = 0;
        for (int i = ds; i < lines.Length && vi < vc; i++)
        {
            var l = lines[i].Trim();
            if (string.IsNullOrEmpty(l))
                continue;
            var p = l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length < 8)
                continue;
            int o = vi * 8;
            _modelData[o] = float.Parse(p[0]);
            _modelData[o + 1] = float.Parse(p[1]);
            _modelData[o + 2] = float.Parse(p[2]);
            _modelData[o + 3] = float.Parse(p[3]);
            _modelData[o + 4] = float.Parse(p[4]);
            _modelData[o + 5] = float.Parse(p[5]);
            _modelData[o + 6] = float.Parse(p[6]);
            _modelData[o + 7] = float.Parse(p[7]);
            vi++;
        }
        return vi == vc;
    }

    private unsafe bool InitBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        var v = new VertexType[_vertexCount];
        var idx = new uint[_indexCount];
        for (int i = 0; i < _vertexCount; i++)
        {
            int o = i * 8;
            v[i].x = _modelData[o];
            v[i].y = _modelData[o + 1];
            v[i].z = _modelData[o + 2];
            v[i].tu = _modelData[o + 3];
            v[i].tv = _modelData[o + 4];
            v[i].nx = _modelData[o + 5];
            v[i].ny = _modelData[o + 6];
            v[i].nz = _modelData[o + 7];
            idx[i] = (uint)i;
        }
        _vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(_vertexArrayId);
        _vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = v)
            gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * v.Length),
                p,
                BufferUsageARB.StaticDraw
            );
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)0
        );
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)(3 * sizeof(float))
        );
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
        fixed (uint* p = idx)
            gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * idx.Length),
                p,
                BufferUsageARB.StaticDraw
            );
        _modelData = null;
        return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
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

using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class Model
{
    private struct VT
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
        _inputContext;
    private Texture[] _textures;
    private float[] _modelData;

    public unsafe bool Initialize(
        GL4 gl,
        string mf,
        string t1,
        bool w1,
        string t2 = null,
        bool w2 = false
    )
    {
        if (!LoadModel(mf))
            return false;
        if (!InitBuf(gl))
            return false;
        _textures = new Texture[3];
        if (t1 != null)
        {
            _textures[0] = new Texture();
            if (!_textures[0].Initialize(gl, t1, 0, w1))
                return false;
        }
        if (t2 != null)
        {
            _textures[1] = new Texture();
            if (!_textures[1].Initialize(gl, t2, 0, w2))
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
        ShutBuf(gl);
    }

    public unsafe void Render(GL4 gl)
    {
        gl.Gl.BindVertexArray(_vertexArrayId);
        gl.Gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)_inputContext,
            DrawElementsType.UnsignedInt,
            (void*)0
        );
    }

    public void SetTexture1(GL4 gl, uint tu)
    {
        _textures?[0]?.SetTexture(gl, tu);
    }

    public void SetTexture2(GL4 gl, uint tu)
    {
        _textures?[1]?.SetTexture(gl, tu);
    }

    bool LoadModel(string fn)
    {
        if (!File.Exists(fn))
            return false;
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
        _inputContext = vc;
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

    unsafe bool InitBuf(GL4 gl)
    {
        var g = gl.Gl;
        var v = new VT[_vertexCount];
        var idx = new uint[_inputContext];
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
        _vertexArrayId = g.GenVertexArray();
        g.BindVertexArray(_vertexArrayId);
        _vertexBufferId = g.GenBuffer();
        g.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VT* p = v)
            g.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VT) * v.Length),
                p,
                BufferUsageARB.StaticDraw
            );
        g.EnableVertexAttribArray(0);
        g.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VT),
            (void*)0
        );
        g.EnableVertexAttribArray(1);
        g.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VT),
            (void*)(3 * sizeof(float))
        );
        g.EnableVertexAttribArray(2);
        g.VertexAttribPointer(
            2,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VT),
            (void*)(5 * sizeof(float))
        );
        _indexBufferId = g.GenBuffer();
        g.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = idx)
            g.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * idx.Length),
                p,
                BufferUsageARB.StaticDraw
            );
        _modelData = null;
        return true;
    }

    void ShutBuf(GL4 gl)
    {
        var g = gl.Gl;
        g.DisableVertexAttribArray(0);
        g.DisableVertexAttribArray(1);
        g.DisableVertexAttribArray(2);
        g.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        g.DeleteBuffer(_vertexBufferId);
        g.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        g.DeleteBuffer(_indexBufferId);
        g.BindVertexArray(0);
        g.DeleteVertexArray(_vertexArrayId);
    }
}

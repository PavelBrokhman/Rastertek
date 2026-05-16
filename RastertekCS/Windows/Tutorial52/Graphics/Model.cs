using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace RastertekCS.Windows.Tutorial52.Graphics;

public unsafe class Model
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
        public float nx, ny, nz;
        public float tx, ty, tz;
        public float bx, by, bz;
    }

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount, _indexCount;
    private Texture _texture1, _texture2, _texture3;

    public bool Initialize(DX11 DirectX, string modelFilename, string tex1, string tex2, string tex3)
    {
        if (!LoadModel(modelFilename)) return false;
        CalculateModelVectors();
        if (!InitializeBuffers(DirectX)) return false;
        _texture1 = new Texture(); if (!_texture1.Initialize(DirectX, tex1, true)) return false;
        _texture2 = new Texture(); if (!_texture2.Initialize(DirectX, tex2, true)) return false;
        _texture3 = new Texture(); if (!_texture3.Initialize(DirectX, tex3, true)) return false;
        return true;
    }

    public void Shutdown()
    {
        _texture3?.Shutdown(); _texture2?.Shutdown(); _texture1?.Shutdown();
        _indexBuffer.Release(); _vertexBuffer.Release();
    }

    public void Render(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        uint stride = (uint)sizeof(VertexType), offset = 0;
        var vb = _vertexBuffer.GetPinnableReference();
        context.IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        context.IASetIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR32Uint, 0);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    }

    public int GetIndexCount() => _indexCount;
    public ComPtr<ID3D11ShaderResourceView> GetTextureView1() => _texture1.GetTextureView();
    public ComPtr<ID3D11ShaderResourceView> GetTextureView2() => _texture2.GetTextureView();
    public ComPtr<ID3D11ShaderResourceView> GetTextureView3() => _texture3.GetTextureView();

    private float[] _pos, _tex, _norm;
    private float[] _tan, _bin;

    private bool LoadModel(string filename)
    {
        if (!File.Exists(filename)) { Console.WriteLine($"Model not found: {filename}"); return false; }
        var lines = File.ReadAllLines(filename);
        int vc = 0, ds = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            var l = lines[i].Trim();
            if (l.StartsWith("Vertex Count:")) vc = int.Parse(l.Substring("Vertex Count:".Length).Trim());
            if (l == "Data:") { ds = i + 1; break; }
        }
        if (vc == 0 || ds < 0) return false;
        _vertexCount = vc; _indexCount = vc;
        _pos = new float[vc * 3]; _tex = new float[vc * 2]; _norm = new float[vc * 3];
        _tan = new float[vc * 3]; _bin = new float[vc * 3];
        int vi = 0;
        for (int i = ds; i < lines.Length && vi < vc; i++)
        {
            var l = lines[i].Trim();
            if (string.IsNullOrEmpty(l)) continue;
            var p = l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length < 8) continue;
            _pos[vi * 3] = float.Parse(p[0]); _pos[vi * 3 + 1] = float.Parse(p[1]); _pos[vi * 3 + 2] = float.Parse(p[2]);
            // DX: no V-flip in Model (matches C++ DirectX/Tutorial52/modelclass.cpp)
            _tex[vi * 2] = float.Parse(p[3]); _tex[vi * 2 + 1] = float.Parse(p[4]);
            _norm[vi * 3] = float.Parse(p[5]); _norm[vi * 3 + 1] = float.Parse(p[6]); _norm[vi * 3 + 2] = float.Parse(p[7]);
            vi++;
        }
        return vi == vc;
    }

    private void CalculateModelVectors()
    {
        int faceCount = _vertexCount / 3;
        for (int f = 0; f < faceCount; f++)
        {
            int i0 = f * 3, i1 = f * 3 + 1, i2 = f * 3 + 2;
            float dx1 = _pos[i1 * 3] - _pos[i0 * 3], dy1 = _pos[i1 * 3 + 1] - _pos[i0 * 3 + 1], dz1 = _pos[i1 * 3 + 2] - _pos[i0 * 3 + 2];
            float dx2 = _pos[i2 * 3] - _pos[i0 * 3], dy2 = _pos[i2 * 3 + 1] - _pos[i0 * 3 + 1], dz2 = _pos[i2 * 3 + 2] - _pos[i0 * 3 + 2];
            float du1 = _tex[i1 * 2] - _tex[i0 * 2], dv1 = _tex[i1 * 2 + 1] - _tex[i0 * 2 + 1];
            float du2 = _tex[i2 * 2] - _tex[i0 * 2], dv2 = _tex[i2 * 2 + 1] - _tex[i0 * 2 + 1];
            float den = 1.0f / (du1 * dv2 - du2 * dv1 + 0.00001f);
            float tx = (dv2 * dx1 - dv1 * dx2) * den, ty = (dv2 * dy1 - dv1 * dy2) * den, tz = (dv2 * dz1 - dv1 * dz2) * den;
            float bx = (du1 * dx2 - du2 * dx1) * den, by = (du1 * dy2 - du2 * dy1) * den, bz = (du1 * dz2 - du2 * dz1) * den;
            float tl = MathF.Sqrt(tx * tx + ty * ty + tz * tz) + 0.00001f;
            tx /= tl; ty /= tl; tz /= tl;
            float bl = MathF.Sqrt(bx * bx + by * by + bz * bz) + 0.00001f;
            bx /= bl; by /= bl; bz /= bl;
            for (int k = 0; k < 3; k++)
            {
                int idx = f * 3 + k;
                _tan[idx * 3] = tx; _tan[idx * 3 + 1] = ty; _tan[idx * 3 + 2] = tz;
                _bin[idx * 3] = bx; _bin[idx * 3 + 1] = by; _bin[idx * 3 + 2] = bz;
            }
        }
    }

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;
        var vertices = new VertexType[_vertexCount];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _vertexCount; i++)
        {
            vertices[i].x = _pos[i * 3]; vertices[i].y = _pos[i * 3 + 1]; vertices[i].z = _pos[i * 3 + 2];
            vertices[i].tu = _tex[i * 2]; vertices[i].tv = _tex[i * 2 + 1];
            vertices[i].nx = _norm[i * 3]; vertices[i].ny = _norm[i * 3 + 1]; vertices[i].nz = _norm[i * 3 + 2];
            vertices[i].tx = _tan[i * 3]; vertices[i].ty = _tan[i * 3 + 1]; vertices[i].tz = _tan[i * 3 + 2];
            vertices[i].bx = _bin[i * 3]; vertices[i].by = _bin[i * 3 + 1]; vertices[i].bz = _bin[i * 3 + 2];
            indices[i] = (uint)i;
        }

        fixed (VertexType* pV = vertices)
        {
            var vbDesc = new BufferDesc { Usage = Usage.Default, ByteWidth = (uint)(sizeof(VertexType) * _vertexCount), BindFlags = (uint)BindFlag.VertexBuffer, CPUAccessFlags = 0, MiscFlags = 0, StructureByteStride = 0 };
            var initVB = new SubresourceData { PSysMem = pV };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&vbDesc, &initVB, ref _vertexBuffer));
        }
        fixed (uint* pI = indices)
        {
            var ibDesc = new BufferDesc { Usage = Usage.Default, ByteWidth = (uint)(sizeof(uint) * _indexCount), BindFlags = (uint)BindFlag.IndexBuffer, CPUAccessFlags = 0, MiscFlags = 0, StructureByteStride = 0 };
            var initIB = new SubresourceData { PSysMem = pI };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&ibDesc, &initIB, ref _indexBuffer));
        }
        _pos = null; _tex = null; _norm = null; _tan = null; _bin = null;
        return true;
    }
}

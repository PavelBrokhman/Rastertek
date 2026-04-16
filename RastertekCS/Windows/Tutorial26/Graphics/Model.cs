using System.Globalization;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial26.Graphics;

public unsafe class Model
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

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount;
    private int _indexCount;
    private ModelType[] _model;
    private Texture _texture;

    public bool Initialize(DX11 DirectX, string modelFilename, string textureFilename, bool wrap)
    {
        var device = DirectX.Device;

        if (!LoadModel(modelFilename))
            return false;

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
            indices[i] = (uint)i;
        }

        fixed (VertexType* pVertices = vertices)
        {
            var vertexBufferDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(VertexType) * vertices.Length),
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var vertexData = new SubresourceData { PSysMem = pVertices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&vertexBufferDesc, &vertexData, ref _vertexBuffer)
            );
        }

        fixed (uint* pIndices = indices)
        {
            var indexBufferDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(uint) * indices.Length),
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var indexData = new SubresourceData { PSysMem = pIndices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&indexBufferDesc, &indexData, ref _indexBuffer)
            );
        }

        _texture = new Texture();
        if (!_texture.Initialize(DirectX, textureFilename, wrap))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _texture?.Shutdown();
        _texture = null;
        _indexBuffer.Release();
        _vertexBuffer.Release();
        _model = null;
    }

    public void Render(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;

        uint stride = (uint)sizeof(VertexType);
        uint offset = 0;
        var vb = _vertexBuffer.GetPinnableReference();
        context.IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        context.IASetIndexBuffer(_indexBuffer, Format.FormatR32Uint, 0);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
    }

    public void SetTexture(DX11 DirectX, uint slot)
    {
        _texture.SetTexture(DirectX, slot);
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
        int idx = 0;

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
}

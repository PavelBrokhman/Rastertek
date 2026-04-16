using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial25.Graphics;

public unsafe class DisplayPlane
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

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount;
    private int _indexCount;

    public bool Initialize(DX11 DirectX, float width, float height)
    {
        var device = DirectX.Device;
        _vertexCount = 6;
        _indexCount = 6;

        var vertices = new VertexType[]
        {
            new()
            {
                x = -width,
                y = height,
                z = 0,
                tu = 0,
                tv = 0,
            },
            new()
            {
                x = width,
                y = -height,
                z = 0,
                tu = 1,
                tv = 1,
            },
            new()
            {
                x = -width,
                y = -height,
                z = 0,
                tu = 0,
                tv = 1,
            },
            new()
            {
                x = -width,
                y = height,
                z = 0,
                tu = 0,
                tv = 0,
            },
            new()
            {
                x = width,
                y = height,
                z = 0,
                tu = 1,
                tv = 0,
            },
            new()
            {
                x = width,
                y = -height,
                z = 0,
                tu = 1,
                tv = 1,
            },
        };
        var indices = new uint[] { 0, 1, 2, 3, 4, 5 };

        fixed (VertexType* pVertices = vertices)
        {
            var vbd = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(VertexType) * _vertexCount),
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var vd = new SubresourceData { PSysMem = pVertices };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&vbd, &vd, ref _vertexBuffer));
        }

        fixed (uint* pIndices = indices)
        {
            var ibd = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(uint) * _indexCount),
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var id = new SubresourceData { PSysMem = pIndices };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&ibd, &id, ref _indexBuffer));
        }

        return true;
    }

    public void Shutdown()
    {
        _indexBuffer.Release();
        _vertexBuffer.Release();
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

    public int GetIndexCount() => _indexCount;
}

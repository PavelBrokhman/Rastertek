using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial49.Graphics;

public unsafe class Model
{
    private struct VertexType
    {
        public Vector3D<float> position;
        public Vector4D<float> color;
    }

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount;
    private int _indexCount;

    public bool Initialize(DX11 DirectX) => InitializeBuffers(DirectX);

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
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopology3ControlPointPatchlist);
    }

    public int GetIndexCount() => _indexCount;

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;
        _vertexCount = 3;
        _indexCount = 3;
        var vertices = new VertexType[3]
        {
            new() { position = new(-1.0f, -1.0f, 0.0f), color = new(0.0f, 1.0f, 0.0f, 1.0f) },
            new() { position = new( 0.0f,  1.0f, 0.0f), color = new(0.0f, 1.0f, 0.0f, 1.0f) },
            new() { position = new( 1.0f, -1.0f, 0.0f), color = new(0.0f, 1.0f, 0.0f, 1.0f) },
        };
        var indices = new uint[] { 0, 1, 2 };

        fixed (VertexType* pV = vertices)
        {
            var vbDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(VertexType) * _vertexCount),
                BindFlags = (uint)BindFlag.VertexBuffer,
            };
            var vbData = new SubresourceData { PSysMem = pV };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&vbDesc, &vbData, ref _vertexBuffer));
        }
        fixed (uint* pI = indices)
        {
            var ibDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(uint) * _indexCount),
                BindFlags = (uint)BindFlag.IndexBuffer,
            };
            var ibData = new SubresourceData { PSysMem = pI };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&ibDesc, &ibData, ref _indexBuffer));
        }
        return true;
    }
}

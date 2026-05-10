using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial50.Graphics;

public unsafe class OrthoWindow
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount, _indexCount;

    public bool Initialize(DX11 DirectX, int windowWidth, int windowHeight)
    {
        var device = DirectX.Device;

        float left = -windowWidth / 2.0f;
        float right = left + windowWidth;
        float top = windowHeight / 2.0f;
        float bottom = top - windowHeight;

        _vertexCount = 6;
        _indexCount = 6;
        var vertices = new VertexType[6]
        {
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
            new() { x = left,  y = bottom, z = 0, tu = 0, tv = 1 },
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = top,    z = 0, tu = 1, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
        };
        var indices = new uint[] { 0, 1, 2, 3, 4, 5 };

        fixed (VertexType* pV = vertices)
        {
            var vbDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(VertexType) * 6),
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
                ByteWidth = (uint)(sizeof(uint) * 6),
                BindFlags = (uint)BindFlag.IndexBuffer,
            };
            var ibData = new SubresourceData { PSysMem = pI };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&ibDesc, &ibData, ref _indexBuffer));
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

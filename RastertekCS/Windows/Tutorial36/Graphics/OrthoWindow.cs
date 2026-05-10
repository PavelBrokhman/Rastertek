using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial36.Graphics;

public unsafe class OrthoWindow
{
    private struct VertexType
    {
        public float x,
            y,
            z;
        public float tu,
            tv;
    }

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount;
    private int _indexCount;

    public bool Initialize(DX11 DirectX, int windowWidth, int windowHeight)
    {
        var device = DirectX.Device;
        float left = -(windowWidth / 2f);
        float right = left + windowWidth;
        float top = windowHeight / 2f;
        float bottom = top - windowHeight;
        _vertexCount = 6;
        _indexCount = 6;
        var vertices = new VertexType[6];
        var indices = new uint[6];
        vertices[0] = new VertexType { x = left, y = top, z = 0, tu = 0, tv = 0 };
        vertices[1] = new VertexType { x = right, y = bottom, z = 0, tu = 1, tv = 1 };
        vertices[2] = new VertexType { x = left, y = bottom, z = 0, tu = 0, tv = 1 };
        vertices[3] = new VertexType { x = left, y = top, z = 0, tu = 0, tv = 0 };
        vertices[4] = new VertexType { x = right, y = top, z = 0, tu = 1, tv = 0 };
        vertices[5] = new VertexType { x = right, y = bottom, z = 0, tu = 1, tv = 1 };
        for (int i = 0; i < 6; i++)
            indices[i] = (uint)i;

        fixed (VertexType* pVertices = vertices)
        {
            var vbDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(VertexType) * 6),
                BindFlags = (uint)BindFlag.VertexBuffer,
            };
            var vbData = new SubresourceData { PSysMem = pVertices };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&vbDesc, &vbData, ref _vertexBuffer));
        }
        fixed (uint* pIndices = indices)
        {
            var ibDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(uint) * 6),
                BindFlags = (uint)BindFlag.IndexBuffer,
            };
            var ibData = new SubresourceData { PSysMem = pIndices };
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

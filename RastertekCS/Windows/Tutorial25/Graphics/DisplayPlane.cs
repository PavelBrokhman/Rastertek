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

    private ComPtr<ID3D11Buffer> m_vertexBuffer;
    private ComPtr<ID3D11Buffer> m_indexBuffer;
    private int m_vertexCount;
    private int m_indexCount;

    public bool Initialize(DX11 DirectX, float width, float height)
    {
        var device = DirectX.Device;
        m_vertexCount = 6;
        m_indexCount = 6;

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
                ByteWidth = (uint)(sizeof(VertexType) * m_vertexCount),
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var vd = new SubresourceData { PSysMem = pVertices };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&vbd, &vd, ref m_vertexBuffer));
        }

        fixed (uint* pIndices = indices)
        {
            var ibd = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(uint) * m_indexCount),
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var id = new SubresourceData { PSysMem = pIndices };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&ibd, &id, ref m_indexBuffer));
        }

        return true;
    }

    public void Shutdown()
    {
        m_indexBuffer.Release();
        m_vertexBuffer.Release();
    }

    public void Render(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        uint stride = (uint)sizeof(VertexType);
        uint offset = 0;
        var vb = m_vertexBuffer.GetPinnableReference();
        context.IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        context.IASetIndexBuffer(m_indexBuffer, Format.FormatR32Uint, 0);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
    }

    public int GetIndexCount() => m_indexCount;
}

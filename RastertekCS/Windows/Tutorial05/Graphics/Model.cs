using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial05.Graphics;

public unsafe class Model
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private ComPtr<ID3D11Buffer> m_vertexBuffer;
    private ComPtr<ID3D11Buffer> m_indexBuffer;
    private int m_indexCount;

    private Texture m_Texture;

    public bool Initialize(DX11 DirectX, string textureFilename, bool wrap)
    {
        var device = DirectX.Device;
        m_indexCount = 3;

        var vertices = new VertexType[]
        {
            new() { x = -1.0f, y = -1.0f, z = 0.0f, tu = 0.0f, tv = 1.0f },
            new() { x =  0.0f, y =  1.0f, z = 0.0f, tu = 0.5f, tv = 0.0f },
            new() { x =  1.0f, y = -1.0f, z = 0.0f, tu = 1.0f, tv = 1.0f },
        };
        var indices = new uint[] { 0, 1, 2 };

        // Create vertex buffer.
        fixed (VertexType* pVertices = vertices)
        {
            var vertexBufferDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(VertexType) * vertices.Length),
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = 0, MiscFlags = 0, StructureByteStride = 0
            };
            var vertexData = new SubresourceData { PSysMem = pVertices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&vertexBufferDesc, &vertexData, ref m_vertexBuffer));
        }

        // Create index buffer.
        fixed (uint* pIndices = indices)
        {
            var indexBufferDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(uint) * indices.Length),
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = 0, MiscFlags = 0, StructureByteStride = 0
            };
            var indexData = new SubresourceData { PSysMem = pIndices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&indexBufferDesc, &indexData, ref m_indexBuffer));
        }

        // Load texture.
        m_Texture = new Texture();
        if (!m_Texture.Initialize(DirectX, textureFilename, wrap)) return false;

        return true;
    }

    public void Shutdown()
    {
        m_Texture?.Shutdown();
        m_Texture = null;
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

    public void SetTexture(DX11 DirectX, uint slot)
    {
        m_Texture.SetTexture(DirectX, slot);
    }

    public int GetIndexCount() => m_indexCount;
}

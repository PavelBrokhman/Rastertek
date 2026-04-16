using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial23.Graphics;

public unsafe class Text
{
    // 5 floats per vertex: x y z tu tv (matches Font.BuildVertexArray output).
    private const int FLOATS_PER_VERTEX = 5;

    private ComPtr<ID3D11Buffer> m_vertexBuffer;
    private ComPtr<ID3D11Buffer> m_indexBuffer;
    private int m_vertexCount;
    private int m_indexCount;
    private int m_maxLength;
    private int m_screenWidth;
    private int m_screenHeight;
    private readonly float[] m_pixelColor = new float[4];

    public bool Initialize(
        DX11 DirectX,
        int screenWidth,
        int screenHeight,
        int maxLength,
        Font font,
        string text,
        int positionX,
        int positionY,
        float red,
        float green,
        float blue
    )
    {
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;
        m_maxLength = maxLength;

        if (!InitializeBuffers(DirectX))
            return false;
        if (!UpdateText(DirectX, font, text, positionX, positionY, red, green, blue))
            return false;
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
        uint stride = FLOATS_PER_VERTEX * sizeof(float);
        uint offset = 0;
        var vb = m_vertexBuffer.GetPinnableReference();
        context.IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        context.IASetIndexBuffer(m_indexBuffer, Format.FormatR32Uint, 0);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    }

    public int GetIndexCount() => m_indexCount;

    public float[] GetPixelColor() => m_pixelColor;

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;

        m_vertexCount = 6 * m_maxLength;
        m_indexCount = m_vertexCount;

        var vertices = new float[m_vertexCount * FLOATS_PER_VERTEX];
        var indices = new uint[m_indexCount];
        for (int i = 0; i < m_indexCount; i++)
            indices[i] = (uint)i;

        var vertexBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)(sizeof(float) * vertices.Length),
            BindFlags = (uint)BindFlag.VertexBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        fixed (float* pVertices = vertices)
        {
            var vertexData = new SubresourceData { PSysMem = pVertices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&vertexBufferDesc, &vertexData, ref m_vertexBuffer)
            );
        }

        var indexBufferDesc = new BufferDesc
        {
            Usage = Usage.Default,
            ByteWidth = (uint)(sizeof(uint) * m_indexCount),
            BindFlags = (uint)BindFlag.IndexBuffer,
            CPUAccessFlags = 0,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        fixed (uint* pIndices = indices)
        {
            var indexData = new SubresourceData { PSysMem = pIndices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&indexBufferDesc, &indexData, ref m_indexBuffer)
            );
        }

        return true;
    }

    public bool UpdateText(
        DX11 DirectX,
        Font font,
        string text,
        int positionX,
        int positionY,
        float red,
        float green,
        float blue
    )
    {
        m_pixelColor[0] = red;
        m_pixelColor[1] = green;
        m_pixelColor[2] = blue;
        m_pixelColor[3] = 1.0f;

        if (text.Length > m_maxLength)
            return false;

        var vertices = new float[m_vertexCount * FLOATS_PER_VERTEX];

        float drawX = -(m_screenWidth / 2) + positionX;
        float drawY = (m_screenHeight / 2) - positionY;

        font.BuildVertexArray(vertices, text, drawX, drawY);

        var context = DirectX.DeviceContext;
        MappedSubresource mappedResource;
        SilkMarshal.ThrowHResult(
            context.Map(m_vertexBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        fixed (float* pSrc = vertices)
        {
            global::System.Buffer.MemoryCopy(
                pSrc,
                mappedResource.PData,
                sizeof(float) * vertices.Length,
                sizeof(float) * vertices.Length
            );
        }
        context.Unmap(m_vertexBuffer, 0);

        return true;
    }
}

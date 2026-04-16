using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial15.Graphics;

public unsafe class Text
{
    // 5 floats per vertex: x y z tu tv (matches Font.BuildVertexArray output).
    private const int FLOATS_PER_VERTEX = 5;

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount;
    private int _indexCount;
    private int _maxLength;
    private int _screenWidth;
    private int _screenHeight;
    private readonly float[] _pixelColor = new float[4];

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
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _maxLength = maxLength;

        if (!InitializeBuffers(DirectX))
            return false;
        if (!UpdateText(DirectX, font, text, positionX, positionY, red, green, blue))
            return false;
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
        uint stride = FLOATS_PER_VERTEX * sizeof(float);
        uint offset = 0;
        var vb = _vertexBuffer.GetPinnableReference();
        context.IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        context.IASetIndexBuffer(_indexBuffer, Format.FormatR32Uint, 0);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    }

    public int GetIndexCount() => _indexCount;

    public float[] GetPixelColor() => _pixelColor;

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;

        _vertexCount = 6 * _maxLength;
        _indexCount = _vertexCount;

        var vertices = new float[_vertexCount * FLOATS_PER_VERTEX];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _indexCount; i++)
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
                device.CreateBuffer(&vertexBufferDesc, &vertexData, ref _vertexBuffer)
            );
        }

        var indexBufferDesc = new BufferDesc
        {
            Usage = Usage.Default,
            ByteWidth = (uint)(sizeof(uint) * _indexCount),
            BindFlags = (uint)BindFlag.IndexBuffer,
            CPUAccessFlags = 0,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        fixed (uint* pIndices = indices)
        {
            var indexData = new SubresourceData { PSysMem = pIndices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&indexBufferDesc, &indexData, ref _indexBuffer)
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
        _pixelColor[0] = red;
        _pixelColor[1] = green;
        _pixelColor[2] = blue;
        _pixelColor[3] = 1.0f;

        if (text.Length > _maxLength)
            return false;

        var vertices = new float[_vertexCount * FLOATS_PER_VERTEX];

        float drawX = -(_screenWidth / 2) + positionX;
        float drawY = (_screenHeight / 2) - positionY;

        font.BuildVertexArray(vertices, text, drawX, drawY);

        var context = DirectX.DeviceContext;
        MappedSubresource mappedResource;
        SilkMarshal.ThrowHResult(
            context.Map(_vertexBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
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
        context.Unmap(_vertexBuffer, 0);

        return true;
    }
}

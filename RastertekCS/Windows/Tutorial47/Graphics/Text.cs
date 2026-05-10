using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial47.Graphics;

public unsafe class Text
{
    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount, _indexCount;
    private int _screenWidth, _screenHeight;
    private int _maxLength;
    private Vector4D<float> _pixelColor;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight, int maxLength,
        Font font, string text, int positionX, int positionY, float r, float g, float b)
    {
        _screenWidth = screenWidth; _screenHeight = screenHeight; _maxLength = maxLength;
        _vertexCount = 6 * maxLength;
        _indexCount = _vertexCount;

        var device = DirectX.Device;
        var vertices = new Font.VertexType[_vertexCount];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _indexCount; i++) indices[i] = (uint)i;

        fixed (Font.VertexType* pV = vertices)
        {
            var vbDesc = new BufferDesc
            {
                Usage = Usage.Dynamic,
                ByteWidth = (uint)(sizeof(Font.VertexType) * _vertexCount),
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = (uint)CpuAccessFlag.Write,
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

        return UpdateText(DirectX, font, text, positionX, positionY, r, g, b);
    }

    public void Shutdown()
    {
        _indexBuffer.Release();
        _vertexBuffer.Release();
    }

    public bool UpdateText(DX11 DirectX, Font font, string text, int positionX, int positionY, float r, float g, float b)
    {
        if (text.Length > _maxLength) return false;
        _pixelColor = new Vector4D<float>(r, g, b, 1.0f);

        var vertices = new Font.VertexType[_vertexCount];
        float drawX = (-_screenWidth / 2.0f) + positionX;
        float drawY = (_screenHeight / 2.0f) - positionY;
        font.BuildVertexArray(vertices, text, drawX, drawY);

        var context = DirectX.DeviceContext;
        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_vertexBuffer, 0, Map.WriteDiscard, 0, &mr));
        fixed (Font.VertexType* pV = vertices)
        {
            global::System.Buffer.MemoryCopy(pV, mr.PData, sizeof(Font.VertexType) * _vertexCount, sizeof(Font.VertexType) * _vertexCount);
        }
        context.Unmap(_vertexBuffer, 0);
        return true;
    }

    public void Render(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        uint stride = (uint)sizeof(Font.VertexType);
        uint offset = 0;
        var vb = _vertexBuffer.GetPinnableReference();
        context.IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        context.IASetIndexBuffer(_indexBuffer, Format.FormatR32Uint, 0);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
    }

    public int GetIndexCount() => _indexCount;
    public Vector4D<float> GetPixelColor() => _pixelColor;
}

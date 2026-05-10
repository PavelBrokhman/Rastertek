using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial47.Graphics;

public unsafe class Bitmap
{
    private struct VertexType
    {
        public float x, y, z;
        public float tu, tv;
    }

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount, _indexCount;
    private int _screenWidth, _screenHeight;
    private int _bitmapWidth, _bitmapHeight;
    private int _renderX, _renderY;
    private int _prevPosX = -1, _prevPosY = -1;
    private Texture _texture;

    public bool Initialize(DX11 DirectX, int screenWidth, int screenHeight, string textureFilename, int renderX, int renderY)
    {
        _screenWidth = screenWidth; _screenHeight = screenHeight;
        _renderX = renderX; _renderY = renderY;

        if (!InitializeBuffers(DirectX)) return false;
        _texture = new Texture();
        if (!_texture.Initialize(DirectX, textureFilename, false)) return false;
        _bitmapWidth = _texture.GetWidth();
        _bitmapHeight = _texture.GetHeight();
        return true;
    }

    public void Shutdown()
    {
        _texture?.Shutdown(); _texture = null;
        _indexBuffer.Release();
        _vertexBuffer.Release();
    }

    public bool Render(DX11 DirectX)
    {
        if (!UpdateBuffers(DirectX)) return false;
        RenderBuffers(DirectX);
        return true;
    }

    public int GetIndexCount() => _indexCount;
    public ComPtr<ID3D11ShaderResourceView> GetTextureView() => _texture.GetTextureView();

    public void SetRenderLocation(int x, int y) { _renderX = x; _renderY = y; }

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;
        _vertexCount = 6;
        _indexCount = 6;
        var vertices = new VertexType[6];
        var indices = new uint[6];
        for (int i = 0; i < 6; i++) indices[i] = (uint)i;

        fixed (VertexType* pV = vertices)
        {
            var vbDesc = new BufferDesc
            {
                Usage = Usage.Dynamic,
                ByteWidth = (uint)(sizeof(VertexType) * _vertexCount),
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
        return true;
    }

    private bool UpdateBuffers(DX11 DirectX)
    {
        if (_prevPosX == _renderX && _prevPosY == _renderY) return true;
        _prevPosX = _renderX; _prevPosY = _renderY;

        float left = (_screenWidth / 2) * -1.0f + _renderX;
        float right = left + _bitmapWidth;
        float top = (_screenHeight / 2) - (float)_renderY;
        float bottom = top - _bitmapHeight;

        var vertices = new VertexType[6]
        {
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
            new() { x = left,  y = bottom, z = 0, tu = 0, tv = 1 },
            new() { x = left,  y = top,    z = 0, tu = 0, tv = 0 },
            new() { x = right, y = top,    z = 0, tu = 1, tv = 0 },
            new() { x = right, y = bottom, z = 0, tu = 1, tv = 1 },
        };

        var context = DirectX.DeviceContext;
        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_vertexBuffer, 0, Map.WriteDiscard, 0, &mr));
        fixed (VertexType* pV = vertices)
        {
            global::System.Buffer.MemoryCopy(pV, mr.PData, sizeof(VertexType) * _vertexCount, sizeof(VertexType) * _vertexCount);
        }
        context.Unmap(_vertexBuffer, 0);
        return true;
    }

    private void RenderBuffers(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        uint stride = (uint)sizeof(VertexType);
        uint offset = 0;
        var vb = _vertexBuffer.GetPinnableReference();
        context.IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        context.IASetIndexBuffer(_indexBuffer, Format.FormatR32Uint, 0);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
    }
}

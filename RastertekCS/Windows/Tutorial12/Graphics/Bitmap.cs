using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial12.Graphics;

public unsafe class Bitmap
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
    private Texture _texture;

    private int _screenWidth,
        _screenHeight;
    private int _bitmapWidth,
        _bitmapHeight;
    private int _renderX,
        _renderY;
    private int _previousPositionX = -1,
        _previousPositionY = -1;

    public bool Initialize(
        DX11 DirectX,
        int screenWidth,
        int screenHeight,
        string textureFilename,
        int bitmapWidth,
        int bitmapHeight
    )
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _bitmapWidth = bitmapWidth;
        _bitmapHeight = bitmapHeight;
        _renderX = 0;
        _renderY = 0;

        if (!InitializeBuffers(DirectX))
            return false;
        if (!LoadTexture(DirectX, textureFilename))
            return false;

        return true;
    }

    public void Shutdown()
    {
        _texture?.Shutdown();
        _texture = null;
        _indexBuffer.Release();
        _vertexBuffer.Release();
    }

    public bool Render(DX11 DirectX)
    {
        if (!UpdateBuffers(DirectX))
            return false;
        RenderBuffers(DirectX);
        return true;
    }

    public int GetIndexCount() => _indexCount;

    public void SetTexture(DX11 DirectX, uint slot) => _texture.SetTexture(DirectX, slot);

    public void SetRenderLocation(int x, int y)
    {
        _renderX = x;
        _renderY = y;
    }

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;

        _vertexCount = 6;
        _indexCount = 6;

        var vertices = new VertexType[_vertexCount];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _indexCount; i++)
            indices[i] = (uint)i;

        var vertexBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)(sizeof(VertexType) * _vertexCount),
            BindFlags = (uint)BindFlag.VertexBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };

        fixed (VertexType* pVertices = vertices)
        {
            var vertexData = new SubresourceData
            {
                PSysMem = pVertices,
                SysMemPitch = 0,
                SysMemSlicePitch = 0,
            };
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
            var indexData = new SubresourceData
            {
                PSysMem = pIndices,
                SysMemPitch = 0,
                SysMemSlicePitch = 0,
            };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&indexBufferDesc, &indexData, ref _indexBuffer)
            );
        }

        return true;
    }

    private bool UpdateBuffers(DX11 DirectX)
    {
        if (_previousPositionX == _renderX && _previousPositionY == _renderY)
            return true;

        _previousPositionX = _renderX;
        _previousPositionY = _renderY;

        float left = (_screenWidth / 2 * -1) + (float)_renderX;
        float right = left + _bitmapWidth;
        float top = (_screenHeight / 2) - (float)_renderY;
        float bottom = top - _bitmapHeight;

        var vertices = new VertexType[6]
        {
            new()
            {
                x = left,
                y = top,
                z = 0,
                tu = 0,
                tv = 0,
            },
            new()
            {
                x = right,
                y = bottom,
                z = 0,
                tu = 1,
                tv = 1,
            },
            new()
            {
                x = left,
                y = bottom,
                z = 0,
                tu = 0,
                tv = 1,
            },
            new()
            {
                x = left,
                y = top,
                z = 0,
                tu = 0,
                tv = 0,
            },
            new()
            {
                x = right,
                y = top,
                z = 0,
                tu = 1,
                tv = 0,
            },
            new()
            {
                x = right,
                y = bottom,
                z = 0,
                tu = 1,
                tv = 1,
            },
        };

        var context = DirectX.DeviceContext;
        MappedSubresource mappedResource;
        SilkMarshal.ThrowHResult(
            context.Map(_vertexBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        fixed (VertexType* pSrc = vertices)
        {
            global::System.Buffer.MemoryCopy(
                pSrc,
                mappedResource.PData,
                sizeof(VertexType) * _vertexCount,
                sizeof(VertexType) * _vertexCount
            );
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
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    }

    private bool LoadTexture(DX11 DirectX, string filename)
    {
        _texture = new Texture();
        return _texture.Initialize(DirectX, filename, false);
    }
}

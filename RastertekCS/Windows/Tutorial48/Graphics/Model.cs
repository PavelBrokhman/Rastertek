using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial48.Graphics;

public unsafe class Model
{
    private struct VertexType
    {
        public Vector3D<float> position;
        public Vector2D<float> texture;
    }

    private struct InstanceType
    {
        public Vector3D<float> position;
    }

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _instanceBuffer;
    private int _vertexCount;
    private int _instanceCount;
    private Texture _texture;

    public bool Initialize(DX11 DirectX, string textureFilename)
    {
        if (!InitializeBuffers(DirectX)) return false;
        _texture = new Texture();
        return _texture.Initialize(DirectX, textureFilename);
    }

    public void Shutdown()
    {
        _texture?.Shutdown(); _texture = null;
        _instanceBuffer.Release();
        _vertexBuffer.Release();
    }

    public void Render(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        uint* strides = stackalloc uint[2] { (uint)sizeof(VertexType), (uint)sizeof(InstanceType) };
        uint* offsets = stackalloc uint[2] { 0, 0 };
        ID3D11Buffer** buffers = stackalloc ID3D11Buffer*[2]
        {
            _vertexBuffer.GetPinnableReference(),
            _instanceBuffer.GetPinnableReference(),
        };
        context.IASetVertexBuffers(0, 2, buffers, strides, offsets);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
    }

    public int GetVertexCount() => _vertexCount;
    public int GetInstanceCount() => _instanceCount;
    public ComPtr<ID3D11ShaderResourceView> GetTextureView() => _texture.GetTextureView();

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;
        _vertexCount = 3;
        var vertices = new VertexType[3]
        {
            new() { position = new(-1.0f, -1.0f, 0.0f), texture = new(0.0f, 1.0f) },
            new() { position = new(0.0f, 1.0f, 0.0f),   texture = new(0.5f, 0.0f) },
            new() { position = new(1.0f, -1.0f, 0.0f),  texture = new(1.0f, 1.0f) },
        };
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

        _instanceCount = 4;
        var instances = new InstanceType[4]
        {
            new() { position = new(-1.5f, -1.5f, 5.0f) },
            new() { position = new(-1.5f,  1.5f, 5.0f) },
            new() { position = new( 1.5f, -1.5f, 5.0f) },
            new() { position = new( 1.5f,  1.5f, 5.0f) },
        };
        fixed (InstanceType* pI = instances)
        {
            var ibDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(InstanceType) * _instanceCount),
                BindFlags = (uint)BindFlag.VertexBuffer,
            };
            var ibData = new SubresourceData { PSysMem = pI };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&ibDesc, &ibData, ref _instanceBuffer));
        }
        return true;
    }
}

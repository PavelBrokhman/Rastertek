using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial38.Graphics;

public unsafe class ParticleSystem
{
    private struct ParticleType
    {
        public float positionX,
            positionY,
            positionZ;
        public float red,
            green,
            blue;
        public float velocity;
        public bool active;
    }

    private struct VertexType
    {
        public float x,
            y,
            z;
        public float tu,
            tv;
        public float red,
            green,
            blue,
            alpha;
    }

    private Texture _texture;
    private ParticleType[] _particleList;
    private VertexType[] _vertices;
    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount;
    private int _indexCount;

    private float _particleDeviationX,
        _particleDeviationY,
        _particleDeviationZ;
    private float _particleVelocity,
        _particleVelocityVariation;
    private float _particleSize;
    private int _particlesPerSecond;
    private int _maxParticles;
    private int _currentParticleCount;
    private float _accumulatedTime;
    private readonly Random _random = new();

    public bool Initialize(DX11 DirectX, string textureFilename)
    {
        _texture = new Texture();
        if (!_texture.Initialize(DirectX, textureFilename, false))
            return false;
        InitializeParticleSystem();
        return InitializeBuffers(DirectX);
    }

    public void Shutdown()
    {
        _indexBuffer.Release();
        _vertexBuffer.Release();
        _vertices = null;
        _particleList = null;
        _texture?.Shutdown();
        _texture = null;
    }

    public void Frame(DX11 DirectX, float frameTime)
    {
        KillParticles();
        EmitParticles(frameTime);
        UpdateParticles(frameTime);
        UpdateBuffers(DirectX);
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
        _texture.SetTexture(DirectX, 0);
    }

    public ComPtr<ID3D11ShaderResourceView> GetTextureView() => _texture.GetTextureView();

    public int GetIndexCount() => _indexCount;

    private void InitializeParticleSystem()
    {
        _particleDeviationX = 0.5f;
        _particleDeviationY = 0.1f;
        _particleDeviationZ = 2.0f;
        _particleVelocity = 1.0f;
        _particleVelocityVariation = 0.2f;
        _particleSize = 0.2f;
        _particlesPerSecond = 100;
        _maxParticles = 1000;
        _particleList = new ParticleType[_maxParticles];
        for (int i = 0; i < _maxParticles; i++)
            _particleList[i].active = false;
        _currentParticleCount = 0;
        _accumulatedTime = 0.0f;
    }

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;
        _vertexCount = _maxParticles * 6;
        _indexCount = _vertexCount;
        _vertices = new VertexType[_vertexCount];
        var indices = new uint[_indexCount];
        for (int i = 0; i < _indexCount; i++)
            indices[i] = (uint)i;

        fixed (VertexType* pVertices = _vertices)
        {
            var vbDesc = new BufferDesc
            {
                Usage = Usage.Dynamic,
                ByteWidth = (uint)(sizeof(VertexType) * _vertexCount),
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = (uint)CpuAccessFlag.Write,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var vbData = new SubresourceData { PSysMem = pVertices };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&vbDesc, &vbData, ref _vertexBuffer));
        }
        fixed (uint* pIndices = indices)
        {
            var ibDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(uint) * _indexCount),
                BindFlags = (uint)BindFlag.IndexBuffer,
            };
            var ibData = new SubresourceData { PSysMem = pIndices };
            SilkMarshal.ThrowHResult(device.CreateBuffer(&ibDesc, &ibData, ref _indexBuffer));
        }
        return true;
    }

    private float Centered() => ((float)_random.NextDouble() - (float)_random.NextDouble());

    private void EmitParticles(float frameTime)
    {
        _accumulatedTime += frameTime;
        bool emitParticle = false;
        if (_accumulatedTime > 1.0f / _particlesPerSecond)
        {
            _accumulatedTime = 0.0f;
            emitParticle = true;
        }
        if (emitParticle && _currentParticleCount < _maxParticles - 1)
        {
            _currentParticleCount++;
            float positionX = Centered() * _particleDeviationX;
            float positionY = Centered() * _particleDeviationY;
            float positionZ = Centered() * _particleDeviationZ;
            float velocity = _particleVelocity + Centered() * _particleVelocityVariation;
            float red = Centered() + 0.5f;
            float green = Centered() + 0.5f;
            float blue = Centered() + 0.5f;
            int index = 0;
            bool found = false;
            while (!found)
            {
                if (!_particleList[index].active || _particleList[index].positionZ < positionZ)
                    found = true;
                else
                    index++;
            }
            int i = _currentParticleCount;
            int j = i - 1;
            while (i != index)
            {
                _particleList[i] = _particleList[j];
                i--;
                j--;
            }
            _particleList[index].positionX = positionX;
            _particleList[index].positionY = positionY;
            _particleList[index].positionZ = positionZ;
            _particleList[index].red = red;
            _particleList[index].green = green;
            _particleList[index].blue = blue;
            _particleList[index].velocity = velocity;
            _particleList[index].active = true;
        }
    }

    private void UpdateParticles(float frameTime)
    {
        for (int i = 0; i < _currentParticleCount; i++)
            _particleList[i].positionY -= _particleList[i].velocity * frameTime * 1.0f;
    }

    private void KillParticles()
    {
        for (int i = 0; i < _maxParticles; i++)
        {
            if (_particleList[i].active && _particleList[i].positionY < -3.0f)
            {
                _particleList[i].active = false;
                _currentParticleCount--;
                for (int j = i; j < _maxParticles - 1; j++)
                    _particleList[j] = _particleList[j + 1];
            }
        }
    }

    private void UpdateBuffers(DX11 DirectX)
    {
        Array.Clear(_vertices, 0, _vertices.Length);
        int index = 0;
        for (int i = 0; i < _currentParticleCount; i++)
        {
            float px = _particleList[i].positionX;
            float py = _particleList[i].positionY;
            float pz = _particleList[i].positionZ;
            float r = _particleList[i].red;
            float g = _particleList[i].green;
            float b = _particleList[i].blue;
            _vertices[index++] = new VertexType { x = px - _particleSize, y = py - _particleSize, z = pz, tu = 0, tv = 1, red = r, green = g, blue = b, alpha = 1 };
            _vertices[index++] = new VertexType { x = px - _particleSize, y = py + _particleSize, z = pz, tu = 0, tv = 0, red = r, green = g, blue = b, alpha = 1 };
            _vertices[index++] = new VertexType { x = px + _particleSize, y = py - _particleSize, z = pz, tu = 1, tv = 1, red = r, green = g, blue = b, alpha = 1 };
            _vertices[index++] = new VertexType { x = px + _particleSize, y = py - _particleSize, z = pz, tu = 1, tv = 1, red = r, green = g, blue = b, alpha = 1 };
            _vertices[index++] = new VertexType { x = px - _particleSize, y = py + _particleSize, z = pz, tu = 0, tv = 0, red = r, green = g, blue = b, alpha = 1 };
            _vertices[index++] = new VertexType { x = px + _particleSize, y = py + _particleSize, z = pz, tu = 1, tv = 0, red = r, green = g, blue = b, alpha = 1 };
        }
        var context = DirectX.DeviceContext;
        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_vertexBuffer, 0, Map.WriteDiscard, 0, &mr));
        fixed (VertexType* p = _vertices)
            Buffer.MemoryCopy(p, mr.PData, sizeof(VertexType) * _vertexCount, sizeof(VertexType) * _vertexCount);
        context.Unmap(_vertexBuffer, 0);
    }
}

using System.Globalization;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial59.Graphics;

public unsafe class ParticleSystem
{
    private struct ParticleType
    {
        public float positionX,
            positionY,
            positionZ;
        public float lifeTime;
        public float scroll1X,
            scroll1Y;
        public bool active;
    }

    private struct VertexType
    {
        public float x,
            y,
            z;
        public float tu,
            tv;
        public float d1,
            d2,
            d3,
            d4;
    }

    private Texture _texture;
    private ParticleType[] _particleList;
    private VertexType[] _vertices;
    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private int _vertexCount;
    private int _indexCount;

    private string _configFilename;
    private string _textureFilename;
    private float _particleSize;
    private float _particlesPerSecond;
    private float _particleLifeTime;
    private int _maxParticles;
    private int _currentParticleCount;
    private float _accumulatedTime;
    private float _angle;
    private readonly Random _random = new();

    public bool Initialize(DX11 DirectX, string configFilename)
    {
        _configFilename = configFilename;

        if (!LoadParticleConfiguration()) return false;
        InitializeParticleSystem();
        if (!InitializeBuffers(DirectX)) return false;

        _texture = new Texture();
        return _texture.Initialize(DirectX, _textureFilename, true);
    }

    public void Shutdown()
    {
        _texture?.Shutdown();
        _texture = null;
        _indexBuffer.Release();
        _vertexBuffer.Release();
        _vertices = null;
        _particleList = null;
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

    /// <summary>
    /// particlesystemclass.cpp reads each value by skipping to the ':' and then
    /// taking what follows; the texture line runs to the end of the line.
    /// </summary>
    private bool LoadParticleConfiguration()
    {
        if (!File.Exists(_configFilename))
        {
            Console.WriteLine($"Particle config not found: {_configFilename}");
            return false;
        }

        var values = new List<string>();
        foreach (var line in File.ReadAllLines(_configFilename))
        {
            var colon = line.IndexOf(':');
            if (colon < 0) continue;
            values.Add(line[(colon + 1)..].Trim());
        }
        if (values.Count < 5) return false;

        _maxParticles = int.Parse(values[0], CultureInfo.InvariantCulture);
        _particlesPerSecond = float.Parse(values[1], CultureInfo.InvariantCulture);
        _particleSize = float.Parse(values[2], CultureInfo.InvariantCulture);
        _particleLifeTime = float.Parse(values[3], CultureInfo.InvariantCulture);
        _textureFilename = values[4];
        return true;
    }

    private void InitializeParticleSystem()
    {
        _particleList = new ParticleType[_maxParticles];
        for (int i = 0; i < _maxParticles; i++)
            _particleList[i].active = false;
        _currentParticleCount = 0;
        _accumulatedTime = 0.0f;
        _angle = 0.0f;
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

    private void EmitParticles(float frameTime)
    {
        // The emitter travels around the circumference of a circle, so particles
        // are born along a moving origin rather than in a static box.
        const float centerX = 0.0f;
        const float centerY = 0.0f;
        const float radius = 1.0f;

        _angle += frameTime * 2.0f;

        float originX = centerX + radius * MathF.Sin(_angle);
        float originY = centerY + radius * MathF.Cos(_angle);
        float originZ = 0.0f;

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

            float positionX = originX;
            float positionY = originY;
            float positionZ = originZ;

            float scroll1X = (float)_random.NextDouble() - (float)_random.NextDouble();
            if (scroll1X < 0.0f)
                scroll1X *= -1.0f;
            float scroll1Y = scroll1X;

            // Particles are drawn back to front for blending, so insert in depth order.
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
                CopyParticle(i, j);
                i--;
                j--;
            }

            _particleList[index].positionX = positionX;
            _particleList[index].positionY = positionY;
            _particleList[index].positionZ = positionZ;
            _particleList[index].active = true;
            _particleList[index].lifeTime = _particleLifeTime;
            _particleList[index].scroll1X = scroll1X;
            _particleList[index].scroll1Y = scroll1Y;
        }
    }

    private void UpdateParticles(float frameTime)
    {
        for (int i = 0; i < _currentParticleCount; i++)
        {
            _particleList[i].lifeTime -= frameTime;

            _particleList[i].scroll1X += frameTime * 0.5f;
            if (_particleList[i].scroll1X > 1.0f)
                _particleList[i].scroll1X -= 1.0f;

            _particleList[i].scroll1Y += frameTime * 0.5f;
            if (_particleList[i].scroll1Y > 1.0f)
                _particleList[i].scroll1Y -= 1.0f;
        }
    }

    private void KillParticles()
    {
        for (int i = 0; i < _maxParticles; i++)
        {
            if (_particleList[i].active && _particleList[i].lifeTime <= 0.0f)
            {
                _particleList[i].active = false;
                _currentParticleCount--;

                for (int j = i; j < _maxParticles - 1; j++)
                    CopyParticle(j, j + 1);
            }
        }
    }

    private void CopyParticle(int dst, int src)
    {
        _particleList[dst].positionX = _particleList[src].positionX;
        _particleList[dst].positionY = _particleList[src].positionY;
        _particleList[dst].positionZ = _particleList[src].positionZ;
        _particleList[dst].active = _particleList[src].active;
        _particleList[dst].lifeTime = _particleList[src].lifeTime;
        _particleList[dst].scroll1X = _particleList[src].scroll1X;
        _particleList[dst].scroll1Y = _particleList[src].scroll1Y;
    }

    private void UpdateBuffers(DX11 DirectX)
    {
        Array.Clear(_vertices, 0, _vertices.Length);

        int index = 0;
        for (int i = 0; i < _currentParticleCount; i++)
        {
            float lifeTime = _particleList[i].lifeTime / _particleLifeTime;
            float scroll1X = _particleList[i].scroll1X;
            float scroll1Y = _particleList[i].scroll1Y;

            float px = _particleList[i].positionX;
            float py = _particleList[i].positionY;
            float pz = _particleList[i].positionZ;

            // Bottom left.
            SetVertex(index++, px - _particleSize, py - _particleSize, pz, 0.0f, 1.0f, lifeTime, scroll1X, scroll1Y);
            // Top left.
            SetVertex(index++, px - _particleSize, py + _particleSize, pz, 0.0f, 0.0f, lifeTime, scroll1X, scroll1Y);
            // Bottom right.
            SetVertex(index++, px + _particleSize, py - _particleSize, pz, 1.0f, 1.0f, lifeTime, scroll1X, scroll1Y);
            // Bottom right.
            SetVertex(index++, px + _particleSize, py - _particleSize, pz, 1.0f, 1.0f, lifeTime, scroll1X, scroll1Y);
            // Top left.
            SetVertex(index++, px - _particleSize, py + _particleSize, pz, 0.0f, 0.0f, lifeTime, scroll1X, scroll1Y);
            // Top right.
            SetVertex(index++, px + _particleSize, py + _particleSize, pz, 1.0f, 0.0f, lifeTime, scroll1X, scroll1Y);
        }

        var context = DirectX.DeviceContext;
        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_vertexBuffer, 0, Map.WriteDiscard, 0, &mr));
        fixed (VertexType* pVertices = _vertices)
        {
            global::System.Buffer.MemoryCopy(pVertices, mr.PData, sizeof(VertexType) * _vertexCount, sizeof(VertexType) * _vertexCount);
        }
        context.Unmap(_vertexBuffer, 0);
    }

    private void SetVertex(int index, float x, float y, float z, float tu, float tv,
        float lifeTime, float scroll1X, float scroll1Y)
    {
        _vertices[index].x = x;
        _vertices[index].y = y;
        _vertices[index].z = z;
        _vertices[index].tu = tu;
        _vertices[index].tv = tv;
        _vertices[index].d1 = lifeTime;
        _vertices[index].d2 = scroll1X;
        _vertices[index].d3 = scroll1Y;
        _vertices[index].d4 = 1.0f;
    }
}

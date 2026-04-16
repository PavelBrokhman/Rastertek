using System.Globalization;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial07.Graphics;

public unsafe class Model
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

    private struct ModelType
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
    private ModelType[] m_model;
    private Texture m_Texture;

    public bool Initialize(DX11 DirectX, string modelFilename, string textureFilename, bool wrap)
    {
        var device = DirectX.Device;

        if (!LoadModel(modelFilename))
            return false;

        var vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];
        for (int i = 0; i < m_vertexCount; i++)
        {
            vertices[i].x = m_model[i].x;
            vertices[i].y = m_model[i].y;
            vertices[i].z = m_model[i].z;
            vertices[i].tu = m_model[i].tu;
            vertices[i].tv = m_model[i].tv;
            vertices[i].nx = m_model[i].nx;
            vertices[i].ny = m_model[i].ny;
            vertices[i].nz = m_model[i].nz;
            indices[i] = (uint)i;
        }

        fixed (VertexType* pVertices = vertices)
        {
            var vertexBufferDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(VertexType) * vertices.Length),
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var vertexData = new SubresourceData { PSysMem = pVertices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&vertexBufferDesc, &vertexData, ref m_vertexBuffer)
            );
        }

        fixed (uint* pIndices = indices)
        {
            var indexBufferDesc = new BufferDesc
            {
                Usage = Usage.Default,
                ByteWidth = (uint)(sizeof(uint) * indices.Length),
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0,
                StructureByteStride = 0,
            };
            var indexData = new SubresourceData { PSysMem = pIndices };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&indexBufferDesc, &indexData, ref m_indexBuffer)
            );
        }

        m_Texture = new Texture();
        if (!m_Texture.Initialize(DirectX, textureFilename, wrap))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_Texture?.Shutdown();
        m_Texture = null;
        m_indexBuffer.Release();
        m_vertexBuffer.Release();
        m_model = null;
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

    private bool LoadModel(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Model file not found: {filename}");
            return false;
        }

        var lines = File.ReadAllLines(filename);
        int idx = 0;

        while (
            idx < lines.Length
            && !lines[idx].StartsWith("Vertex Count", StringComparison.OrdinalIgnoreCase)
        )
            idx++;
        if (idx >= lines.Length)
            return false;
        var parts = lines[idx].Split(':');
        if (parts.Length < 2)
            return false;
        m_vertexCount = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
        m_indexCount = m_vertexCount;
        m_model = new ModelType[m_vertexCount];
        idx++;

        while (
            idx < lines.Length
            && !lines[idx].Trim().StartsWith("Data", StringComparison.OrdinalIgnoreCase)
        )
            idx++;
        idx++;

        int vi = 0;
        while (idx < lines.Length && vi < m_vertexCount)
        {
            var line = lines[idx].Trim();
            idx++;
            if (line.Length == 0)
                continue;
            var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 8)
                continue;
            m_model[vi].x = float.Parse(tokens[0], CultureInfo.InvariantCulture);
            m_model[vi].y = float.Parse(tokens[1], CultureInfo.InvariantCulture);
            m_model[vi].z = float.Parse(tokens[2], CultureInfo.InvariantCulture);
            m_model[vi].tu = float.Parse(tokens[3], CultureInfo.InvariantCulture);
            m_model[vi].tv = float.Parse(tokens[4], CultureInfo.InvariantCulture);
            m_model[vi].nx = float.Parse(tokens[5], CultureInfo.InvariantCulture);
            m_model[vi].ny = float.Parse(tokens[6], CultureInfo.InvariantCulture);
            m_model[vi].nz = float.Parse(tokens[7], CultureInfo.InvariantCulture);
            vi++;
        }
        return vi == m_vertexCount;
    }
}

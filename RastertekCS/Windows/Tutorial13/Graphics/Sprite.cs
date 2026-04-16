using System.Globalization;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial13.Graphics;

public unsafe class Sprite
{
    private struct VertexType
    {
        public float x,
            y,
            z;
        public float tu,
            tv;
    }

    private ComPtr<ID3D11Buffer> m_vertexBuffer;
    private ComPtr<ID3D11Buffer> m_indexBuffer;
    private int m_vertexCount;
    private int m_indexCount;
    private Texture[] m_Textures;
    private int m_textureCount;
    private int m_currentTexture;
    private float m_cycleTime;
    private float m_frameTime;

    private int m_screenWidth,
        m_screenHeight;
    private int m_bitmapWidth,
        m_bitmapHeight;
    private int m_renderX,
        m_renderY;
    private int m_prevPosX = -1,
        m_prevPosY = -1;

    public bool Initialize(
        DX11 DirectX,
        int screenWidth,
        int screenHeight,
        string spriteFilename,
        int renderX,
        int renderY
    )
    {
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;
        m_renderX = renderX;
        m_renderY = renderY;
        m_currentTexture = 0;
        m_frameTime = 0;

        if (!LoadSpriteFile(spriteFilename))
            return false;

        m_Textures = new Texture[m_textureCount];
        var lines = File.ReadAllLines(spriteFilename);
        int startLine = 3;
        for (int i = 0; i < m_textureCount; i++)
        {
            var texFile = lines[startLine + i].Trim();
            m_Textures[i] = new Texture();
            if (!m_Textures[i].Initialize(DirectX, texFile, false))
                return false;
        }

        if (!InitializeBuffers(DirectX))
            return false;
        return true;
    }

    public void Shutdown()
    {
        if (m_Textures != null)
        {
            foreach (var t in m_Textures)
                t?.Shutdown();
            m_Textures = null;
        }
        m_indexBuffer.Release();
        m_vertexBuffer.Release();
    }

    public void Update(float frameTimeMs)
    {
        m_frameTime += frameTimeMs;
        if (m_frameTime >= m_cycleTime)
        {
            m_frameTime -= m_cycleTime;
            m_currentTexture++;
            if (m_currentTexture >= m_textureCount)
                m_currentTexture = 0;
        }
    }

    public bool Render(DX11 DirectX)
    {
        if (!UpdateBuffers(DirectX))
            return false;
        RenderBuffers(DirectX);
        return true;
    }

    public int GetIndexCount() => m_indexCount;

    public void SetTexture(DX11 DirectX, uint slot) =>
        m_Textures[m_currentTexture].SetTexture(DirectX, slot);

    public void SetRenderLocation(int x, int y)
    {
        m_renderX = x;
        m_renderY = y;
    }

    private bool InitializeBuffers(DX11 DirectX)
    {
        var device = DirectX.Device;

        m_vertexCount = 6;
        m_indexCount = 6;

        var vertices = new VertexType[m_vertexCount];
        var indices = new uint[m_indexCount];
        for (int i = 0; i < m_indexCount; i++)
            indices[i] = (uint)i;

        var vertexBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)(sizeof(VertexType) * m_vertexCount),
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
            var indexData = new SubresourceData
            {
                PSysMem = pIndices,
                SysMemPitch = 0,
                SysMemSlicePitch = 0,
            };
            SilkMarshal.ThrowHResult(
                device.CreateBuffer(&indexBufferDesc, &indexData, ref m_indexBuffer)
            );
        }

        return true;
    }

    private bool UpdateBuffers(DX11 DirectX)
    {
        if (m_prevPosX == m_renderX && m_prevPosY == m_renderY)
            return true;

        m_prevPosX = m_renderX;
        m_prevPosY = m_renderY;

        float left = (m_screenWidth / 2 * -1) + (float)m_renderX;
        float right = left + m_bitmapWidth;
        float top = (m_screenHeight / 2) - (float)m_renderY;
        float bottom = top - m_bitmapHeight;

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
            context.Map(m_vertexBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        fixed (VertexType* pSrc = vertices)
        {
            global::System.Buffer.MemoryCopy(
                pSrc,
                mappedResource.PData,
                sizeof(VertexType) * m_vertexCount,
                sizeof(VertexType) * m_vertexCount
            );
        }
        context.Unmap(m_vertexBuffer, 0);

        return true;
    }

    private void RenderBuffers(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        uint stride = (uint)sizeof(VertexType);
        uint offset = 0;
        var vb = m_vertexBuffer.GetPinnableReference();
        context.IASetVertexBuffers(0, 1, &vb, &stride, &offset);
        context.IASetIndexBuffer(m_indexBuffer, Format.FormatR32Uint, 0);
        context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    }

    private bool LoadSpriteFile(string filename)
    {
        if (!File.Exists(filename))
            return false;
        var lines = File.ReadAllLines(filename);
        m_textureCount = int.Parse(lines[0].Split(':')[1].Trim(), CultureInfo.InvariantCulture);
        m_cycleTime = float.Parse(lines[1].Split(':')[1].Trim(), CultureInfo.InvariantCulture);
        var size = lines[2].Split(':')[1].Trim().Split(' ');
        m_bitmapWidth = int.Parse(size[0], CultureInfo.InvariantCulture);
        m_bitmapHeight = int.Parse(size[1], CultureInfo.InvariantCulture);
        return true;
    }
}

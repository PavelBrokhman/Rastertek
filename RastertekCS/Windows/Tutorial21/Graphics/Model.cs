using System.Globalization;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial21.Graphics;

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
        public float tx,
            ty,
            tz;
        public float bx,
            by,
            bz;
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
        public float tx,
            ty,
            tz;
        public float bx,
            by,
            bz;
    }

    private ComPtr<ID3D11Buffer> m_vertexBuffer;
    private ComPtr<ID3D11Buffer> m_indexBuffer;
    private int m_vertexCount;
    private int m_indexCount;
    private ModelType[] m_model;
    private Texture m_Texture1;
    private Texture m_Texture2;
    private Texture m_Texture3;

    public bool Initialize(
        DX11 DirectX,
        string modelFilename,
        string textureFilename1,
        string textureFilename2,
        string textureFilename3,
        bool wrap
    )
    {
        var device = DirectX.Device;

        if (!LoadModel(modelFilename))
            return false;

        CalculateModelVectors();

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
            vertices[i].tx = m_model[i].tx;
            vertices[i].ty = m_model[i].ty;
            vertices[i].tz = m_model[i].tz;
            vertices[i].bx = m_model[i].bx;
            vertices[i].by = m_model[i].by;
            vertices[i].bz = m_model[i].bz;
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

        m_Texture1 = new Texture();
        if (!m_Texture1.Initialize(DirectX, textureFilename1, wrap))
            return false;
        m_Texture2 = new Texture();
        if (!m_Texture2.Initialize(DirectX, textureFilename2, wrap))
            return false;
        m_Texture3 = new Texture();
        if (!m_Texture3.Initialize(DirectX, textureFilename3, wrap))
            return false;

        return true;
    }

    public void Shutdown()
    {
        m_Texture1?.Shutdown();
        m_Texture2?.Shutdown();
        m_Texture3?.Shutdown();
        m_Texture1 = null;
        m_Texture2 = null;
        m_Texture3 = null;
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

    public void SetTextures(DX11 DirectX)
    {
        m_Texture1.SetTexture(DirectX, 0);
        m_Texture2.SetTexture(DirectX, 1);
        m_Texture3.SetTexture(DirectX, 2);
    }

    public int GetIndexCount() => m_indexCount;

    private void CalculateModelVectors()
    {
        int faceCount = m_vertexCount / 3;
        int index = 0;
        for (int i = 0; i < faceCount; i++)
        {
            var v1 = m_model[index];
            index++;
            var v2 = m_model[index];
            index++;
            var v3 = m_model[index];
            index++;

            CalculateTangentBinormal(
                v1,
                v2,
                v3,
                out float tx,
                out float ty,
                out float tz,
                out float bx,
                out float by,
                out float bz
            );

            m_model[index - 1].tx = tx;
            m_model[index - 1].ty = ty;
            m_model[index - 1].tz = tz;
            m_model[index - 1].bx = bx;
            m_model[index - 1].by = by;
            m_model[index - 1].bz = bz;
            m_model[index - 2].tx = tx;
            m_model[index - 2].ty = ty;
            m_model[index - 2].tz = tz;
            m_model[index - 2].bx = bx;
            m_model[index - 2].by = by;
            m_model[index - 2].bz = bz;
            m_model[index - 3].tx = tx;
            m_model[index - 3].ty = ty;
            m_model[index - 3].tz = tz;
            m_model[index - 3].bx = bx;
            m_model[index - 3].by = by;
            m_model[index - 3].bz = bz;
        }
    }

    private static void CalculateTangentBinormal(
        ModelType v1,
        ModelType v2,
        ModelType v3,
        out float tx,
        out float ty,
        out float tz,
        out float bx,
        out float by,
        out float bz
    )
    {
        float vec1x = v2.x - v1.x;
        float vec1y = v2.y - v1.y;
        float vec1z = v2.z - v1.z;
        float vec2x = v3.x - v1.x;
        float vec2y = v3.y - v1.y;
        float vec2z = v3.z - v1.z;

        float tu0 = v2.tu - v1.tu;
        float tv0 = v2.tv - v1.tv;
        float tu1 = v3.tu - v1.tu;
        float tv1 = v3.tv - v1.tv;

        float den = 1.0f / (tu0 * tv1 - tu1 * tv0);

        tx = (tv1 * vec1x - tv0 * vec2x) * den;
        ty = (tv1 * vec1y - tv0 * vec2y) * den;
        tz = (tv1 * vec1z - tv0 * vec2z) * den;

        bx = (tu0 * vec2x - tu1 * vec1x) * den;
        by = (tu0 * vec2y - tu1 * vec1y) * den;
        bz = (tu0 * vec2z - tu1 * vec1z) * den;

        float len = MathF.Sqrt(tx * tx + ty * ty + tz * tz);
        tx /= len;
        ty /= len;
        tz /= len;
        len = MathF.Sqrt(bx * bx + by * by + bz * bz);
        bx /= len;
        by /= len;
        bz /= len;
    }

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

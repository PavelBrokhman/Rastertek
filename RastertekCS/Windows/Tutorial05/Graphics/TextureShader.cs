using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial05.Graphics;

public unsafe class TextureShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private ComPtr<ID3D11VertexShader> m_vertexShader;
    private ComPtr<ID3D11PixelShader> m_pixelShader;
    private ComPtr<ID3D11InputLayout> m_layout;
    private ComPtr<ID3D11Buffer> m_matrixBuffer;

    public bool Initialize(DX11 DirectX)
    {
        return InitializeShader(DirectX, "Shaders/Texture.vs", "Shaders/Texture.ps");
    }

    public void Shutdown()
    {
        m_matrixBuffer.Release();
        m_layout.Release();
        m_pixelShader.Release();
        m_vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix)
    {
        if (!SetShaderParameters(DirectX, worldMatrix, viewMatrix, projectionMatrix))
            return false;
        RenderShader(DirectX, indexCount);
        return true;
    }

    private bool InitializeShader(DX11 DirectX, string vsFilename, string psFilename)
    {
        var device = DirectX.Device;
        var compiler = D3DCompiler.GetApi();

        // Compile vertex shader.
        ComPtr<ID3D10Blob> vsBlob = default;
        ComPtr<ID3D10Blob> errorBlob = default;

        var vsSource = File.ReadAllText(vsFilename);
        SilkMarshal.ThrowHResult(
            compiler.Compile(
                (void*)SilkMarshal.StringToPtr(vsSource, NativeStringEncoding.Ansi),
                (nuint)vsSource.Length,
                vsFilename, null, null,
                "TextureVertexShader", "vs_5_0",
                0, 0, ref vsBlob, ref errorBlob));

        SilkMarshal.ThrowHResult(
            device.CreateVertexShader(vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(),
                (ID3D11ClassLinkage*)null, ref m_vertexShader));

        // Compile pixel shader.
        ComPtr<ID3D10Blob> psBlob = default;

        var psSource = File.ReadAllText(psFilename);
        SilkMarshal.ThrowHResult(
            compiler.Compile(
                (void*)SilkMarshal.StringToPtr(psSource, NativeStringEncoding.Ansi),
                (nuint)psSource.Length,
                psFilename, null, null,
                "TexturePixelShader", "ps_5_0",
                0, 0, ref psBlob, ref errorBlob));

        SilkMarshal.ThrowHResult(
            device.CreatePixelShader(psBlob.GetBufferPointer(), psBlob.GetBufferSize(),
                (ID3D11ClassLinkage*)null, ref m_pixelShader));

        // Create input layout.
        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texCoordName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);

        var layoutDesc = new InputElementDesc[]
        {
            new()
            {
                SemanticName = (byte*)posName, SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32B32Float,
                InputSlot = 0, AlignedByteOffset = 0,
                InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0
            },
            new()
            {
                SemanticName = (byte*)texCoordName, SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32Float,
                InputSlot = 0, AlignedByteOffset = 12,
                InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0
            }
        };

        fixed (InputElementDesc* pLayout = layoutDesc)
        {
            SilkMarshal.ThrowHResult(
                device.CreateInputLayout(pLayout, (uint)layoutDesc.Length,
                    vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(), ref m_layout));
        }

        SilkMarshal.Free(posName);
        SilkMarshal.Free(texCoordName);
        vsBlob.Release();
        psBlob.Release();

        // Create the constant buffer for matrices.
        var matrixBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(MatrixBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0, StructureByteStride = 0
        };
        SilkMarshal.ThrowHResult(
            device.CreateBuffer(&matrixBufferDesc, null, ref m_matrixBuffer));

        return true;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix)
    {
        var context = DirectX.DeviceContext;

        // Transpose matrices for DirectX.
        Matrix4X4.Transpose(worldMatrix, out worldMatrix);
        Matrix4X4.Transpose(viewMatrix, out viewMatrix);
        Matrix4X4.Transpose(projectionMatrix, out projectionMatrix);

        MappedSubresource mappedResource;
        SilkMarshal.ThrowHResult(
            context.Map(m_matrixBuffer, 0, Map.WriteDiscard, 0, &mappedResource));

        var dataPtr = (MatrixBufferType*)mappedResource.PData;
        dataPtr->world = worldMatrix;
        dataPtr->view = viewMatrix;
        dataPtr->projection = projectionMatrix;

        context.Unmap(m_matrixBuffer, 0);

        var cb = m_matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &cb);

        return true;
    }

    private void RenderShader(DX11 DirectX, int indexCount)
    {
        var context = DirectX.DeviceContext;

        context.IASetInputLayout(m_layout);
        context.VSSetShader(m_vertexShader, null, 0);
        context.PSSetShader(m_pixelShader, null, 0);
        context.DrawIndexed((uint)indexCount, 0, 0);
    }
}

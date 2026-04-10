using System.Runtime.CompilerServices;
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
        var vsBytes = global::System.Text.Encoding.ASCII.GetBytes(vsSource);
        var pFilename = (byte*)SilkMarshal.StringToPtr(vsFilename, NativeStringEncoding.Ansi);
        var pVsEntry = (byte*)SilkMarshal.StringToPtr("TextureVertexShader", NativeStringEncoding.Ansi);
        var pVsTarget = (byte*)SilkMarshal.StringToPtr("vs_5_0", NativeStringEncoding.Ansi);

        fixed (byte* pVsSource = vsBytes)
        {
            ID3D10Blob* pVsBlob = null;
            ID3D10Blob* pErrorBlob = null;
            SilkMarshal.ThrowHResult(
                compiler.Compile(pVsSource, (nuint)vsBytes.Length,
                    pFilename, null, (ID3DInclude*)null,
                    pVsEntry, pVsTarget,
                    0, 0, &pVsBlob, &pErrorBlob));
            vsBlob = pVsBlob;
            errorBlob = pErrorBlob;
        }

        SilkMarshal.Free((nint)pFilename);
        SilkMarshal.Free((nint)pVsEntry);
        SilkMarshal.Free((nint)pVsTarget);

        // Reinterpret as ID3D11Device1 — extensions only exist on Device1 in Silk.NET 2.22.
        ref var device1 = ref Unsafe.As<ComPtr<ID3D11Device>, ComPtr<ID3D11Device1>>(ref device);
        SilkMarshal.ThrowHResult(
            device1.CreateVertexShader(
                vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref m_vertexShader));

        // Compile pixel shader.
        ComPtr<ID3D10Blob> psBlob = default;

        var psSource = File.ReadAllText(psFilename);
        var psBytes = global::System.Text.Encoding.ASCII.GetBytes(psSource);
        var pPsFilename = (byte*)SilkMarshal.StringToPtr(psFilename, NativeStringEncoding.Ansi);
        var pPsEntry = (byte*)SilkMarshal.StringToPtr("TexturePixelShader", NativeStringEncoding.Ansi);
        var pPsTarget = (byte*)SilkMarshal.StringToPtr("ps_5_0", NativeStringEncoding.Ansi);

        fixed (byte* pPsSource = psBytes)
        {
            ID3D10Blob* pPsBlob = null;
            ID3D10Blob* pErrBlob = null;
            SilkMarshal.ThrowHResult(
                compiler.Compile(pPsSource, (nuint)psBytes.Length,
                    pPsFilename, null, (ID3DInclude*)null,
                    pPsEntry, pPsTarget,
                    0, 0, &pPsBlob, &pErrBlob));
            psBlob = pPsBlob;
        }

        SilkMarshal.Free((nint)pPsFilename);
        SilkMarshal.Free((nint)pPsEntry);
        SilkMarshal.Free((nint)pPsTarget);

        SilkMarshal.ThrowHResult(
            device1.CreatePixelShader(
                psBlob.GetBufferPointer(), psBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref m_pixelShader));

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
        worldMatrix = Matrix4X4.Transpose(worldMatrix);
        viewMatrix = Matrix4X4.Transpose(viewMatrix);
        projectionMatrix = Matrix4X4.Transpose(projectionMatrix);

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

using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial11.Graphics;

public unsafe class LightShader
{
    public const int NUM_LIGHTS = 4;

    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    // Each element must be 16-byte aligned (float4) per HLSL cbuffer array rules.
    private struct LightPositionBufferType
    {
        public float pos1X,
            pos1Y,
            pos1Z,
            pad1;
        public float pos2X,
            pos2Y,
            pos2Z,
            pad2;
        public float pos3X,
            pos3Y,
            pos3Z,
            pad3;
        public float pos4X,
            pos4Y,
            pos4Z,
            pad4;
    }

    private struct LightColorBufferType
    {
        public float c1R,
            c1G,
            c1B,
            c1A;
        public float c2R,
            c2G,
            c2B,
            c2A;
        public float c3R,
            c3G,
            c3B,
            c3A;
        public float c4R,
            c4G,
            c4B,
            c4A;
    }

    private ComPtr<ID3D11VertexShader> m_vertexShader;
    private ComPtr<ID3D11PixelShader> m_pixelShader;
    private ComPtr<ID3D11InputLayout> m_layout;
    private ComPtr<ID3D11Buffer> m_matrixBuffer;
    private ComPtr<ID3D11Buffer> m_lightPositionBuffer;
    private ComPtr<ID3D11Buffer> m_lightColorBuffer;

    public bool Initialize(DX11 DirectX)
    {
        return InitializeShader(DirectX, "Shaders/Light.vs", "Shaders/Light.ps");
    }

    public void Shutdown()
    {
        m_lightColorBuffer.Release();
        m_lightPositionBuffer.Release();
        m_matrixBuffer.Release();
        m_layout.Release();
        m_pixelShader.Release();
        m_vertexShader.Release();
    }

    public bool Render(
        DX11 DirectX,
        int indexCount,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        float[] lightPositions,
        float[] diffuseColors
    )
    {
        if (
            !SetShaderParameters(
                DirectX,
                worldMatrix,
                viewMatrix,
                projectionMatrix,
                lightPositions,
                diffuseColors
            )
        )
            return false;
        RenderShader(DirectX, indexCount);
        return true;
    }

    private bool InitializeShader(DX11 DirectX, string vsFilename, string psFilename)
    {
        var device = DirectX.Device;
        var compiler = D3DCompiler.GetApi();

        ID3D10Blob* pVsBlob = null;
        ID3D10Blob* pErrorBlob = null;

        var vsSource = File.ReadAllBytes(vsFilename);
        fixed (byte* pVsSource = vsSource)
        {
            SilkMarshal.ThrowHResult(
                compiler.Compile(
                    pVsSource,
                    (nuint)vsSource.Length,
                    (byte*)SilkMarshal.StringToPtr(vsFilename, NativeStringEncoding.Ansi),
                    null,
                    (ID3DInclude*)null,
                    (byte*)SilkMarshal.StringToPtr("LightVertexShader", NativeStringEncoding.Ansi),
                    (byte*)SilkMarshal.StringToPtr("vs_5_0", NativeStringEncoding.Ansi),
                    0,
                    0,
                    &pVsBlob,
                    &pErrorBlob
                )
            );
        }
        ComPtr<ID3D10Blob> vsBlob = pVsBlob;

        SilkMarshal.ThrowHResult(
            device.CreateVertexShader(
                vsBlob.GetBufferPointer(),
                vsBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref m_vertexShader
            )
        );

        ID3D10Blob* pPsBlob = null;

        var psSource = File.ReadAllBytes(psFilename);
        fixed (byte* pPsSource = psSource)
        {
            SilkMarshal.ThrowHResult(
                compiler.Compile(
                    pPsSource,
                    (nuint)psSource.Length,
                    (byte*)SilkMarshal.StringToPtr(psFilename, NativeStringEncoding.Ansi),
                    null,
                    (ID3DInclude*)null,
                    (byte*)SilkMarshal.StringToPtr("LightPixelShader", NativeStringEncoding.Ansi),
                    (byte*)SilkMarshal.StringToPtr("ps_5_0", NativeStringEncoding.Ansi),
                    0,
                    0,
                    &pPsBlob,
                    &pErrorBlob
                )
            );
        }
        ComPtr<ID3D10Blob> psBlob = pPsBlob;

        SilkMarshal.ThrowHResult(
            device.CreatePixelShader(
                psBlob.GetBufferPointer(),
                psBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref m_pixelShader
            )
        );

        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);
        var normalName = SilkMarshal.StringToPtr("NORMAL", NativeStringEncoding.Ansi);

        var layoutDesc = new InputElementDesc[]
        {
            new()
            {
                SemanticName = (byte*)posName,
                SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32B32Float,
                InputSlot = 0,
                AlignedByteOffset = 0,
                InputSlotClass = InputClassification.PerVertexData,
                InstanceDataStepRate = 0,
            },
            new()
            {
                SemanticName = (byte*)texName,
                SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32Float,
                InputSlot = 0,
                AlignedByteOffset = 12,
                InputSlotClass = InputClassification.PerVertexData,
                InstanceDataStepRate = 0,
            },
            new()
            {
                SemanticName = (byte*)normalName,
                SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32B32Float,
                InputSlot = 0,
                AlignedByteOffset = 20,
                InputSlotClass = InputClassification.PerVertexData,
                InstanceDataStepRate = 0,
            },
        };

        fixed (InputElementDesc* pLayout = layoutDesc)
        {
            SilkMarshal.ThrowHResult(
                device.CreateInputLayout(
                    pLayout,
                    (uint)layoutDesc.Length,
                    vsBlob.GetBufferPointer(),
                    vsBlob.GetBufferSize(),
                    ref m_layout
                )
            );
        }

        SilkMarshal.Free(posName);
        SilkMarshal.Free(texName);
        SilkMarshal.Free(normalName);
        vsBlob.Release();
        psBlob.Release();

        var matrixBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(MatrixBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&matrixBufferDesc, null, ref m_matrixBuffer));

        var lightPosBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(LightPositionBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        SilkMarshal.ThrowHResult(
            device.CreateBuffer(&lightPosBufferDesc, null, ref m_lightPositionBuffer)
        );

        var lightColorBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(LightColorBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        SilkMarshal.ThrowHResult(
            device.CreateBuffer(&lightColorBufferDesc, null, ref m_lightColorBuffer)
        );

        return true;
    }

    private bool SetShaderParameters(
        DX11 DirectX,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        float[] lightPositions,
        float[] diffuseColors
    )
    {
        var context = DirectX.DeviceContext;

        worldMatrix = Matrix4X4.Transpose(worldMatrix);
        viewMatrix = Matrix4X4.Transpose(viewMatrix);
        projectionMatrix = Matrix4X4.Transpose(projectionMatrix);

        MappedSubresource mappedResource;
        SilkMarshal.ThrowHResult(
            context.Map(m_matrixBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        var matrixPtr = (MatrixBufferType*)mappedResource.PData;
        matrixPtr->world = worldMatrix;
        matrixPtr->view = viewMatrix;
        matrixPtr->projection = projectionMatrix;
        context.Unmap(m_matrixBuffer, 0);

        var mcb = m_matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(
            context.Map(m_lightPositionBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        var posPtr = (LightPositionBufferType*)mappedResource.PData;
        posPtr->pos1X = lightPositions[0];
        posPtr->pos1Y = lightPositions[1];
        posPtr->pos1Z = lightPositions[2];
        posPtr->pad1 = 1.0f;
        posPtr->pos2X = lightPositions[3];
        posPtr->pos2Y = lightPositions[4];
        posPtr->pos2Z = lightPositions[5];
        posPtr->pad2 = 1.0f;
        posPtr->pos3X = lightPositions[6];
        posPtr->pos3Y = lightPositions[7];
        posPtr->pos3Z = lightPositions[8];
        posPtr->pad3 = 1.0f;
        posPtr->pos4X = lightPositions[9];
        posPtr->pos4Y = lightPositions[10];
        posPtr->pos4Z = lightPositions[11];
        posPtr->pad4 = 1.0f;
        context.Unmap(m_lightPositionBuffer, 0);

        var pcb = m_lightPositionBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(1, 1, &pcb);

        SilkMarshal.ThrowHResult(
            context.Map(m_lightColorBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        var colorPtr = (LightColorBufferType*)mappedResource.PData;
        colorPtr->c1R = diffuseColors[0];
        colorPtr->c1G = diffuseColors[1];
        colorPtr->c1B = diffuseColors[2];
        colorPtr->c1A = diffuseColors[3];
        colorPtr->c2R = diffuseColors[4];
        colorPtr->c2G = diffuseColors[5];
        colorPtr->c2B = diffuseColors[6];
        colorPtr->c2A = diffuseColors[7];
        colorPtr->c3R = diffuseColors[8];
        colorPtr->c3G = diffuseColors[9];
        colorPtr->c3B = diffuseColors[10];
        colorPtr->c3A = diffuseColors[11];
        colorPtr->c4R = diffuseColors[12];
        colorPtr->c4G = diffuseColors[13];
        colorPtr->c4B = diffuseColors[14];
        colorPtr->c4A = diffuseColors[15];
        context.Unmap(m_lightColorBuffer, 0);

        var ccb = m_lightColorBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &ccb);

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

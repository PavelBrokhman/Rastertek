using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial28.Graphics;

public unsafe class TranslateShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct TranslateBufferType
    {
        public float textureTranslation;
        public float pad0;
        public float pad1;
        public float pad2;
    }

    private ComPtr<ID3D11VertexShader> m_vertexShader;
    private ComPtr<ID3D11PixelShader> m_pixelShader;
    private ComPtr<ID3D11InputLayout> m_layout;
    private ComPtr<ID3D11Buffer> m_matrixBuffer;
    private ComPtr<ID3D11Buffer> m_translateBuffer;

    public bool Initialize(DX11 DirectX)
    {
        return InitializeShader(DirectX, "Shaders/Translate.vs", "Shaders/Translate.ps");
    }

    public void Shutdown()
    {
        m_translateBuffer.Release();
        m_matrixBuffer.Release();
        m_layout.Release();
        m_pixelShader.Release();
        m_vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        float textureTranslation)
    {
        if (!SetShaderParameters(DirectX, worldMatrix, viewMatrix, projectionMatrix, textureTranslation))
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
            SilkMarshal.ThrowHResult(
                compiler.Compile(pVsSource, (nuint)vsSource.Length,
                    (byte*)SilkMarshal.StringToPtr(vsFilename, NativeStringEncoding.Ansi),
                    null, (ID3DInclude*)null,
                    (byte*)SilkMarshal.StringToPtr("TranslateVertexShader", NativeStringEncoding.Ansi),
                    (byte*)SilkMarshal.StringToPtr("vs_5_0", NativeStringEncoding.Ansi),
                    0, 0, &pVsBlob, &pErrorBlob));
        ComPtr<ID3D10Blob> vsBlob = pVsBlob;
        SilkMarshal.ThrowHResult(
            device.CreateVertexShader(vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref m_vertexShader));

        ID3D10Blob* pPsBlob = null;
        var psSource = File.ReadAllBytes(psFilename);
        fixed (byte* pPsSource = psSource)
            SilkMarshal.ThrowHResult(
                compiler.Compile(pPsSource, (nuint)psSource.Length,
                    (byte*)SilkMarshal.StringToPtr(psFilename, NativeStringEncoding.Ansi),
                    null, (ID3DInclude*)null,
                    (byte*)SilkMarshal.StringToPtr("TranslatePixelShader", NativeStringEncoding.Ansi),
                    (byte*)SilkMarshal.StringToPtr("ps_5_0", NativeStringEncoding.Ansi),
                    0, 0, &pPsBlob, &pErrorBlob));
        ComPtr<ID3D10Blob> psBlob = pPsBlob;
        SilkMarshal.ThrowHResult(
            device.CreatePixelShader(psBlob.GetBufferPointer(), psBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref m_pixelShader));

        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);
        var layoutDesc = new InputElementDesc[]
        {
            new() { SemanticName = (byte*)posName, SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0,
                AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)texName, SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32Float, InputSlot = 0,
                AlignedByteOffset = 12, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 }
        };
        fixed (InputElementDesc* pLayout = layoutDesc)
            SilkMarshal.ThrowHResult(
                device.CreateInputLayout(pLayout, (uint)layoutDesc.Length,
                    vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(), ref m_layout));
        SilkMarshal.Free(posName); SilkMarshal.Free(texName);
        vsBlob.Release(); psBlob.Release();

        var matrixBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(MatrixBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0, StructureByteStride = 0
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&matrixBufferDesc, null, ref m_matrixBuffer));

        var clipPlaneBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(TranslateBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0, StructureByteStride = 0
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&clipPlaneBufferDesc, null, ref m_translateBuffer));

        return true;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        float textureTranslation)
    {
        var context = DirectX.DeviceContext;
        worldMatrix = Matrix4X4.Transpose(worldMatrix);
        viewMatrix = Matrix4X4.Transpose(viewMatrix);
        projectionMatrix = Matrix4X4.Transpose(projectionMatrix);

        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(m_matrixBuffer, 0, Map.WriteDiscard, 0, &mr));
        var mp = (MatrixBufferType*)mr.PData;
        mp->world = worldMatrix; mp->view = viewMatrix; mp->projection = projectionMatrix;
        context.Unmap(m_matrixBuffer, 0);

        var mcb = m_matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(context.Map(m_translateBuffer, 0, Map.WriteDiscard, 0, &mr));
        var fp = (TranslateBufferType*)mr.PData;
        fp->textureTranslation = textureTranslation; fp->pad0 = 0; fp->pad1 = 0; fp->pad2 = 0;
        context.Unmap(m_translateBuffer, 0);

        var cpcb = m_translateBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &cpcb);

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

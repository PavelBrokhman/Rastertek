using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial33.Graphics;

public unsafe class FireShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct NoiseBufferType
    {
        public float frameTime;
        public Vector3D<float> scrollSpeeds;
        public Vector3D<float> scales;
        public float padding;
    }

    private struct DistortionBufferType
    {
        public Vector2D<float> distortion1;
        public Vector2D<float> distortion2;
        public Vector2D<float> distortion3;
        public float distortionScale;
        public float distortionBias;
    }

    private ComPtr<ID3D11VertexShader> m_vertexShader;
    private ComPtr<ID3D11PixelShader> m_pixelShader;
    private ComPtr<ID3D11InputLayout> m_layout;
    private ComPtr<ID3D11Buffer> m_matrixBuffer;
    private ComPtr<ID3D11Buffer> m_noiseBuffer;
    private ComPtr<ID3D11Buffer> m_distortionBuffer;
    private ComPtr<ID3D11SamplerState> m_sampleState;

    public bool Initialize(DX11 DirectX) =>
        InitializeShader(DirectX, "Shaders/fire.vs", "Shaders/fire.ps");

    public void Shutdown()
    {
        m_sampleState.Release();
        m_distortionBuffer.Release();
        m_noiseBuffer.Release();
        m_matrixBuffer.Release();
        m_layout.Release();
        m_pixelShader.Release();
        m_vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        float frameTime, float[] scrollSpeeds, float[] scales,
        float[] distortion1, float[] distortion2, float[] distortion3,
        float distortionScale, float distortionBias,
        ComPtr<ID3D11ShaderResourceView> fireTexture,
        ComPtr<ID3D11ShaderResourceView> noiseTexture,
        ComPtr<ID3D11ShaderResourceView> alphaTexture)
    {
        if (!SetShaderParameters(DirectX, world, view, projection,
                frameTime, scrollSpeeds, scales,
                distortion1, distortion2, distortion3,
                distortionScale, distortionBias,
                fireTexture, noiseTexture, alphaTexture))
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
                    (byte*)SilkMarshal.StringToPtr("FireVertexShader", NativeStringEncoding.Ansi),
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
                    (byte*)SilkMarshal.StringToPtr("FirePixelShader", NativeStringEncoding.Ansi),
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

        var matrixDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(MatrixBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&matrixDesc, null, ref m_matrixBuffer));

        var noiseDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(NoiseBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&noiseDesc, null, ref m_noiseBuffer));

        var distDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(DistortionBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&distDesc, null, ref m_distortionBuffer));

        var samplerDesc = new SamplerDesc
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap,
            MipLODBias = 0, MaxAnisotropy = 1, ComparisonFunc = ComparisonFunc.Always,
            MinLOD = 0, MaxLOD = float.MaxValue
        };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&samplerDesc, ref m_sampleState));

        return true;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        float frameTime, float[] scrollSpeeds, float[] scales,
        float[] distortion1, float[] distortion2, float[] distortion3,
        float distortionScale, float distortionBias,
        ComPtr<ID3D11ShaderResourceView> fireTexture,
        ComPtr<ID3D11ShaderResourceView> noiseTexture,
        ComPtr<ID3D11ShaderResourceView> alphaTexture)
    {
        var context = DirectX.DeviceContext;
        world = Matrix4X4.Transpose(world); view = Matrix4X4.Transpose(view); projection = Matrix4X4.Transpose(projection);

        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(m_matrixBuffer, 0, Map.WriteDiscard, 0, &mr));
        var mp = (MatrixBufferType*)mr.PData;
        mp->world = world; mp->view = view; mp->projection = projection;
        context.Unmap(m_matrixBuffer, 0);
        var mcb = m_matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(context.Map(m_noiseBuffer, 0, Map.WriteDiscard, 0, &mr));
        var np = (NoiseBufferType*)mr.PData;
        np->frameTime = frameTime;
        np->scrollSpeeds = new Vector3D<float>(scrollSpeeds[0], scrollSpeeds[1], scrollSpeeds[2]);
        np->scales = new Vector3D<float>(scales[0], scales[1], scales[2]);
        np->padding = 0;
        context.Unmap(m_noiseBuffer, 0);
        var ncb = m_noiseBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(1, 1, &ncb);

        SilkMarshal.ThrowHResult(context.Map(m_distortionBuffer, 0, Map.WriteDiscard, 0, &mr));
        var dp = (DistortionBufferType*)mr.PData;
        dp->distortion1 = new Vector2D<float>(distortion1[0], distortion1[1]);
        dp->distortion2 = new Vector2D<float>(distortion2[0], distortion2[1]);
        dp->distortion3 = new Vector2D<float>(distortion3[0], distortion3[1]);
        dp->distortionScale = distortionScale;
        dp->distortionBias = distortionBias;
        context.Unmap(m_distortionBuffer, 0);
        var dcb = m_distortionBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &dcb);

        var ft = fireTexture.GetPinnableReference();  context.PSSetShaderResources(0, 1, &ft);
        var nt = noiseTexture.GetPinnableReference(); context.PSSetShaderResources(1, 1, &nt);
        var at = alphaTexture.GetPinnableReference(); context.PSSetShaderResources(2, 1, &at);

        return true;
    }

    private void RenderShader(DX11 DirectX, int indexCount)
    {
        var context = DirectX.DeviceContext;
        context.IASetInputLayout(m_layout);
        context.VSSetShader(m_vertexShader, null, 0);
        context.PSSetShader(m_pixelShader, null, 0);
        var smp = m_sampleState.GetPinnableReference();
        context.PSSetSamplers(0, 1, &smp);
        context.DrawIndexed((uint)indexCount, 0, 0);
    }
}

using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial51.Graphics;

public unsafe class SsaoShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct SsaoBufferType
    {
        public float screenWidth;
        public float screenHeight;
        public float randomTextureSize;
        public float sampleRadius;
        public float ssaoScale;
        public float ssaoBias;
        public float ssaoIntensity;
        public float padding;
    }

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11InputLayout> _layout;
    private ComPtr<ID3D11Buffer> _matrixBuffer;
    private ComPtr<ID3D11Buffer> _ssaoBuffer;
    private ComPtr<ID3D11SamplerState> _sampleStateWrap;
    private ComPtr<ID3D11SamplerState> _sampleStateClamp;

    public bool Initialize(DX11 DirectX) => InitializeShader(DirectX, "Shaders/ssao.vs", "Shaders/ssao.ps");

    public void Shutdown()
    {
        _sampleStateClamp.Release();
        _sampleStateWrap.Release();
        _ssaoBuffer.Release();
        _matrixBuffer.Release();
        _layout.Release();
        _pixelShader.Release();
        _vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        ComPtr<ID3D11ShaderResourceView> positionTexture,
        ComPtr<ID3D11ShaderResourceView> normalTexture,
        ComPtr<ID3D11ShaderResourceView> randomTexture,
        float screenWidth, float screenHeight, float randomTextureSize,
        float sampleRadius, float ssaoScale, float ssaoBias, float ssaoIntensity)
    {
        if (!SetShaderParameters(DirectX, world, view, projection, positionTexture, normalTexture, randomTexture,
                screenWidth, screenHeight, randomTextureSize, sampleRadius, ssaoScale, ssaoBias, ssaoIntensity)) return false;
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
            SilkMarshal.ThrowHResult(compiler.Compile(pVsSource, (nuint)vsSource.Length,
                (byte*)SilkMarshal.StringToPtr(vsFilename, NativeStringEncoding.Ansi), null, (ID3DInclude*)null,
                (byte*)SilkMarshal.StringToPtr("SsaoVertexShader", NativeStringEncoding.Ansi),
                (byte*)SilkMarshal.StringToPtr("vs_5_0", NativeStringEncoding.Ansi),
                0, 0, &pVsBlob, &pErrorBlob));
        ComPtr<ID3D10Blob> vsBlob = pVsBlob;
        SilkMarshal.ThrowHResult(device.CreateVertexShader(vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(),
            ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _vertexShader));

        ID3D10Blob* pPsBlob = null;
        var psSource = File.ReadAllBytes(psFilename);
        fixed (byte* pPsSource = psSource)
            SilkMarshal.ThrowHResult(compiler.Compile(pPsSource, (nuint)psSource.Length,
                (byte*)SilkMarshal.StringToPtr(psFilename, NativeStringEncoding.Ansi), null, (ID3DInclude*)null,
                (byte*)SilkMarshal.StringToPtr("SsaoPixelShader", NativeStringEncoding.Ansi),
                (byte*)SilkMarshal.StringToPtr("ps_5_0", NativeStringEncoding.Ansi),
                0, 0, &pPsBlob, &pErrorBlob));
        ComPtr<ID3D10Blob> psBlob = pPsBlob;
        SilkMarshal.ThrowHResult(device.CreatePixelShader(psBlob.GetBufferPointer(), psBlob.GetBufferSize(),
            ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _pixelShader));

        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);
        var layoutDesc = new InputElementDesc[]
        {
            new() { SemanticName = (byte*)posName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 0,  InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)texName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32Float,    InputSlot = 0, AlignedByteOffset = 12, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
        };
        fixed (InputElementDesc* pLayout = layoutDesc)
            SilkMarshal.ThrowHResult(device.CreateInputLayout(pLayout, (uint)layoutDesc.Length, vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(), ref _layout));
        SilkMarshal.Free(posName); SilkMarshal.Free(texName);
        vsBlob.Release(); psBlob.Release();

        var matrixBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(MatrixBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&matrixBufferDesc, null, ref _matrixBuffer));

        var ssaoBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(SsaoBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&ssaoBufferDesc, null, ref _ssaoBuffer));

        var wrapDesc = new SamplerDesc { Filter = Filter.MinMagMipLinear, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, MipLODBias = 0.0f, MaxAnisotropy = 1, ComparisonFunc = ComparisonFunc.Always, MinLOD = 0, MaxLOD = float.MaxValue };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&wrapDesc, ref _sampleStateWrap));

        var clampDesc = new SamplerDesc { Filter = Filter.MinMagMipLinear, AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, MipLODBias = 0.0f, MaxAnisotropy = 1, ComparisonFunc = ComparisonFunc.Always, MinLOD = 0, MaxLOD = float.MaxValue };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&clampDesc, ref _sampleStateClamp));
        return true;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        ComPtr<ID3D11ShaderResourceView> positionTexture,
        ComPtr<ID3D11ShaderResourceView> normalTexture,
        ComPtr<ID3D11ShaderResourceView> randomTexture,
        float screenWidth, float screenHeight, float randomTextureSize,
        float sampleRadius, float ssaoScale, float ssaoBias, float ssaoIntensity)
    {
        var context = DirectX.DeviceContext;
        world = Matrix4X4.Transpose(world); view = Matrix4X4.Transpose(view); projection = Matrix4X4.Transpose(projection);

        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_matrixBuffer, 0, Map.WriteDiscard, 0, &mr));
        var mp = (MatrixBufferType*)mr.PData;
        mp->world = world; mp->view = view; mp->projection = projection;
        context.Unmap(_matrixBuffer, 0);
        var mcb = _matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(context.Map(_ssaoBuffer, 0, Map.WriteDiscard, 0, &mr));
        var sp = (SsaoBufferType*)mr.PData;
        sp->screenWidth = screenWidth;
        sp->screenHeight = screenHeight;
        sp->randomTextureSize = randomTextureSize;
        sp->sampleRadius = sampleRadius;
        sp->ssaoScale = ssaoScale;
        sp->ssaoBias = ssaoBias;
        sp->ssaoIntensity = ssaoIntensity;
        sp->padding = 0.0f;
        context.Unmap(_ssaoBuffer, 0);
        var scb = _ssaoBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &scb);

        var srvPos = positionTexture.GetPinnableReference();
        var srvNrm = normalTexture.GetPinnableReference();
        var srvRnd = randomTexture.GetPinnableReference();
        context.PSSetShaderResources(0, 1, &srvPos);
        context.PSSetShaderResources(1, 1, &srvNrm);
        context.PSSetShaderResources(2, 1, &srvRnd);
        return true;
    }

    private void RenderShader(DX11 DirectX, int indexCount)
    {
        var context = DirectX.DeviceContext;
        context.IASetInputLayout(_layout);
        context.VSSetShader(_vertexShader, null, 0);
        context.PSSetShader(_pixelShader, null, 0);
        var wrap = _sampleStateWrap.GetPinnableReference();
        var clamp = _sampleStateClamp.GetPinnableReference();
        context.PSSetSamplers(0, 1, &wrap);
        context.PSSetSamplers(1, 1, &clamp);
        context.DrawIndexed((uint)indexCount, 0, 0);
    }
}

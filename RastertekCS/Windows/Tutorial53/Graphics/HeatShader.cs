using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial53.Graphics;

public unsafe class HeatShader
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

    private struct GlowBufferType
    {
        public float emissiveMultiplier;
        public Vector3D<float> padding;
    }

    private struct DistortionBufferType
    {
        public Vector2D<float> distortion1;
        public Vector2D<float> distortion2;
        public Vector2D<float> distortion3;
        public float distortionScale;
        public float distortionBias;
    }

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11InputLayout> _layout;
    private ComPtr<ID3D11Buffer> _matrixBuffer;
    private ComPtr<ID3D11Buffer> _noiseBuffer;
    private ComPtr<ID3D11Buffer> _glowBuffer;
    private ComPtr<ID3D11Buffer> _distortionBuffer;
    private ComPtr<ID3D11SamplerState> _sampleStateClamp;
    private ComPtr<ID3D11SamplerState> _sampleStateWrap;

    public bool Initialize(DX11 DirectX) => InitializeShader(DirectX, "Shaders/heat.vs", "Shaders/heat.ps");

    public void Shutdown()
    {
        _sampleStateWrap.Release();
        _sampleStateClamp.Release();
        _distortionBuffer.Release();
        _glowBuffer.Release();
        _noiseBuffer.Release();
        _matrixBuffer.Release();
        _layout.Release();
        _pixelShader.Release();
        _vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        ComPtr<ID3D11ShaderResourceView> colorTexture,
        ComPtr<ID3D11ShaderResourceView> glowTexture,
        ComPtr<ID3D11ShaderResourceView> noiseTexture,
        float emissiveMultiplier, float frameTime,
        Vector3D<float> scrollSpeeds, Vector3D<float> scales,
        Vector2D<float> distortion1, Vector2D<float> distortion2, Vector2D<float> distortion3,
        float distortionScale, float distortionBias)
    {
        if (!SetShaderParameters(DirectX, world, view, projection, colorTexture, glowTexture, noiseTexture,
                emissiveMultiplier, frameTime, scrollSpeeds, scales,
                distortion1, distortion2, distortion3, distortionScale, distortionBias)) return false;
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
                (byte*)SilkMarshal.StringToPtr("HeatVertexShader", NativeStringEncoding.Ansi),
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
                (byte*)SilkMarshal.StringToPtr("HeatPixelShader", NativeStringEncoding.Ansi),
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

        var noiseBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(NoiseBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&noiseBufferDesc, null, ref _noiseBuffer));

        var glowBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(GlowBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&glowBufferDesc, null, ref _glowBuffer));

        var distortionBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(DistortionBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&distortionBufferDesc, null, ref _distortionBuffer));

        var clampDesc = new SamplerDesc { Filter = Filter.MinMagMipLinear, AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, MipLODBias = 0.0f, MaxAnisotropy = 1, ComparisonFunc = ComparisonFunc.Always, MinLOD = 0, MaxLOD = float.MaxValue };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&clampDesc, ref _sampleStateClamp));

        var wrapDesc = new SamplerDesc { Filter = Filter.MinMagMipLinear, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, MipLODBias = 0.0f, MaxAnisotropy = 1, ComparisonFunc = ComparisonFunc.Always, MinLOD = 0, MaxLOD = float.MaxValue };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&wrapDesc, ref _sampleStateWrap));
        return true;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        ComPtr<ID3D11ShaderResourceView> colorTexture,
        ComPtr<ID3D11ShaderResourceView> glowTexture,
        ComPtr<ID3D11ShaderResourceView> noiseTexture,
        float emissiveMultiplier, float frameTime,
        Vector3D<float> scrollSpeeds, Vector3D<float> scales,
        Vector2D<float> distortion1, Vector2D<float> distortion2, Vector2D<float> distortion3,
        float distortionScale, float distortionBias)
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

        SilkMarshal.ThrowHResult(context.Map(_noiseBuffer, 0, Map.WriteDiscard, 0, &mr));
        var np = (NoiseBufferType*)mr.PData;
        np->frameTime = frameTime;
        np->scrollSpeeds = scrollSpeeds;
        np->scales = scales;
        np->padding = 0.0f;
        context.Unmap(_noiseBuffer, 0);
        var ncb = _noiseBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(1, 1, &ncb);

        SilkMarshal.ThrowHResult(context.Map(_glowBuffer, 0, Map.WriteDiscard, 0, &mr));
        var gp = (GlowBufferType*)mr.PData;
        gp->emissiveMultiplier = emissiveMultiplier;
        gp->padding = new Vector3D<float>(0.0f, 0.0f, 0.0f);
        context.Unmap(_glowBuffer, 0);
        var gcb = _glowBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &gcb);

        SilkMarshal.ThrowHResult(context.Map(_distortionBuffer, 0, Map.WriteDiscard, 0, &mr));
        var dp = (DistortionBufferType*)mr.PData;
        dp->distortion1 = distortion1;
        dp->distortion2 = distortion2;
        dp->distortion3 = distortion3;
        dp->distortionScale = distortionScale;
        dp->distortionBias = distortionBias;
        context.Unmap(_distortionBuffer, 0);
        var dcb = _distortionBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(1, 1, &dcb);

        var srvColor = colorTexture.GetPinnableReference();
        var srvGlow = glowTexture.GetPinnableReference();
        var srvNoise = noiseTexture.GetPinnableReference();
        context.PSSetShaderResources(0, 1, &srvColor);
        context.PSSetShaderResources(1, 1, &srvGlow);
        context.PSSetShaderResources(2, 1, &srvNoise);
        return true;
    }

    private void RenderShader(DX11 DirectX, int indexCount)
    {
        var context = DirectX.DeviceContext;
        context.IASetInputLayout(_layout);
        context.VSSetShader(_vertexShader, null, 0);
        context.PSSetShader(_pixelShader, null, 0);
        var clamp = _sampleStateClamp.GetPinnableReference();
        var wrap = _sampleStateWrap.GetPinnableReference();
        context.PSSetSamplers(0, 1, &clamp);
        context.PSSetSamplers(1, 1, &wrap);
        context.DrawIndexed((uint)indexCount, 0, 0);
    }
}

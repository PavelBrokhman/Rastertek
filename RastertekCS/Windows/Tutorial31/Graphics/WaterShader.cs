using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial31.Graphics;

public unsafe class WaterShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct ReflectionBufferType
    {
        public Matrix4X4<float> reflection;
    }

    private struct WaterBufferType
    {
        public float waterTranslation;
        public float reflectRefractScale;
        public float pad0,
            pad1;
    }

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11InputLayout> _layout;
    private ComPtr<ID3D11Buffer> _matrixBuffer;
    private ComPtr<ID3D11Buffer> _reflectionBuffer;
    private ComPtr<ID3D11Buffer> _waterBuffer;
    private ComPtr<ID3D11SamplerState> _sampleState;

    public bool Initialize(DX11 DirectX) =>
        InitializeShader(DirectX, "Shaders/water.vs", "Shaders/water.ps");

    public void Shutdown()
    {
        _sampleState.Release();
        _waterBuffer.Release();
        _reflectionBuffer.Release();
        _matrixBuffer.Release();
        _layout.Release();
        _pixelShader.Release();
        _vertexShader.Release();
    }

    public bool Render(
        DX11 DirectX,
        int indexCount,
        Matrix4X4<float> world,
        Matrix4X4<float> view,
        Matrix4X4<float> projection,
        Matrix4X4<float> reflection,
        ComPtr<ID3D11ShaderResourceView> reflectionTexture,
        ComPtr<ID3D11ShaderResourceView> refractionTexture,
        ComPtr<ID3D11ShaderResourceView> normalTexture,
        float waterTranslation,
        float reflectRefractScale
    )
    {
        if (
            !SetShaderParameters(
                DirectX,
                world,
                view,
                projection,
                reflection,
                reflectionTexture,
                refractionTexture,
                normalTexture,
                waterTranslation,
                reflectRefractScale
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
            SilkMarshal.ThrowHResult(
                compiler.Compile(
                    pVsSource,
                    (nuint)vsSource.Length,
                    (byte*)SilkMarshal.StringToPtr(vsFilename, NativeStringEncoding.Ansi),
                    null,
                    (ID3DInclude*)null,
                    (byte*)SilkMarshal.StringToPtr("WaterVertexShader", NativeStringEncoding.Ansi),
                    (byte*)SilkMarshal.StringToPtr("vs_5_0", NativeStringEncoding.Ansi),
                    0,
                    0,
                    &pVsBlob,
                    &pErrorBlob
                )
            );
        ComPtr<ID3D10Blob> vsBlob = pVsBlob;
        SilkMarshal.ThrowHResult(
            device.CreateVertexShader(
                vsBlob.GetBufferPointer(),
                vsBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref _vertexShader
            )
        );

        ID3D10Blob* pPsBlob = null;
        var psSource = File.ReadAllBytes(psFilename);
        fixed (byte* pPsSource = psSource)
            SilkMarshal.ThrowHResult(
                compiler.Compile(
                    pPsSource,
                    (nuint)psSource.Length,
                    (byte*)SilkMarshal.StringToPtr(psFilename, NativeStringEncoding.Ansi),
                    null,
                    (ID3DInclude*)null,
                    (byte*)SilkMarshal.StringToPtr("WaterPixelShader", NativeStringEncoding.Ansi),
                    (byte*)SilkMarshal.StringToPtr("ps_5_0", NativeStringEncoding.Ansi),
                    0,
                    0,
                    &pPsBlob,
                    &pErrorBlob
                )
            );
        ComPtr<ID3D10Blob> psBlob = pPsBlob;
        SilkMarshal.ThrowHResult(
            device.CreatePixelShader(
                psBlob.GetBufferPointer(),
                psBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref _pixelShader
            )
        );

        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);
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
        };
        fixed (InputElementDesc* pLayout = layoutDesc)
            SilkMarshal.ThrowHResult(
                device.CreateInputLayout(
                    pLayout,
                    (uint)layoutDesc.Length,
                    vsBlob.GetBufferPointer(),
                    vsBlob.GetBufferSize(),
                    ref _layout
                )
            );
        SilkMarshal.Free(posName);
        SilkMarshal.Free(texName);
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
        SilkMarshal.ThrowHResult(device.CreateBuffer(&matrixBufferDesc, null, ref _matrixBuffer));

        var reflectionBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(ReflectionBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        SilkMarshal.ThrowHResult(
            device.CreateBuffer(&reflectionBufferDesc, null, ref _reflectionBuffer)
        );

        var waterBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(WaterBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&waterBufferDesc, null, ref _waterBuffer));

        var samplerDesc = new SamplerDesc
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            MipLODBias = 0.0f,
            MaxAnisotropy = 1,
            ComparisonFunc = ComparisonFunc.Always,
            MinLOD = 0,
            MaxLOD = float.MaxValue,
        };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&samplerDesc, ref _sampleState));

        return true;
    }

    private bool SetShaderParameters(
        DX11 DirectX,
        Matrix4X4<float> world,
        Matrix4X4<float> view,
        Matrix4X4<float> projection,
        Matrix4X4<float> reflection,
        ComPtr<ID3D11ShaderResourceView> reflectionTexture,
        ComPtr<ID3D11ShaderResourceView> refractionTexture,
        ComPtr<ID3D11ShaderResourceView> normalTexture,
        float waterTranslation,
        float reflectRefractScale
    )
    {
        var context = DirectX.DeviceContext;
        world = Matrix4X4.Transpose(world);
        view = Matrix4X4.Transpose(view);
        projection = Matrix4X4.Transpose(projection);
        reflection = Matrix4X4.Transpose(reflection);

        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_matrixBuffer, 0, Map.WriteDiscard, 0, &mr));
        var mp = (MatrixBufferType*)mr.PData;
        mp->world = world;
        mp->view = view;
        mp->projection = projection;
        context.Unmap(_matrixBuffer, 0);
        var mcb = _matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(context.Map(_reflectionBuffer, 0, Map.WriteDiscard, 0, &mr));
        var rp = (ReflectionBufferType*)mr.PData;
        rp->reflection = reflection;
        context.Unmap(_reflectionBuffer, 0);
        var rcb = _reflectionBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(1, 1, &rcb);

        SilkMarshal.ThrowHResult(context.Map(_waterBuffer, 0, Map.WriteDiscard, 0, &mr));
        var wp = (WaterBufferType*)mr.PData;
        wp->waterTranslation = waterTranslation;
        wp->reflectRefractScale = reflectRefractScale;
        wp->pad0 = 0;
        wp->pad1 = 0;
        context.Unmap(_waterBuffer, 0);
        var wcb = _waterBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &wcb);

        var srv0 = reflectionTexture.GetPinnableReference();
        var srv1 = refractionTexture.GetPinnableReference();
        var srv2 = normalTexture.GetPinnableReference();
        context.PSSetShaderResources(0, 1, &srv0);
        context.PSSetShaderResources(1, 1, &srv1);
        context.PSSetShaderResources(2, 1, &srv2);

        return true;
    }

    private void RenderShader(DX11 DirectX, int indexCount)
    {
        var context = DirectX.DeviceContext;
        context.IASetInputLayout(_layout);
        context.VSSetShader(_vertexShader, null, 0);
        context.PSSetShader(_pixelShader, null, 0);
        var smp = _sampleState.GetPinnableReference();
        context.PSSetSamplers(0, 1, &smp);
        context.DrawIndexed((uint)indexCount, 0, 0);
    }
}

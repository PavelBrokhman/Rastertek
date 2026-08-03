using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial52.Graphics;

public unsafe class PbrShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct CameraBufferType
    {
        public Vector3D<float> cameraPosition;
        public float padding;
    }

    private struct LightBufferType
    {
        public Vector3D<float> lightDirection;
        public float padding;
    }

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11InputLayout> _layout;
    private ComPtr<ID3D11Buffer> _matrixBuffer;
    private ComPtr<ID3D11Buffer> _cameraBuffer;
    private ComPtr<ID3D11Buffer> _lightBuffer;
    private ComPtr<ID3D11SamplerState> _sampleState;

    public bool Initialize(DX11 DirectX) => InitializeShader(DirectX, "Shaders/pbr.vs", "Shaders/pbr.ps");

    public void Shutdown()
    {
        _sampleState.Release();
        _lightBuffer.Release(); _cameraBuffer.Release(); _matrixBuffer.Release();
        _layout.Release(); _pixelShader.Release(); _vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        ComPtr<ID3D11ShaderResourceView> diffuse, ComPtr<ID3D11ShaderResourceView> normal, ComPtr<ID3D11ShaderResourceView> rm,
        Vector3D<float> cameraPosition, Vector3D<float> lightDirection)
    {
        if (!SetShaderParameters(DirectX, world, view, projection, diffuse, normal, rm, cameraPosition, lightDirection)) return false;
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
                (byte*)SilkMarshal.StringToPtr("PBRVertexShader", NativeStringEncoding.Ansi),
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
                (byte*)SilkMarshal.StringToPtr("PBRPixelShader", NativeStringEncoding.Ansi),
                (byte*)SilkMarshal.StringToPtr("ps_5_0", NativeStringEncoding.Ansi),
                0, 0, &pPsBlob, &pErrorBlob));
        ComPtr<ID3D10Blob> psBlob = pPsBlob;
        SilkMarshal.ThrowHResult(device.CreatePixelShader(psBlob.GetBufferPointer(), psBlob.GetBufferSize(),
            ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _pixelShader));

        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);
        var nrmName = SilkMarshal.StringToPtr("NORMAL", NativeStringEncoding.Ansi);
        var tanName = SilkMarshal.StringToPtr("TANGENT", NativeStringEncoding.Ansi);
        var binName = SilkMarshal.StringToPtr("BINORMAL", NativeStringEncoding.Ansi);
        var layoutDesc = new InputElementDesc[]
        {
            new() { SemanticName = (byte*)posName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 0,  InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)texName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32Float,    InputSlot = 0, AlignedByteOffset = 12, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)nrmName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 20, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)tanName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 32, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)binName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 44, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
        };
        fixed (InputElementDesc* pLayout = layoutDesc)
            SilkMarshal.ThrowHResult(device.CreateInputLayout(pLayout, (uint)layoutDesc.Length, vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(), ref _layout));
        SilkMarshal.Free(posName); SilkMarshal.Free(texName); SilkMarshal.Free(nrmName); SilkMarshal.Free(tanName); SilkMarshal.Free(binName);
        vsBlob.Release(); psBlob.Release();

        var matrixBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(MatrixBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&matrixBufferDesc, null, ref _matrixBuffer));

        var cameraBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(CameraBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&cameraBufferDesc, null, ref _cameraBuffer));

        var lightBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(LightBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&lightBufferDesc, null, ref _lightBuffer));

        var samplerDesc = new SamplerDesc { Filter = Filter.MinMagMipLinear, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, MipLODBias = 0.0f, MaxAnisotropy = 1, ComparisonFunc = ComparisonFunc.Always, MinLOD = 0, MaxLOD = float.MaxValue };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&samplerDesc, ref _sampleState));
        return true;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        ComPtr<ID3D11ShaderResourceView> diffuse, ComPtr<ID3D11ShaderResourceView> normal, ComPtr<ID3D11ShaderResourceView> rm,
        Vector3D<float> cameraPosition, Vector3D<float> lightDirection)
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

        SilkMarshal.ThrowHResult(context.Map(_cameraBuffer, 0, Map.WriteDiscard, 0, &mr));
        var cp = (CameraBufferType*)mr.PData;
        cp->cameraPosition = cameraPosition; cp->padding = 0;
        context.Unmap(_cameraBuffer, 0);
        var ccb = _cameraBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(1, 1, &ccb);

        SilkMarshal.ThrowHResult(context.Map(_lightBuffer, 0, Map.WriteDiscard, 0, &mr));
        var lp = (LightBufferType*)mr.PData;
        lp->lightDirection = lightDirection; lp->padding = 0;
        context.Unmap(_lightBuffer, 0);
        var lcb = _lightBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &lcb);

        var d = diffuse.GetPinnableReference(); var n = normal.GetPinnableReference(); var r = rm.GetPinnableReference();
        ID3D11ShaderResourceView** srvs = stackalloc ID3D11ShaderResourceView*[3] { d, n, r };
        context.PSSetShaderResources(0, 3, srvs);
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

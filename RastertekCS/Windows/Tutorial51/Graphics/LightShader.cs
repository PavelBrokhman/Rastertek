using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial51.Graphics;

public unsafe class LightShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct ViewBufferType
    {
        public Matrix4X4<float> cameraView;
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
    private ComPtr<ID3D11Buffer> _viewBuffer;
    private ComPtr<ID3D11Buffer> _lightBuffer;
    private ComPtr<ID3D11SamplerState> _sampleStatePoint;

    public bool Initialize(DX11 DirectX) => InitializeShader(DirectX, "Shaders/light.vs", "Shaders/light.ps");

    public void Shutdown()
    {
        _sampleStatePoint.Release();
        _lightBuffer.Release();
        _viewBuffer.Release();
        _matrixBuffer.Release();
        _layout.Release();
        _pixelShader.Release();
        _vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        Vector3D<float> lightDirection,
        ComPtr<ID3D11ShaderResourceView> normalsTexture,
        ComPtr<ID3D11ShaderResourceView> ssaoTexture,
        Matrix4X4<float> cameraViewMatrix,
        ComPtr<ID3D11ShaderResourceView> colorsTexture)
    {
        if (!SetShaderParameters(DirectX, world, view, projection, lightDirection,
                normalsTexture, ssaoTexture, cameraViewMatrix, colorsTexture)) return false;
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
                (byte*)SilkMarshal.StringToPtr("LightVertexShader", NativeStringEncoding.Ansi),
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
                (byte*)SilkMarshal.StringToPtr("LightPixelShader", NativeStringEncoding.Ansi),
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

        var viewBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(ViewBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&viewBufferDesc, null, ref _viewBuffer));

        var lightBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(LightBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&lightBufferDesc, null, ref _lightBuffer));

        var samplerDesc = new SamplerDesc { Filter = Filter.MinMagMipPoint, AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, MipLODBias = 0.0f, MaxAnisotropy = 1, ComparisonFunc = ComparisonFunc.Always, MinLOD = 0, MaxLOD = float.MaxValue };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&samplerDesc, ref _sampleStatePoint));
        return true;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        Vector3D<float> lightDirection,
        ComPtr<ID3D11ShaderResourceView> normalsTexture,
        ComPtr<ID3D11ShaderResourceView> ssaoTexture,
        Matrix4X4<float> cameraViewMatrix,
        ComPtr<ID3D11ShaderResourceView> colorsTexture)
    {
        var context = DirectX.DeviceContext;
        world = Matrix4X4.Transpose(world); view = Matrix4X4.Transpose(view); projection = Matrix4X4.Transpose(projection);
        cameraViewMatrix = Matrix4X4.Transpose(cameraViewMatrix);

        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_matrixBuffer, 0, Map.WriteDiscard, 0, &mr));
        var mp = (MatrixBufferType*)mr.PData;
        mp->world = world; mp->view = view; mp->projection = projection;
        context.Unmap(_matrixBuffer, 0);
        var mcb = _matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(context.Map(_viewBuffer, 0, Map.WriteDiscard, 0, &mr));
        var vp = (ViewBufferType*)mr.PData;
        vp->cameraView = cameraViewMatrix;
        context.Unmap(_viewBuffer, 0);
        var vcb = _viewBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(1, 1, &vcb);

        SilkMarshal.ThrowHResult(context.Map(_lightBuffer, 0, Map.WriteDiscard, 0, &mr));
        var lp = (LightBufferType*)mr.PData;
        lp->lightDirection = lightDirection;
        lp->padding = 0.0f;
        context.Unmap(_lightBuffer, 0);
        var lcb = _lightBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(2, 1, &lcb);

        var srvNormals = normalsTexture.GetPinnableReference();
        var srvSsao = ssaoTexture.GetPinnableReference();
        var srvColors = colorsTexture.GetPinnableReference();
        context.PSSetShaderResources(0, 1, &srvNormals);
        context.PSSetShaderResources(1, 1, &srvSsao);
        context.PSSetShaderResources(2, 1, &srvColors);
        return true;
    }

    private void RenderShader(DX11 DirectX, int indexCount)
    {
        var context = DirectX.DeviceContext;
        context.IASetInputLayout(_layout);
        context.VSSetShader(_vertexShader, null, 0);
        context.PSSetShader(_pixelShader, null, 0);
        var smp = _sampleStatePoint.GetPinnableReference();
        context.PSSetSamplers(0, 1, &smp);
        context.DrawIndexed((uint)indexCount, 0, 0);
    }
}

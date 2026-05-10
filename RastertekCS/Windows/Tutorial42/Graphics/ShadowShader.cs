using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial42.Graphics;

public unsafe class ShadowShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
        public Matrix4X4<float> lightView;
        public Matrix4X4<float> lightProjection;
        public Matrix4X4<float> lightView2;
        public Matrix4X4<float> lightProjection2;
    }

    private struct LightPositionBufferType
    {
        public Vector3D<float> lightPosition;
        public float padding1;
        public Vector3D<float> lightPosition2;
        public float padding2;
    }

    private struct LightBufferType
    {
        public Vector4D<float> ambientColor;
        public Vector4D<float> diffuseColor;
        public Vector4D<float> diffuseColor2;
        public float bias;
        public Vector3D<float> padding;
    }

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11InputLayout> _layout;
    private ComPtr<ID3D11Buffer> _matrixBuffer;
    private ComPtr<ID3D11Buffer> _lightPositionBuffer;
    private ComPtr<ID3D11Buffer> _lightBuffer;
    private ComPtr<ID3D11SamplerState> _sampleState;

    public bool Initialize(DX11 DirectX) => InitializeShader(DirectX, "Shaders/shadow.vs", "Shaders/shadow.ps");

    public void Shutdown()
    {
        _sampleState.Release();
        _lightBuffer.Release();
        _lightPositionBuffer.Release();
        _matrixBuffer.Release();
        _layout.Release();
        _pixelShader.Release();
        _vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        Matrix4X4<float> lightViewMatrix, Matrix4X4<float> lightProjectionMatrix,
        Matrix4X4<float> lightViewMatrix2, Matrix4X4<float> lightProjectionMatrix2,
        ComPtr<ID3D11ShaderResourceView> texture,
        ComPtr<ID3D11ShaderResourceView> depthMapTexture,
        ComPtr<ID3D11ShaderResourceView> depthMapTexture2,
        Vector4D<float> ambientColor,
        Vector4D<float> diffuseColor, Vector3D<float> lightPosition,
        Vector4D<float> diffuseColor2, Vector3D<float> lightPosition2,
        float bias)
    {
        if (!SetShaderParameters(DirectX, worldMatrix, viewMatrix, projectionMatrix,
                lightViewMatrix, lightProjectionMatrix, lightViewMatrix2, lightProjectionMatrix2,
                texture, depthMapTexture, depthMapTexture2,
                ambientColor, diffuseColor, lightPosition, diffuseColor2, lightPosition2, bias)) return false;
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
                (byte*)SilkMarshal.StringToPtr("ShadowVertexShader", NativeStringEncoding.Ansi),
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
                (byte*)SilkMarshal.StringToPtr("ShadowPixelShader", NativeStringEncoding.Ansi),
                (byte*)SilkMarshal.StringToPtr("ps_5_0", NativeStringEncoding.Ansi),
                0, 0, &pPsBlob, &pErrorBlob));
        ComPtr<ID3D10Blob> psBlob = pPsBlob;
        SilkMarshal.ThrowHResult(device.CreatePixelShader(psBlob.GetBufferPointer(), psBlob.GetBufferSize(),
            ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _pixelShader));

        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);
        var nrmName = SilkMarshal.StringToPtr("NORMAL", NativeStringEncoding.Ansi);
        var layoutDesc = new InputElementDesc[]
        {
            new() { SemanticName = (byte*)posName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)texName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32Float, InputSlot = 0, AlignedByteOffset = 12, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)nrmName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 20, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
        };
        fixed (InputElementDesc* pLayout = layoutDesc)
            SilkMarshal.ThrowHResult(device.CreateInputLayout(pLayout, (uint)layoutDesc.Length, vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(), ref _layout));
        SilkMarshal.Free(posName); SilkMarshal.Free(texName); SilkMarshal.Free(nrmName);
        vsBlob.Release(); psBlob.Release();

        var matrixBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(MatrixBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&matrixBufferDesc, null, ref _matrixBuffer));

        var lightPositionBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(LightPositionBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&lightPositionBufferDesc, null, ref _lightPositionBuffer));

        var lightBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(LightBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&lightBufferDesc, null, ref _lightBuffer));

        var samplerDesc = new SamplerDesc
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            MipLODBias = 0.0f,
            MaxAnisotropy = 1,
            ComparisonFunc = ComparisonFunc.Always,
            MinLOD = 0,
            MaxLOD = float.MaxValue,
        };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&samplerDesc, ref _sampleState));
        return true;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> worldMatrix, Matrix4X4<float> viewMatrix, Matrix4X4<float> projectionMatrix,
        Matrix4X4<float> lightViewMatrix, Matrix4X4<float> lightProjectionMatrix,
        Matrix4X4<float> lightViewMatrix2, Matrix4X4<float> lightProjectionMatrix2,
        ComPtr<ID3D11ShaderResourceView> texture,
        ComPtr<ID3D11ShaderResourceView> depthMapTexture,
        ComPtr<ID3D11ShaderResourceView> depthMapTexture2,
        Vector4D<float> ambientColor,
        Vector4D<float> diffuseColor, Vector3D<float> lightPosition,
        Vector4D<float> diffuseColor2, Vector3D<float> lightPosition2,
        float bias)
    {
        var context = DirectX.DeviceContext;
        worldMatrix = Matrix4X4.Transpose(worldMatrix);
        viewMatrix = Matrix4X4.Transpose(viewMatrix);
        projectionMatrix = Matrix4X4.Transpose(projectionMatrix);
        lightViewMatrix = Matrix4X4.Transpose(lightViewMatrix);
        lightProjectionMatrix = Matrix4X4.Transpose(lightProjectionMatrix);
        lightViewMatrix2 = Matrix4X4.Transpose(lightViewMatrix2);
        lightProjectionMatrix2 = Matrix4X4.Transpose(lightProjectionMatrix2);

        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_matrixBuffer, 0, Map.WriteDiscard, 0, &mr));
        var mp = (MatrixBufferType*)mr.PData;
        mp->world = worldMatrix; mp->view = viewMatrix; mp->projection = projectionMatrix;
        mp->lightView = lightViewMatrix; mp->lightProjection = lightProjectionMatrix;
        mp->lightView2 = lightViewMatrix2; mp->lightProjection2 = lightProjectionMatrix2;
        context.Unmap(_matrixBuffer, 0);
        var mcb = _matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(context.Map(_lightPositionBuffer, 0, Map.WriteDiscard, 0, &mr));
        var lpp = (LightPositionBufferType*)mr.PData;
        lpp->lightPosition = lightPosition; lpp->padding1 = 0;
        lpp->lightPosition2 = lightPosition2; lpp->padding2 = 0;
        context.Unmap(_lightPositionBuffer, 0);
        var lpcb = _lightPositionBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(1, 1, &lpcb);

        SilkMarshal.ThrowHResult(context.Map(_lightBuffer, 0, Map.WriteDiscard, 0, &mr));
        var lp = (LightBufferType*)mr.PData;
        lp->ambientColor = ambientColor;
        lp->diffuseColor = diffuseColor;
        lp->diffuseColor2 = diffuseColor2;
        lp->bias = bias;
        lp->padding = new Vector3D<float>(0, 0, 0);
        context.Unmap(_lightBuffer, 0);
        var lcb = _lightBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &lcb);

        var srv0 = texture.GetPinnableReference();
        context.PSSetShaderResources(0, 1, &srv0);
        var srv1 = depthMapTexture.GetPinnableReference();
        context.PSSetShaderResources(1, 1, &srv1);
        var srv2 = depthMapTexture2.GetPinnableReference();
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

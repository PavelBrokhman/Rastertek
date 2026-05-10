using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial49.Graphics;

public unsafe class ColorShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct TessellationBufferType
    {
        public float tessellationAmount;
        public Vector3D<float> padding;
    }

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11HullShader> _hullShader;
    private ComPtr<ID3D11DomainShader> _domainShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11InputLayout> _layout;
    private ComPtr<ID3D11Buffer> _matrixBuffer;
    private ComPtr<ID3D11Buffer> _tessellationBuffer;

    public bool Initialize(DX11 DirectX) =>
        InitializeShader(DirectX, "Shaders/color.vs", "Shaders/color.hs", "Shaders/color.ds", "Shaders/color.ps");

    public void Shutdown()
    {
        _tessellationBuffer.Release();
        _matrixBuffer.Release();
        _layout.Release();
        _pixelShader.Release();
        _domainShader.Release();
        _hullShader.Release();
        _vertexShader.Release();
    }

    public bool Render(DX11 DirectX, int indexCount,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        float tessellationAmount)
    {
        if (!SetShaderParameters(DirectX, world, view, projection, tessellationAmount)) return false;
        RenderShader(DirectX, indexCount);
        return true;
    }

    private bool InitializeShader(DX11 DirectX, string vsFile, string hsFile, string dsFile, string psFile)
    {
        var device = DirectX.Device;
        var compiler = D3DCompiler.GetApi();

        ComPtr<ID3D10Blob> vsBlob = CompileShader(compiler, vsFile, "ColorVertexShader", "vs_5_0");
        SilkMarshal.ThrowHResult(device.CreateVertexShader(vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(),
            ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _vertexShader));

        ComPtr<ID3D10Blob> hsBlob = CompileShader(compiler, hsFile, "ColorHullShader", "hs_5_0");
        SilkMarshal.ThrowHResult(device.CreateHullShader(hsBlob.GetBufferPointer(), hsBlob.GetBufferSize(),
            ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _hullShader));
        hsBlob.Release();

        ComPtr<ID3D10Blob> dsBlob = CompileShader(compiler, dsFile, "ColorDomainShader", "ds_5_0");
        SilkMarshal.ThrowHResult(device.CreateDomainShader(dsBlob.GetBufferPointer(), dsBlob.GetBufferSize(),
            ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _domainShader));
        dsBlob.Release();

        ComPtr<ID3D10Blob> psBlob = CompileShader(compiler, psFile, "ColorPixelShader", "ps_5_0");
        SilkMarshal.ThrowHResult(device.CreatePixelShader(psBlob.GetBufferPointer(), psBlob.GetBufferSize(),
            ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _pixelShader));
        psBlob.Release();

        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var colName = SilkMarshal.StringToPtr("COLOR", NativeStringEncoding.Ansi);
        var layoutDesc = new InputElementDesc[]
        {
            new() { SemanticName = (byte*)posName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32Float, InputSlot = 0, AlignedByteOffset = 0, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
            new() { SemanticName = (byte*)colName, SemanticIndex = 0, Format = Silk.NET.DXGI.Format.FormatR32G32B32A32Float, InputSlot = 0, AlignedByteOffset = 12, InputSlotClass = InputClassification.PerVertexData, InstanceDataStepRate = 0 },
        };
        fixed (InputElementDesc* pLayout = layoutDesc)
            SilkMarshal.ThrowHResult(device.CreateInputLayout(pLayout, (uint)layoutDesc.Length, vsBlob.GetBufferPointer(), vsBlob.GetBufferSize(), ref _layout));
        SilkMarshal.Free(posName); SilkMarshal.Free(colName);
        vsBlob.Release();

        var matrixBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(MatrixBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&matrixBufferDesc, null, ref _matrixBuffer));

        var tessBufferDesc = new BufferDesc { Usage = Usage.Dynamic, ByteWidth = (uint)sizeof(TessellationBufferType), BindFlags = (uint)BindFlag.ConstantBuffer, CPUAccessFlags = (uint)CpuAccessFlag.Write };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&tessBufferDesc, null, ref _tessellationBuffer));
        return true;
    }

    private static ComPtr<ID3D10Blob> CompileShader(D3DCompiler compiler, string filename, string entrypoint, string profile)
    {
        ID3D10Blob* pBlob = null;
        ID3D10Blob* pErr = null;
        var src = File.ReadAllBytes(filename);
        fixed (byte* pSrc = src)
            SilkMarshal.ThrowHResult(compiler.Compile(pSrc, (nuint)src.Length,
                (byte*)SilkMarshal.StringToPtr(filename, NativeStringEncoding.Ansi), null, (ID3DInclude*)null,
                (byte*)SilkMarshal.StringToPtr(entrypoint, NativeStringEncoding.Ansi),
                (byte*)SilkMarshal.StringToPtr(profile, NativeStringEncoding.Ansi),
                0, 0, &pBlob, &pErr));
        return pBlob;
    }

    private bool SetShaderParameters(DX11 DirectX,
        Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> projection,
        float tessellationAmount)
    {
        var context = DirectX.DeviceContext;
        world = Matrix4X4.Transpose(world); view = Matrix4X4.Transpose(view); projection = Matrix4X4.Transpose(projection);

        MappedSubresource mr;
        SilkMarshal.ThrowHResult(context.Map(_matrixBuffer, 0, Map.WriteDiscard, 0, &mr));
        var mp = (MatrixBufferType*)mr.PData;
        mp->world = world; mp->view = view; mp->projection = projection;
        context.Unmap(_matrixBuffer, 0);
        var mcb = _matrixBuffer.GetPinnableReference();
        context.DSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(context.Map(_tessellationBuffer, 0, Map.WriteDiscard, 0, &mr));
        var tp = (TessellationBufferType*)mr.PData;
        tp->tessellationAmount = tessellationAmount;
        tp->padding = default;
        context.Unmap(_tessellationBuffer, 0);
        var tcb = _tessellationBuffer.GetPinnableReference();
        context.HSSetConstantBuffers(0, 1, &tcb);

        return true;
    }

    private void RenderShader(DX11 DirectX, int indexCount)
    {
        var context = DirectX.DeviceContext;
        context.IASetInputLayout(_layout);
        context.VSSetShader(_vertexShader, null, 0);
        context.HSSetShader(_hullShader, null, 0);
        context.DSSetShader(_domainShader, null, 0);
        context.PSSetShader(_pixelShader, null, 0);
        context.DrawIndexed((uint)indexCount, 0, 0);
    }
}

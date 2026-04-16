using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial21.Graphics;

public unsafe class SpecMapShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct CameraBufferType
    {
        public float camX,
            camY,
            camZ;
        public float padding;
    }

    private struct LightBufferType
    {
        public float diffuseR,
            diffuseG,
            diffuseB,
            diffuseA;
        public float specularR,
            specularG,
            specularB,
            specularA;
        public float specularPower;
        public float lightDirX,
            lightDirY,
            lightDirZ;
    }

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11InputLayout> _layout;
    private ComPtr<ID3D11Buffer> _matrixBuffer;
    private ComPtr<ID3D11Buffer> _cameraBuffer;
    private ComPtr<ID3D11Buffer> _lightBuffer;

    public bool Initialize(DX11 DirectX)
    {
        return InitializeShader(DirectX, "Shaders/SpecMap.vs", "Shaders/SpecMap.ps");
    }

    public void Shutdown()
    {
        _lightBuffer.Release();
        _cameraBuffer.Release();
        _matrixBuffer.Release();
        _layout.Release();
        _pixelShader.Release();
        _vertexShader.Release();
    }

    public bool Render(
        DX11 DirectX,
        int indexCount,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        float[] lightDirection,
        float[] diffuseColor,
        float[] cameraPosition,
        float[] specularColor,
        float specularPower
    )
    {
        if (
            !SetShaderParameters(
                DirectX,
                worldMatrix,
                viewMatrix,
                projectionMatrix,
                lightDirection,
                diffuseColor,
                cameraPosition,
                specularColor,
                specularPower
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
                    (byte*)
                        SilkMarshal.StringToPtr("SpecMapVertexShader", NativeStringEncoding.Ansi),
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
                ref _vertexShader
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
                    (byte*)SilkMarshal.StringToPtr("SpecMapPixelShader", NativeStringEncoding.Ansi),
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
                ref _pixelShader
            )
        );

        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);
        var normalName = SilkMarshal.StringToPtr("NORMAL", NativeStringEncoding.Ansi);
        var tangentName = SilkMarshal.StringToPtr("TANGENT", NativeStringEncoding.Ansi);
        var binormalName = SilkMarshal.StringToPtr("BINORMAL", NativeStringEncoding.Ansi);

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
            new()
            {
                SemanticName = (byte*)tangentName,
                SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32B32Float,
                InputSlot = 0,
                AlignedByteOffset = 32,
                InputSlotClass = InputClassification.PerVertexData,
                InstanceDataStepRate = 0,
            },
            new()
            {
                SemanticName = (byte*)binormalName,
                SemanticIndex = 0,
                Format = Silk.NET.DXGI.Format.FormatR32G32B32Float,
                InputSlot = 0,
                AlignedByteOffset = 44,
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
                    ref _layout
                )
            );
        }

        SilkMarshal.Free(posName);
        SilkMarshal.Free(texName);
        SilkMarshal.Free(normalName);
        SilkMarshal.Free(tangentName);
        SilkMarshal.Free(binormalName);
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

        var cameraBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(CameraBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&cameraBufferDesc, null, ref _cameraBuffer));

        var lightBufferDesc = new BufferDesc
        {
            Usage = Usage.Dynamic,
            ByteWidth = (uint)sizeof(LightBufferType),
            BindFlags = (uint)BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint)CpuAccessFlag.Write,
            MiscFlags = 0,
            StructureByteStride = 0,
        };
        SilkMarshal.ThrowHResult(device.CreateBuffer(&lightBufferDesc, null, ref _lightBuffer));

        return true;
    }

    private bool SetShaderParameters(
        DX11 DirectX,
        Matrix4X4<float> worldMatrix,
        Matrix4X4<float> viewMatrix,
        Matrix4X4<float> projectionMatrix,
        float[] lightDirection,
        float[] diffuseColor,
        float[] cameraPosition,
        float[] specularColor,
        float specularPower
    )
    {
        var context = DirectX.DeviceContext;

        worldMatrix = Matrix4X4.Transpose(worldMatrix);
        viewMatrix = Matrix4X4.Transpose(viewMatrix);
        projectionMatrix = Matrix4X4.Transpose(projectionMatrix);

        MappedSubresource mappedResource;
        SilkMarshal.ThrowHResult(
            context.Map(_matrixBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        var mp = (MatrixBufferType*)mappedResource.PData;
        mp->world = worldMatrix;
        mp->view = viewMatrix;
        mp->projection = projectionMatrix;
        context.Unmap(_matrixBuffer, 0);

        var mcb = _matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &mcb);

        SilkMarshal.ThrowHResult(
            context.Map(_cameraBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        var cp = (CameraBufferType*)mappedResource.PData;
        cp->camX = cameraPosition[0];
        cp->camY = cameraPosition[1];
        cp->camZ = cameraPosition[2];
        cp->padding = 0;
        context.Unmap(_cameraBuffer, 0);

        var ccb = _cameraBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(1, 1, &ccb);

        SilkMarshal.ThrowHResult(
            context.Map(_lightBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        var lp = (LightBufferType*)mappedResource.PData;
        lp->diffuseR = diffuseColor[0];
        lp->diffuseG = diffuseColor[1];
        lp->diffuseB = diffuseColor[2];
        lp->diffuseA = diffuseColor[3];
        lp->specularR = specularColor[0];
        lp->specularG = specularColor[1];
        lp->specularB = specularColor[2];
        lp->specularA = specularColor[3];
        lp->specularPower = specularPower;
        lp->lightDirX = lightDirection[0];
        lp->lightDirY = lightDirection[1];
        lp->lightDirZ = lightDirection[2];
        context.Unmap(_lightBuffer, 0);

        var lcb = _lightBuffer.GetPinnableReference();
        context.PSSetConstantBuffers(0, 1, &lcb);

        return true;
    }

    private void RenderShader(DX11 DirectX, int indexCount)
    {
        var context = DirectX.DeviceContext;
        context.IASetInputLayout(_layout);
        context.VSSetShader(_vertexShader, null, 0);
        context.PSSetShader(_pixelShader, null, 0);
        context.DrawIndexed((uint)indexCount, 0, 0);
    }
}

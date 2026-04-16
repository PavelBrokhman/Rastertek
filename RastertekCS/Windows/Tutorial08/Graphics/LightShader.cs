using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial08.Graphics;

public unsafe class LightShader
{
    private struct MatrixBufferType
    {
        public Matrix4X4<float> world;
        public Matrix4X4<float> view;
        public Matrix4X4<float> projection;
    }

    private struct LightBufferType
    {
        public float diffuseColorR,
            diffuseColorG,
            diffuseColorB,
            diffuseColorA;
        public float lightDirX,
            lightDirY,
            lightDirZ;
        public float padding;
    }

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11InputLayout> _layout;
    private ComPtr<ID3D11Buffer> _matrixBuffer;
    private ComPtr<ID3D11Buffer> _lightBuffer;

    public bool Initialize(DX11 DirectX)
    {
        return InitializeShader(DirectX, "Shaders/Light.vs", "Shaders/Light.ps");
    }

    public void Shutdown()
    {
        _lightBuffer.Release();
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
        float[] diffuseLightColor
    )
    {
        if (
            !SetShaderParameters(
                DirectX,
                worldMatrix,
                viewMatrix,
                projectionMatrix,
                lightDirection,
                diffuseLightColor
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

        // Compile vertex shader.
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
                    (byte*)SilkMarshal.StringToPtr("LightVertexShader", NativeStringEncoding.Ansi),
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

        // Compile pixel shader.
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
                    (byte*)SilkMarshal.StringToPtr("LightPixelShader", NativeStringEncoding.Ansi),
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

        // Create the input layout — position + texcoord + normal.
        var posName = SilkMarshal.StringToPtr("POSITION", NativeStringEncoding.Ansi);
        var texName = SilkMarshal.StringToPtr("TEXCOORD", NativeStringEncoding.Ansi);
        var normalName = SilkMarshal.StringToPtr("NORMAL", NativeStringEncoding.Ansi);

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
        vsBlob.Release();
        psBlob.Release();

        // Create matrix constant buffer (VS slot 0).
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

        // Create light constant buffer (PS slot 0).
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
        float[] diffuseLightColor
    )
    {
        var context = DirectX.DeviceContext;

        // Transpose matrices for DirectX.
        worldMatrix = Matrix4X4.Transpose(worldMatrix);
        viewMatrix = Matrix4X4.Transpose(viewMatrix);
        projectionMatrix = Matrix4X4.Transpose(projectionMatrix);

        // Update matrix buffer.
        MappedSubresource mappedResource;
        SilkMarshal.ThrowHResult(
            context.Map(_matrixBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        var matrixPtr = (MatrixBufferType*)mappedResource.PData;
        matrixPtr->world = worldMatrix;
        matrixPtr->view = viewMatrix;
        matrixPtr->projection = projectionMatrix;
        context.Unmap(_matrixBuffer, 0);

        var cb = _matrixBuffer.GetPinnableReference();
        context.VSSetConstantBuffers(0, 1, &cb);

        // Update light buffer.
        SilkMarshal.ThrowHResult(
            context.Map(_lightBuffer, 0, Map.WriteDiscard, 0, &mappedResource)
        );
        var lightPtr = (LightBufferType*)mappedResource.PData;
        lightPtr->diffuseColorR = diffuseLightColor[0];
        lightPtr->diffuseColorG = diffuseLightColor[1];
        lightPtr->diffuseColorB = diffuseLightColor[2];
        lightPtr->diffuseColorA = diffuseLightColor[3];
        lightPtr->lightDirX = lightDirection[0];
        lightPtr->lightDirY = lightDirection[1];
        lightPtr->lightDirZ = lightDirection[2];
        lightPtr->padding = 0.0f;
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

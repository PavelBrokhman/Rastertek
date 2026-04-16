using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial25.Graphics;

public unsafe class RenderTexture
{
    private ComPtr<ID3D11Texture2D> _renderTargetTexture;
    private ComPtr<ID3D11RenderTargetView> _renderTargetView;
    private ComPtr<ID3D11ShaderResourceView> _shaderResourceView;
    private ComPtr<ID3D11Texture2D> _depthStencilBuffer;
    private ComPtr<ID3D11DepthStencilView> _depthStencilView;
    private Viewport _viewport;
    private Matrix4X4<float> _projectionMatrix;
    private int _textureWidth,
        _textureHeight;

    public bool Initialize(
        DX11 DirectX,
        int textureWidth,
        int textureHeight,
        float screenDepth,
        float screenNear
    )
    {
        var device = DirectX.Device;
        _textureWidth = textureWidth;
        _textureHeight = textureHeight;

        var texDesc = new Texture2DDesc
        {
            Width = (uint)textureWidth,
            Height = (uint)textureHeight,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.FormatR8G8B8A8Unorm,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.RenderTarget | BindFlag.ShaderResource),
            CPUAccessFlags = 0,
            MiscFlags = 0,
        };
        SilkMarshal.ThrowHResult(device.CreateTexture2D(&texDesc, null, ref _renderTargetTexture));

        var rtvDesc = new RenderTargetViewDesc
        {
            Format = Format.FormatR8G8B8A8Unorm,
            ViewDimension = RtvDimension.Texture2D,
            Texture2D = new Tex2DRtv { MipSlice = 0 },
        };
        SilkMarshal.ThrowHResult(
            device.CreateRenderTargetView(_renderTargetTexture, &rtvDesc, ref _renderTargetView)
        );

        var srvDesc = new ShaderResourceViewDesc
        {
            Format = Format.FormatR8G8B8A8Unorm,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
            Texture2D = new Tex2DSrv { MostDetailedMip = 0, MipLevels = 1 },
        };
        SilkMarshal.ThrowHResult(
            device.CreateShaderResourceView(_renderTargetTexture, &srvDesc, ref _shaderResourceView)
        );

        var depthDesc = new Texture2DDesc
        {
            Width = (uint)textureWidth,
            Height = (uint)textureHeight,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.FormatD24UnormS8Uint,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.DepthStencil,
            CPUAccessFlags = 0,
            MiscFlags = 0,
        };
        SilkMarshal.ThrowHResult(device.CreateTexture2D(&depthDesc, null, ref _depthStencilBuffer));

        var dsvDesc = new DepthStencilViewDesc
        {
            Format = Format.FormatD24UnormS8Uint,
            ViewDimension = DsvDimension.Texture2D,
            Texture2D = new Tex2DDsv { MipSlice = 0 },
        };
        SilkMarshal.ThrowHResult(
            device.CreateDepthStencilView(_depthStencilBuffer, &dsvDesc, ref _depthStencilView)
        );

        _viewport = new Viewport
        {
            TopLeftX = 0,
            TopLeftY = 0,
            Width = textureWidth,
            Height = textureHeight,
            MinDepth = 0.0f,
            MaxDepth = 1.0f,
        };

        _projectionMatrix = DXMath.PerspectiveFovLH(
            MathF.PI / 4.0f,
            (float)textureWidth / textureHeight,
            screenNear,
            screenDepth
        );

        return true;
    }

    public void Shutdown()
    {
        _depthStencilView.Release();
        _depthStencilBuffer.Release();
        _shaderResourceView.Release();
        _renderTargetView.Release();
        _renderTargetTexture.Release();
    }

    public void SetRenderTarget(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        var rtv = _renderTargetView.GetPinnableReference();
        context.OMSetRenderTargets(1, &rtv, _depthStencilView);
        var vp = _viewport;
        context.RSSetViewports(1, &vp);
    }

    public void ClearRenderTarget(DX11 DirectX, float r, float g, float b, float a)
    {
        var context = DirectX.DeviceContext;
        float* color = stackalloc float[4] { r, g, b, a };
        context.ClearRenderTargetView(_renderTargetView, color);
        context.ClearDepthStencilView(_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
    }

    public ComPtr<ID3D11ShaderResourceView> GetShaderResourceView() => _shaderResourceView;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

    public int GetTextureWidth() => _textureWidth;

    public int GetTextureHeight() => _textureHeight;
}

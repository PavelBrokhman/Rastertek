using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial51.Graphics;

public unsafe class DeferredBuffers
{
    // deferredbuffersclass.h: positions, normals, colours.
    public const int BufferCount = 3;

    private ComPtr<ID3D11Texture2D>[] _renderTargetTextures = new ComPtr<ID3D11Texture2D>[BufferCount];
    private ComPtr<ID3D11RenderTargetView>[] _renderTargetViews = new ComPtr<ID3D11RenderTargetView>[BufferCount];
    private ComPtr<ID3D11ShaderResourceView>[] _shaderResourceViews = new ComPtr<ID3D11ShaderResourceView>[BufferCount];
    private ComPtr<ID3D11Texture2D> _depthStencilBuffer;
    private ComPtr<ID3D11DepthStencilView> _depthStencilView;
    private Viewport _viewport;
    private int _textureWidth, _textureHeight;

    public bool Initialize(DX11 DirectX, int textureWidth, int textureHeight, float screenDepth, float screenNear)
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
            Format = Format.FormatR32G32B32A32Float,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.RenderTarget | BindFlag.ShaderResource),
            CPUAccessFlags = 0,
            MiscFlags = 0,
        };
        for (int i = 0; i < BufferCount; i++)
            SilkMarshal.ThrowHResult(device.CreateTexture2D(&texDesc, null, ref _renderTargetTextures[i]));

        var rtvDesc = new RenderTargetViewDesc
        {
            Format = texDesc.Format,
            ViewDimension = RtvDimension.Texture2D,
            Texture2D = new Tex2DRtv { MipSlice = 0 },
        };
        for (int i = 0; i < BufferCount; i++)
            SilkMarshal.ThrowHResult(device.CreateRenderTargetView(_renderTargetTextures[i], &rtvDesc, ref _renderTargetViews[i]));

        var srvDesc = new ShaderResourceViewDesc
        {
            Format = texDesc.Format,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
            Texture2D = new Tex2DSrv { MostDetailedMip = 0, MipLevels = 1 },
        };
        for (int i = 0; i < BufferCount; i++)
            SilkMarshal.ThrowHResult(device.CreateShaderResourceView(_renderTargetTextures[i], &srvDesc, ref _shaderResourceViews[i]));

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
        };
        SilkMarshal.ThrowHResult(device.CreateTexture2D(&depthDesc, null, ref _depthStencilBuffer));

        var dsvDesc = new DepthStencilViewDesc
        {
            Format = Format.FormatD24UnormS8Uint,
            ViewDimension = DsvDimension.Texture2D,
            Texture2D = new Tex2DDsv { MipSlice = 0 },
        };
        SilkMarshal.ThrowHResult(device.CreateDepthStencilView(_depthStencilBuffer, &dsvDesc, ref _depthStencilView));

        _viewport = new Viewport
        {
            TopLeftX = 0, TopLeftY = 0,
            Width = textureWidth, Height = textureHeight,
            MinDepth = 0.0f, MaxDepth = 1.0f,
        };
        return true;
    }

    public void Shutdown()
    {
        _depthStencilView.Release();
        _depthStencilBuffer.Release();
        for (int i = 0; i < BufferCount; i++)
        {
            _shaderResourceViews[i].Release();
            _renderTargetViews[i].Release();
            _renderTargetTextures[i].Release();
        }
    }

    public void SetRenderTargets(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        ID3D11RenderTargetView** rtvs = stackalloc ID3D11RenderTargetView*[BufferCount];
        for (int i = 0; i < BufferCount; i++) rtvs[i] = _renderTargetViews[i].GetPinnableReference();
        context.OMSetRenderTargets(BufferCount, rtvs, _depthStencilView);
        var vp = _viewport;
        context.RSSetViewports(1, &vp);
    }

    public void ClearRenderTargets(DX11 DirectX, float r, float g, float b, float a)
    {
        var context = DirectX.DeviceContext;
        float* color = stackalloc float[4] { r, g, b, a };
        for (int i = 0; i < BufferCount; i++)
            context.ClearRenderTargetView(_renderTargetViews[i], color);
        context.ClearDepthStencilView(_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
    }

    public ComPtr<ID3D11ShaderResourceView> GetShaderResourceView(int index) => _shaderResourceViews[index];

    public ComPtr<ID3D11ShaderResourceView> GetShaderResourcePositions() => _shaderResourceViews[0];

    public ComPtr<ID3D11ShaderResourceView> GetShaderResourceNormals() => _shaderResourceViews[1];

    public ComPtr<ID3D11ShaderResourceView> GetShaderResourceColors() => _shaderResourceViews[2];
}

using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace RastertekCS.Windows.Tutorial32.Graphics;

public unsafe class RenderTexture
{
    private ComPtr<ID3D11Texture2D> m_renderTargetTexture;
    private ComPtr<ID3D11RenderTargetView> m_renderTargetView;
    private ComPtr<ID3D11ShaderResourceView> m_shaderResourceView;
    private ComPtr<ID3D11Texture2D> m_depthStencilBuffer;
    private ComPtr<ID3D11DepthStencilView> m_depthStencilView;
    private Viewport m_viewport;
    private Matrix4X4<float> m_projectionMatrix;
    private int m_textureWidth, m_textureHeight;

    public bool Initialize(DX11 DirectX, int textureWidth, int textureHeight, float screenDepth, float screenNear)
    {
        var device = DirectX.Device;
        m_textureWidth = textureWidth;
        m_textureHeight = textureHeight;

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
            CPUAccessFlags = 0, MiscFlags = 0
        };
        SilkMarshal.ThrowHResult(
            device.CreateTexture2D(&texDesc, null, ref m_renderTargetTexture));

        var rtvDesc = new RenderTargetViewDesc
        {
            Format = Format.FormatR8G8B8A8Unorm,
            ViewDimension = RtvDimension.Texture2D,
            Texture2D = new Tex2DRtv { MipSlice = 0 }
        };
        SilkMarshal.ThrowHResult(
            device.CreateRenderTargetView(m_renderTargetTexture, &rtvDesc, ref m_renderTargetView));

        var srvDesc = new ShaderResourceViewDesc
        {
            Format = Format.FormatR8G8B8A8Unorm,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
            Texture2D = new Tex2DSrv { MostDetailedMip = 0, MipLevels = 1 }
        };
        SilkMarshal.ThrowHResult(
            device.CreateShaderResourceView(m_renderTargetTexture, &srvDesc, ref m_shaderResourceView));

        var depthDesc = new Texture2DDesc
        {
            Width = (uint)textureWidth,
            Height = (uint)textureHeight,
            MipLevels = 1, ArraySize = 1,
            Format = Format.FormatD24UnormS8Uint,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.DepthStencil,
            CPUAccessFlags = 0, MiscFlags = 0
        };
        SilkMarshal.ThrowHResult(
            device.CreateTexture2D(&depthDesc, null, ref m_depthStencilBuffer));

        var dsvDesc = new DepthStencilViewDesc
        {
            Format = Format.FormatD24UnormS8Uint,
            ViewDimension = DsvDimension.Texture2D,
            Texture2D = new Tex2DDsv { MipSlice = 0 }
        };
        SilkMarshal.ThrowHResult(
            device.CreateDepthStencilView(m_depthStencilBuffer, &dsvDesc, ref m_depthStencilView));

        m_viewport = new Viewport
        {
            TopLeftX = 0, TopLeftY = 0,
            Width = textureWidth, Height = textureHeight,
            MinDepth = 0.0f, MaxDepth = 1.0f
        };

        m_projectionMatrix = DXMath.PerspectiveFovLH(MathF.PI / 4.0f,
            (float)textureWidth / textureHeight, screenNear, screenDepth);

        return true;
    }

    public void Shutdown()
    {
        m_depthStencilView.Release();
        m_depthStencilBuffer.Release();
        m_shaderResourceView.Release();
        m_renderTargetView.Release();
        m_renderTargetTexture.Release();
    }

    public void SetRenderTarget(DX11 DirectX)
    {
        var context = DirectX.DeviceContext;
        var rtv = m_renderTargetView.GetPinnableReference();
        context.OMSetRenderTargets(1, &rtv, m_depthStencilView);
        var vp = m_viewport;
        context.RSSetViewports(1, &vp);
    }

    public void ClearRenderTarget(DX11 DirectX, float r, float g, float b, float a)
    {
        var context = DirectX.DeviceContext;
        float* color = stackalloc float[4] { r, g, b, a };
        context.ClearRenderTargetView(m_renderTargetView, color);
        context.ClearDepthStencilView(m_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
    }

    public ComPtr<ID3D11ShaderResourceView> GetShaderResourceView() => m_shaderResourceView;
    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;
    public int GetTextureWidth() => m_textureWidth;
    public int GetTextureHeight() => m_textureHeight;
}

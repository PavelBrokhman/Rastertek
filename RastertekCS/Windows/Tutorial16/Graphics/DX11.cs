using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.Windows.Tutorial16.Graphics;

public unsafe class DX11
{
    private D3D11 m_d3d11;
    private DXGI m_dxgi;

    private ComPtr<ID3D11Device> m_device;
    private ComPtr<ID3D11DeviceContext> m_deviceContext;
    private ComPtr<IDXGISwapChain> m_swapChain;
    private ComPtr<ID3D11RenderTargetView> m_renderTargetView;
    private ComPtr<ID3D11Texture2D> m_depthStencilBuffer;
    private ComPtr<ID3D11DepthStencilState> m_depthStencilState;
    private ComPtr<ID3D11DepthStencilState> m_depthDisabledStencilState;
    private ComPtr<ID3D11DepthStencilView> m_depthStencilView;
    private ComPtr<ID3D11RasterizerState> m_rasterState;
    private ComPtr<ID3D11BlendState> m_alphaEnableBlendingState;
    private ComPtr<ID3D11BlendState> m_alphaDisableBlendingState;

    private Matrix4X4<float> m_worldMatrix;
    private Matrix4X4<float> m_projectionMatrix;
    private Matrix4X4<float> m_orthoMatrix;
    private string m_videoCardDescription;
    private bool m_vsyncEnabled;

    public ComPtr<ID3D11Device> Device => m_device;
    public ComPtr<ID3D11DeviceContext> DeviceContext => m_deviceContext;

    public bool Initialize(IWindow window, int screenWidth, int screenHeight,
                           float screenDepth, float screenNear, bool vsync)
    {
        m_vsyncEnabled = vsync;
        m_d3d11 = D3D11.GetApi();
        m_dxgi = DXGI.GetApi();

        nint hwnd = window.Native!.Win32!.Value.Hwnd;

        ComPtr<IDXGIFactory> factory = default;
        m_dxgi.CreateDXGIFactory(SilkMarshal.GuidPtrOf<IDXGIFactory>(), (void**)&factory);

        IDXGIAdapter* pAdapter = null;
        factory.EnumAdapters(0, &pAdapter);
        ComPtr<IDXGIAdapter> adapter = pAdapter;

        AdapterDesc adapterDesc;
        adapter.GetDesc(&adapterDesc);
        m_videoCardDescription = new string((char*)adapterDesc.Description);

        adapter.Release();
        factory.Release();

        var swapChainDesc = new SwapChainDesc
        {
            BufferDesc = new ModeDesc
            {
                Width = (uint)screenWidth, Height = (uint)screenHeight,
                RefreshRate = new Rational(0, 1),
                Format = Format.FormatR8G8B8A8Unorm,
                ScanlineOrdering = ModeScanlineOrder.Unspecified,
                Scaling = ModeScaling.Unspecified
            },
            SampleDesc = new SampleDesc(1, 0),
            BufferUsage = DXGI.UsageRenderTargetOutput,
            BufferCount = 1, OutputWindow = hwnd, Windowed = true,
            SwapEffect = SwapEffect.Discard, Flags = 0
        };

        var featureLevel = D3DFeatureLevel.Level110;
        IDXGISwapChain* pSwapChain = null;
        ID3D11Device* pDevice = null;
        ID3D11DeviceContext* pDeviceContext = null;
        SilkMarshal.ThrowHResult(
            m_d3d11.CreateDeviceAndSwapChain(
                (IDXGIAdapter*)null, D3DDriverType.Hardware, 0, 0,
                &featureLevel, 1, D3D11.SdkVersion,
                &swapChainDesc, &pSwapChain, &pDevice, null, &pDeviceContext));
        m_swapChain = pSwapChain;
        m_device = pDevice;
        m_deviceContext = pDeviceContext;

        ComPtr<ID3D11Texture2D> backBuffer = default;
        SilkMarshal.ThrowHResult(
            m_swapChain.GetBuffer(0, SilkMarshal.GuidPtrOf<ID3D11Texture2D>(), (void**)&backBuffer));
        SilkMarshal.ThrowHResult(
            m_device.CreateRenderTargetView(backBuffer, null, ref m_renderTargetView));
        backBuffer.Release();

        var depthBufferDesc = new Texture2DDesc
        {
            Width = (uint)screenWidth, Height = (uint)screenHeight,
            MipLevels = 1, ArraySize = 1,
            Format = Format.FormatD24UnormS8Uint,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.DepthStencil, CPUAccessFlags = 0, MiscFlags = 0
        };
        SilkMarshal.ThrowHResult(
            m_device.CreateTexture2D(&depthBufferDesc, null, ref m_depthStencilBuffer));

        var depthStencilDesc = new DepthStencilDesc
        {
            DepthEnable = true, DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.Less,
            StencilEnable = true, StencilReadMask = 0xFF, StencilWriteMask = 0xFF,
            FrontFace = new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep, StencilDepthFailOp = StencilOp.Incr,
                StencilPassOp = StencilOp.Keep, StencilFunc = ComparisonFunc.Always
            },
            BackFace = new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep, StencilDepthFailOp = StencilOp.Decr,
                StencilPassOp = StencilOp.Keep, StencilFunc = ComparisonFunc.Always
            }
        };
        SilkMarshal.ThrowHResult(
            m_device.CreateDepthStencilState(&depthStencilDesc, ref m_depthStencilState));
        m_deviceContext.OMSetDepthStencilState(m_depthStencilState, 1);

        var depthDisabledStencilDesc = new DepthStencilDesc
        {
            DepthEnable = false, DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.Less,
            StencilEnable = true, StencilReadMask = 0xFF, StencilWriteMask = 0xFF,
            FrontFace = new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep, StencilDepthFailOp = StencilOp.Incr,
                StencilPassOp = StencilOp.Keep, StencilFunc = ComparisonFunc.Always
            },
            BackFace = new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep, StencilDepthFailOp = StencilOp.Decr,
                StencilPassOp = StencilOp.Keep, StencilFunc = ComparisonFunc.Always
            }
        };
        SilkMarshal.ThrowHResult(
            m_device.CreateDepthStencilState(&depthDisabledStencilDesc, ref m_depthDisabledStencilState));

        var dsvDesc = new DepthStencilViewDesc
        {
            Format = Format.FormatD24UnormS8Uint,
            ViewDimension = DsvDimension.Texture2D,
            Texture2D = new Tex2DDsv { MipSlice = 0 }
        };
        SilkMarshal.ThrowHResult(
            m_device.CreateDepthStencilView(m_depthStencilBuffer, &dsvDesc, ref m_depthStencilView));

        var rtv = m_renderTargetView.GetPinnableReference();
        m_deviceContext.OMSetRenderTargets(1, &rtv, m_depthStencilView);

        var rasterDesc = new RasterizerDesc
        {
            AntialiasedLineEnable = false, CullMode = CullMode.Back,
            DepthBias = 0, DepthBiasClamp = 0.0f, DepthClipEnable = true,
            FillMode = FillMode.Solid, FrontCounterClockwise = false,
            MultisampleEnable = false, ScissorEnable = false, SlopeScaledDepthBias = 0.0f
        };
        SilkMarshal.ThrowHResult(
            m_device.CreateRasterizerState(&rasterDesc, ref m_rasterState));
        m_deviceContext.RSSetState(m_rasterState);

        var viewport = new Viewport
        {
            TopLeftX = 0, TopLeftY = 0,
            Width = screenWidth, Height = screenHeight,
            MinDepth = 0.0f, MaxDepth = 1.0f
        };
        m_deviceContext.RSSetViewports(1, &viewport);

        m_worldMatrix = Matrix4X4<float>.Identity;
        float fov = MathF.PI / 4.0f;
        float aspect = (float)screenWidth / screenHeight;
        m_projectionMatrix = DXMath.PerspectiveFovLH(fov, aspect, screenNear, screenDepth);
        m_orthoMatrix = DXMath.OrthographicLH(screenWidth, screenHeight, screenNear, screenDepth);

        var blendStateDesc = new BlendDesc { AlphaToCoverageEnable = false, IndependentBlendEnable = false };
        blendStateDesc.RenderTarget[0] = new RenderTargetBlendDesc
        {
            BlendEnable = true,
            SrcBlend = Blend.One,
            DestBlend = Blend.InvSrcAlpha,
            BlendOp = BlendOp.Add,
            SrcBlendAlpha = Blend.One,
            DestBlendAlpha = Blend.Zero,
            BlendOpAlpha = BlendOp.Add,
            RenderTargetWriteMask = (byte)ColorWriteEnable.All
        };
        SilkMarshal.ThrowHResult(
            m_device.CreateBlendState(&blendStateDesc, ref m_alphaEnableBlendingState));

        blendStateDesc.RenderTarget[0].BlendEnable = false;
        SilkMarshal.ThrowHResult(
            m_device.CreateBlendState(&blendStateDesc, ref m_alphaDisableBlendingState));

        return true;
    }

    public void Shutdown()
    {
        m_deviceContext.ClearState();
        m_alphaDisableBlendingState.Release();
        m_alphaEnableBlendingState.Release();
        m_rasterState.Release();
        m_depthStencilView.Release();
        m_depthDisabledStencilState.Release();
        m_depthStencilState.Release();
        m_depthStencilBuffer.Release();
        m_renderTargetView.Release();
        m_swapChain.Release();
        m_deviceContext.Release();
        m_device.Release();
        m_dxgi?.Dispose();
        m_d3d11?.Dispose();
    }

    public void BeginScene(float red, float green, float blue, float alpha)
    {
        float* color = stackalloc float[4] { red, green, blue, alpha };
        m_deviceContext.ClearRenderTargetView(m_renderTargetView, color);
        m_deviceContext.ClearDepthStencilView(m_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
    }

    public void EndScene()
    {
        m_swapChain.Present(m_vsyncEnabled ? 1u : 0u, 0);
    }

    public Matrix4X4<float> GetWorldMatrix() => m_worldMatrix;
    public Matrix4X4<float> GetProjectionMatrix() => m_projectionMatrix;
    public Matrix4X4<float> GetOrthoMatrix() => m_orthoMatrix;
    public string GetVideoCardInfo() => m_videoCardDescription;

    public void TurnZBufferOn() => m_deviceContext.OMSetDepthStencilState(m_depthStencilState, 1);
    public void TurnZBufferOff() => m_deviceContext.OMSetDepthStencilState(m_depthDisabledStencilState, 1);

    public void EnableAlphaBlending()
    {
        float* blendFactor = stackalloc float[4] { 0, 0, 0, 0 };
        m_deviceContext.OMSetBlendState(m_alphaEnableBlendingState, blendFactor, 0xffffffff);
    }

    public void DisableAlphaBlending()
    {
        float* blendFactor = stackalloc float[4] { 0, 0, 0, 0 };
        m_deviceContext.OMSetBlendState(m_alphaDisableBlendingState, blendFactor, 0xffffffff);
    }
}

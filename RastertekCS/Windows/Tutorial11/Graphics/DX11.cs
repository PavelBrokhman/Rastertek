using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RastertekCS.Windows.Tutorial11.Graphics;

public unsafe class DX11
{
    private D3D11 _d3d11;
    private DXGI _dxgi;

    private ComPtr<ID3D11Device> _device;
    private ComPtr<ID3D11DeviceContext> _deviceContext;
    private ComPtr<IDXGISwapChain> _swapChain;
    private ComPtr<ID3D11RenderTargetView> _renderTargetView;
    private ComPtr<ID3D11Texture2D> _depthStencilBuffer;
    private ComPtr<ID3D11DepthStencilState> _depthStencilState;
    private ComPtr<ID3D11DepthStencilView> _depthStencilView;
    private ComPtr<ID3D11RasterizerState> _rasterState;

    private Matrix4X4<float> _worldMatrix;
    private Matrix4X4<float> _projectionMatrix;
    private string _videoCardDescription;
    private bool _vsyncEnabled;

    public ComPtr<ID3D11Device> Device => _device;
    public ComPtr<ID3D11DeviceContext> DeviceContext => _deviceContext;

    public bool Initialize(
        IWindow window,
        int screenWidth,
        int screenHeight,
        float screenDepth,
        float screenNear,
        bool vsync
    )
    {
        _vsyncEnabled = vsync;
        _d3d11 = D3D11.GetApi();
        _dxgi = DXGI.GetApi();

        nint hwnd = window.Native!.Win32!.Value.Hwnd;

        ComPtr<IDXGIFactory> factory = default;
        _dxgi.CreateDXGIFactory(SilkMarshal.GuidPtrOf<IDXGIFactory>(), (void**)&factory);

        IDXGIAdapter* pAdapter = null;
        factory.EnumAdapters(0, &pAdapter);
        ComPtr<IDXGIAdapter> adapter = pAdapter;

        AdapterDesc adapterDesc;
        adapter.GetDesc(&adapterDesc);
        _videoCardDescription = new string((char*)adapterDesc.Description);

        adapter.Release();
        factory.Release();

        var swapChainDesc = new SwapChainDesc
        {
            BufferDesc = new ModeDesc
            {
                Width = (uint)screenWidth,
                Height = (uint)screenHeight,
                RefreshRate = new Rational(0, 1),
                Format = Format.FormatR8G8B8A8Unorm,
                ScanlineOrdering = ModeScanlineOrder.Unspecified,
                Scaling = ModeScaling.Unspecified,
            },
            SampleDesc = new SampleDesc(1, 0),
            BufferUsage = DXGI.UsageRenderTargetOutput,
            BufferCount = 1,
            OutputWindow = hwnd,
            Windowed = true,
            SwapEffect = SwapEffect.Discard,
            Flags = 0,
        };

        var featureLevel = D3DFeatureLevel.Level110;
        IDXGISwapChain* pSwapChain = null;
        ID3D11Device* pDevice = null;
        ID3D11DeviceContext* pDeviceContext = null;
        SilkMarshal.ThrowHResult(
            _d3d11.CreateDeviceAndSwapChain(
                (IDXGIAdapter*)null,
                D3DDriverType.Hardware,
                0,
                0,
                &featureLevel,
                1,
                D3D11.SdkVersion,
                &swapChainDesc,
                &pSwapChain,
                &pDevice,
                null,
                &pDeviceContext
            )
        );
        _swapChain = pSwapChain;
        _device = pDevice;
        _deviceContext = pDeviceContext;

        ComPtr<ID3D11Texture2D> backBuffer = default;
        SilkMarshal.ThrowHResult(
            _swapChain.GetBuffer(0, SilkMarshal.GuidPtrOf<ID3D11Texture2D>(), (void**)&backBuffer)
        );
        SilkMarshal.ThrowHResult(
            _device.CreateRenderTargetView(backBuffer, null, ref _renderTargetView)
        );
        backBuffer.Release();

        var depthBufferDesc = new Texture2DDesc
        {
            Width = (uint)screenWidth,
            Height = (uint)screenHeight,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.FormatD24UnormS8Uint,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.DepthStencil,
            CPUAccessFlags = 0,
            MiscFlags = 0,
        };
        SilkMarshal.ThrowHResult(
            _device.CreateTexture2D(&depthBufferDesc, null, ref _depthStencilBuffer)
        );

        var depthStencilDesc = new DepthStencilDesc
        {
            DepthEnable = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.Less,
            StencilEnable = true,
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            FrontFace = new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep,
                StencilDepthFailOp = StencilOp.Incr,
                StencilPassOp = StencilOp.Keep,
                StencilFunc = ComparisonFunc.Always,
            },
            BackFace = new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep,
                StencilDepthFailOp = StencilOp.Decr,
                StencilPassOp = StencilOp.Keep,
                StencilFunc = ComparisonFunc.Always,
            },
        };
        SilkMarshal.ThrowHResult(
            _device.CreateDepthStencilState(&depthStencilDesc, ref _depthStencilState)
        );
        _deviceContext.OMSetDepthStencilState(_depthStencilState, 1);

        var dsvDesc = new DepthStencilViewDesc
        {
            Format = Format.FormatD24UnormS8Uint,
            ViewDimension = DsvDimension.Texture2D,
            Texture2D = new Tex2DDsv { MipSlice = 0 },
        };
        SilkMarshal.ThrowHResult(
            _device.CreateDepthStencilView(_depthStencilBuffer, &dsvDesc, ref _depthStencilView)
        );

        var rtv = _renderTargetView.GetPinnableReference();
        _deviceContext.OMSetRenderTargets(1, &rtv, _depthStencilView);

        var rasterDesc = new RasterizerDesc
        {
            AntialiasedLineEnable = false,
            CullMode = CullMode.Back,
            DepthBias = 0,
            DepthBiasClamp = 0.0f,
            DepthClipEnable = true,
            FillMode = FillMode.Solid,
            FrontCounterClockwise = false,
            MultisampleEnable = false,
            ScissorEnable = false,
            SlopeScaledDepthBias = 0.0f,
        };
        SilkMarshal.ThrowHResult(_device.CreateRasterizerState(&rasterDesc, ref _rasterState));
        _deviceContext.RSSetState(_rasterState);

        var viewport = new Viewport
        {
            TopLeftX = 0,
            TopLeftY = 0,
            Width = screenWidth,
            Height = screenHeight,
            MinDepth = 0.0f,
            MaxDepth = 1.0f,
        };
        _deviceContext.RSSetViewports(1, &viewport);

        _worldMatrix = Matrix4X4<float>.Identity;
        float fov = MathF.PI / 4.0f;
        float aspect = (float)screenWidth / screenHeight;
        _projectionMatrix = DXMath.PerspectiveFovLH(fov, aspect, screenNear, screenDepth);

        return true;
    }

    public void Shutdown()
    {
        _deviceContext.ClearState();
        _rasterState.Release();
        _depthStencilView.Release();
        _depthStencilState.Release();
        _depthStencilBuffer.Release();
        _renderTargetView.Release();
        _swapChain.Release();
        _deviceContext.Release();
        _device.Release();
        _dxgi?.Dispose();
        _d3d11?.Dispose();
    }

    public void BeginScene(float red, float green, float blue, float alpha)
    {
        float* color = stackalloc float[4] { red, green, blue, alpha };
        _deviceContext.ClearRenderTargetView(_renderTargetView, color);
        _deviceContext.ClearDepthStencilView(_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
    }

    public void EndScene()
    {
        _swapChain.Present(_vsyncEnabled ? 1u : 0u, 0);
    }

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

    public string GetVideoCardInfo() => _videoCardDescription;
}

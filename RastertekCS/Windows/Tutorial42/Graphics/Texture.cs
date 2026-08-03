using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial42.Graphics;

public unsafe class Texture
{
    private ComPtr<ID3D11Texture2D> _texture;
    private ComPtr<ID3D11ShaderResourceView> _textureView;
    private ComPtr<ID3D11SamplerState> _samplerState;
    private bool _loaded;

    public bool Initialize(DX11 DirectX, string filename, bool wrap)
    {
        var device = DirectX.Device;
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Texture not found: {filename}");
            return false;
        }
        if (!LoadTga(filename, out int width, out int height, out byte[] pixels))
        {
            Console.WriteLine($"Could not load texture: {filename}");
            return false;
        }
        var textureDesc = new Texture2DDesc
        {
            Width = (uint)width,
            Height = (uint)height,
            MipLevels = 0,
            ArraySize = 1,
            Format = Format.FormatR8G8B8A8Unorm,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.ShaderResource | BindFlag.RenderTarget),
            CPUAccessFlags = 0,
            MiscFlags = (uint)ResourceMiscFlag.GenerateMips,
        };
        // Like textureclass.cpp: create empty with a full mip chain, fill mip 0,
        // then let the GPU build the rest.
        SilkMarshal.ThrowHResult(device.CreateTexture2D(&textureDesc, null, ref _texture));

        fixed (byte* pPixels = pixels)
        {
            DirectX.DeviceContext.UpdateSubresource(
                _texture, 0, null, pPixels, (uint)(width * 4), 0);
        }
        var srvDesc = new ShaderResourceViewDesc
        {
            Format = Format.FormatR8G8B8A8Unorm,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
            Texture2D = new Tex2DSrv { MostDetailedMip = 0, MipLevels = unchecked((uint)-1) },
        };
        SilkMarshal.ThrowHResult(
            device.CreateShaderResourceView(_texture, &srvDesc, ref _textureView)
        );

        DirectX.DeviceContext.GenerateMips(_textureView);
        var samplerDesc = new SamplerDesc
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = wrap ? TextureAddressMode.Wrap : TextureAddressMode.Clamp,
            AddressV = wrap ? TextureAddressMode.Wrap : TextureAddressMode.Clamp,
            AddressW = wrap ? TextureAddressMode.Wrap : TextureAddressMode.Clamp,
            MipLODBias = 0.0f,
            MaxAnisotropy = 1,
            ComparisonFunc = ComparisonFunc.Always,
            MinLOD = 0,
            MaxLOD = float.MaxValue,
        };
        SilkMarshal.ThrowHResult(device.CreateSamplerState(&samplerDesc, ref _samplerState));
        _loaded = true;
        return true;
    }

    public void Shutdown()
    {
        if (_loaded)
        {
            _samplerState.Release();
            _textureView.Release();
            _texture.Release();
            _loaded = false;
        }
    }

    public ComPtr<ID3D11ShaderResourceView> GetTextureView() => _textureView;

    public void SetTexture(DX11 DirectX, uint slot)
    {
        var context = DirectX.DeviceContext;
        var srv = _textureView.GetPinnableReference();
        context.PSSetShaderResources(slot, 1, &srv);
        var sampler = _samplerState.GetPinnableReference();
        context.PSSetSamplers(slot, 1, &sampler);
    }

    private static bool LoadTga(string filename, out int width, out int height, out byte[] rgba)
    {
        width = 0;
        height = 0;
        rgba = null;
        byte[] data = File.ReadAllBytes(filename);
        if (data.Length < 18)
            return false;
        int idLength = data[0],
            imageType = data[2],
            bpp = data[16],
            descriptor = data[17];
        width = data[12] | (data[13] << 8);
        height = data[14] | (data[15] << 8);
        if (imageType != 2)
            return false;
        if (bpp != 24 && bpp != 32)
            return false;
        int offset = 18 + idLength,
            channels = bpp / 8,
            pixelCount = width * height;
        if (data.Length < offset + pixelCount * channels)
            return false;
        rgba = new byte[pixelCount * 4];
        bool topLeft = (descriptor & 0x20) != 0;
        for (int y = 0; y < height; y++)
        {
            int srcRow = topLeft ? y : (height - 1 - y);
            int srcOff = offset + srcRow * width * channels;
            int dstOff = y * width * 4;
            for (int x = 0; x < width; x++)
            {
                rgba[dstOff + x * 4 + 0] = data[srcOff + x * channels + 2];
                rgba[dstOff + x * 4 + 1] = data[srcOff + x * channels + 1];
                rgba[dstOff + x * 4 + 2] = data[srcOff + x * channels + 0];
                rgba[dstOff + x * 4 + 3] = channels == 4 ? data[srcOff + x * channels + 3] : (byte)255;
            }
        }
        return true;
    }
}

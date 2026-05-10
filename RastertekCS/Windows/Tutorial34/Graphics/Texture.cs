using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial34.Graphics;

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
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.FormatR8G8B8A8Unorm,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.ShaderResource,
            CPUAccessFlags = 0,
            MiscFlags = 0,
        };

        fixed (byte* pPixels = pixels)
        {
            var initData = new SubresourceData
            {
                PSysMem = pPixels,
                SysMemPitch = (uint)(width * 4),
                SysMemSlicePitch = 0,
            };
            SilkMarshal.ThrowHResult(device.CreateTexture2D(&textureDesc, &initData, ref _texture));
        }

        var srvDesc = new ShaderResourceViewDesc
        {
            Format = Format.FormatR8G8B8A8Unorm,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
            Texture2D = new Tex2DSrv { MostDetailedMip = 0, MipLevels = 1 },
        };
        SilkMarshal.ThrowHResult(
            device.CreateShaderResourceView(_texture, &srvDesc, ref _textureView)
        );

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
        samplerDesc.BorderColor[0] = 0;
        samplerDesc.BorderColor[1] = 0;
        samplerDesc.BorderColor[2] = 0;
        samplerDesc.BorderColor[3] = 0;
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

        int idLength = data[0];
        int imageType = data[2];
        width = data[12] | (data[13] << 8);
        height = data[14] | (data[15] << 8);
        int bpp = data[16];
        int descriptor = data[17];

        if (imageType != 2)
            return false;
        if (bpp != 24 && bpp != 32)
            return false;

        int offset = 18 + idLength;
        int channels = bpp / 8;
        int pixelCount = width * height;
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
                byte b = data[srcOff + x * channels + 0];
                byte g = data[srcOff + x * channels + 1];
                byte r = data[srcOff + x * channels + 2];
                byte a = channels == 4 ? data[srcOff + x * channels + 3] : (byte)255;

                rgba[dstOff + x * 4 + 0] = r;
                rgba[dstOff + x * 4 + 1] = g;
                rgba[dstOff + x * 4 + 2] = b;
                rgba[dstOff + x * 4 + 3] = a;
            }
        }

        return true;
    }
}

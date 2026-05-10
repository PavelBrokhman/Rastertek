using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial48.Graphics;

public unsafe class Texture
{
    private ComPtr<ID3D11Texture2D> _texture;
    private ComPtr<ID3D11ShaderResourceView> _textureView;
    private bool _loaded;
    private int _width, _height;

    public bool Initialize(DX11 DirectX, string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Texture not found: {filename}");
            return false;
        }
        if (!LoadTga32(filename, out _width, out _height, out byte[] pixels))
        {
            Console.WriteLine($"Could not load texture (32-bit only): {filename}");
            return false;
        }

        var device = DirectX.Device;
        var context = DirectX.DeviceContext;

        var textureDesc = new Texture2DDesc
        {
            Width = (uint)_width,
            Height = (uint)_height,
            MipLevels = 0,
            ArraySize = 1,
            Format = Format.FormatR8G8B8A8Unorm,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.ShaderResource | BindFlag.RenderTarget),
            CPUAccessFlags = 0,
            MiscFlags = (uint)ResourceMiscFlag.GenerateMips,
        };
        SilkMarshal.ThrowHResult(device.CreateTexture2D(&textureDesc, null, ref _texture));

        uint rowPitch = (uint)(_width * 4);
        fixed (byte* pPixels = pixels)
        {
            context.UpdateSubresource(_texture, 0, null, pPixels, rowPitch, 0);
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

        context.GenerateMips(_textureView);
        _loaded = true;
        return true;
    }

    public void Shutdown()
    {
        if (_loaded)
        {
            _textureView.Release();
            _texture.Release();
            _loaded = false;
        }
    }

    public ComPtr<ID3D11ShaderResourceView> GetTextureView() => _textureView;

    public int GetWidth() => _width;
    public int GetHeight() => _height;

    private static bool LoadTga32(string filename, out int width, out int height, out byte[] rgba)
    {
        width = 0; height = 0; rgba = null;
        byte[] data = File.ReadAllBytes(filename);
        if (data.Length < 18) return false;
        int idLength = data[0], imageType = data[2], bpp = data[16], descriptor = data[17];
        width = data[12] | (data[13] << 8);
        height = data[14] | (data[15] << 8);
        if (imageType != 2) return false;
        if (bpp != 32) return false;
        int offset = 18 + idLength, channels = 4, pixelCount = width * height;
        if (data.Length < offset + pixelCount * channels) return false;
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
                rgba[dstOff + x * 4 + 3] = data[srcOff + x * channels + 3];
            }
        }
        return true;
    }
}

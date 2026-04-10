using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace RastertekCS.Windows.Tutorial14.Graphics;

public unsafe class Texture
{
    private ComPtr<ID3D11Texture2D> m_texture;
    private ComPtr<ID3D11ShaderResourceView> m_textureView;
    private ComPtr<ID3D11SamplerState> m_samplerState;
    private bool m_loaded;
    private int m_width;
    private int m_height;

    public int GetWidth() => m_width;
    public int GetHeight() => m_height;

    public bool Initialize(DX11 DirectX, string filename, bool wrap)
    {
        var device = DirectX.Device;

        // If file is missing, generate a checkerboard TGA.
        if (!File.Exists(filename))
        {
            GenerateCheckerboardTga(filename, 64);
        }

        // Read TGA file.
        if (!LoadTga(filename, out int width, out int height, out byte[] pixels))
        {
            Console.WriteLine($"Could not load texture: {filename}");
            return false;
        }

        m_width = width;
        m_height = height;

        // Create the texture.
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
            MiscFlags = 0
        };

        fixed (byte* pPixels = pixels)
        {
            var initData = new SubresourceData
            {
                PSysMem = pPixels,
                SysMemPitch = (uint)(width * 4),
                SysMemSlicePitch = 0
            };
            SilkMarshal.ThrowHResult(
                device.CreateTexture2D(&textureDesc, &initData, ref m_texture));
        }

        // Create shader resource view.
        var srvDesc = new ShaderResourceViewDesc
        {
            Format = Format.FormatR8G8B8A8Unorm,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
            Texture2D = new Tex2DSrv { MostDetailedMip = 0, MipLevels = 1 }
        };
        SilkMarshal.ThrowHResult(
            device.CreateShaderResourceView(m_texture, &srvDesc, ref m_textureView));

        // Create sampler state.
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
            MaxLOD = float.MaxValue
        };
        samplerDesc.BorderColor[0] = 0;
        samplerDesc.BorderColor[1] = 0;
        samplerDesc.BorderColor[2] = 0;
        samplerDesc.BorderColor[3] = 0;
        SilkMarshal.ThrowHResult(
            device.CreateSamplerState(&samplerDesc, ref m_samplerState));

        m_loaded = true;
        return true;
    }

    public void Shutdown()
    {
        if (m_loaded)
        {
            m_samplerState.Release();
            m_textureView.Release();
            m_texture.Release();
            m_loaded = false;
        }
    }

    public void SetTexture(DX11 DirectX, uint slot)
    {
        var context = DirectX.DeviceContext;
        var srv = m_textureView.GetPinnableReference();
        context.PSSetShaderResources(slot, 1, &srv);
        var sampler = m_samplerState.GetPinnableReference();
        context.PSSetSamplers(slot, 1, &sampler);
    }

    private static bool LoadTga(string filename, out int width, out int height, out byte[] rgba)
    {
        width = 0; height = 0; rgba = null;

        byte[] data = File.ReadAllBytes(filename);
        if (data.Length < 18) return false;

        int idLength = data[0];
        int imageType = data[2];
        width = data[12] | (data[13] << 8);
        height = data[14] | (data[15] << 8);
        int bpp = data[16];
        int descriptor = data[17];

        if (imageType != 2) return false;
        if (bpp != 24 && bpp != 32) return false;

        int offset = 18 + idLength;
        int channels = bpp / 8;
        int pixelCount = width * height;
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

    private static void GenerateCheckerboardTga(string filename, int size)
    {
        var dir = Path.GetDirectoryName(filename);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        byte[] header = new byte[18];
        header[2] = 2;
        header[12] = (byte)(size & 0xFF);
        header[13] = (byte)((size >> 8) & 0xFF);
        header[14] = (byte)(size & 0xFF);
        header[15] = (byte)((size >> 8) & 0xFF);
        header[16] = 32;
        header[17] = 0x28;

        byte[] pixels = new byte[size * size * 4];
        int cell = size / 8;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool white = (((x / cell) + (y / cell)) & 1) == 0;
                int i = (y * size + x) * 4;
                pixels[i + 0] = white ? (byte)220 : (byte)40;
                pixels[i + 1] = white ? (byte)220 : (byte)40;
                pixels[i + 2] = white ? (byte)220 : (byte)40;
                pixels[i + 3] = 255;
            }
        }

        using var fs = File.Create(filename);
        fs.Write(header, 0, header.Length);
        fs.Write(pixels, 0, pixels.Length);
    }
}

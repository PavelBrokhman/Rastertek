using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial33.Graphics;

public class Texture
{
    private uint m_id;
    private bool m_loaded;

    public unsafe bool Initialize(GL4 gl, string fn, uint tu, bool wrap)
    {
        var g = gl.Gl;
        if (!File.Exists(fn))
        {
            Console.WriteLine($"Texture not found: {fn}");
            return false;
        }
        if (!LoadTga(fn, out int w, out int h, out byte[] px))
        {
            Console.WriteLine($"Failed: {fn}");
            return false;
        }
        Console.WriteLine($"Loaded {fn} ({w}x{h})");
        g.ActiveTexture(TextureUnit.Texture0 + (int)tu);
        m_id = g.GenTexture();
        g.BindTexture(TextureTarget.Texture2D, m_id);
        fixed (byte* p = px)
            g.TexImage2D(
                TextureTarget.Texture2D,
                0,
                (int)InternalFormat.Rgba,
                (uint)w,
                (uint)h,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                p
            );
        g.GenerateMipmap(TextureTarget.Texture2D);
        var wm = wrap ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge;
        g.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wm);
        g.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wm);
        g.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear
        );
        g.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
        );
        m_loaded = true;
        return true;
    }

    public void SetTexture(GL4 gl, uint tu)
    {
        if (m_loaded)
        {
            gl.Gl.ActiveTexture(TextureUnit.Texture0 + (int)tu);
            gl.Gl.BindTexture(TextureTarget.Texture2D, m_id);
        }
    }

    public void Shutdown(GL4 gl)
    {
        if (m_loaded)
        {
            gl.Gl.DeleteTexture(m_id);
            m_loaded = false;
        }
    }

    static bool LoadTga(string fn, out int w, out int h, out byte[] rgba)
    {
        w = 0;
        h = 0;
        rgba = null;
        var d = File.ReadAllBytes(fn);
        if (d.Length < 18)
            return false;
        int il = d[0],
            it = d[2],
            bpp = d[16],
            desc = d[17];
        w = d[12] | (d[13] << 8);
        h = d[14] | (d[15] << 8);
        if (it != 2 || (bpp != 24 && bpp != 32))
            return false;
        int off = 18 + il,
            ch = bpp / 8,
            pc = w * h;
        if (d.Length < off + pc * ch)
            return false;
        rgba = new byte[pc * 4];
        bool tl = (desc & 0x20) != 0;
        for (int y = 0; y < h; y++)
        {
            int sr = tl ? y : h - 1 - y;
            int so = off + sr * w * ch;
            int doff = y * w * 4;
            for (int x = 0; x < w; x++)
            {
                rgba[doff + x * 4] = d[so + x * ch + 2];
                rgba[doff + x * 4 + 1] = d[so + x * ch + 1];
                rgba[doff + x * 4 + 2] = d[so + x * ch];
                rgba[doff + x * 4 + 3] = ch == 4 ? d[so + x * ch + 3] : (byte)255;
            }
        }
        return true;
    }
}

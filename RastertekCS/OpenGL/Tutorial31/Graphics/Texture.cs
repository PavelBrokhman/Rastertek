using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class Texture
{
    private uint m_textureId;
    private bool m_loaded;

    public unsafe bool Initialize(GL4 OpenGL, string filename, uint textureUnit, bool wrap)
    {
        var gl = OpenGL.Gl;
        if (!File.Exists(filename))
            GenerateCheckerboardTga(filename, 64);
        if (!LoadTga(filename, out int w, out int h, out byte[] px))
        {
            Console.WriteLine($"Failed: {filename}");
            return false;
        }
        gl.ActiveTexture(TextureUnit.Texture0 + (int)textureUnit);
        m_textureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, m_textureId);
        fixed (byte* p = px)
            gl.TexImage2D(
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
        gl.GenerateMipmap(TextureTarget.Texture2D);
        var wm = wrap ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge;
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wm);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wm);
        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear
        );
        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
        );
        m_loaded = true;
        return true;
    }

    public void SetTexture(GL4 OpenGL, uint tu)
    {
        if (m_loaded)
        {
            OpenGL.Gl.ActiveTexture(TextureUnit.Texture0 + (int)tu);
            OpenGL.Gl.BindTexture(TextureTarget.Texture2D, m_textureId);
        }
    }

    public void Shutdown(GL4 OpenGL)
    {
        if (m_loaded)
        {
            OpenGL.Gl.DeleteTexture(m_textureId);
            m_loaded = false;
        }
    }

    private static bool LoadTga(string fn, out int w, out int h, out byte[] rgba)
    {
        w = 0;
        h = 0;
        rgba = null;
        byte[] d = File.ReadAllBytes(fn);
        if (d.Length < 18)
            return false;
        int idLen = d[0],
            it = d[2],
            bpp = d[16],
            desc = d[17];
        w = d[12] | (d[13] << 8);
        h = d[14] | (d[15] << 8);
        if (it != 2 || (bpp != 24 && bpp != 32))
            return false;
        int off = 18 + idLen,
            ch = bpp / 8,
            pc = w * h;
        if (d.Length < off + pc * ch)
            return false;
        rgba = new byte[pc * 4];
        bool tl = (desc & 0x20) != 0;
        for (int y = 0; y < h; y++)
        {
            int sr = tl ? y : (h - 1 - y);
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

    private static void GenerateCheckerboardTga(string fn, int sz)
    {
        var dir = Path.GetDirectoryName(fn);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        byte cr = 180,
            cg = 180,
            cb = 180;
        var nm = Path.GetFileNameWithoutExtension(fn).ToLower();
        if (nm.Contains("ground"))
        {
            cr = 80;
            cg = 140;
            cb = 60;
        }
        else if (nm.Contains("wall"))
        {
            cr = 200;
            cg = 180;
            cb = 160;
        }
        else if (nm.Contains("marble"))
        {
            cr = 220;
            cg = 220;
            cb = 230;
        }
        else if (nm.Contains("water"))
        {
            cr = 40;
            cg = 80;
            cb = 200;
        }
        byte[] hdr = new byte[18];
        hdr[2] = 2;
        hdr[12] = (byte)(sz & 0xFF);
        hdr[13] = (byte)((sz >> 8) & 0xFF);
        hdr[14] = (byte)(sz & 0xFF);
        hdr[15] = (byte)((sz >> 8) & 0xFF);
        hdr[16] = 32;
        hdr[17] = 0x28;
        byte[] px = new byte[sz * sz * 4];
        int cell = sz / 8;
        for (int y = 0; y < sz; y++)
        for (int x = 0; x < sz; x++)
        {
            bool lit = (((x / cell) + (y / cell)) & 1) == 0;
            int i = (y * sz + x) * 4;
            px[i] = lit ? cb : (byte)20;
            px[i + 1] = lit ? cg : (byte)20;
            px[i + 2] = lit ? cr : (byte)20;
            px[i + 3] = 255;
        }
        using var fs = File.Create(fn);
        fs.Write(hdr);
        fs.Write(px);
    }
}

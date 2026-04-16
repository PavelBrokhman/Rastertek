using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class TextureShader
{
    private uint m_vs,
        m_fs,
        m_prog;

    public bool Initialize(GL4 gl) => Init(gl, "Shaders/texture.vs", "Shaders/texture.ps");

    public void Shutdown(GL4 gl)
    {
        var g = gl.Gl;
        g.DetachShader(m_prog, m_vs);
        g.DetachShader(m_prog, m_fs);
        g.DeleteShader(m_vs);
        g.DeleteShader(m_fs);
        g.DeleteProgram(m_prog);
    }

    public unsafe bool SetShaderParameters(
        GL4 gl,
        Matrix4X4<float> w,
        Matrix4X4<float> v,
        Matrix4X4<float> p
    )
    {
        var g = gl.Gl;
        g.UseProgram(m_prog);
        int loc;
        loc = g.GetUniformLocation(m_prog, "worldMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&w);
        loc = g.GetUniformLocation(m_prog, "viewMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&v);
        loc = g.GetUniformLocation(m_prog, "projectionMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&p);
        loc = g.GetUniformLocation(m_prog, "shaderTexture");
        if (loc >= 0)
            g.Uniform1(loc, 0);
        return true;
    }

    bool Init(GL4 gl, string vsf, string psf)
    {
        var g = gl.Gl;
        m_vs = g.CreateShader(ShaderType.VertexShader);
        g.ShaderSource(m_vs, File.ReadAllText(vsf));
        g.CompileShader(m_vs);
        g.GetShader(m_vs, ShaderParameterName.CompileStatus, out int s);
        if (s != 1)
        {
            Console.WriteLine($"VS: {g.GetShaderInfoLog(m_vs)}");
            return false;
        }
        m_fs = g.CreateShader(ShaderType.FragmentShader);
        g.ShaderSource(m_fs, File.ReadAllText(psf));
        g.CompileShader(m_fs);
        g.GetShader(m_fs, ShaderParameterName.CompileStatus, out s);
        if (s != 1)
        {
            Console.WriteLine($"PS: {g.GetShaderInfoLog(m_fs)}");
            return false;
        }
        m_prog = g.CreateProgram();
        g.AttachShader(m_prog, m_vs);
        g.AttachShader(m_prog, m_fs);
        g.BindAttribLocation(m_prog, 0, "inputPosition");
        g.BindAttribLocation(m_prog, 1, "inputTexCoord");
        g.LinkProgram(m_prog);
        g.GetProgram(m_prog, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1)
        {
            Console.WriteLine($"Link: {g.GetProgramInfoLog(m_prog)}");
            return false;
        }
        return true;
    }
}

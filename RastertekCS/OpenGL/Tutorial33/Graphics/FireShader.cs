using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial33.Graphics;

public class FireShader
{
    private uint m_vs,
        m_fs,
        m_prog;

    public bool Initialize(GL4 gl) => Init(gl, "Shaders/fire.vs", "Shaders/fire.ps");

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
        Matrix4X4<float> p,
        float frameTime,
        float[] scrollSpeeds,
        float[] scales,
        float[] d1,
        float[] d2,
        float[] d3,
        float dScale,
        float dBias
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
        loc = g.GetUniformLocation(m_prog, "frameTime");
        if (loc >= 0)
            g.Uniform1(loc, frameTime);
        loc = g.GetUniformLocation(m_prog, "scrollSpeeds");
        if (loc >= 0)
            fixed (float* pp = scrollSpeeds)
                g.Uniform3(loc, 1, pp);
        loc = g.GetUniformLocation(m_prog, "scales");
        if (loc >= 0)
            fixed (float* pp = scales)
                g.Uniform3(loc, 1, pp);
        loc = g.GetUniformLocation(m_prog, "fireTexture");
        if (loc >= 0)
            g.Uniform1(loc, 0);
        loc = g.GetUniformLocation(m_prog, "noiseTexture");
        if (loc >= 0)
            g.Uniform1(loc, 1);
        loc = g.GetUniformLocation(m_prog, "alphaTexture");
        if (loc >= 0)
            g.Uniform1(loc, 2);
        loc = g.GetUniformLocation(m_prog, "distortion1");
        if (loc >= 0)
            fixed (float* pp = d1)
                g.Uniform2(loc, 1, pp);
        loc = g.GetUniformLocation(m_prog, "distortion2");
        if (loc >= 0)
            fixed (float* pp = d2)
                g.Uniform2(loc, 1, pp);
        loc = g.GetUniformLocation(m_prog, "distortion3");
        if (loc >= 0)
            fixed (float* pp = d3)
                g.Uniform2(loc, 1, pp);
        loc = g.GetUniformLocation(m_prog, "distortionScale");
        if (loc >= 0)
            g.Uniform1(loc, dScale);
        loc = g.GetUniformLocation(m_prog, "distortionBias");
        if (loc >= 0)
            g.Uniform1(loc, dBias);
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
        g.BindAttribLocation(m_prog, 2, "inputNormal");
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

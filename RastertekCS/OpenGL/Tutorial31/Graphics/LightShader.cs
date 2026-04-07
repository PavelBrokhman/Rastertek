using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class LightShader
{
    private uint m_vs, m_fs, m_prog;

    public bool Initialize(GL4 gl) => Init(gl, "Shaders/light.vs", "Shaders/light.ps");
    public void Shutdown(GL4 gl) { var g = gl.Gl; g.DetachShader(m_prog, m_vs); g.DetachShader(m_prog, m_fs); g.DeleteShader(m_vs); g.DeleteShader(m_fs); g.DeleteProgram(m_prog); }

    public unsafe bool SetShaderParameters(GL4 gl, Matrix4X4<float> world, Matrix4X4<float> view, Matrix4X4<float> proj,
        float[] lightDir, float[] diffuse, float[] ambient)
    {
        var g = gl.Gl; g.UseProgram(m_prog);
        int loc;
        loc = g.GetUniformLocation(m_prog, "worldMatrix"); if (loc >= 0) g.UniformMatrix4(loc, 1, false, (float*)&world);
        loc = g.GetUniformLocation(m_prog, "viewMatrix"); if (loc >= 0) g.UniformMatrix4(loc, 1, false, (float*)&view);
        loc = g.GetUniformLocation(m_prog, "projectionMatrix"); if (loc >= 0) g.UniformMatrix4(loc, 1, false, (float*)&proj);
        loc = g.GetUniformLocation(m_prog, "shaderTexture"); if (loc >= 0) g.Uniform1(loc, 0);
        loc = g.GetUniformLocation(m_prog, "lightDirection"); if (loc >= 0) fixed (float* p = lightDir) g.Uniform3(loc, 1, p);
        loc = g.GetUniformLocation(m_prog, "diffuseLightColor"); if (loc >= 0) fixed (float* p = diffuse) g.Uniform4(loc, 1, p);
        loc = g.GetUniformLocation(m_prog, "ambientLight"); if (loc >= 0) fixed (float* p = ambient) g.Uniform4(loc, 1, p);
        return true;
    }

    private bool Init(GL4 gl, string vsf, string psf)
    {
        var g = gl.Gl;
        m_vs = g.CreateShader(ShaderType.VertexShader); g.ShaderSource(m_vs, File.ReadAllText(vsf)); g.CompileShader(m_vs);
        g.GetShader(m_vs, ShaderParameterName.CompileStatus, out int s); if (s != 1) { Console.WriteLine($"VS: {g.GetShaderInfoLog(m_vs)}"); return false; }
        m_fs = g.CreateShader(ShaderType.FragmentShader); g.ShaderSource(m_fs, File.ReadAllText(psf)); g.CompileShader(m_fs);
        g.GetShader(m_fs, ShaderParameterName.CompileStatus, out s); if (s != 1) { Console.WriteLine($"PS: {g.GetShaderInfoLog(m_fs)}"); return false; }
        m_prog = g.CreateProgram(); g.AttachShader(m_prog, m_vs); g.AttachShader(m_prog, m_fs);
        g.BindAttribLocation(m_prog, 0, "inputPosition"); g.BindAttribLocation(m_prog, 1, "inputTexCoord"); g.BindAttribLocation(m_prog, 2, "inputNormal");
        g.LinkProgram(m_prog); g.GetProgram(m_prog, ProgramPropertyARB.LinkStatus, out int ls); if (ls != 1) { Console.WriteLine($"Link: {g.GetProgramInfoLog(m_prog)}"); return false; }
        return true;
    }
}

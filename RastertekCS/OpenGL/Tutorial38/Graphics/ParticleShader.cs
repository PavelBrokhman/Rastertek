using Silk.NET.Maths; using Silk.NET.OpenGL;
namespace RastertekCS.OpenGL.Tutorial38.Graphics;
public class ParticleShader
{
    private uint m_vs, m_fs, m_prog;
    public bool Initialize(GL4 gl4) => Init(gl4, "Shaders/Particle.vs", "Shaders/Particle.ps");
    public void Shutdown(GL4 gl4) { var g=gl4.Gl; g.DetachShader(m_prog,m_vs); g.DetachShader(m_prog,m_fs); g.DeleteShader(m_vs); g.DeleteShader(m_fs); g.DeleteProgram(m_prog); }
    public bool SetShaderParameters(GL4 gl4, Matrix4X4<float> w, Matrix4X4<float> v, Matrix4X4<float> p)
    {
        var g=gl4.Gl; g.UseProgram(m_prog); int loc;
        loc=g.GetUniformLocation(m_prog,"worldMatrix"); if(loc==-1) return false; unsafe{g.UniformMatrix4(loc,1,false,(float*)&w);}
        loc=g.GetUniformLocation(m_prog,"viewMatrix"); if(loc==-1) return false; unsafe{g.UniformMatrix4(loc,1,false,(float*)&v);}
        loc=g.GetUniformLocation(m_prog,"projectionMatrix"); if(loc==-1) return false; unsafe{g.UniformMatrix4(loc,1,false,(float*)&p);}
        loc=g.GetUniformLocation(m_prog,"shaderTexture"); if(loc==-1) return false; g.Uniform1(loc,0);
        return true;
    }
    private bool Init(GL4 gl4, string vsf, string psf)
    {
        var g=gl4.Gl; m_vs=g.CreateShader(ShaderType.VertexShader); g.ShaderSource(m_vs,File.ReadAllText(vsf)); g.CompileShader(m_vs);
        g.GetShader(m_vs,ShaderParameterName.CompileStatus,out int s); if(s!=1){Console.WriteLine($"Compile {vsf}: {g.GetShaderInfoLog(m_vs)}"); return false;}
        m_fs=g.CreateShader(ShaderType.FragmentShader); g.ShaderSource(m_fs,File.ReadAllText(psf)); g.CompileShader(m_fs);
        g.GetShader(m_fs,ShaderParameterName.CompileStatus,out s); if(s!=1){Console.WriteLine($"Compile {psf}: {g.GetShaderInfoLog(m_fs)}"); return false;}
        m_prog=g.CreateProgram(); g.AttachShader(m_prog,m_vs); g.AttachShader(m_prog,m_fs);
        g.BindAttribLocation(m_prog,0,"inputPosition"); g.BindAttribLocation(m_prog,1,"inputTexCoord"); g.BindAttribLocation(m_prog,2,"inputColor");
        g.LinkProgram(m_prog); g.GetProgram(m_prog,ProgramPropertyARB.LinkStatus,out int ls); if(ls!=1){Console.WriteLine($"Link: {g.GetProgramInfoLog(m_prog)}"); return false;}
        return true;
    }
}

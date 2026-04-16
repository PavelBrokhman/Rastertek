using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial32.Graphics;

public class GlassShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 gl) => Init(gl, "Shaders/glass.vs", "Shaders/glass.ps");

    public void Shutdown(GL4 gl)
    {
        var g = gl.Gl;
        g.DetachShader(_shaderProgram, _vertexShader);
        g.DetachShader(_shaderProgram, _fragmentShader);
        g.DeleteShader(_vertexShader);
        g.DeleteShader(_fragmentShader);
        g.DeleteProgram(_shaderProgram);
    }

    public unsafe bool SetShaderParameters(
        GL4 gl,
        Matrix4X4<float> w,
        Matrix4X4<float> v,
        Matrix4X4<float> p,
        float refrScale
    )
    {
        var g = gl.Gl;
        g.UseProgram(_shaderProgram);
        int loc;
        loc = g.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&w);
        loc = g.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&v);
        loc = g.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&p);
        loc = g.GetUniformLocation(_shaderProgram, "colorTexture");
        if (loc >= 0)
            g.Uniform1(loc, 0);
        loc = g.GetUniformLocation(_shaderProgram, "normalTexture");
        if (loc >= 0)
            g.Uniform1(loc, 1);
        loc = g.GetUniformLocation(_shaderProgram, "refractionTexture");
        if (loc >= 0)
            g.Uniform1(loc, 2);
        loc = g.GetUniformLocation(_shaderProgram, "refractionScale");
        if (loc >= 0)
            g.Uniform1(loc, refrScale);
        return true;
    }

    bool Init(GL4 gl, string vsf, string psf)
    {
        var g = gl.Gl;
        _vertexShader = g.CreateShader(ShaderType.VertexShader);
        g.ShaderSource(_vertexShader, File.ReadAllText(vsf));
        g.CompileShader(_vertexShader);
        g.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1)
        {
            Console.WriteLine($"VS: {g.GetShaderInfoLog(_vertexShader)}");
            return false;
        }
        _fragmentShader = g.CreateShader(ShaderType.FragmentShader);
        g.ShaderSource(_fragmentShader, File.ReadAllText(psf));
        g.CompileShader(_fragmentShader);
        g.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1)
        {
            Console.WriteLine($"PS: {g.GetShaderInfoLog(_fragmentShader)}");
            return false;
        }
        _shaderProgram = g.CreateProgram();
        g.AttachShader(_shaderProgram, _vertexShader);
        g.AttachShader(_shaderProgram, _fragmentShader);
        g.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        g.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        g.BindAttribLocation(_shaderProgram, 2, "inputNormal");
        g.LinkProgram(_shaderProgram);
        g.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1)
        {
            Console.WriteLine($"Link: {g.GetProgramInfoLog(_shaderProgram)}");
            return false;
        }
        return true;
    }
}

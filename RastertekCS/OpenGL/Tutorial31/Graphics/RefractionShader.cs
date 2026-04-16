using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class RefractionShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 gl) => Init(gl, "Shaders/refraction.vs", "Shaders/refraction.ps");

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
        Matrix4X4<float> world,
        Matrix4X4<float> view,
        Matrix4X4<float> proj,
        float[] lightDir,
        float[] diffuse,
        float[] ambient,
        float[] clipPlane
    )
    {
        var g = gl.Gl;
        g.UseProgram(_shaderProgram);
        int loc;
        loc = g.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&world);
        loc = g.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&view);
        loc = g.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&proj);
        loc = g.GetUniformLocation(_shaderProgram, "clipPlane");
        if (loc >= 0)
            fixed (float* p = clipPlane)
                g.Uniform4(loc, 1, p);
        loc = g.GetUniformLocation(_shaderProgram, "shaderTexture");
        if (loc >= 0)
            g.Uniform1(loc, 0);
        loc = g.GetUniformLocation(_shaderProgram, "lightDirection");
        if (loc >= 0)
            fixed (float* p = lightDir)
                g.Uniform3(loc, 1, p);
        loc = g.GetUniformLocation(_shaderProgram, "diffuseLightColor");
        if (loc >= 0)
            fixed (float* p = diffuse)
                g.Uniform4(loc, 1, p);
        loc = g.GetUniformLocation(_shaderProgram, "ambientLight");
        if (loc >= 0)
            fixed (float* p = ambient)
                g.Uniform4(loc, 1, p);
        return true;
    }

    private bool Init(GL4 gl, string vsf, string psf)
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

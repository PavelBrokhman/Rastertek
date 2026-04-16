using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial31.Graphics;

public class WaterShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 gl) => Init(gl, "Shaders/water.vs", "Shaders/water.ps");

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
        Matrix4X4<float> reflection,
        float waterTrans,
        float reflRefScale
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
        loc = g.GetUniformLocation(_shaderProgram, "reflectionMatrix");
        if (loc >= 0)
            g.UniformMatrix4(loc, 1, false, (float*)&reflection);
        loc = g.GetUniformLocation(_shaderProgram, "normalTexture");
        if (loc >= 0)
            g.Uniform1(loc, 0);
        loc = g.GetUniformLocation(_shaderProgram, "refractionTexture");
        if (loc >= 0)
            g.Uniform1(loc, 1);
        loc = g.GetUniformLocation(_shaderProgram, "reflectionTexture");
        if (loc >= 0)
            g.Uniform1(loc, 2);
        loc = g.GetUniformLocation(_shaderProgram, "waterTranslation");
        if (loc >= 0)
            g.Uniform1(loc, waterTrans);
        loc = g.GetUniformLocation(_shaderProgram, "reflectRefractScale");
        if (loc >= 0)
            g.Uniform1(loc, reflRefScale);
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

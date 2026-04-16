using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial30.Graphics;

public class ReflectionShader
{
    private uint _vertexShader,
        _fragmentShader,
        _shaderProgram;

    public bool Initialize(GL4 OpenGL) =>
        InitializeShader(OpenGL, "Shaders/reflection.vs", "Shaders/reflection.ps");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(_shaderProgram, _vertexShader);
        gl.DetachShader(_shaderProgram, _fragmentShader);
        gl.DeleteShader(_vertexShader);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteProgram(_shaderProgram);
    }

    public unsafe bool SetShaderParameters(
        GL4 OpenGL,
        Matrix4X4<float> world,
        Matrix4X4<float> view,
        Matrix4X4<float> projection,
        Matrix4X4<float> reflection
    )
    {
        var gl = OpenGL.Gl;
        gl.UseProgram(_shaderProgram);
        int loc;
        loc = gl.GetUniformLocation(_shaderProgram, "worldMatrix");
        if (loc >= 0)
            gl.UniformMatrix4(loc, 1, false, (float*)&world);
        loc = gl.GetUniformLocation(_shaderProgram, "viewMatrix");
        if (loc >= 0)
            gl.UniformMatrix4(loc, 1, false, (float*)&view);
        loc = gl.GetUniformLocation(_shaderProgram, "projectionMatrix");
        if (loc >= 0)
            gl.UniformMatrix4(loc, 1, false, (float*)&projection);
        loc = gl.GetUniformLocation(_shaderProgram, "reflectionMatrix");
        if (loc >= 0)
            gl.UniformMatrix4(loc, 1, false, (float*)&reflection);
        loc = gl.GetUniformLocation(_shaderProgram, "shaderTexture");
        if (loc >= 0)
            gl.Uniform1(loc, 0);
        loc = gl.GetUniformLocation(_shaderProgram, "reflectionTexture");
        if (loc >= 0)
            gl.Uniform1(loc, 1);
        return true;
    }

    private bool InitializeShader(GL4 OpenGL, string vsFile, string psFile)
    {
        var gl = OpenGL.Gl;
        _vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(_vertexShader, File.ReadAllText(vsFile));
        gl.CompileShader(_vertexShader);
        gl.GetShader(_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1)
        {
            Console.WriteLine($"VS compile: {gl.GetShaderInfoLog(_vertexShader)}");
            return false;
        }

        _fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(_fragmentShader, File.ReadAllText(psFile));
        gl.CompileShader(_fragmentShader);
        gl.GetShader(_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1)
        {
            Console.WriteLine($"PS compile: {gl.GetShaderInfoLog(_fragmentShader)}");
            return false;
        }

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);
        gl.BindAttribLocation(_shaderProgram, 0, "inputPosition");
        gl.BindAttribLocation(_shaderProgram, 1, "inputTexCoord");
        gl.LinkProgram(_shaderProgram);
        gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1)
        {
            Console.WriteLine($"Link: {gl.GetProgramInfoLog(_shaderProgram)}");
            return false;
        }
        return true;
    }
}
